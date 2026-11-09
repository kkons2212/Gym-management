using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace GymManagement.Server.Core.Services.Reporting;

/// <summary>
/// Lớp bổ trợ đóng gói và kết xuất dữ liệu báo cáo sang định dạng Excel (.xlsx) và PDF (.pdf).
/// Sử dụng hoàn toàn thư viện chuẩn .NET BCL (ZipArchive, Streams) đảm bảo hiệu năng cao và không phụ thuộc bên ngoài.
/// </summary>
internal static class ReportExportHelper
{
    private const string ContentTypeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string ContentTypePdf = "application/pdf";

    /// <summary>
    /// Xuất dữ liệu bảng biểu ra tệp tin OpenXML Spreadsheet (.xlsx).
    /// </summary>
    public static (string ContentType, byte[] Bytes) GenerateExcel(
        string title,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            WriteZipEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
            WriteZipEntry(archive, "_rels/.rels", BuildRootRelsXml());
            WriteZipEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelsXml());
            WriteZipEntry(archive, "xl/workbook.xml", BuildWorkbookXml());
            WriteZipEntry(archive, "xl/styles.xml", BuildStylesXml());
            WriteZipEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(title, headers, rows));
        }

        byte[] resultBytes = memoryStream.ToArray();
        return (ContentTypeExcel, resultBytes);
    }

    /// <summary>
    /// Xuất dữ liệu bảng biểu ra tệp tin chuẩn PDF 1.4.
    /// </summary>
    public static (string ContentType, byte[] Bytes) GeneratePdf(
        string title,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 16 Tf");
        contentBuilder.AppendLine("50 780 Td");
        contentBuilder.AppendLine($"({EscapePdfText(title)}) Tj");
        contentBuilder.AppendLine("0 -24 Td");
        contentBuilder.AppendLine("/F1 9 Tf");
        contentBuilder.AppendLine($"({EscapePdfText("Xuat luc: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC")}) Tj");
        contentBuilder.AppendLine("0 -20 Td");

        // Vẽ dòng tiêu đề cột
        contentBuilder.AppendLine("/F1 10 Tf");
        string headerLine = string.Join("  |  ", headers);
        contentBuilder.AppendLine($"({EscapePdfText(headerLine)}) Tj");
        contentBuilder.AppendLine("0 -15 Td");

        // Vẽ đường phân cách
        string separatorLine = new string('-', Math.Min(100, Math.Max(20, headerLine.Length * 2)));
        contentBuilder.AppendLine($"({EscapePdfText(separatorLine)}) Tj");
        contentBuilder.AppendLine("0 -14 Td");

        // Vẽ các dòng dữ liệu
        contentBuilder.AppendLine("/F1 9 Tf");
        for (int i = 0; i < rows.Count; i++)
        {
            IReadOnlyList<string> row = rows[i];
            string rowLine = string.Join("  |  ", row);
            contentBuilder.AppendLine($"({EscapePdfText(rowLine)}) Tj");
            contentBuilder.AppendLine("0 -13 Td");
        }

        contentBuilder.AppendLine("ET");

        byte[] streamBytes = Encoding.UTF8.GetBytes(contentBuilder.ToString());

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.ASCII, leaveOpen: true))
        {
            var offsets = new List<long>();

            writer.Write("%PDF-1.4\n");
            writer.Flush();

            // Object 1: Catalog
            offsets.Add(memoryStream.Position);
            writer.Write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
            writer.Flush();

            // Object 2: Pages
            offsets.Add(memoryStream.Position);
            writer.Write("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
            writer.Flush();

            // Object 3: Page
            offsets.Add(memoryStream.Position);
            writer.Write("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n");
            writer.Flush();

            // Object 4: Contents Stream
            offsets.Add(memoryStream.Position);
            writer.Write($"4 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n");
            writer.Flush();
            memoryStream.Write(streamBytes, 0, streamBytes.Length);
            writer.Write("\nendstream\nendobj\n");
            writer.Flush();

            // Object 5: Font Helvetica
            offsets.Add(memoryStream.Position);
            writer.Write("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
            writer.Flush();

            // XRef Table
            long xrefOffset = memoryStream.Position;
            writer.Write($"xref\n0 {offsets.Count + 1}\n");
            writer.Write("0000000000 65535 f \n");
            for (int i = 0; i < offsets.Count; i++)
            {
                writer.Write(offsets[i].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
            }

            // Trailer
            writer.Write($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\n");
            writer.Write($"startxref\n{xrefOffset}\n%%EOF");
            writer.Flush();
        }

        byte[] resultBytes = memoryStream.ToArray();
        return (ContentTypePdf, resultBytes);
    }

    private static void WriteZipEntry(ZipArchive archive, string entryName, string content)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(content);
    }

    private static string BuildContentTypesXml()
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" +
               "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">\n" +
               "  <Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>\n" +
               "  <Default Extension=\"xml\" ContentType=\"application/xml\"/>\n" +
               "  <Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>\n" +
               "  <Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>\n" +
               "  <Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>\n" +
               "</Types>";
    }

    private static string BuildRootRelsXml()
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" +
               "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">\n" +
               "  <Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>\n" +
               "</Relationships>";
    }

    private static string BuildWorkbookRelsXml()
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" +
               "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">\n" +
               "  <Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>\n" +
               "  <Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>\n" +
               "</Relationships>";
    }

    private static string BuildWorkbookXml()
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" +
               "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">\n" +
               "  <sheets>\n" +
               "    <sheet name=\"Report\" sheetId=\"1\" r:id=\"rId1\"/>\n" +
               "  </sheets>\n" +
               "</workbook>";
    }

    private static string BuildStylesXml()
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" +
               "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">\n" +
               "  <fonts count=\"1\">\n" +
               "    <font><name val=\"Calibri\"/><sz val=\"11\"/></font>\n" +
               "  </fonts>\n" +
               "  <fills count=\"1\">\n" +
               "    <fill><patternFill patternType=\"none\"/></fill>\n" +
               "  </fills>\n" +
               "  <borders count=\"1\">\n" +
               "    <border><left/><right/><top/><bottom/></border>\n" +
               "  </borders>\n" +
               "  <cellStyleXfs count=\"1\">\n" +
               "    <xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/>\n" +
               "  </cellStyleXfs>\n" +
               "  <cellXfs count=\"1\">\n" +
               "    <xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>\n" +
               "  </cellXfs>\n" +
               "</styleSheet>";
    }

    private static string BuildWorksheetXml(
        string title,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.AppendLine("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        sb.AppendLine("  <sheetData>");

        int rowIndex = 1;

        // Dòng 1: Tiêu đề báo cáo
        sb.AppendLine($"    <row r=\"{rowIndex}\">");
        sb.AppendLine($"      <c r=\"A{rowIndex}\" t=\"inlineStr\"><is><t>{EscapeXml(title)}</t></is></c>");
        sb.AppendLine("    </row>");
        rowIndex++;

        // Dòng 2: Mốc thời gian xuất
        string exportedAt = "Xuat luc: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";
        sb.AppendLine($"    <row r=\"{rowIndex}\">");
        sb.AppendLine($"      <c r=\"A{rowIndex}\" t=\"inlineStr\"><is><t>{EscapeXml(exportedAt)}</t></is></c>");
        sb.AppendLine("    </row>");
        rowIndex++;

        // Dòng trống cách quãng
        rowIndex++;

        // Dòng tiêu đề các cột
        sb.AppendLine($"    <row r=\"{rowIndex}\">");
        for (int col = 0; col < headers.Count; col++)
        {
            string colLetter = GetColumnLetter(col + 1);
            sb.AppendLine($"      <c r=\"{colLetter}{rowIndex}\" t=\"inlineStr\"><is><t>{EscapeXml(headers[col])}</t></is></c>");
        }
        sb.AppendLine("    </row>");
        rowIndex++;

        // Các dòng dữ liệu
        for (int r = 0; r < rows.Count; r++)
        {
            IReadOnlyList<string> row = rows[r];
            sb.AppendLine($"    <row r=\"{rowIndex}\">");
            for (int col = 0; col < row.Count; col++)
            {
                string colLetter = GetColumnLetter(col + 1);
                sb.AppendLine($"      <c r=\"{colLetter}{rowIndex}\" t=\"inlineStr\"><is><t>{EscapeXml(row[col])}</t></is></c>");
            }
            sb.AppendLine("    </row>");
            rowIndex++;
        }

        sb.AppendLine("  </sheetData>");
        sb.AppendLine("</worksheet>");

        return sb.ToString();
    }

    private static string GetColumnLetter(int columnNumber)
    {
        string dividend = string.Empty;
        int modulo;

        while (columnNumber > 0)
        {
            modulo = (columnNumber - 1) % 26;
            dividend = Convert.ToChar(65 + modulo) + dividend;
            columnNumber = (columnNumber - modulo) / 26;
        }

        return dividend;
    }

    private static string EscapeXml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static string EscapePdfText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Loại bỏ ký tự đặc biệt của PDF
        return input
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }
}

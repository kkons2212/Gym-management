# VISUAL USER INTERFACE RULES (UI SYSTEM RULES)
## Hệ Thống Quản Lý Phòng Tập Thể Thao (Gym Management System)
### WinUI 3 Desktop (Admin) & Blazor Server (Member/PT Portal)

> **Mục đích tài liệu:** Tài liệu này quy định toàn bộ **Quy chuẩn Giao diện Thị giác (UI - Visual User Interface)**, Hệ thống Design Tokens, Bố cục lưới, Cấu trúc thành phần giao diện cho cả 10 Phân hệ nghiệp vụ (37 Use Cases theo [`PRD.md`](file:///c:/Users/nguye/Desktop/Gym-management/Document/PRD.md)) dành cho các lập trình viên và AI Agent khi xây dựng mã nguồn XAML (`WinUI 3 Desktop`) và HTML/CSS/Tailwind/Razor (`Blazor Server Web`).
>
> **Triết lý Thiết kế Cốt lõi (Core Visual Philosophy):** Tối giản, Đậm chất Thể thao Năng động (Athletic Modernism), Tương phản cao chuẩn WCAG 2.2 AA ($\ge 4.5:1$ cho chữ, $\ge 3:1$ cho viền/vùng tương tác), Màu sắc chuẩn ngữ nghĩa, **độc tôn giao diện tối (Dark Mode 100%)** — **tuyệt đối không sử dụng hiệu ứng viền/chấm nhấp nháy, không dùng âm thanh gây phiền toái, và không hiển thị mã Use Case kỹ thuật `(UC...)` lên bất kỳ thành phần giao diện người dùng nào**.

---

## 1. HỆ THỐNG DESIGN TOKENS & NỀN TẢNG THỊ GIÁC (NEW COLORS PANEL - 100% DARK MODE)

Mọi mã nguồn XAML, CSS hoặc thành phần giao diện bắt buộc tuân theo bảng định danh trực quan 6 phân tầng (L0 $\rightarrow$ L5) dưới đây:

### 1.1. Bảng Màu Chuẩn 6 Phân Tầng (New Athletic Dark Palette)

Hệ thống sử dụng duy nhất giao diện tối (Dark Mode 100%) đồng nhất trên cả WinUI 3 Desktop và Blazor Web. Bảng màu được chuẩn hóa tỷ lệ tương phản cao trên nền tối, tách biệt rõ ràng giữa màu chữ/nhãn và màu nền khối:

| Phân Tầng | Tên Token (Role) | Mã Hex / RGB | Tailwind CSS (Web) | XAML SolidColorBrush (WinUI 3) | Tương Phản WCAG & Ràng Buộc Nghiệp Vụ |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **L0: Không Gian (Surfaces)** | **`bg-canvas`** | `#0A0F1D` | `bg-[#0A0F1D]` | `AppBackgroundBrush` | Nền gốc toàn bộ ứng dụng (Obsidian Deep Dark), triệt tiêu ánh sáng xanh chói. |
| | **`bg-surface-base`** | `#111827` | `bg-gray-900` | `LayerFillColorDefaultBrush` | Nền thanh điều hướng (NavigationView), TitleBar quầy Admin. |
| | **`bg-surface-card`** | `#1E293B` | `bg-slate-800` | `CardBackgroundBrush` | Thẻ gói tập, thẻ hội viên, card KPI dashboard, giỏ hàng POS quầy. |
| | **`bg-surface-elevated`**| `#27354A` | `bg-slate-750` | `ElevatedSurfaceBrush` | Khung popover, menu thả xuống, tooltip giải thích xung đột lịch. |
| | **`bg-surface-modal`** | `#162032` | `bg-slate-850` | `DialogBackgroundBrush` | Hộp thoại ContentDialog Desktop, Modal thanh toán VietQR Web. |
| **L1: Đường Viền (Borders)** | **`border-subtle`** | `#334155` | `border-slate-700` | `SubtleBorderBrush` | Đường viền thanh mảnh `1px solid` phân tách các khối nội dung. |
| | **`border-interactive`** | `#475569` | `border-slate-600` | `ControlStrokeColorDefault` | Viền ô TextBox nhập liệu, viền nút bấm thứ cấp ở trạng thái nghỉ. |
| | **`border-focus`** | `#38BDF8` | `ring-sky-400` | `FocusVisualPrimaryBrush` | Vòng ring tiêu điểm `:focus-visible` dày 2px với offset 2px ($\ge 3:1$). |
| **L2: Kiểu Chữ (Text)** | **`text-primary`** | `#F8FAFC` | `text-slate-50` | `TextFillColorPrimaryBrush` | Tỷ lệ 14.8:1: Tiêu đề, số tiền POS, tên hội viên tại cổng check-in. |
| | **`text-secondary`** | `#94A3B8` | `text-slate-400` | `TextFillColorSecondaryBrush` | Tỷ lệ 5.2:1: Nhãn form, số điện thoại, ngày hết hạn gói, chú thích InBody. |
| | **`text-tertiary`** | `#64748B` | `text-slate-500` | `TextFillColorTertiaryBrush` | Placeholder gợi ý tìm kiếm, chú thích không bắt buộc. |
| | **`text-disabled`** | `#475569` | `text-slate-600` | `TextFillColorDisabledBrush` | Chữ của nút/ô nhập liệu đang bị khóa hoặc slot giờ đã kín. |
| **L3: Điểm Nhấn (Accent)** | **`accent-primary`** | `#0EA5E9` | `bg-sky-500` | `AccentFillColorDefaultBrush` | Nút CTA chính: Quét QR, Thanh toán, Đặt lịch PT, Đăng ký gói. |
| | **`accent-hover`** | `#38BDF8` | `hover:bg-sky-400` | `AccentFillColorSecondaryBrush`| Trạng thái di chuột vào nút CTA chính. |
| | **`accent-pressed`** | `#0284C7` | `active:bg-sky-600` | `AccentFillColorTertiaryBrush` | Trạng thái nhấn giữ nút hoặc tab đang kích hoạt. |
| | **`accent-subtle`** | `rgba(14,165,233,0.15)`| `bg-sky-500/15` | `AccentSubtleBrush` | Highlight hàng đang chọn trong DataGrid hoặc gói tập được chọn. |
| **L4: Trạng Thái (Status)** | **`status-success`** | Chữ: `#4ADE80`<br>Nền: `#064E3B`<br>Viền: `#059669` | `text-emerald-400 bg-emerald-950/60 border-emerald-600` | `StatusSuccessBrush` | Check-in hợp lệ, Cổng turnstile mở, Gói `ACTIVE`, Hóa đơn `PAID`. |
| | **`status-warning`** | Chữ: `#FBBF24`<br>Nền: `#78350F`<br>Viền: `#D97706` | `text-amber-400 bg-amber-950/60 border-amber-600` | `StatusWarningBrush` | Gói sắp hết hạn ($\le 7$ ngày), Gói bảo lưu `FROZEN`, Cảnh báo hủy ca $<2$h. |
| | **`status-danger`** | Chữ: `#F87171`<br>Nền: `#7F1D1D`<br>Viền: `#DC2626` | `text-red-400 bg-red-950/60 border-red-600` | `StatusDangerBrush` | Gói `EXPIRED`, `CANCELLED`, Check-in bị từ chối/Cổng khóa, Trùng lịch PT. |
| | **`status-pt`** | Chữ: `#C084FC`<br>Nền: `#4C1D95`<br>Viền: `#7C3AED` | `text-purple-400 bg-purple-950/60 border-purple-600` | `StatusPtBrush` | Ca tập PT đã đặt (`PTTrainingSession`), khối thông tin chỉ số InBody. |
| | **`slot-pt-open`** | Chữ: `#E2E8F0`<br>Nền: `#1E293B`<br>Viền: `#475569` | `text-slate-200 bg-slate-800 border-slate-600` | `PtSlotOpenBrush` | Khung giờ rảnh PT (`PTAvailability`) — **Màu trung tính thể thao, CẤM dùng màu xanh lá**. |
| **L5: Biểu Đồ (Charts)** | **`chart-underweight`**| `#60A5FA` (Blue 400) | `text-blue-400` | `BmiUnderweightBrush` | Phân loại BMI Gầy ($< 18.5$). |
| | **`chart-normal`** | `#34D399` (Emerald 400)| `text-emerald-400` | `BmiNormalBrush` | Phân loại BMI Chuẩn ($18.5 - 24.9$). |
| | **`chart-overweight`** | `#FBBF24` (Amber 400) | `text-amber-400` | `BmiOverweightBrush` | Phân loại BMI Thừa cân ($25 - 29.9$). |
| | **`chart-obese`** | `#F87171` (Rose 400) | `text-rose-400` | `BmiObeseBrush` | Phân loại BMI Béo phì ($\ge 30$). |
| | **`chart-peak-hour`** | `#0EA5E9` (Sky 500) | `fill-sky-500` | `ChartPeakHourBrush` | Cột giờ cao điểm (17h - 20h) trên biểu đồ lưu lượng ra vào. |

> [!IMPORTANT]
> **Quy định về Chip Trạng Thái & Nhãn:** Giữ nhãn trạng thái ở dạng văn bản thuần túy (ví dụ: `Đang hoạt động`, `Đang bảo lưu`, `Đã hết hạn`), **tuyệt đối không thêm biểu tượng icon** vào chip trạng thái để giữ giao diện phẳng, tối giản.

---

### 1.2. Kiểu Chữ (Typography Scale) & Quy Chuẩn Vi Mô (Micro-Typography Tokens)

* **Phông chữ tiêu chuẩn:**
  * **WinUI 3 Desktop:** `Segoe UI Variable Display` (Tiêu đề) và `Segoe UI Variable Text` (Nội dung thông thường).
  * **Blazor Server Web:** `Inter`, `-apple-system`, `BlinkMacSystemFont`, `Segoe UI`, `sans-serif`.
* **Thang kích thước chữ (Type Scale):**
  * `Display 1` ($28\text{px} - 32\text{px}$, Bold): Số tiền thanh toán POS, Số lượt tập còn lại của gói tập, Đồng hồ đếm ngược.
  * `Heading 2` ($20\text{px} - 24\text{px}$, SemiBold): Tiêu đề trang, Tên hội viên tại màn hình Check-in, Tên gói tập.
  * `Body Text` ($14\text{px} - 16\text{px}$, Regular): Nội dung bảng dữ liệu, nhãn form, thông số chi tiết.
  * `Caption / Micro` ($11\text{px} - 12\text{px}$, Medium): Nhãn trạng thái, ngày hết hạn, mã định danh gói tập.
* **Quy chuẩn Vi mô (Micro-Typography):**
  * **Ký tự dấu ba chấm:** Bắt buộc sử dụng ký tự ba chấm chuẩn `…` (`U+2026` / `&hellip;`), nghiêm cấm gõ 3 dấu chấm liên tiếp `...`.
  * **Khoảng trắng không ngắt (Non-Breaking Space):** Giữa số lượng và đơn vị đo lường bắt buộc sử dụng `&nbsp;` (Web) hoặc `\u00A0` (XAML/C#) để tránh hiện tượng rớt chữ (ví dụ: `800.000&nbsp;VNĐ`, `10&nbsp;kg`, `60&nbsp;giây`, `2&nbsp;giờ`, `175&nbsp;cm`).
  * **Chữ số Đơn cách (Tabular Numbers):** Mọi hiển thị tiền tệ, chỉ số đếm ngược, số lượt tập còn lại và các cột số liệu trong bảng dữ liệu bắt buộc cấu hình `font-variant-numeric: tabular-nums` (CSS) hoặc `Typography.NumeralAlignment="Tabular"` (XAML) để đảm bảo độ rộng ký tự đồng đều, không gây rung lắc bố cục khi số thay đổi.
  * **Xử lý tràn văn bản (Truncation & Line Clamp):**
    * Áp dụng class `truncate` hoặc `line-clamp-2` cho tên gói tập và ghi chú. Mọi thẻ con trong bố cục Flex/Grid bắt buộc có `min-w-0` để ngăn tràn khung.
    * Trong WinUI 3: Sử dụng `TextTrimming="CharacterEllipsis"` cho mọi khối văn bản động.
* **Quy chuẩn bo góc (Border Radius):**
  * Hộp thoại (Modal / Dialog) & Khối giao diện (Card): `rounded-xl` ($12\text{px}$).
  * Nút bấm (Button) & Ô nhập liệu (Input field): `rounded-lg` ($8\text{px}$).
  * Huy hiệu trạng thái (Status Badge / Chip): `rounded-full` ($9999\text{px}$).
* **Quy chuẩn đổ bóng (Shadows):**
  * Tối giản: Ưu tiên dùng đường viền `1px solid` (`border-subtle`) thay cho hiệu ứng bóng đổ mờ ảo đậm nét để giữ giao diện sạch và hiện đại.
* **Quy tắc hiển thị nhãn kỹ thuật:**
  * **Tuyệt đối không hiển thị mã Use Case kỹ thuật `(UC...)` lên bất kỳ thành phần giao diện người dùng nào** (tiêu đề trang, menu, nút bấm, nhãn form, popup). Mã Use Case chỉ dùng làm quy chuẩn đối chiếu nội bộ trong tài liệu kỹ thuật và mã nguồn.

---

### 1.3. Chuẩn Vòng Tiêu Điểm Tiếp Cận (Accessible Focus Rings & High Contrast)

* **Web Portal (CSS):**
  * Tuyệt đối không sử dụng `outline: none` mà không có kiểu thay thế.
  * Sử dụng `:focus-visible` với vòng ring kép dày 2px màu Sky 400, khoảng đệm offset 2px tách biệt khỏi nền tối:
    ```css
    :focus-visible {
      outline: none;
      box-shadow: 0 0 0 2px #0A0F1D, 0 0 0 4px #38BDF8;
    }
    ```
* **WinUI 3 Desktop (XAML):**
  * Cấu hình đồng bộ trong từ điển tài nguyên toàn cục:
    ```xaml
    <SolidColorBrush x:Key="FocusVisualPrimaryBrush" Color="#38BDF8" />
    <SolidColorBrush x:Key="FocusVisualSecondaryBrush" Color="#0A0F1D" />
    <Thickness x:Key="FocusVisualPrimaryThickness">2</Thickness>
    ```
  * Đáp ứng tỷ lệ tương phản tối thiểu $3:1$ cho focus indicator và $4.5:1$ cho nội dung văn bản theo tiêu chuẩn WCAG 2.2 AA.

---

### 1.4. Quy Chuẩn 4 Trạng Thái Giao Diện Dữ Liệu (UI States: Loading, Empty, Error, Content)

Mọi danh sách hoặc thành phần tải dữ liệu bắt buộc hiện thực trọn vẹn 4 trạng thái:
1. **Trạng thái Đang tải (Loading State):**
   * Web: Sử dụng Skeleton Loader (`animate-pulse bg-slate-800/80 rounded-lg`) khớp kích thước khối dữ liệu thật, không để màn hình trống.
   * WinUI 3: Sử dụng `ProgressRing` hoặc `ProgressBar` có gắn thuộc tính trợ năng `AutomationProperties.Name="Đang tải dữ liệu"`.
2. **Trạng thái Trống (Empty State):**
   * Khi danh sách hội viên, lịch sử ca tập hoặc hóa đơn chưa có dữ liệu: Hiển thị khối thẻ rỗng (`bg-slate-800/40 border border-dashed border-slate-700 rounded-xl p-8 text-center`).
   * Chứa tiêu đề rõ nghĩa (ví dụ: *"Chưa có buổi đo InBody nào"*), một dòng hướng dẫn phụ và nút hành động gợi ý tiếp theo (ví dụ: `[+ Thực hiện đo InBody ngay]`).
3. **Trạng thái Lỗi (Error State):**
   * Hiển thị thông báo viền đỏ `status-danger`, giải thích rõ nguyên nhân bằng ngôn ngữ người dùng (không lộ exception kỹ thuật), kèm nút `"Thử lại"`.
4. **Trạng thái Nội dung (Content State):** Hiển thị dữ liệu hoàn chỉnh, chuyển tiếp mượt mà.

---

### 1.5. Tiêu Chuẩn Hiệu Năng XAML & Blazor Rendering

* **WinUI 3 Desktop:**
  * Bắt buộc sử dụng compiled bindings `{x:Bind Mode=OneWay}` có khai báo kiểu dữ liệu `x:DataType` tường minh để tối ưu thời gian biên dịch và loại bỏ cơ chế Reflection.
  * Danh sách $>50$ phần tử (Bảng hội viên, Bảng log check-in) bắt buộc sử dụng `ListView` hoặc `ItemsRepeater` có cơ chế cuộn ảo hóa (`ItemsStackPanel` có `CacheLength="2"`).
  * Giữ cây giao diện `DataTemplate` có cấu trúc nông (tối đa 2 cấp lồng nhau), tránh lồng ghép nhiều cấp `Grid`/`StackPanel` gây chậm trễ khi cuộn.
* **Blazor Server Web:**
  * Bắt buộc khai báo thuộc tính `@key` trên các phần tử con trong vòng lặp `@foreach`.
  * Đối với danh sách dài: Sử dụng component `<Virtualize Context="item">` để chỉ render các phần tử nằm trong khung nhìn (Viewport).

---

### 1.6. Quy Chuẩn Hỗ Trợ Giảm Chuyển Động (Reduced Motion)

* **Web Portal:** Tôn trọng thiết lập hệ điều hành thông qua media query:
  ```css
  @media (prefers-reduced-motion: reduce) {
    *, ::before, ::after {
      animation-duration: 0.01ms !important;
      animation-iteration-count: 1 !important;
      transition-duration: 0.01ms !important;
      scroll-behavior: auto !important;
    }
  }
  ```
* **WinUI 3 Desktop:** Kiểm tra `Windows.UI.ViewManagement.UISettings.AnimationsEnabled` trước khi kích hoạt các hiệu ứng chuyển trang hoặc hoạt họa phức tạp.
* **Thời lượng chuyển động:** Giữ thời lượng ngắn ($150\text{ms} - 200\text{ms}$), chỉ animate các thuộc tính nhẹ (`transform`, `opacity`). Cấm hoàn toàn hiệu ứng viền/chấm nhấp nháy.

---

## 2. QUY CHUẨN THÀNH PHẦN GIAO DIỆN HỘI VIÊN & PT (BLAZOR SERVER - MOBILE-FIRST)

Giao diện Web Portal (`GymManagement.Web`) được tối ưu hóa cho màn hình di động (Viewport chuẩn $375\text{px} - 430\text{px}$, canh giữa với `max-w-md mx-auto` khi xem trên máy tính).

### 2.1. Giao Diện Đăng Nhập Cổng Web (`Login.razor` - UC01)
* **Bố cục:** Thẻ Card trung tâm căn giữa `max-w-md mx-auto min-h-screen px-4 flex items-center justify-center`.
* **Thành phần UI:**
  * Logo phòng gym thể thao + Tiêu đề `"Đăng Nhập Cổng Hội Viên & PT"`.
  * Input Tên đăng nhập / Email: `type="text" autocomplete="username"` có viền `border-slate-700` bo góc `rounded-lg`.
  * Input Mật khẩu: `type="password" autocomplete="current-password"` kèm nút mở xem mật khẩu.
  * Nút CTA chính: `"Đăng Nhập"` (`bg-sky-500 hover:bg-sky-400 text-white font-semibold h-11 w-full rounded-lg`). Khi gửi request, nút tự khóa và hiển thị spinner quay.
  * Tự động điều hướng sau khi xác thực: PT chuyển tới Lịch dạy `/pt/schedule`, Member chuyển tới Cổng hội viên `/member/dashboard`.

---

### 2.2. Khối Thông Tin Gói Tập Số & Mã QR Điểm Danh 1-Chạm (`DigitalSubscriptionWidget.razor` - UC09, UC03)
* **Bố cục Khối Gói Tập Số (Digital Subscription Widget):**
  * Thiết kế dạng khối thẻ thông tin hiện đại trên nền tối thể thao sang trọng (`bg-slate-800`), viền kim loại mảnh `border border-slate-700`, bo góc `rounded-xl`.
  * **Góc trên bên trái:** Logo phòng gym + Tên gói tập hiện tại (`PackagePlan.Name`).
  * **Góc trên bên phải:** Chip trạng thái dạng nhãn chữ tĩnh (không nhấp nháy, không icon):
    * `ACTIVE`: Nhãn xanh lá `Đang hoạt động` (`text-emerald-400 bg-emerald-950/60 border border-emerald-600`).
    * `FROZEN`: Nhãn vàng hổ phách `Đang bảo lưu` (`text-amber-400 bg-amber-950/60 border border-amber-600`).
    * `EXPIRED`: Nhãn đỏ `Đã hết hạn` (`text-red-400 bg-red-950/60 border border-red-600`).
* **Hiển thị Mã QR Điểm Danh 1-Chạm:**
  * Mã QR điểm danh được tích hợp trực tiếp hoặc mở nhanh 1-chạm qua nút `"Mở mã QR điểm danh"` lớn và trực quan.
  * QR hiển thị ở kích thước tối thiểu $240\text{px} \times 240\text{px}$, co giãn theo viewport nhưng không vượt quá `min(80vw, 320px)`.
  * Đặt QR đen trên nền trắng đặc, có vùng trống (quiet zone) tối thiểu 4 module xung quanh; không phủ logo, chữ, họa tiết, bo góc hoặc hiệu ứng lên vùng mã.
  * Hiển thị họ tên và mã hội viên bên ngoài vùng quét để đối chiếu. Không đưa thông tin nhạy cảm vào nội dung chuỗi QR.
  * Bên dưới QR, hiển thị thời gian còn hiệu lực và thời điểm tự làm mới mỗi 60 giây có đồng hồ đếm ngược; có nút `"Làm mới mã QR"`.
  * Khi QR hết hạn hoặc gói tập không còn hợp lệ, mã cũ được ẩn đi và thay bằng thông báo giải thích rõ ràng.
* **Hàng Thông Số Buổi Tập (Session Counters):**
  * Chia 2 cột đều nhau trên khối thông tin gói tập:
    * Cột 1: `"Lượt Gym Còn Lại"` $\rightarrow$ Số lớn nổi bật (`tabular-nums text-2xl font-bold text-sky-400`).
    * Cột 2: `"Buổi PT Còn Lại"` $\rightarrow$ Số lớn nổi bật (`tabular-nums text-2xl font-bold text-purple-400`).
  * Chân khối hiển thị ngày hết hạn: `"Hết hạn: 31/12/2026"` (`text-xs text-slate-400`).

---

### 2.3. Danh Mục Gói Tập & Modal Thanh Toán VietQR (`PackageCatalog.razor`, `VietQrModal.razor` - UC05, UC02)
* **Lưới Danh Mục Gói Tập (Package Plan Cards):**
  * Trình bày dạng danh sách thẻ cuộn dọc (hoặc lưới 2 cột trên máy tính bảng).
  * Mỗi thẻ gói tập có: Tên gói, Giá tiền lớn kèm đơn vị VNĐ (`tabular-nums`), Thời hạn (ví dụ: `30&nbsp;ngày`, `90&nbsp;ngày`), Số buổi tập gym, Số buổi PT đi kèm.
  * **Chính sách giá niêm yết cố định:** Giá hiển thị và thanh toán bằng đúng giá niêm yết trong bảng `PackagePlan.Price` (không có ô nhập mã ưu đãi / giảm giá).
  * Nút CTA: `"Đăng Ký Gói Này"` màu xanh `bg-sky-500 hover:bg-sky-400 text-white font-semibold h-11 w-full rounded-lg`.
* **Modal Thanh Toán VietQR Tự Động (VietQR Pop-up Modal):**
  * Nền slate đậm, bo góc `rounded-xl`, phủ backdrop làm mờ xung quanh.
  * **Mã QR VietQR chuẩn NAPAS 247:** Kích thước cố định $220\text{px} \times 220\text{px}$, hiển thị logo ngân hàng và logo Napas ở giữa.
  * **Hộp Thông Tin Chuyển Khoản Minh Bạch:**
    * Số tiền: Hiển thị to rõ màu xanh lá (ví dụ: `800.000&nbsp;VNĐ` font `tabular-nums`).
    * Nội dung chuyển khoản: Ô văn bản font đơn cách (`font-mono tracking-wider bg-slate-700 p-2 rounded`) kèm nút 1-chạm `"Sao chép"`.
  * **Thanh Đếm Ngược:** Hiển thị thời gian hiệu lực của mã QR `09:59` đếm lùi, khi hết giờ nút chuyển sang `"Tạo lại mã thanh toán"`.
  * **Nút Giả Lập Thanh Toán (Sandbox Simulation):**
    * Dưới mã QR có nút `"Giả lập thanh toán thành công"` phục vụ kiểm thử luồng tích hợp.
    * **Ràng buộc biến môi trường:** Nút này chỉ được hiển thị khi biến môi trường `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`.
    * Trong môi trường Production (`VIETQR_ENABLE_SIMULATION=false`), nút này tự động ẩn hoàn toàn khỏi giao diện người dùng.
    * Khi nhấn, nút tự động khóa (disable) ngay lập tức để tránh gửi lệnh lặp, hiển thị trạng thái `"Đang xử lý mô phỏng…"` và chỉ kích hoạt gói sau khi máy chủ xác nhận kết quả.

---

### 2.4. Đặt Lịch Huấn Luyện Viên PT 3 Bước & Modal Hủy Ca (`PtBookingWizard.razor`, `CancelBookingModal.razor` - UC10)
* **Quy trình Đặt Lịch 3 Bước Trực Quan (`PtBookingWizard.razor` - UC 10.1):**
  * Header: Thông tin PT phụ trách cố định (`AssignedPtId`) và số buổi PT khả dụng còn lại.
  * **Bước 1 - Bộ Chọn Ngày Dạng Băng Chuyền Ngang (Horizontal Date Picker):** Cho phép bấm chọn nhanh từng ngày trong tuần (Thứ 2 đến Chủ nhật). Ngày được chọn sáng nổi bật với nền xanh `bg-sky-500 text-white`.
  * **Bước 2 - Lưới Khung Giờ (Time Slot Grid):**
    * Khung giờ từ 06:00 đến 21:00 trình bày dạng nút bấm phẳng 2 cột hoặc 3 cột:
    * Khung giờ rảnh PT: Nền Slate 800 viền Slate 600 (`slot-pt-open`: `bg-slate-800 text-slate-200 border border-slate-600 hover:bg-slate-700`). **Tuyệt đối không dùng màu xanh lá thành công cho slot rảnh**.
    * Khung giờ đã kín lịch: Nền Slate 900 tối (`bg-slate-900/60 text-slate-600 border border-slate-800 cursor-not-allowed line-through`).
  * **Bước 3 - Xác nhận Đặt Lịch:** Hộp thoại tóm tắt ngày giờ, tên PT; nút `"Xác Nhận Đặt Lịch"` chuyển ca sang trạng thái Đã đặt (màu tím), tạm giữ 1 buổi tập.
* **Hộp Thoại Cảnh Báo Hủy Ca Muộn < 2 Giờ (`CancelBookingModal.razor` - UC 10.2):**
  * Hộp thoại bật lên cảnh báo viền đỏ/hổ phách:
    * Tiêu đề: `"Xác Nhận Hủy Buổi Tập"`.
    * **Nếu $\ge 2$ giờ trước ca:** Thông báo: *"Bạn có chắc chắn muốn hủy ca tập này? 1 buổi tập sẽ được hoàn lại vào gói của bạn."* $\rightarrow$ Nút `"Xác Nhận Hủy"`.
    * **Nếu $< 2$ giờ trước ca:** Thông báo cảnh báo viền đỏ: *"Thời gian hủy chỉ còn [X phút]. Theo quy định phòng tập, hủy ca trước dưới 2 giờ sẽ KHÔNG ĐƯỢC HOÀN LƯỢT TẬP. Bạn có chắc chắn muốn hủy ca này không?"* $\rightarrow$ 2 Nút: `"Giữ Lại Ca Tập"` (Nút chính, nền Slate 700) và `"Xác Nhận Hủy (Mất buổi)"` (Nút phụ viền đỏ).
* **Đánh Giá Ca Tập 1-5 Sao (`SessionRatingModal.razor` - UC 10.3):**
  * Component chấm điểm sao (Star Rating) và ô TextArea nhập cảm nhận sau khi ca tập chuyển `COMPLETED`.
  * Nút `"Gửi Đánh Giá"` khóa không cho sửa sau khi gửi.
* **Đổi Lịch Sang Ca Trống Khác (`RescheduleBookingModal.razor` - UC 10.4):**
  * So sánh ca cũ vs. lưới ca trống mới của cùng PT; bấm xác nhận đổi lịch trong 1 Transaction duy nhất.

---

### 2.5. Cổng Tác Nghiệp Dành Cho Huấn Luyện Viên PT (Blazor Web - UC08)
* **Đăng Ký Khung Giờ Rảnh (`PtAvailabilityGrid.razor` - UC 8.1.1):**
  * Lưới các ô giờ trong tuần từ 06:00 đến 21:00. Chạm để bật/tắt slot rảnh.
  * Slot rảnh hiển thị màu trung tính (`slot-pt-open`).
  * Khóa không cho phép hủy slot nếu slot đó đã có học viên đặt lịch (kèm thông báo giải thích).
* **Thời Khóa Biểu Tuần Của PT (`PtWeeklySchedule.razor` - UC 8.1.2 & UC 8.2.3):**
  * Dòng thời gian hiển thị danh sách các ca dạy trong ngày/tuần: Khung giờ, Tên học viên, SĐT (link gọi nhanh `tel:`), Trạng thái ca (`Sắp diễn ra`, `COMPLETED`, `Đã hủy`).
  * Nút lớn trên ca đang diễn ra: `"Xác Nhận Hoàn Thành Ca Tập"` (chỉ bấm được khi ca đã/đang diễn ra, tự động trừ 1 buổi của học viên).
* **Danh Sách Học Viên Phụ Trách (`PtStudentsList.razor` - UC 8.2.1):**
  * Danh sách dạng thẻ (Cards) trên mobile: Họ tên học viên, SĐT, Gói tập đang theo học, Số buổi PT còn lại (`RemainingPtSessions`), Ngày hết hạn.
  * Nút xem `"Lịch sử ca tập"` và nút `"Ghi chú InBody"`.
* **Biểu Mẫu Nhập InBody Của PT (`InBodyEntryModal.razor` - UC 8.2.2):**
  * Ô nhập số liệu: Cân nặng (`WeightKg` - kg), Chiều cao (`HeightCm` - cm), Tỷ lệ mỡ (`BodyFatPercentage` - %), Khối lượng cơ xương (`SkeletalMuscleMass` - kg), Ghi chú bài tập.
  * Tự động tính chỉ số BMI hiển thị tức thời: $\text{Bmi} = \text{WeightKg} / ((\text{HeightCm} / 100)^2)$.

---

### 2.6. Bảng Tiến Trình Sức Khỏe InBody (`InBodyDashboard.razor` - UC06, UC09)
* **Hàng 4 Thẻ Chỉ Số Sức Khỏe (Metric Cards Grid - Khớp Bảng MemberBodyMetric):**
  * Cân nặng (`WeightKg` - kg), Chiều cao (`HeightCm` - cm), Tỷ lệ mỡ (`BodyFatPercentage` - %), Khối lượng cơ xương (`SkeletalMuscleMass` - kg).
* **Thanh Trực Quan Hóa Phân Loại BMI (BMI Visual Gauge):**
  * Thanh dải màu ngang chia 4 phân vùng: Xanh dương (< 18.5: Gầy), Xanh lá (18.5 - 24.9: Chuẩn), Vàng (25 - 29.9: Thừa cân), Đỏ ($\ge 30$: Béo phì).
  * Con trỏ kim vị trí chỉ chính xác số đo `Bmi` của hội viên kèm nhãn trạng thái lớn.
* **Biểu Đồ Tiến Trình Theo Lịch Sử Đo (Line Chart):**
  * Biểu đồ đường mượt mà hiển thị biến thiên cân nặng (`WeightKg`) và tỷ lệ mỡ (`BodyFatPercentage`) qua các lần đo lịch sử (`measured_at`).

---

### 2.7. Dòng Thời Gian Lịch Sử Tập Luyện & Check-in (`WorkoutHistoryTimeline.razor` - UC 9.2)
* **Bố cục:** Dòng thời gian kết hợp trên điện thoại di động.
* **Thành phần UI:**
  * Bộ lọc khoảng thời gian: `Tuần này`, `Tháng này`, `Tùy chọn`.
  * Thẻ lượt Check-in: Ngày, Giờ quét turnstile vào phòng, Trạng thái vào phòng.
  * Thẻ ca tập PT: Ngày, Khung giờ, Tên PT phụ trách, Trạng thái tham gia và ghi chú bài tập từ PT.

---

## 3. QUY CHUẨN THÀNH PHẦN GIAO DIỆN QUẢN TRỊ (WINUI 3 DESKTOP - DÀNH CHO ADMIN)

Giao diện ứng dụng WinUI 3 Desktop (`GymManagement.Desktop`) được thiết kế theo tư duy **Desktop-First MVVM**, phục vụ màn hình máy tính làm việc của Admin với mật độ thông tin cao, thao tác bàn phím nhạy và giao diện Fluent Design phẳng độc tôn Dark Mode.

### 3.1. Khung Giao Diện Chính & Điều Hướng (`MainWindow.xaml`)
* **Cột Điều Hướng Trái (NavigationView):**
  * Độ rộng mở rộng `w-64` ($256\text{px}$), hỗ trợ thu gọn về dạng icon (`Compact Mode` $48\text{px}$).
  * Bố cục 7 mục chức năng của Admin chuẩn PRD (tuyệt đối không hiển thị mã UC):
    1. `Quầy Điểm Danh & Cổng Tự Động`
    2. `Bán Gói & Thanh Toán POS`
    3. `Quản Lý Hội Viên & Gói Tập`
    4. `Lịch Biểu Huấn Luyện Viên PT`
    5. `Báo Cáo & Thống Kê Doanh Thu`
    6. `Quản Lý Nhân Sự & Danh Mục Gói`
    7. `Cấu Hình & Sổ Kiểm Toán Hệ Thống`
* **Thanh Tiêu Đề Trên Cùng (App TitleBar):**
  * Tích hợp thanh tìm kiếm hội viên toàn cục nhanh, thông tin Admin đang đăng nhập kèm Badge vai trò (`Admin`/`Super Admin`). **Độc tôn Dark Mode, không có nút chuyển đổi Light/Dark mode**.

---

### 3.2. Màn Hình Đăng Nhập Quản Trị (`LoginPage.xaml` - UC01)
* **Bố cục:** Khối Card trung tâm $420\text{px}$ trên nền `bg-canvas` (`#0A0F1D`), viền `border-subtle`, bo góc `rounded-xl`.
* **Thành phần UI:**
  * Logo phòng gym thể thao + Tiêu đề lớn `"Đăng Nhập Quản Trị"`.
  * Ô Tên đăng nhập: `TextBox` hỗ trợ dán (`never block paste`), `PlaceholderText="Nhập tên đăng nhập hoặc email…"`.
  * Ô Mật khẩu: `PasswordBox` tích hợp nút con mắt mở xem mật khẩu (`PasswordRevealMode="Peek"`).
  * `CheckBox`: `"Ghi nhớ tên đăng nhập"`.
  * Nút CTA chính: `"Đăng Nhập"` (`accent-primary` Sky 500, phím `Enter` kích hoạt, tích hợp `ProgressRing` quay khi đang xác thực).
  * Khối thông báo lỗi `InfoBar`: Hiển thị khi sai mật khẩu (*"Tên đăng nhập hoặc mật khẩu không chính xác"* — tự xóa mật khẩu, giữ tên đăng nhập) hoặc tài khoản bị khóa (*"Tài khoản đã bị khóa. Vui lòng liên hệ Super Admin."*).

---

### 3.3. Màn Hình Quầy Điểm Danh Cửa Ra Vào (`CheckInCounterPage.xaml` - UC03)
* **Bố Cục 2 Cột Đối Xứng:**
  * **Cột Trái ($40\%$ Chiều rộng) - Khu vực Webcam Quét Mã QR & Tra Cứu Thủ Công:**
    * **Webcam Quét Mã QR (UC 3.1):** Hội viên đưa mã QR từ mobile app trước webcam tại quầy để nhận diện tự động theo thời gian thực. Hệ thống không sử dụng đầu đọc thẻ USB hay máy quét barcode cầm tay, và tuyệt đối không có ô text để nhập chuỗi mã QR. Không sử dụng phím tắt `F1`.
    * **Tra Cứu & Điểm Danh Thủ Công (UC 3.2):** Ô nhập liệu duy nhất hỗ trợ tìm kiếm bằng **Mã hội viên (`UserName`)** hoặc **Số điện thoại (`User.Phone`)** kèm phím `Enter` để thực hiện điểm danh (dự phòng khi quên điện thoại hoặc hết pin).
    * **Nút thao tác nhanh:** `"Cho vào thủ công (Override)"` (bắt buộc chọn lý do: quên điện thoại, kẹt cổng… để lưu `VerifiedByAdminId`) và `"Mở cổng kiểm tra"`.
  * **Cột Phải ($60\%$ Chiều rộng) - Bảng Hiển Thị Kết Quả Điểm Danh & Giám Sát Ra/Vào:**
    * **Kết quả điểm danh tức thì ($<1$ giây):**
      * Khi chưa quét: Hiển thị biểu tượng camera chờ quét với thông báo *"Sẵn sàng tiếp nhận hội viên…"*.
      * Toàn bộ việc kiểm tra trạng thái gói tập (`ACTIVE`, còn hạn, còn lượt) được thực hiện bên **Backend**; Desktop hiển thị kết quả từ API phản hồi.
      * **Hoàn toàn không sử dụng âm thanh phản hồi** (không có tiếng "Beep" hay "Buzz"). Mọi phản hồi đều thể hiện qua thông báo văn bản trực quan.
      * **Hoàn toàn không yêu cầu ảnh chân dung** (không hiển thị khung ảnh bo tròn).
      * **Nội dung kết quả điểm danh tập trung tinh gọn đúng 2 thông tin cốt lõi:**
        1. **Mã hội viên (`UserName`)**
        2. **Họ tên hội viên (`FullName`)**
      * **Khi Check-in Hợp Lệ (Backend trả về Success):**
        * Chuyển sang banner xanh tĩnh: `"CHECK-IN THÀNH CÔNG • CỔNG MỞ"` (`bg-emerald-600 text-white text-xl font-bold p-3 rounded-lg`). **Không sử dụng hiệu ứng nhấp nháy viền**.
        * Thông tin chi tiết: Mã hội viên (`UserName`) và Họ tên (`FullName`) in hoa to rõ (`text-2xl font-bold`).
        * Dòng thông tin gói tập & số lượt còn lại: `"Còn 18 lượt tập gym"` / `"Còn 5 buổi tập PT"`.
        * Đồng hồ 7 giây đếm lùi để tự động trở về trạng thái chờ.
      * **Khi Check-in Thất Bại (Backend trả về Lỗi):**
        * Chuyển sang banner đỏ tĩnh: `"TỪ CHỐI CHECK-IN • CỔNG KHÓA"` (`bg-red-600 text-white text-xl font-bold p-3 rounded-lg`). **Không sử dụng hiệu ứng nhấp nháy viền**.
        * Thông tin chi tiết: Mã hội viên (`UserName`) và Họ tên (`FullName`).
        * Dòng lý do do Backend trả về in chữ đậm rõ nét: *"Gói tập trong tài khoản đã hết hạn"* hoặc *"Gói tập đã hết lượt vào phòng"*.
    * **Bảng Giám sát Ra/Vào Thời gian Thực (UC 3.3):**
      * Nửa dưới cột phải: DataGrid cuộn ảo hiển thị các lượt check-in hôm nay (lượt mới nhất xếp trên cùng).
      * Cột dữ liệu: Giờ check-in (`tabular-nums`), Mã hội viên, Họ tên, Gói tập, Phương thức (Quét QR / Thủ công), Trạng thái (Thành công / Từ chối).
      * Bộ lọc theo ca: ComboBox (`Tất cả`, `Ca Sáng 06:00-12:00`, `Ca Chiều 12:00-17:00`, `Ca Tối 17:00-22:00`).
      * Badge số liệu: `"Hiện diện trong phòng: [X] người"`.

---

### 3.4. Màn Hình Quản Lý Hội Viên & Hộp Thoại Tạo Mới (`MembersPage.xaml` - UC02)
* **Bố Cục Trang Quản Lý Hội Viên:**
  * **Header trang:**
    * Cột trái: Tiêu đề `"QUẢN LÝ HỘI VIÊN & GÓI TẬP"` + Mô tả phụ.
    * Cột phải: **Nút CTA chính `"+ Đăng Ký Hội Viên Mới"`** (`accent-primary` Sky 500, phím tắt `Ctrl+N` / `F4`).
  * **Thanh Tìm Kiếm & Bộ Lọc:**
    * `AutoSuggestBox` tìm kiếm nhanh theo Họ tên, SĐT, Mã hội viên (`F3`).
    * Nhóm nút lọc trạng thái chữ thuần túy: `Tất cả`, `Đang hoạt động`, `Đang bảo lưu`, `Đã hết hạn`.
  * **DataGrid Danh Sách Hội Viên Cuộn Ảo (Virtualization):**
    * Bảng dữ liệu phẳng hỗ trợ cuộn ảo (`ItemsStackPanel`) cho hàng ngàn hội viên.
    * Cột dữ liệu: Mã hội viên, Họ tên + SĐT, Gói tập hiện tại, Hạn sử dụng, Số buổi còn lại (Gym/PT), Trạng thái (Chip chữ thuần túy).
    * Cột thao tác nhanh trên từng dòng: `Xem/Sửa`, `Gia hạn`, `Bảo lưu`, `Đo InBody`.
* **Hộp Thoại Wizard Đăng Ký Hội Viên Mới (`RegisterMemberWizard.xaml` - UC 02.1):**
  * Hộp thoại 2 bước (`ContentDialog` kích thước $760\text{px} \times 620\text{px}$):
    * **Bước 1 (Thông tin cá nhân):** Họ tên, SĐT (`inputmode="numeric"`), Email, CCCD, Ngày sinh, Giới tính, Địa chỉ. Báo lỗi trùng SĐT/CCCD tức thì.
    * **Bước 2 (Gói tập & Thu tiền):** Lưới thẻ gói tập `PackagePlan` hiển thị Đơn giá niêm yết to rõ (`tabular-nums`), Thời hạn `DurationDays`, Lượt gym, Buổi PT; Bộ chọn `StartDatePicker`; Bảng tính tổng thanh toán bằng đúng giá niêm yết; Bộ 3 nút chọn thanh toán (`Tiền mặt`, `VietQR`, `Thẻ POS`); Nút `"Hoàn tất & Kích hoạt Hội viên"`.
* **Hộp Thoại Cập Nhật Hồ Sơ (`MemberDetailDialog.xaml` - UC 02.2):** Header hiển thị Mã hội viên + Chip trạng thái; Form cho phép sửa SĐT, Email, Địa chỉ; Khóa Mã, CCCD, Ngày tạo.
* **Hộp Thoại Gia Hạn / Nâng Cấp (`RenewUpgradeModal.xaml` - UC 02.3):** Bảng tính tự động tiền chênh lệch; Nút `"Xác nhận Thanh toán"`.
* **Hộp Thoại Bảo Lưu Gói Tập (`FreezeSubscriptionDialog.xaml` - UC 02.4):** DateRangePicker chọn ngày bắt đầu & kết thúc bảo lưu; Hộp tính hiển thị ngày hết hạn mới dự kiến $\text{EndDate mới} = \text{EndDate cũ} + \text{Số ngày bảo lưu}$; TextBox bắt buộc nhập lý do; Nút xác nhận chuyển trạng thái sang chip hổ phách `FROZEN`.
* **Hộp Thoại Hủy Đăng Ký Gói Tập (`CancelSubscriptionDialog.xaml` - UC 02.5):** Modal cảnh báo viền đỏ `status-danger`; TextBox bắt buộc nhập lý do; 2 nút: `"Giữ Lại Gói"` và `"Xác Nhận Hủy Gói"`.

---

### 3.5. Màn Hình POS Bán Gói & Xuất Hóa Đơn (`PosBillingPage.xaml` - UC05)
* **Bố Cục 2 Khối Thao Tác (Khớp 100% Thuộc Tính Bảng Invoice & PackagePlan):**
  * **Khối Danh Mục & Sản Phẩm (Bên Trái):**
    * Ô tìm kiếm gói tập nhanh theo tên hoặc mã (`PackagePlan.PlanId`, `PackagePlan.Name`).
    * Lưới thẻ gói tập (Card Grid) hiển thị đơn giá niêm yết (`PackagePlan.Price`), thời hạn (`DurationDays`), số buổi gym (`MaxCheckInSessions`), số buổi PT (`TotalPtSessions`). Click 1 chạm để nạp vào hóa đơn.
  * **Khối Chi Tiết Hóa Đơn & Thu Tiền (Bên Phải):**
    * Ô tìm kiếm hội viên gợi ý nhanh liên kết `User.UserName` kèm **nút shortcut `[+] Khách mới`** để tạo nhanh hội viên mới khi khách chưa có hồ sơ.
    * Bảng chi tiết hóa đơn: Tên gói tập, Thời hạn, Đơn giá gói, Tạm tính `SubtotalAmount`.
    * **Tổng thanh toán (`Invoice.TotalAmount`):** Đồng bộ chính xác với tiền niêm yết của gói (`TotalAmount = CAST(SubtotalAmount AS INT)`). **Tuyệt đối không có ô nhập mã giảm giá, voucher hay dòng trừ chiết khấu** vì bảng `Invoice` trong CSDL không có cột lưu trữ giảm giá.
    * Mã hóa đơn: `Invoice.InvoiceCode` sinh tự động duy nhất (ví dụ: `INV-20261108-001`).
    * Loại giao dịch: `Invoice.TransactionType` (`SUBSCRIPTION_PURCHASE` hoặc `SUBSCRIPTION_RENEWAL`).
* **Bộ Nút Chọn Phương Thức Thanh Toán Lớn:**
  * 3 Nút bấm kích thước lớn: `[Tiền Mặt]`, `[Chuyển Khoản VietQR]` (Khuyên dùng), `[Thanh Toán Thẻ Ngân Hàng (POS Terminal)]`.
* **Cửa Sổ Bật Lên Thanh Toán VietQR (Dialog):**
  * Mở hộp thoại `ContentDialog` chứa mã VietQR động lớn ở trung tâm.
  * Khi chạy môi trường phát triển/kiểm thử, hiển thị nút `"Giả lập thanh toán thành công"` dưới mã QR khi biến môi trường `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`. Khóa nút ngay sau khi bấm.
  * Trong môi trường Production (`VIETQR_ENABLE_SIMULATION=false`), nút này tự động ẩn hoàn toàn.
  * Thu ngân được bấm nút xác nhận thu tiền thủ công ngay khi có bằng chứng chuyển khoản thành công, đi kèm hộp thoại xác nhận nhanh 2 bước để phòng ngừa bấm nhầm.
* **Tra Cứu Lịch Sử Hóa Đơn & In Lại (`InvoiceHistoryPage.xaml` - UC 05.2):**
  * Bảng tra cứu lịch sử hóa đơn kèm nút `"In hóa đơn"` (in nhiệt K80) và `"Xuất PDF"`.
* **Hộp Thoại Hoàn Tiền Hóa Đơn (`RefundDialog.xaml` - UC 05.3):**
  * Đối soát số tiền gốc, nhập số tiền hoàn $\le$ gốc, chọn hình thức hoàn, bắt buộc nhập lý do hoàn tiền; chuyển trạng thái `REFUNDED` và ghi dòng tiền âm.

---

### 3.6. Bảng Lịch Huấn Luyện Viên & Quản Lý Ca Dạy (`PtSchedulePage.xaml` - UC04)
* **Lưới Thời Gian Tuần (Weekly Timeline Dispatcher - UC 04.6):**
  * Trục tung: Danh sách giờ từ 06:00 đến 22:00.
  * Trục hoành: 7 Cột biểu diễn các ngày trong tuần (hoặc từng cột đại diện cho 1 PT khi ở chế độ xem theo ngày).
  * Ca tập được biểu diễn bằng các khối chữ nhật màu tím `#A78BFA` có tên hội viên, số điện thoại và thời gian bắt đầu - kết thúc.
* **Cảnh Báo Xung Đột Trùng Lịch (Collision Guardrail Border):**
  * Nếu người dùng kéo thả hoặc xếp lịch mới đè lên khung giờ đã có ca dạy của huấn luyện viên:
    * Khối lịch lập tức chuyển sang viền đỏ rực `#F87171` kèm dòng thông báo cảnh báo rõ ràng.
    * Tooltip nổi lên giải thích chi tiết: *"Xung đột: Huấn luyện viên đã có lịch dạy ca [Tên ca khác] trong khung giờ này!"*.
    * Nút Lưu lịch bị vô hiệu hóa cho đến khi chuyển sang khung giờ hợp lệ.
* **Hộp Thoại Thêm Mới Hồ Sơ PT (`TrainerCreateDialog.xaml` - UC 04.1):** Nhập chuyên môn, mức lương, kinh nghiệm, kiểm tra SĐT/Email duy nhất.
* **Hộp Thoại Phân Công PT Cho Học Viên (`PtAssignDialog.xaml` - UC 04.4):** Gán quan hệ 1-1 giữa học viên có gói PT và danh sách PT khả dụng kèm thông số định mức học viên hiện tại.
* **Hộp Thoại Rà Soát Ca Ngoại Lệ (`SessionReviewDialog.xaml` - UC 04.5):** Danh sách ca quá giờ chưa xác nhận; 4 nút hành động: `[Hoàn thành]`, `[Học viên vắng]`, `[PT vắng]`, `[Hoãn ca]`.

---

### 3.7. Màn Hình Quản Lý Nhân Sự RBAC & Danh Mục Gói Tập (`StaffManagementPage.xaml`, `PackageCatalogPage.xaml` - UC07)
* **Quản Lý Nhân Sự & Phân Quyền (`StaffManagementPage.xaml` - UC 7.1):**
  * Nút `"Thêm Tài Khoản Nhân Sự"`: Nhập Họ tên, Email, SĐT, Tên đăng nhập, Mật khẩu ban đầu và chọn vai trò (`Admin`, `PT`, `Member`).
  * Bảng danh sách nhân viên kèm Switch Toggle chuyển đổi `ACTIVE` / `LOCKED`.
  * **Hàng rào an toàn chống tự phá vỡ quyền quản trị:**
    * Cấm Super Admin tự thay đổi vai trò quản trị của chính tài khoản mình (khóa dropdown vai trò kèm cảnh báo).
    * Cấm Quản trị viên tự khóa tài khoản của chính mình (vô hiệu hóa switch toggle trên dòng của tài khoản đang đăng nhập).
* **Quản Lý Danh Mục Gói Tập (`PackageCatalogPage.xaml` - UC 7.2):**
  * Form thêm mới gói tập: Tên gói, Đơn giá (>0), Thời hạn `DurationDays`, Lượt gym, Buổi PT, Số ngày bảo lưu tối đa, Mô tả quyền lợi.
  * Form chỉnh sửa gói: Cập nhật giá/quyền lợi áp dụng cho đợt đăng ký mới, bảo lưu quyền lợi gói cũ.
  * Xóa gói: Nếu đã phát sinh hóa đơn $\rightarrow$ Chặn xóa vật lý, chuyển cờ trạng thái sang `DISCONTINUED` (Ngừng kinh doanh).

---

### 3.8. Bảng Điều Khiển Thống Kê Doanh Thu & BI Analytics (`DashboardPage.xaml` - UC06)
* **4 Thẻ Chỉ Số KPI Phẳng Đầu Trang:**
  * Nền slate đậm, viền mảnh `border-subtle`, số liệu in đậm cỡ lớn (`tabular-nums text-3xl font-bold`).
  * Chỉ số hiển thị: `Doanh Thu Hôm Nay (VNĐ)`, `Lượt Check-in Hôm Nay`, `Gói Tập Mới Trong Tháng`, `Số Giờ PT Đã Dạy`.
* **Biểu Đồ Doanh Thu & Giờ Cao Điểm:**
  * Biểu đồ cột biểu diễn phân bố lượt khách vào phòng tập theo từng khung giờ trong ngày (nhận diện khung giờ vàng 17h - 20h).
  * Biểu đồ phân bố tỷ lệ doanh thu theo loại gói tập (Gói Tháng, Gói Năm, Gói PT).
* **Bộ Nút Xuất Báo Cáo (UC 06.2):** Nút `"Xuất Excel (.xlsx)"` và `"Xuất PDF"` tải về trực tiếp trong $<3$s qua luồng bộ nhớ đệm.

---

## 4. MA TRẬN ĐỐI CHIẾU THÀNH PHẦN UI: WINUI 3 DESKTOP VS. BLAZOR WEB

| Phân Hệ Nghiệp Vụ | Ứng Dụng WinUI 3 Desktop (Quản Trị Admin) | Ứng Dụng Blazor Server Web (Hội Viên / PT) |
| :--- | :--- | :--- |
| **UC01: Đăng Nhập & Xác Thực** | `LoginPage.xaml`: Card trung tâm $420\text{px}$, PasswordBox nút con mắt, CheckBox ghi nhớ, InfoBar đỏ khi sai pass hoặc khóa tài khoản. | `Login.razor`: Mobile card `max-w-md`, form tối giản, tự động điều hướng theo Role (PT vào Lịch, Member vào Cổng hội viên). |
| **UC02: Hội Viên & Vòng Đời Gói** | `MembersPage.xaml`: Header có nút CTA `"+ Đăng Ký Hội Viên Mới"`, Wizard 2 bước `RegisterMemberWizard.xaml`, Hộp thoại Gia hạn, Bảo lưu `FROZEN`, Hủy gói `CANCELLED`. | `DigitalSubscriptionWidget.razor`: Khối thẻ gói tập số, chip trạng thái tĩnh, đồng hồ đếm ngược số ngày và buổi tập còn lại. |
| **UC03: Check-in Điểm Danh** | `CheckInCounterPage.xaml`: Màn hình 2 cột, webcam quét $<1$s, tra cứu SĐT + Enter, nút Override lý do, bảng log ra/vào theo ca, banner tĩnh (không âm thanh, không ảnh). | Khối mã QR điểm danh 1-chạm kích thước $240 \times 240\text{px}$, nền trắng đặc, quiet zone 4 modules, tự làm mới mỗi 60 giây. |
| **UC04 & UC08: Dịch Vụ Huấn Luyện Viên PT** | `PtSchedulePage.xaml`: Bảng lịch tuần đa năng nhiều PT, kéo thả khung giờ, hàng rào viền đỏ chặn đứng xung đột trùng lịch, phân công 1-1, rà soát ca ngoại lệ. | `PtAvailabilityGrid.razor` & `PtWeeklySchedule.razor`: Chọn slot rảnh màu trung tính, xem timeline ca dạy trên điện thoại, nút xác nhận hoàn thành ca 1-chạm. |
| **UC05: Thanh Toán & Hóa Đơn** | `PosBillingPage.xaml`: POS bán hàng, bảng giá niêm yết không discount, shortcut `[+] Khách mới`, VietQR sandbox toggle, in nhiệt K80 và Modal Hoàn tiền. | `PackageCatalog.razor` & `VietQrModal.razor`: Danh mục gói dạng thẻ cuộn dọc, modal VietQR NAPAS 247 đếm ngược 10 phút, tự kích hoạt gói qua SignalR. |
| **UC06: Báo Cáo & Thống Kê** | `DashboardPage.xaml`: Dashboard KPI doanh thu thời gian thực, biểu đồ cột giờ cao điểm 17h–20h, bảng xếp hạng PT, xuất Excel/PDF $<3$s. | `WorkoutHistoryTimeline.razor`: Dòng thời gian lịch sử các lượt quét QR vào phòng và các ca tập hoàn thành cùng PT. |
| **UC07: Phân Quyền & Gói Tập** | `StaffManagementPage.xaml`: Quản lý nhân sự RBAC, chống tự khóa/tự tước quyền, thêm/sửa/ngừng kinh doanh gói `PackagePlan` (`DISCONTINUED`). | Không áp dụng (chỉ dành riêng cho Super Admin trên Desktop). |
| **UC10: Đặt Lịch PT & Đánh Giá** | Không áp dụng trực tiếp (Admin chỉ giám sát và giải quyết tranh chấp qua `SessionReviewDialog.xaml`). | `PtBookingWizard.razor`: Đặt ca 3 bước trực quan, cảnh báo phạt hủy ca $<2$h (mất buổi tập), component đánh giá 1–5 sao sau buổi tập. |

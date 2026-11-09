# USER EXPERIENCE & INTERACTION FLOW RULES (UX SYSTEM RULES)
## Hệ Thống Quản Lý Phòng Tập Thể Thao (Gym Management System)
### WinUI 3 Desktop (Admin) & Blazor Server (Member/PT Portal)

> **Mục đích tài liệu:** Tài liệu này quy định toàn bộ tiêu chuẩn **Trải nghiệm Người dùng & Luồng Tương tác Nghiệp vụ (UX - User Experience)** cho toàn bộ lập trình viên và AI Agent khi triển khai giao diện, quản lý trạng thái, xử lý sự kiện và kiểm soát ngoại lệ trên 2 ứng dụng client: **WinUI 3 Desktop App** (dành riêng cho Admin) và **Blazor Server Web App** (dành cho Hội viên và Huấn luyện viên cá nhân PT).
> **Triết lý UX Cốt lõi (Core UX Philosophy):** 
> * **Đối với Hội viên & PT (Web Portal):** Thao tác tức thì (Zero-Friction), thông tin gói tập điện tử trực quan, hiển thị mã QR nhanh chóng, minh bạch lượt tập và lịch hẹn, cảnh báo hủy ca trước 2 giờ rõ ràng.
> * **Đối với Ban quản trị Admin (Desktop App):** Tốc độ quầy cực nhanh (< 1 giây điểm danh), hiển thị kết quả tinh gọn (Mã hội viên + Họ tên), an toàn giao dịch tài chính (VietQR tự động khớp, chống trùng hóa đơn), kiểm soát chống gian lận và trực quan hóa xung đột lịch dạy PT.
> * **Quy tắc toàn cục về nhãn giao diện:** **Tuyệt đối không hiển thị mã Use Case kỹ thuật `(UC...)` lên bất kỳ thành phần giao diện người dùng nào** (tiêu đề trang, menu điều hướng, nút bấm, nhãn form, popup). Mã Use Case chỉ dùng làm quy chuẩn đối chiếu nội bộ trong tài liệu kỹ thuật và mã nguồn.

---

## 1. NỀN TẢNG THIẾT KẾ & QUY CHUẨN TRẢI NGHIỆM CƠ BẢN (100% DARK MODE)

Mọi màn hình và thành phần giao diện khi hiện thực đều phải tuân thủ nghiêm ngặt các ràng buộc thiết kế sau:

### 1.1. Bảng Mã Màu Ngữ Nghĩa Hệ Thống (Semantic Status Colors)

Hệ thống độc tôn giao diện tối (Dark Mode 100%) trên cả Desktop và Web, tối ưu cho môi trường ánh sáng phòng tập thể thao với tỷ lệ tương phản cao ($\ge 4.5:1$ theo chuẩn WCAG AA):

| Vai Trò Token | Mã Màu Hex / Tailwind | Ràng Buộc Ứng Dụng Nghiệp Vụ Bắt Buộc |
| :--- | :--- | :--- |
| **Nền Chính (Primary Background)** | `#0F172A` (Slate 900) | Giao diện Desktop và Web độc tôn sử dụng Dark Mode. |
| **Bề Mặt Khối (Surface)** | `#1E293B` (Slate 800) | Khối hiển thị thông tin gói tập, giỏ hàng POS và ca tập; viền `1px solid #334155`. |
| **Màu Chữ Chính (Primary Text)** | `#F8FAFC` (Slate 50) | Dùng cho tiêu đề, nội dung, họ tên hội viên và thông tin quan trọng. |
| **Màu Chữ Phụ (Muted Text)** | `#94A3B8` (Slate 400) | Dùng cho nhãn phụ, ngày hết hạn, mã định danh gói tập và chú thích chỉ số InBody. |
| **Điểm Nhấn Hành Động (Primary Accent)** | `#0EA5E9` (Sky 500) | **Đúng 1 màu nổi bật duy nhất** cho các nút CTA chính: Check-in, Thanh toán, Đặt lịch PT, Đăng ký gói tập. |
| **Trạng Thái Thành Công (Success)** | Chữ: `#4ADE80` (Green 400) • Nền: `#14532D` viền `#15803D` | Sử dụng cho: Check-in hợp lệ, Mở cổng Turnstile, gói tập `ACTIVE`, hóa đơn `PAID`, Đặt lịch PT thành công. |
| **Trạng Thái Cảnh Báo (Warning)** | Chữ: `#FBBF24` (Amber 400) • Nền: `#78350F` viền `#B45309` | Sử dụng cho: Gói tập sắp hết hạn ($\le 7$ ngày), Gói tập bảo lưu `FROZEN`, Cảnh báo hủy ca tập $< 2\text{h}$. |
| **Trạng Thái Lỗi / Bị Khóa (Danger)** | Chữ: `#F87171` (Red 400) • Nền: `#7F1D1D` viền `#B91C1C` | Sử dụng cho: Gói tập hết hạn `EXPIRED`, Gói tập bị hủy `CANCELLED`, Điểm danh thất bại, Trùng lịch dạy PT. |
| **Trạng Thái Ca PT Đã Đặt** | Chữ: `#A78BFA` (Purple 400) • Nền: `#4C1D95` viền `#6D28D9` | Điểm nhấn nhận diện ca dạy PT đã đặt (`PTTrainingSession`) và chỉ số InBody. |
| **Khung Giờ Rảnh PT (Open Slot)** | Chữ: `#CBD5E1` (Slate 300) • Nền: `#1E293B` viền `#475569` | Khung giờ rảnh của PT (`PTAvailability`) — **dùng màu trung tính, tuyệt đối không dùng màu xanh lá**. |

> [!IMPORTANT]
> **Quy chuẩn nhãn trạng thái:** Giữ nguyên các chip trạng thái ở dạng nhãn chữ thuần túy, **không thêm biểu tượng icon** vào chip trạng thái. **Hoàn toàn không sử dụng âm thanh phản hồi** (không có tiếng "Beep", "Buzz"), mọi kết quả đều được truyền tải qua thông báo văn bản trực quan.

### 1.2. Kiểu Chữ, Vùng Bấm (Touch Targets) & Tương Tác Phím Tắt

* **Font chữ hệ thống:** Sử dụng phông chữ Sans-serif hiện đại (`Segoe UI Variable` trên WinUI 3 Desktop và `Inter` / `system-ui` trên Blazor Web) hỗ trợ hiển thị tiếng Việt hoàn hảo, không lỗi dấu.
* **Quy chuẩn vùng bấm (Touch & Click Target):**
  * Trên Web Mobile (Hội viên): Mọi nút bấm, tab và mã QR phải có kích thước tối thiểu **$44\text{px} \times 44\text{px}$** với khoảng đệm tối thiểu $8\text{px}$.
  * Trên Desktop (Admin): Nút quét QR và thanh toán lớn, dễ click chuột nhanh hoặc hỗ trợ màn hình cảm ứng POS tại quầy.
* **Hỗ trợ phím tắt quầy (POS & Check-in Shortcuts):**
  * Màn hình Admin hỗ trợ phím tắt thao tác nhanh: `F2` (Tạo hóa đơn mới), `F3` (Tìm hội viên nhanh), `F5` (Làm mới bảng lịch PT), `Enter` (Xác nhận thanh toán / In hóa đơn / Điểm danh thủ công), `Esc` (Đóng hộp thoại). Tuyệt đối không dùng phím tắt `F1` cho điểm danh.
* **Hiệu ứng chuyển động (Motion & Feedback):**
  * Tinh giản, nhanh gọn (`duration-150` đến `duration-200`). Cấm các hiệu ứng viền/chấm nhấp nháy (Flashing/Blinking) hoặc chuyển động rườm rà làm chậm tốc độ điểm danh và thanh toán tại quầy.

---

## 2. QUY CHUẨN TRẢI NGHIỆM HỘI VIÊN & HUẤN LUYỆN VIÊN (MEMBER & PT JOURNEY - BLAZOR WEB)

Cổng thông tin Web Portal (`GymManagement.Web`) được thiết kế theo tư duy **Mobile-First Responsive**, tối ưu cho hội viên và huấn luyện viên sử dụng trên điện thoại di động khi đến phòng tập.

### 2.1. Giai đoạn 1: Đăng nhập Một Chạm & Khối Thông Tin Gói Tập Số (Khớp UC02)
* **Khối Thông Tin Gói Tập Số (Digital Subscription Widget):**
  * Sau khi đăng nhập, màn hình chính (Dashboard) hiển thị ngay Khối Thông Tin Gói Tập Số ở vị trí trên cùng.
  * Khối hiển thị rõ: Họ tên hội viên (`User.FullName`), Tên gói tập hiện tại (`PackagePlan.Name`), Trạng thái gói tập dạng nhãn tĩnh (`MemberSubscription.Status`), Số ngày còn lại (tính từ `MemberSubscription.EndDate`) và Số lượt tập còn lại (`RemainingCheckInSessions`, `RemainingPtSessions`).
* **Cơ Chế Mã QR Điểm Danh:**
  * Dashboard hiển thị nút `"Mở mã QR điểm danh"` nổi bật; khi mở, QR xuất hiện ở kích thước tối thiểu **240 × 240 px**, co giãn phù hợp màn hình điện thoại mà không bị cắt.
  * QR đen trên nền trắng đặc, giữ vùng trống (quiet zone) tối thiểu 4 module. Không đặt chữ, logo, họa tiết hoặc lớp phủ lên mã để bảo đảm máy quét đọc được.
  * Trang QR hiển thị Mã hội viên và Họ tên để đối chiếu. Mã QR hội viên là mã tĩnh chứa mã hội viên (Member Code), tải một lần dùng xuyên suốt gói tập, không tự động xoay vòng theo thời gian.
  * Khi gói tập không hợp lệ (hết hạn, bảo lưu), ẩn mã hoặc làm mờ kèm thông báo giải thích rõ lý do.
* **Cảnh Báo Hết Hạn & Bảo Lưu Gói Tập:**
  * Khi gói tập còn dưới 7 ngày hoặc dưới 3 lượt tập: Hiển thị thanh thông báo màu vàng hổ phách mời gia hạn kèm nút `"Gia hạn ngay"` dẫn thẳng tới màn hình thanh toán VietQR.
  * Nếu gói tập đang ở trạng thái `FROZEN`: Hiển thị rõ ngày kết thúc bảo lưu và nút `"Yêu cầu mở gói tập sớm"`.

### 2.2. Giai đoạn 2: Khám Phá Gói Tập & Thanh Toán VietQR (Khớp UC03)
* **Bảng Giá & So Sánh Gói Tập Minh Bạch:**
  * Liệt kê danh mục gói tập dạng thẻ trực quan khớp bảng `PackagePlan` (`PlanId`, `Name`, `Description`).
  * Hiển thị rõ đơn giá niêm yết (`PackagePlan.Price`), thời hạn sử dụng (`DurationDays`), số buổi tập gym (`MaxCheckInSessions`), số buổi tập PT đi kèm (`TotalPtSessions`).
  * **Chính sách Giá Niêm Yết Cố Định:** Giá hóa đơn hiển thị đúng bằng giá niêm yết của gói (`Invoice.TotalAmount = CAST(PackagePlan.Price AS INT)`). Hệ thống không áp dụng chiết khấu/giảm giá vì mô hình CSDL vật lý không lưu trữ giảm giá.
* **Thanh Toán VietQR Không Cần Nhập Liệu:**
  * Khi hội viên chọn `"Đăng ký gói"` $\rightarrow$ Hệ thống gọi API sinh mã thanh toán VietQR chuẩn NAPAS 247 có sẵn số tiền chính xác và nội dung chuyển khoản định danh duy nhất (ví dụ: `GYM SUB 1042`).
  * Hiển thị đồng hồ đếm ngược phiên thanh toán **10 phút (600 giây)** kèm hướng dẫn: `"Mở ứng dụng ngân hàng bất kỳ để quét mã"`.
  * **Nút Giả Lập Thanh Toán Sandbox:**
    * Dưới mã QR có nút `"Giả lập thanh toán thành công"` phục vụ kiểm thử luồng tích hợp.
    * Nút chỉ hiển thị khi biến môi trường `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`.
    * Trong môi trường Production (`VIETQR_ENABLE_SIMULATION=false`), nút này tự động ẩn hoàn toàn khỏi giao diện người dùng.
    * Khóa nút ngay sau khi nhấn để tránh gửi lệnh trùng lặp.
  * **Cơ chế Kích hoạt Thời gian Thực:**
    * Client lắng nghe kết quả qua SignalR / WebSocket hoặc cơ chế Polling 2 giây/lần.
    * Ngay khi nhận được tín hiệu thanh toán thành công:
      1. Tự động đóng modal QR code ngay lập tức (không cần tải lại trang).
      2. Thông báo biểu thị trạng thái: `"Kích hoạt gói tập thành công!"`.
      3. Cập nhật ngay trạng thái gói tập sang `ACTIVE` và cộng đủ số lượt tập.
      4. Cung cấp nút xem/tải Hóa đơn điện tử PDF.

### 2.3. Giai đoạn 3: Đặt Lịch Tập Với Huấn Luyện Viên Cá Nhân (Khớp UC07)
* **Lưới Chọn Khung Giờ Trực Quan:**
  * Hội viên chọn Huấn luyện viên phụ trách (`AssignedPtId`) $\rightarrow$ Hệ thống hiển thị lịch tuần dạng lưới các khung giờ rảnh khả dụng (`PTAvailability`).
  * Khung giờ rảnh hiển thị màu trung tính (`pt-slot-open`). Các khung giờ đã có người đặt hoặc PT bận được làm mờ (`opacity-40 cursor-not-allowed`) kèm nhãn `"Đã kín"`.
* **Ràng Buộc Quy Tắc Hủy Ca Trước 2 Giờ Chuẩn PRD:**
  * Khi hội viên xem danh sách ca tập đã đặt:
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$\ge 2$ giờ**: Nút `"Hủy ca tập"` hiển thị bình thường. Khi xác nhận, hoàn lại 1 lượt tập `RemainingPtSessions` về gói tập mà không phạt.
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$< 2$ giờ**: Nút `"Hủy ca tập"` đổi sang màu đỏ cảnh báo. Khi click, bật hộp thoại cảnh báo: `"Chú ý: Hủy ca tập trước dưới 2 tiếng sẽ bị tính là một buổi tập đã sử dụng theo nội quy phòng tập. Bạn có chắc chắn muốn hủy ca này không?"` với 2 lựa chọn: `"Giữ Lại Ca Tập"` hoặc `"Xác Nhận Hủy"`.
* **Giao Diện Huấn Luyện Viên (Khớp UC08):**
  * PT đăng nhập xem được ngay: Danh sách ca dạy trong ngày theo dòng thời gian (Timeline), thông tin hội viên của ca đó, số điện thoại liên hệ nhanh, và trạng thái hội viên đã check-in hay chưa.
  * Nút chuyển đổi trạng thái ca tập: `"Bắt đầu buổi tập"` $\rightarrow$ `"Hoàn thành buổi tập"` kèm ô nhập ghi chú buổi tập (bài tập chính, mức tạ, nhận xét thể lực).

### 2.4. Giai đoạn 4: Theo Dõi Sức Khỏe & Tiến Trình InBody (Khớp UC06)
* **Bảng Chỉ Số Đo Lường Cá Nhân:**
  * Hiển thị kết quả đo InBody mới nhất khớp bảng `MemberBodyMetric`: Chiều cao (`HeightCm` - cm), Cân nặng (`WeightKg` - kg), Tỷ lệ mỡ (`BodyFatPercentage` - %), Khối lượng cơ xương (`SkeletalMuscleMass` - kg), và Chỉ số khối cơ thể tính tự động:
    $$\text{Bmi} = \frac{\text{WeightKg}}{(\text{HeightCm} / 100)^2}$$
* **Phân Loại Tình Trạng Thể Thao:**
  * Sử dụng thước đo trực quan có dải màu chuẩn (Thiếu cân - Xanh dương, Chuẩn - Xanh lá, Thừa cân - Vàng, Béo phì - Đỏ).
* **Biểu Đồ Tiến Trình Theo Thời Gian:**
  * Biểu đồ đường phẳng (Line chart) thể hiện xu hướng biến thiên cân nặng (`WeightKg`), tỷ lệ mỡ (`BodyFatPercentage`) và cơ xương (`SkeletalMuscleMass`) qua các mốc thời gian (`measured_at`).

---

## 3. QUY CHUẨN TRẢI NGHIỆM DÀNH CHO ADMIN (ADMIN WORKFLOW - WINUI 3 DESKTOP)

Ứng dụng WinUI 3 Desktop (`GymManagement.Desktop`) được thiết kế theo tư duy **Desktop-First MVVM**, phục vụ màn hình làm việc của Admin tại quầy tiếp đón với tốc độ xử lý tức thì, mật độ thông tin cao và giao diện phẳng Fluent Design độc tôn Dark Mode.

### 3.1. Phân Hệ Quầy Điểm Danh & Cổng Tự Động (Khớp UC04)
* **Tốc Độ Xử Lý Điểm Danh < 1 Giây:**
  * Luồng check-in là luồng quan trọng bậc nhất của phòng gym, không được phép trễ nải gây ùn tắc tại cửa ra vào.
* **Giao Diện Màn Hình Điểm Danh Quầy Tiếp Đón (Chia 2 Cột Rõ Ràng):**
  * **Cột Trái (Webcam Quét Mã QR & Tra Cứu Thủ Công):**
    * Hệ thống không dùng thẻ vật lý, không có đầu đọc thẻ USB, và không dùng máy quét barcode cầm tay. Tuyệt đối không cung cấp ô text để gõ chuỗi mã QR. Không dùng phím tắt `F1`.
    * Hội viên xuất trình mã QR từ ứng dụng Web/Mobile trước webcam tại quầy; webcam nhận diện và giải mã tự động theo thời gian thực.
    * Kèm theo ô nhập nhanh để tra cứu thủ công bằng **Mã hội viên (`UserName`)** hoặc **Số điện thoại (`User.Phone`)** khi hội viên quên điện thoại hoặc hết pin.
    * Nút thao tác nhanh: `"Cho vào thủ công (Override)"` (kèm chọn lý do) và `"Mở cổng kiểm tra"`.
  * **Cột Phải (Kết Quả Xác Minh Từ Backend Tinh Gọn):**
    * Desktop App đóng vai trò Presentation: Không tự kiểm tra điều kiện nghiệp vụ cục bộ mà gửi `QrCode` (từ webcam) hoặc `MemberCode`/`Phone` lên `GymManagement API` (`POST /api/v1/checkin`).
    * Toàn bộ việc kiểm tra tính hợp lệ của gói tập (`MemberSubscription.Status == ACTIVE`, còn hạn sử dụng `EndDate >= Today`, còn lượt vào phòng `RemainingCheckInSessions > 0`, kiểm tra gói có đang bảo lưu `FROZEN` hay không) đều được Backend xử lý tập trung trong transaction.
    * **Hoàn toàn không sử dụng âm thanh phản hồi** (không phát tiếng "Beep" hay "Buzz").
    * **Hoàn toàn không yêu cầu ảnh chân dung** (không hiển thị khung ảnh bo tròn).
    * **Bảng kết quả & log điểm danh tập trung tinh gọn đúng 2 thông tin cốt lõi:**
      1. **Mã hội viên (`UserName`)**
      2. **Họ tên hội viên (`FullName`)**
    * **Khi Check-in Hợp Lệ (Backend trả về Success):** Màn hình hiển thị banner xanh tĩnh: `"CHECK-IN THÀNH CÔNG • CỔNG MỞ"`, to rõ Mã hội viên (`UserName`), Họ tên (`FullName`), Tên gói tập và số buổi gym còn lại (`RemainingCheckInSessions`). **Không nhấp nháy viền**.
    * **Khi Gói Tập Không Hợp Lệ (Backend trả về BusinessRuleException):** Màn hình hiển thị banner đỏ tĩnh: `"TỪ CHỐI CHECK-IN • CỔNG KHÓA"`, to rõ Mã hội viên (`UserName`), Họ tên (`FullName`), và dòng lý do chi tiết do Backend phản hồi (ví dụ: *"Gói tập trong tài khoản đã hết hạn vào ngày 15/10/2026"*, *"Gói tập đã hết số lượt vào phòng"*, *"Gói tập đang tạm ngưng bảo lưu"*). **Không nhấp nháy viền**.
* **Nút Ghi Đè Thủ Công (Manual Override):**
  * Trường hợp hội viên hết pin điện thoại hoặc cổng turnstile kẹt cơ khí, Admin tra cứu tài khoản hội viên qua Mã hội viên hoặc SĐT và dùng nút `"Cho phép vào thủ công (Manual Check-in)"` kèm bắt buộc chọn lý do để lưu vào vết kiểm toán (`CheckInLog`, ghi nhận `VerifiedByAdminId`).

### 3.2. Phân Hệ Bán Gói & Xuất Hóa Đơn (Khớp UC03)
* **Tối Ưu Hóa Quy Trình Tạo Hóa Đơn (3-Step Fast Checkout Khớp Bảng Invoice):**
  1. **Bước 1 - Tìm / Chọn Hội viên:** Ô tìm kiếm thông minh hỗ trợ gõ tên hoặc số điện thoại (`User.Phone`) có gợi ý tự động trong $< 300\text{ms}$ để liên kết `Invoice.UserName`.
  2. **Bước 2 - Chọn Gói Tập / Dịch Vụ:** Lưới các gói tập (`PackagePlan`) hiển thị tên gói (`PackagePlan.Name`), đơn giá niêm yết (`PackagePlan.Price`), thời hạn (`DurationDays`), số buổi gym (`MaxCheckInSessions`) và số buổi kèm PT (`TotalPtSessions`). Hệ thống tự động gán `Invoice.SubtotalAmount = PackagePlan.Price` và `Invoice.TotalAmount = CAST(PackagePlan.Price AS INT)`.
     * **Ràng Buộc Tuyệt Đối Về Giá:** Không có trường giảm giá hay mã ưu đãi vì bảng `Invoice` trong CSDL vật lý không lưu trữ giảm giá. Hóa đơn phản ánh đúng 100% giá niêm yết của gói.
  3. **Bước 3 - Chọn Phương Thức Thanh Toán:**
     * Nút bấm lớn chuyển đổi nhanh các giá trị lưu vào `Invoice.PaymentMethod`: `Tiền mặt (CASH)`, `Chuyển khoản VietQR (VIETQR)`, `Thanh toán thẻ ngân hàng (CREDIT_CARD)`.
     * Xác định loại giao dịch lưu vào `Invoice.TransactionType`: `SUBSCRIPTION_PURCHASE` (đăng ký mới) hoặc `SUBSCRIPTION_RENEWAL` (gia hạn).
* **Cơ Chế Thanh Toán VietQR & Nút Giả Lập Sandbox:**
  * Khi chọn VietQR: Màn hình hiển thị cửa sổ bật lên (Dialog) có mã QR động lớn và tổng số tiền thanh toán `Invoice.TotalAmount`.
  * **Nút Giả Lập Thanh Toán Sandbox:** Hiển thị nút `"Giả lập thanh toán thành công"` khi biến môi trường `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`. Ẩn hoàn toàn trong môi trường Production (`VIETQR_ENABLE_SIMULATION=false`). Khóa nút ngay sau khi nhấn để tránh gửi lệnh trùng lặp.
  * **Không khóa cưỡng bức thời gian:** Thu ngân được bấm nút xác nhận thu tiền thủ công ngay khi có bằng chứng chuyển khoản thành công, đi kèm hộp thoại xác nhận nhanh 2 bước để phòng ngừa bấm nhầm (không đếm lùi cưỡng bức 10 giây).
* **Tự Động Sinh Hóa Đơn & Mở Khóa Dịch Vụ (ACID Integrity):**
  * Ngay khi giao dịch hoàn tất: Tạo bản ghi `Invoice` (sinh mã `InvoiceCode` duy nhất), cập nhật `MemberSubscription` mới hoặc gia hạn thêm ngày (`EndDate`) / lượt (`RemainingCheckInSessions`, `RemainingPtSessions`) cho gói cũ trong cùng 1 transaction CSDL.
  * Hộp thoại in hóa đơn xuất hiện tức thì với nút in nhanh `Enter` hoặc đóng `Esc`.

### 3.3. Phân Hệ Quản Lý Lịch Dạy & Chống Xung Đột Huấn Luyện Viên (Khớp UC05)
* **Giao Diện Bảng Lịch Huấn Luyện Viên Dạng Lưới Tuần (Weekly Timeline View):**
  * Trục dọc thể hiện khung giờ (06:00 đến 22:00, mỗi nấc 60 phút). Trục ngang thể hiện các ngày trong tuần hoặc danh sách các PT đang làm việc.
  * Các khối thời gian hiển thị trạng thái bằng màu sắc rõ nét:
    * Khung giờ PT đăng ký rảnh (`PTAvailability`): Nền Slate 800 viền Slate 600 (`pt-slot-open`).
    * Ca tập đã có hội viên đăng ký (`PTTrainingSession`): Nền Tím `#4C1D95` viền `#6D28D9` (`status-pt`).
    * Ngoài ca làm việc của PT: Nền Slate 900 mờ.
* **Hàng Rào Chống Trùng Lịch Nghiêm Ngặt:**
  * Khi nhân viên hoặc hội viên đặt lịch: Nếu khung giờ đã trùng với một ca dạy khác của cùng PT, hệ thống:
    1. Lập tức viền đỏ ô lịch xung đột.
    2. Vô hiệu hóa nút `"Xác nhận đặt lịch"`.
    3. Hiển thị thông báo lỗi rõ ràng: `"Huấn luyện viên [Tên PT] đã có ca dạy trong khung giờ này. Vui lòng chọn khung giờ khác."`.

### 3.4. Phân Hệ Quản Lý Hội Viên & Nhập Chỉ Số Sức Khỏe (Khớp UC02 & UC06)
* **Bảng Dữ Liệu Hội Viên Đa Năng:**
  * Hỗ trợ tìm kiếm, phân trang mượt mà, lọc theo trạng thái gói tập (`ACTIVE`, `FROZEN`, `EXPIRED`).
  * Nút hành động nhanh trên từng dòng: `Xem chi tiết`, `Gia hạn gói tập`, `Bảo lưu gói tập`, `Nhập InBody`.
* **Quy Trình Bảo Lưu Gói Tập (Freeze Subscription Dialog):**
  * Nhân viên chọn Ngày bắt đầu bảo lưu (`FreezeStartDate`) và Ngày kết thúc (`FreezeEndDate`).
  * Hệ thống tự động tính số ngày bảo lưu và hiển thị ngày hết hạn mới dự kiến:
    $$\text{EndDate mới} = \text{EndDate cũ} + \text{Số ngày bảo lưu}$$
  * Có cảnh báo rõ ràng nếu số ngày bảo lưu vượt quá chính sách gói tập quy định.
* **Biểu Mẫu Nhập Chỉ Số InBody Phòng Vệ (Khớp Bảng MemberBodyMetric):**
  * Ràng buộc giới hạn hợp lệ đầu vào khớp các trường của `MemberBodyMetric`:
    * Chiều cao (`HeightCm`): $100\text{cm} \le \text{HeightCm} \le 250\text{cm}$.
    * Cân nặng (`WeightKg`): $30\text{kg} \le \text{WeightKg} \le 250\text{kg}$.
    * Tỷ lệ mỡ (`BodyFatPercentage`): $3\% \le \text{BodyFatPercentage} \le 60\%$.
    * Khối lượng cơ xương (`SkeletalMuscleMass`): $10\text{kg} \le \text{SkeletalMuscleMass} \le 100\text{kg}$.
  * Tự động tính và hiển thị ngay chỉ số $\text{Bmi} = \text{WeightKg} / ((\text{HeightCm} / 100) \times (\text{HeightCm} / 100))$ khi người dùng vừa gõ xong Chiều cao và Cân nặng trước khi bấm nút Lưu.
  * Tự động lưu `measured_at = Ngày hiện tại`, `RecordedByPtId = ID của PT đang đăng nhập`.

### 3.5. Báo Cáo Doanh Thu, Vận Hành & Nhật Ký Kiểm Toán (Khớp UC10)
* **Thẻ Chỉ Số KPI Trực Quan:**
  * 4 thẻ đầu trang: `Doanh thu hôm nay (VNĐ)`, `Tổng lượt check-in hôm nay`, `Số hội viên mới trong tháng`, `Số ca PT hoàn thành`.
* **Biểu Đồ Xu Hướng Giờ Cao Điểm:**
  * Biểu đồ cột thể hiện phân bố lượt vào phòng theo từng khung giờ trong ngày (giúp ban quản lý nhận biết khung giờ cao điểm 17:00 - 20:00 để bố trí nhân sự tạ và máy móc).
* **Nhật Ký Kiểm Toán Không Thể Sửa:**
  * Mọi thao tác nhạy cảm: Hủy hóa đơn, Cho check-in thủ công, Chỉnh sửa thông tin hội viên đều được lưu vết chi tiết kèm User ID của nhân viên, thời điểm thực hiện và địa chỉ IP.

---

## 4. MA TRẬN ĐỐI CHIẾU TRẢI NGHIỆM: HỘI VIÊN / PT (WEB) VS. QUẢN TRỊ VIÊN ADMIN (DESKTOP)

| Thành Phần Nghiệp Vụ | Trải Nghiệm Hội Viên & PT (Blazor Server - Mobile) | Trải Nghiệm Quản Trị Viên (WinUI 3 - Desktop) |
| :--- | :--- | :--- |
| **Điểm danh & Ra vào** | Mở mã QR gói tập trên điện thoại, hiển thị mã QR tĩnh chứa mã hội viên để quét qua quầy check-in. | Màn hình Admin tiếp nhận quét camera trong $<1\text{s}$, kết quả hiển thị tinh gọn **Mã hội viên + Họ tên**, banner tĩnh xanh/đỏ (không âm thanh, không ảnh). |
| **Gói tập & Thanh toán** | Tự xem danh mục gói tập, chọn mua và quét mã VietQR tự động kích hoạt gói tập trong 3 giây. Hỗ trợ nút giả lập thanh toán sandbox qua biến môi trường. | Admin chọn nhanh hội viên, gói tập, xuất hóa đơn in nhiệt K80, kiểm tra tiền về qua Webhook hoặc xác nhận thu tiền mặt (không khóa cưỡng bức thời gian). Hỗ trợ nút giả lập sandbox qua biến môi trường. |
| **Lịch Huấn luyện viên PT** | Xem lịch rảnh của PT trên lưới tuần (màu trung tính), đặt ca tập chỉ với 2 click; cảnh báo phạt nếu hủy ca $<2$ tiếng. | PT xem danh sách học viên theo Timeline ca dạy; Quản trị viên xếp lịch PT, phát hiện và chặn đứng xung đột trùng giờ. |
| **Chỉ số Sức khỏe InBody** | Xem biểu đồ trực quan biến thiên cân nặng, tỷ lệ mỡ và khối lượng cơ qua từng mốc thời gian. | Nhân viên đo nhập số liệu trên form có kiểm tra biên an toàn, tự động tính chỉ số BMI ngay tức thì. |
| **Quản lý & Báo cáo** | Nhận thông báo tự động khi gói tập sắp hết hạn hoặc đến giờ hẹn tập cùng huấn luyện viên. | Theo dõi Dashboard KPI doanh thu thời gian thực, xem mật độ giờ cao điểm, trích xuất báo cáo Excel/CSV và sổ kiểm toán. |

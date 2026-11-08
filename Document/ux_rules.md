# USER EXPERIENCE & INTERACTION FLOW RULES (UX SYSTEM RULES)
## Hệ Thống Quản Lý Phòng Tập Thể Thao (Gym Management System)
### WinUI 3 Desktop (Admin) & Blazor Server (Member/PT Portal)

> **Mục đích tài liệu:** Tài liệu này quy định toàn bộ tiêu chuẩn **Trải nghiệm Người dùng & Luồng Tương tác Nghiệp vụ (UX - User Experience)** cho toàn bộ lập trình viên và AI Agent khi triển khai giao diện, quản lý trạng thái, xử lý sự kiện và kiểm soát ngoại lệ trên 2 ứng dụng client: **WinUI 3 Desktop App** (dành riêng cho Admin) và **Blazor Server Web App** (dành cho Hội viên và Huấn luyện viên cá nhân PT).
> **Triết lý UX Cốt lõi (Core UX Philosophy):** 
> * **Đối với Hội viên & PT (Web Portal):** Thao tác tức thì (Zero-Friction), gói tập điện tử quét mã QR trong 1 chạm, minh bạch lượt tập và lịch hẹn, cảnh báo hủy ca trước 2 giờ rõ ràng.
> * **Đối với Ban quản trị Admin (Desktop App):** Tốc độ quầy cực nhanh (< 1 giây điểm danh), an toàn giao dịch tài chính (VietQR tự động khớp, chống trùng hóa đơn), kiểm soát chống gian lận và trực quan hóa xung đột lịch dạy PT.

---

## 1. NỀN TẢNG THIẾT KẾ & QUY CHUẨN TRẢI NGHIỆM CƠ BẢN

Mọi màn hình và thành phần giao diện khi hiện thực đều phải tuân thủ nghiêm ngặt các ràng buộc thiết kế sau:

### 1.1. Bảng Mã Màu Ngữ Nghĩa Hệ Thống (Semantic Status Colors)

Hệ thống sử dụng bảng màu hiện đại, tương phản cao, tối ưu cho môi trường ánh sáng phòng tập thể thao:

| Vai Trò Token | Mã Màu Hex / Tailwind | Ràng Buộc Ứng Dụng Nghiệp Vụ Bắt Buộc |
| :--- | :--- | :--- |
| **Nền Chính (Primary Background)** | `#0F172A` (Slate 900) | Giao diện Desktop và Web chỉ sử dụng Dark Mode; Web Portal tối ưu hiển thị rõ nét trên thiết bị di động. |
| **Bề Mặt Khối (Card / Surface)** | `#1E293B` (Slate 800) | Khối hiển thị thông tin gói tập, giỏ hàng POS và ca tập; viền `1px solid #334155`. |
| **Màu Chữ Chính (Primary Text)** | `#F8FAFC` (Slate 50) | Dùng cho tiêu đề, nội dung và thông tin quan trọng; bảo đảm tương phản tốt trên nền tối. |
| **Màu Chữ Phụ (Muted Text)** | `#94A3B8` (Slate 400) | Dùng cho nhãn phụ, ngày hết hạn, mã định danh gói tập và chú thích chỉ số InBody. |
| **Điểm Nhấn Hành Động (Primary Accent)** | `#0EA5E9` (Cyan/Sky) hoặc `#2563EB` (Blue) | Đúng 1 màu nổi bật cho các nút CTA chính: Check-in, Thanh toán, Đặt lịch PT, Đăng ký gói tập. |
| **Trạng Thái Thành Công (Success)** | `#16A34A` / `text-green-400` | Sử dụng cho: Check-in hợp lệ, Mở cổng Turnstile, `ACTIVE`, `PAID`, Đặt lịch PT thành công. |
| **Trạng Thái Cảnh Báo (Warning)** | `#D97706` / `text-amber-400` | Sử dụng cho: Gói tập sắp hết hạn ($\le 7$ ngày), Gói tập bảo lưu `FROZEN`, Hủy ca tập $< 2\text{h}$ tính phí phạt, Lượt tập còn ít ($\le 2$). |
| **Trạng Thái Lỗi / Bị Khóa (Danger)** | `#DC2626` / `text-red-400` | Sử dụng cho: Gói tập hết hạn `EXPIRED`, Gói tập bị hủy `CANCELLED`, Điểm danh thất bại (Hết lượt/Sai mã), Trùng lịch dạy PT. |
| **Trạng Thái PT & Lịch Tập (PT Accent)** | `#8B5CF6` (Purple/Violet) | Điểm nhấn chuyên biệt nhận diện ca dạy PT, chỉ số InBody và lịch biểu huấn luyện viên. |

### 1.2. Kiểu Chữ, Vùng Bấm (Touch Targets) & Tương Tác Phím Tắt

* **Font chữ hệ thống:** Sử dụng phông chữ Sans-serif hiện đại (`Segoe UI Variable` trên WinUI 3 Desktop và `Inter` / `system-ui` trên Blazor Web) hỗ trợ hiển thị tiếng Việt hoàn hảo, không lỗi dấu.
* **Quy chuẩn vùng bấm (Touch & Click Target):**
  * Trên Web Mobile (Hội viên): Mọi nút bấm, tab và mã QR phải có kích thước tối thiểu **$44\text{px} \times 44\text{px}$** với khoảng đệm tối thiểu $8\text{px}$.
  * Trên Desktop (Admin): Nút quét QR và thanh toán lớn, dễ click chuột nhanh hoặc hỗ trợ màn hình cảm ứng POS tại quầy.
* **Hỗ trợ phím tắt quầy (POS & Check-in Shortcuts):**
  * Màn hình Admin hỗ trợ phím tắt thao tác nhanh: `F1` (Bật camera quét mã QR Check-in), `F2` (Tạo hóa đơn mới), `F3` (Tìm hội viên nhanh), `F5` (Làm mới bảng lịch PT), `Enter` (Xác nhận thanh toán / In hóa đơn), `Esc` (Đóng hộp thoại).
* **Hiệu ứng chuyển động (Motion & Feedback):**
  * Tinh giản, nhanh gọn (`duration-150` đến `duration-200`). Cấm các hiệu ứng chuyển động rườm rà làm chậm tốc độ điểm danh và thanh toán tại quầy.

---

## 2. QUY CHUẨN TRẢI NGHIỆM HỘI VIÊN & HUẤN LUYỆN VIÊN (MEMBER & PT JOURNEY - BLAZOR WEB)

Cổng thông tin Web Portal (`GymManagement.Web`) được thiết kế theo tư duy **Mobile-First Responsive**, tối ưu cho hội viên và huấn luyện viên sử dụng trên điện thoại di động khi đến phòng tập.

### 2.1. Giai đoạn 1: Đăng nhập Một Chạm & Gói Tập Hội Viên Số (Digital Membership Card - UC01 & UC02)
* **Gói Tập Số Tức Thời (Zero-Delay Digital Card):**
  * Sau khi đăng nhập, màn hình chính (Dashboard) hiển thị ngay lập tức Khối Thông Tin Gói Tập Điện Tử ở vị trí trên cùng.
  * Khối hiển thị rõ: Họ tên hội viên (`User.FullName`), Ảnh đại diện, Tên gói tập hiện tại (`PackagePlan.Name`), Trạng thái gói tập (`MemberSubscription.Status`), Số ngày còn lại (tính từ `MemberSubscription.EndDate`) và Số lượt tập còn lại (`RemainingCheckInSessions`, `RemainingPtSessions`).
* **Cơ Chế Mã QR Động Chống Gian Lận (Dynamic QR Check-in):**
  * Dashboard hiển thị nút `"Mở mã QR điểm danh"` nổi bật; khi mở, QR xuất hiện riêng ở kích thước tối thiểu **240 × 240 px**, co giãn phù hợp màn hình điện thoại mà không bị cắt.
  * QR đen trên nền trắng đặc, giữ vùng trống (quiet zone) tối thiểu 4 module. Không đặt chữ, logo, họa tiết hoặc lớp phủ lên mã để bảo đảm máy quét đọc được.
  * Trang QR hiển thị tên và ảnh nhỏ của hội viên để đối chiếu; thông tin nhạy cảm không được nhúng trực tiếp vào mã.
  * Mã có thời hạn ngắn, được cấp và xác thực an toàn từ máy chủ, tự làm mới mỗi **60 giây**. Hiển thị đếm ngược rõ ràng và trạng thái đang làm mới.
  * Có nút `"Làm mới mã QR"`; nếu mất mạng hoặc làm mới thất bại, hiển thị thông báo và trạng thái mã hiện tại, không giả vờ rằng mã mới đã được cấp.
  * Khi mã hết hạn hoặc gói tập không hợp lệ, ẩn mã cũ và thông báo rõ lý do. Không yêu cầu ứng dụng tự thay đổi độ sáng thiết bị.
* **Cảnh Báo Hết Hạn & Bảo Lưu Gói Tập (Smart Expiry & Freeze Alert):**
  * Khi gói tập còn dưới 7 ngày hoặc dưới 3 lượt tập: Hiển thị thanh thông báo màu vàng hổ phách mời gia hạn kèm nút `"Gia hạn ngay"` dẫn thẳng tới cổng thanh toán VietQR.
  * Nếu gói tập đang ở trạng thái `FROZEN`: Hiển thị rõ ngày kết thúc bảo lưu và nút `"Yêu cầu mở gói tập sớm"`.

### 2.2. Giai đoạn 2: Khám Phá Gói Tập & Thanh Toán VietQR Sandbox (Catalog & POS Payment - UC03 & UC07)
* **Bảng Giá & So Sánh Gói Tập Minh Bạch (Freemium to Premium Catalog):**
  * Liệt kê danh mục gói tập dạng thẻ trực quan khớp bảng `PackagePlan` (`PlanId`, `Name`, `Description`).
  * Hiển thị rõ đơn giá niêm yết (`PackagePlan.Price`), thời hạn sử dụng (`DurationDays`), số buổi tập gym (`MaxCheckInSessions`), số buổi tập PT đi kèm (`TotalPtSessions`) và cờ kèm PT (`is_PtIncluded`).
  * **Chính sách Giá Niêm Yết Cố Định:** Giá hóa đơn hiển thị đúng bằng giá niêm yết của gói (`Invoice.TotalAmount = CAST(PackagePlan.Price AS INT)`). Hệ thống không áp dụng chiết khấu/giảm giá vì mô hình CSDL vật lý không lưu trữ giảm giá.
* **Thanh Toán VietQR Không Cần Nhập Liệu (Zero-Friction VietQR Checkout):**
  * Khi hội viên chọn `"Đăng ký gói"` $\rightarrow$ Hệ thống gọi API sinh mã thanh toán VietQR chuẩn NAPAS 247 có sẵn số tiền chính xác và nội dung chuyển khoản định danh duy nhất (ví dụ: `GYM SUB 1042`).
  * Hiển thị đồng hồ đếm ngược phiên thanh toán **10 phút (600 giây)** kèm hướng dẫn: `"Mở ứng dụng ngân hàng bất kỳ để quét mã"`.
  * **Cơ chế Kích hoạt Thời gian Thực (Real-time Webhook Activation):**
    * Client lắng nghe kết quả qua SignalR / WebSocket hoặc cơ chế Polling 2 giây/lần.
    * Ngay khi nhận được tín hiệu thanh toán thành công từ Webhook ngân hàng:
      1. Tự động đóng modal QR code ngay lập tức (không cần tải lại trang).
      2. Hiệu ứng chúc mừng tinh tế (Checkmark xanh) kèm thông báo `"Kích hoạt gói tập thành công!"`.
      3. Cập nhật ngay trạng thái gói tập sang `ACTIVE` và cộng đủ số lượt tập.
      4. Cung cấp nút xem/tải Hóa đơn điện tử PDF.

### 2.3. Giai đoạn 3: Đặt Lịch Tập Với Huấn Luyện Viên Cá Nhân (PT Booking - UC05)
* **Lưới Chọn Khung Giờ Trực Quan (Interactive Slot Picker):**
  * Hội viên chọn Huấn luyện viên phụ trách (`AssignedPtId`) $\rightarrow$ Hệ thống hiển thị lịch tuần dạng lưới các khung giờ rảnh khả dụng (`PTAvailability`).
  * Các khung giờ đã có người đặt hoặc PT bận được làm mờ (`opacity-40 cursor-not-allowed`) kèm nhãn `"Đã kín"`.
* **Ràng Buộc Quy Tắc Hủy Ca Trước 2 Giờ (Strict 2-Hour Cancellation Rule):**
  * Khi hội viên xem danh sách ca tập đã đặt:
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$\ge 2$ giờ**: Nút `"Hủy ca tập"` hiển thị bình thường. Khi xác nhận, hoàn lại 1 lượt tập `RemainingPtSessions` về gói tập mà không phạt.
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$< 2$ giờ**: Nút `"Hủy ca tập"` đổi sang màu đỏ cảnh báo. Khi click, bật hộp thoại cảnh báo: `"Chú ý: Hủy ca tập trước dưới 2 tiếng sẽ bị tính là một buổi tập đã sử dụng theo nội quy phòng tập. Bạn có chắc chắn muốn hủy?"`.
* **Giao Diện Huấn Luyện Viên (PT View Workflow):**
  * PT đăng nhập xem được ngay: Danh sách ca dạy trong ngày theo dòng thời gian (Timeline), thông tin hội viên của ca đó, số điện thoại liên hệ nhanh, và trạng thái hội viên đã check-in hay chưa.
  * Nút chuyển đổi trạng thái ca tập: `"Bắt đầu buổi tập"` $\rightarrow$ `"Hoàn thành buổi tập"` kèm ô nhập ghi chú buổi tập (bài tập chính, mức tạ, nhận xét thể lực).

### 2.4. Giai đoạn 4: Theo Dõi Sức Khỏe & Tiến Trình InBody (Health & InBody Tracking - UC06)
* **Bảng Chỉ Số Đo Lường Cá Nhân (Personal Metric Dashboard):**
  * Hiển thị kết quả đo InBody mới nhất khớp bảng `MemberBodyMetric`: Chiều cao (`HeightCm` - cm), Cân nặng (`WeightKg` - kg), Tỷ lệ mỡ (`BodyFatPercentage` - %), Khối lượng cơ xương (`SkeletalMuscleMass` - kg), và Chỉ số khối cơ thể tính tự động:
    $$\text{Bmi} = \frac{\text{WeightKg}}{(\text{HeightCm} / 100)^2}$$
* **Phân Loại Tình Trạng Thể Thao (Visual BMI Categorization):**
  * Sử dụng thước đo trực quan có dải màu chuẩn (Thiếu cân - Xanh dương, Chuẩn - Xanh lá, Thừa cân - Vàng, Béo phì - Đỏ).
* **Biểu Đồ Tiến Trình Theo Thời Gian (Progress Chart):**
  * Biểu đồ đường phẳng (Line chart) thể hiện xu hướng biến thiên cân nặng (`WeightKg`), tỷ lệ mỡ (`BodyFatPercentage`) và cơ xương (`SkeletalMuscleMass`) qua các mốc thời gian (`measured_at`) giúp hội viên và PT cùng đánh giá hiệu quả huấn luyện.

---

## 3. QUY CHUẨN TRẢI NGHIỆM DÀNH CHO ADMIN (ADMIN WORKFLOW - WINUI 3 DESKTOP)

Ứng dụng WinUI 3 Desktop (`GymManagement.Desktop`) được thiết kế theo tư duy **Desktop-First MVVM**, phục vụ màn hình làm việc của Admin tại quầy tiếp đón với tốc độ xử lý tức thì, mật độ thông tin cao và giao diện phẳng Fluent Design.

### 3.1. Phân Hệ Quầy Điểm Danh & Cổng Tự Động (Turnstile Check-in Counter - UC04)
* **Tốc Độ Xử Lý Điểm Danh < 1 Giây:**
  * Luồng check-in là luồng quan trọng bậc nhất của phòng gym, không được phép trễ nải gây ùn tắc tại cửa ra vào.
* **Giao Diện Màn Hình Điểm Danh Quầy Tiếp Đón (Chia 2 Cột Rõ Ràng):**
  * **Cột Trái (Camera Quét Mã QR Hội Viên):**
    * Hệ thống **hoàn toàn không dùng thẻ vật lý và không sử dụng đầu đọc thẻ USB**. Hội viên mở mã QR điện tử động trên ứng dụng Web/Mobile của mình.
    * Màn hình Admin trang bị nút lớn `"Bật Camera Quét Mã QR"` (phím tắt `F1`). Khi kích hoạt, webcam quầy sẽ mở lên để nhận diện và giải mã chuỗi `QrCode` từ màn hình điện thoại hội viên theo thời gian thực.
    * Kèm theo ô nhập nhanh Số điện thoại (`User.Phone`) để tra cứu tài khoản nếu hội viên hết pin hoặc quên điện thoại.
  * **Cột Phải (Kết Quả Xác Minh Từ Backend Tức Thì):**
    * Desktop App đóng vai trò Presentation: Không tự kiểm tra điều kiện nghiệp vụ cục bộ mà gửi chuỗi QR/SĐT lên `GymManagement API` (`POST /api/v1/checkin`).
    * Toàn bộ việc kiểm tra tính hợp lệ của **Gói tập trong tài khoản hội viên** (`MemberSubscription.Status == ACTIVE`, còn hạn sử dụng `EndDate >= Today`, còn lượt vào phòng `RemainingCheckInSessions > 0`, kiểm tra gói có đang tạm ngưng bảo lưu `FROZEN` hay không) đều được **Backend** (`CheckInService`) xử lý tập trung và trừ lượt tập trong database transaction.
    * **Khi Check-in Hợp Lệ (Backend trả về Success):** Màn hình nháy viền xanh lá `#16A34A`, phát âm thanh báo thành công ("Beep" ngắn), hiển thị to rõ: Ảnh chân dung hội viên, Họ tên, Tên gói tập trong tài khoản, Số buổi gym còn lại (`RemainingCheckInSessions`), và tín hiệu `"ĐÃ MỞ CỔNG TURNSTILE"`.
    * **Khi Gói Tập Không Hợp Lệ (Backend trả về BusinessRuleException):** Màn hình đổi nền cảnh báo đỏ `#DC2626`, phát âm thanh cảnh báo lỗi ("Buzz"), hiển thị nguyên nhân chi tiết do Backend phản hồi (ví dụ: *"Gói tập trong tài khoản đã hết hạn vào ngày 15/10/2026"*, *"Gói tập đã hết số lượt vào phòng"*, *"Gói tập đang tạm ngưng bảo lưu"*).
* **Nút Ghi Đè Thủ Công (Manual Override):**
  * Trường hợp hội viên hết pin điện thoại hoặc cổng turnstile kẹt cơ khí, Admin tra cứu tài khoản hội viên qua SĐT và dùng nút `"Cho phép vào thủ công (Manual Check-in)"` kèm bắt buộc chọn lý do để lưu vào vết kiểm toán (`CheckInLog`, ghi nhận `VerifiedByAdminId`).

### 3.2. Phân Hệ Bán Gói & Xuất Hóa Đơn (POS & Billing Workflow - UC07)
* **Tối Ưu Hóa Quy Trình Tạo Hóa Đơn (3-Step Fast Checkout Khớp Bảng Invoice):**
  1. **Bước 1 - Tìm / Chọn Hội viên:** Ô tìm kiếm thông minh hỗ trợ gõ tên hoặc số điện thoại (`User.Phone`) có gợi ý tự động (Auto-complete dropdown) trong $< 300\text{ms}$ để liên kết `Invoice.UserID`.
  2. **Bước 2 - Chọn Gói Tập / Dịch Vụ:** Lưới các gói tập (`PackagePlan`) hiển thị tên gói (`PackagePlan.Name`), đơn giá niêm yết (`PackagePlan.Price`), thời hạn (`DurationDays`), số buổi gym (`MaxCheckInSessions`) và số buổi kèm PT (`TotalPtSessions`). Hệ thống tự động gán `Invoice.SubtotalAmount = PackagePlan.Price` và `Invoice.TotalAmount = CAST(PackagePlan.Price AS INT)`.
     * **Ràng Buộc Tuyệt Đối Về Giá:** Không có trường giảm giá hay mã ưu đãi vì bảng `Invoice` trong CSDL vật lý chỉ gồm các trường: `SubtotalAmount`, `TotalAmount`, `RefundAmount`, `RefundReason`, `PaymentMethod`, `TransactionType`, `paid_at`, `CreatedByAdminId`. Hóa đơn phản ánh đúng 100% giá niêm yết của gói.
  3. **Bước 3 - Chọn Phương Thức Thanh Toán:**
     * Nút bấm lớn chuyển đổi nhanh các giá trị lưu vào `Invoice.PaymentMethod`: `Tiền mặt (CASH)`, `Chuyển khoản VietQR (VIETQR)`, `Thẻ ngân hàng / Quẹt thẻ POS (CREDIT_CARD)`.
     * Xác định loại giao dịch lưu vào `Invoice.TransactionType`: `SUBSCRIPTION_PURCHASE` (đăng ký mới) hoặc `SUBSCRIPTION_RENEWAL` (gia hạn).
* **Cơ Chế Khóa An Toàn Khi Chờ Thanh Toán VietQR:**
  * Khi chọn VietQR: Màn hình hiển thị cửa sổ bật lên (Dialog) có mã QR động lớn và tổng số tiền thanh toán `Invoice.TotalAmount`.
  * Nút `"Xác nhận thu tiền thủ công"` bị vô hiệu hóa trong 10 giây đầu để khuyến khích chờ hệ thống đối soát tự động qua Webhook, tránh Admin bấm nhầm khi tiền chưa vào tài khoản.
* **Tự Động Sinh Hóa Đơn & Mở Khóa Dịch Vụ (ACID Integrity):**
  * Ngay khi giao dịch hoàn tất: Tạo bản ghi `Invoice` (sinh mã `InvoiceCode` duy nhất), cập nhật `MemberSubscription` mới hoặc gia hạn thêm ngày (`EndDate`) / lượt (`RemainingCheckInSessions`, `RemainingPtSessions`) cho gói cũ trong cùng 1 transaction CSDL.
  * Hộp thoại in hóa đơn xuất hiện tức thì với nút in nhanh `Enter` hoặc đóng `Esc`.

### 3.3. Phân Hệ Quản Lý Lịch Dạy & Chống Xung Đột Huấn Luyện Viên (PT Scheduling - UC05)
* **Giao Diện Bảng Lịch Huấn Luyện Viên Dạng Lưới Tuần (Weekly Timeline View):**
  * Trục dọc thể hiện khung giờ (06:00 đến 22:00, mỗi nấc 60 phút). Trục ngang thể hiện các ngày trong tuần hoặc danh sách các PT đang làm việc.
  * Các khối thời gian hiển thị trạng thái bằng màu sắc rõ nét:
    * Xanh lá nhạt: Khung giờ PT đăng ký rảnh (`PTAvailability`).
    * Tím: Ca tập đã có hội viên đăng ký (`PTTrainingSession`).
    * Xám: Ngoài ca làm việc của PT.
* **Hàng Rào Chống Trùng Lịch Nghiêm Ngặt (Strict Schedule Collision Guardrail):**
  * Khi nhân viên hoặc hội viên đặt lịch: Nếu khung giờ đã trùng với một ca dạy khác của cùng PT, hệ thống:
    1. Lập tức viền đỏ ô lịch xung đột.
    2. Vô hiệu hóa nút `"Xác nhận đặt lịch"`.
    3. Hiển thị thông báo lỗi rõ ràng: `"Huấn luyện viên [Tên PT] đã có ca dạy từ 14:00 - 15:00. Vui lòng chọn khung giờ khác."`.

### 3.4. Phân Hệ Quản Lý Hội Viên & Nhập Chỉ Số Sức Khỏe (Member & InBody Management - UC02 & UC06)
* **Bảng Dữ Liệu Hội Viên Đa Năng (DataGrid with Action Chips):**
  * Hỗ trợ tìm kiếm, phân trang mượt mà, lọc theo trạng thái gói tập (`ACTIVE`, `FROZEN`, `EXPIRED`).
  * Nút hành động nhanh trên từng dòng: `Xem chi tiết`, `Gia hạn gói tập`, `Bảo lưu gói tập (Freeze)`, `Nhập InBody`.
* **Quy Trình Bảo Lưu Gói Tập (Freeze Subscription Dialog):**
  * Nhân viên chọn Ngày bắt đầu bảo lưu (`FreezeStartDate`) và Ngày kết thúc (`FreezeEndDate`).
  * Hệ thống tự động tính số ngày bảo lưu và hiển thị ngày hết hạn mới dự kiến:
    $$\text{EndDate mới} = \text{EndDate cũ} + \text{Số ngày bảo lưu}$$
  * Có cảnh báo rõ ràng nếu số ngày bảo lưu vượt quá chính sách gói tập quy định.
* **Biểu Mẫu Nhập Chỉ Số InBody Phòng Vệ (Defensive InBody Form - Khớp Bảng MemberBodyMetric):**
  * Ràng buộc giới hạn hợp lệ đầu vào khớp các trường của `MemberBodyMetric`:
    * Chiều cao (`HeightCm`): $100\text{cm} \le \text{HeightCm} \le 250\text{cm}$.
    * Cân nặng (`WeightKg`): $30\text{kg} \le \text{WeightKg} \le 250\text{kg}$.
    * Tỷ lệ mỡ (`BodyFatPercentage`): $3\% \le \text{BodyFatPercentage} \le 60\%$.
    * Khối lượng cơ xương (`SkeletalMuscleMass`): $10\text{kg} \le \text{SkeletalMuscleMass} \le 100\text{kg}$.
  * Tự động tính và hiển thị ngay chỉ số $\text{Bmi} = \text{WeightKg} / ((\text{HeightCm} / 100) \times (\text{HeightCm} / 100))$ khi người dùng vừa gõ xong Chiều cao và Cân nặng trước khi bấm nút Lưu.
  * Tự động lưu `measured_at = Ngày hiện tại`, `RecordedByPtId = ID của PT đang đăng nhập`.

### 3.5. Báo Cáo Doanh Thu, Vận Hành & Nhật Ký Kiểm Toán (Analytics & Audit Trail - UC08 & UC10)
* **Thẻ Chỉ Số KPI Trực Quan (Executive KPI Cards):**
  * 4 thẻ đầu trang: `Doanh thu hôm nay (VNĐ)`, `Tổng lượt check-in hôm nay`, `Số hội viên mới trong tháng`, `Số ca PT hoàn thành`.
* **Biểu Đồ Xu Hướng Giờ Cao Điểm (Peak Hours Density):**
  * Biểu đồ cột thể hiện phân bố lượt vào phòng theo từng khung giờ trong ngày (giúp ban quản lý nhận biết khung giờ cao điểm 17:00 - 20:00 để bố trí nhân sự tạ và máy móc).
* **Nhật Ký Kiểm Toán Không Thể Sửa (Immutable Audit Trail Log):**
  * Mọi thao tác nhạy cảm: Hủy hóa đơn, Cho check-in thủ công, Chỉnh sửa thông tin hội viên đều được lưu vết chi tiết kèm User ID của nhân viên, thời điểm thực hiện và địa chỉ IP.

---

## 4. MA TRẬN ĐỐI CHIẾU TRẢI NGHIỆM: HỘI VIÊN / PT (WEB) VS. QUẢN TRỊ VIÊN ADMIN (DESKTOP)

| Thành Phần Nghiệp Vụ | Trải Nghiệm Hội Viên & PT (Blazor Server - Mobile) | Trải Nghiệm Quản Trị Viên (WinUI 3 - Desktop) |
| :--- | :--- | :--- |
| **Điểm danh & Ra vào (UC04)** | Mở mã QR gói tập trên điện thoại, hiển thị mã QR động tự đổi mỗi 60 giây để quét qua turnstile. | Màn hình Admin tiếp nhận quét camera trong $<1\text{s}$, đối chiếu ảnh chân dung, hiển thị trạng thái xanh/đỏ và âm thanh phản hồi. |
| **Gói tập & Thanh toán (UC03, UC07)** | Tự xem danh mục gói tập, chọn mua và quét mã VietQR tự động kích hoạt gói tập trong 3 giây. | Admin chọn nhanh hội viên, gói tập, xuất hóa đơn in nhiệt K80, kiểm tra tiền về qua Webhook hoặc xác nhận thu tiền mặt. |
| **Lịch Huấn luyện viên PT (UC05)** | Xem lịch rảnh của PT trên lưới tuần, đặt ca tập chỉ với 2 click; cảnh báo phạt nếu hủy ca $<2$ tiếng. | PT xem danh sách học viên theo Timeline ca dạy; Quản trị viên xếp lịch PT, phát hiện và chặn đứng xung đột trùng giờ. |
| **Chỉ số Sức khỏe InBody (UC06)** | Xem biểu đồ trực quan biến thiên cân nặng, tỷ lệ mỡ và khối lượng cơ qua từng mốc thời gian. | Nhân viên đo nhập số liệu trên form có kiểm tra biên an toàn (Guard clauses), tự động tính chỉ số BMI ngay tức thì. |
| **Quản lý & Báo cáo (UC08, UC10)** | Nhận thông báo tự động khi gói tập sắp hết hạn hoặc đến giờ hẹn tập cùng huấn luyện viên. | Theo dõi Dashboard KPI doanh thu thời gian thực, xem mật độ giờ cao điểm, trích xuất báo cáo Excel/CSV và sổ kiểm toán. |

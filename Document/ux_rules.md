# USER EXPERIENCE & INTERACTION FLOW RULES (UX SYSTEM RULES)
## Hệ Thống Quản Lý Phòng Tập Thể Thao (Gym Management System)
### WinUI 3 Desktop (Admin/Staff) & Blazor Server (Member/PT Portal)

> **Mục đích tài liệu:** Tài liệu này quy định toàn bộ tiêu chuẩn **Trải nghiệm Người dùng & Luồng Tương tác Nghiệp vụ (UX - User Experience)** cho toàn bộ lập trình viên và AI Agent khi triển khai giao diện, quản lý trạng thái, xử lý sự kiện và kiểm soát ngoại lệ trên 2 ứng dụng client: **WinUI 3 Desktop App** (dành cho Admin, Lễ tân, Thu ngân) và **Blazor Server Web App** (dành cho Hội viên và Huấn luyện viên cá nhân PT).
> **Triết lý UX Cốt lõi (Core UX Philosophy):** 
> * **Đối với Hội viên & PT (Web Portal):** Thao tác tức thì (Zero-Friction), thẻ tập điện tử quét mã trong 1 chạm, minh bạch lượt tập và lịch hẹn, cảnh báo hủy ca trước 2 giờ rõ ràng.
> * **Đối với Nhân viên & Ban quản lý (Desktop App):** Tốc độ quầy cực nhanh (< 1 giây điểm danh), an toàn giao dịch tài chính (VietQR tự động khớp, chống trùng hóa đơn), kiểm soát chống gian lận thẻ tập và trực quan hóa xung đột lịch dạy PT.

---

## 1. NỀN TẢNG THIẾT KẾ & QUY CHUẨN TRẢI NGHIỆM CƠ BẢN

Mọi màn hình và thành phần giao diện khi hiện thực đều phải tuân thủ nghiêm ngặt các ràng buộc thiết kế sau:

### 1.1. Bảng Mã Màu Ngữ Nghĩa Hệ Thống (Semantic Status Colors)

Hệ thống sử dụng bảng màu hiện đại, tương phản cao, tối ưu cho môi trường ánh sáng phòng tập thể thao:

| Vai Trò Token | Mã Màu Hex / Tailwind | Ràng Buộc Ứng Dụng Nghiệp Vụ Bắt Buộc |
| :--- | :--- | :--- |
| **Nền Chính (Primary Background)** | `#0F172A` (Dark) / `#FFFFFF` (Light) | Giao diện Desktop hỗ trợ Fluent Dark/Light; Web Portal tối ưu hiển thị rõ nét trên thiết bị di động. |
| **Bề Mặt Thẻ (Card / Surface)** | `#1E293B` (Dark) / `#F8FAFC` (Light) | Khối hiển thị thông tin thẻ tập, giỏ hàng POS, ca tập với viền `1px solid #334155` (Dark) hoặc `#E2E8F0` (Light). |
| **Màu Chữ Chính (Primary Text)** | `#F8FAFC` (Dark) / `#0F172A` (Light) | Độ tương phản WCAG AAA ($\ge 7:1$) bảo đảm nhìn rõ từ xa tại quầy lễ tân và màn hình turnstile. |
| **Màu Chữ Phụ (Muted Text)** | `#94A3B8` (Dark) / `#64748B` (Light) | Dùng cho nhãn phụ, ngày hết hạn, mã định danh thẻ tập, chú thích chỉ số InBody. |
| **Điểm Nhấn Hành Động (Primary Accent)** | `#0EA5E9` (Cyan/Sky) hoặc `#2563EB` (Blue) | Đúng 1 màu nổi bật cho các nút CTA chính: Check-in, Thanh toán, Đặt lịch PT, Đăng ký gói tập. |
| **Trạng Thái Thành Công (Success)** | `#16A34A` (`bg-green-600` / `#DCFCE7`) | Sử dụng cho: Check-in hợp lệ, Mở cổng Turnstile, `ACTIVE`, `PAID`, Đặt lịch PT thành công. |
| **Trạng Thái Cảnh Báo (Warning)** | `#D97706` (`bg-amber-500` / `#FEF3C7`) | Sử dụng cho: Thẻ sắp hết hạn ($\le 7$ ngày), Thẻ bảo lưu `FROZEN`, Hủy ca tập $< 2\text{h}$ tính phí phạt, Lượt tập còn ít ($\le 2$). |
| **Trạng Thái Lỗi / Bị Khóa (Danger)** | `#DC2626` (`bg-red-600` / `#FEE2E2`) | Sử dụng cho: Thẻ hết hạn `EXPIRED`, Thẻ bị hủy `CANCELLED`, Điểm danh thất bại (Hết lượt/Sai mã), Trùng lịch dạy PT. |
| **Trạng Thái PT & Lịch Tập (PT Accent)** | `#8B5CF6` (Purple/Violet) | Điểm nhấn chuyên biệt nhận diện ca dạy PT, chỉ số InBody và lịch biểu huấn luyện viên. |

### 1.2. Kiểu Chữ, Vùng Bấm (Touch Targets) & Tương Tác Phím Tắt

* **Font chữ hệ thống:** Sử dụng phông chữ Sans-serif hiện đại (`Segoe UI Variable` trên WinUI 3 Desktop và `Inter` / `system-ui` trên Blazor Web) hỗ trợ hiển thị tiếng Việt hoàn hảo, không lỗi dấu.
* **Quy chuẩn vùng bấm (Touch & Click Target):**
  * Trên Web Mobile (Hội viên): Mọi nút bấm, tab và mã QR phải có kích thước tối thiểu **$44\text{px} \times 44\text{px}$** với khoảng đệm tối thiểu $8\text{px}$.
  * Trên Desktop (Lễ tân/Thu ngân): Nút quét thẻ và thanh toán lớn, dễ click chuột nhanh hoặc hỗ trợ màn hình cảm ứng POS tại quầy.
* **Hỗ trợ phím tắt quầy (POS & Check-in Shortcuts):**
  * Màn hình lễ tân/thu ngân hỗ trợ phím tắt thao tác nhanh: `F1` (Mở ô quét mã Check-in), `F2` (Tạo hóa đơn mới), `F3` (Tìm hội viên nhanh), `F5` (Làm mới bảng lịch PT), `Enter` (Xác nhận thanh toán / In hóa đơn), `Esc` (Đóng hộp thoại).
* **Hiệu ứng chuyển động (Motion & Feedback):**
  * Tinh giản, nhanh gọn (`duration-150` đến `duration-200`). Cấm các hiệu ứng chuyển động rườm rà làm chậm tốc độ điểm danh và thu ngân tại quầy.

---

## 2. QUY CHUẨN TRẢI NGHIỆM HỘI VIÊN & HUẤN LUYỆN VIÊN (MEMBER & PT JOURNEY - BLAZOR WEB)

Cổng thông tin Web Portal (`GymManagement.Web`) được thiết kế theo tư duy **Mobile-First Responsive**, tối ưu cho hội viên và huấn luyện viên sử dụng trên điện thoại di động khi đến phòng tập.

### 2.1. Giai đoạn 1: Đăng nhập Một Chạm & Thẻ Hội Viên Số (Digital Membership Card - UC01 & UC02)
* **Thẻ Tập Số Tức Thời (Zero-Delay Digital Card):**
  * Sau khi đăng nhập, màn hình chính (Dashboard) hiển thị ngay lập tức Thẻ Hội Viên Điện Tử ở vị trí trên cùng.
  * Thẻ hiển thị rõ: Họ tên hội viên, Ảnh đại diện, Tên gói tập hiện tại (`PackagePlan`), Trạng thái thẻ (`ACTIVE`, `FROZEN`, `EXPIRED`), Số ngày còn lại và Số lượt tập còn lại (`RemainingCheckInSessions`, `RemainingPtSessions`).
* **Cơ Chế Mã QR Động Chống Gian Lận (Dynamic QR Check-in):**
  * Mã QR hiển thị to, rõ ở trung tâm thẻ (tối thiểu $200\text{px} \times 200\text{px}$), độ sáng màn hình tự động đẩy tối đa khi mở mã QR.
  * Chuỗi mã QR được mã hóa theo định dạng bảo mật kèm timestamp và tự động làm mới mỗi **60 giây** để chống hành vi chụp ảnh màn hình chuyển tiếp cho người khác tập hộ.
  * Kèm theo nút bấm 1 chạm `"Làm mới mã QR"` nếu kết nối mạng của hội viên bị gián đoạn.
* **Cảnh Báo Hết Hạn & Đóng Băng Thẻ (Smart Expiry & Freeze Alert):**
  * Khi thẻ còn dưới 7 ngày hoặc dưới 3 lượt tập: Hiển thị thanh thông báo màu vàng hổ phách mời gia hạn kèm nút `"Gia hạn ngay"` dẫn thẳng tới cổng thanh toán VietQR.
  * Nếu thẻ đang ở trạng thái `FROZEN`: Hiển thị rõ ngày kết thúc bảo lưu và nút `"Yêu cầu mở thẻ sớm"`.

### 2.2. Giai đoạn 2: Khám Phá Gói Tập & Thanh Toán VietQR Tự Động (Catalog & POS Payment - UC03 & UC07)
* **Bảng Giá & So Sánh Gói Tập Minh Bạch (Freemium to Premium Catalog):**
  * Liệt kê danh mục gói tập dạng thẻ trực quan (Gói Ngày, Gói Tháng, Gói VIP, Gói Kèm PT).
  * Hiển thị rõ giá niêm yết, thời hạn sử dụng, số buổi tập gym và số buổi tập PT đi kèm.
* **Thanh Toán VietQR Không Cần Nhập Liệu (Zero-Friction VietQR Checkout):**
  * Khi hội viên chọn `"Đăng ký gói"` $\rightarrow$ Hệ thống gọi API sinh mã thanh toán VietQR chuẩn NAPAS 247 có sẵn số tiền chính xác và nội dung chuyển khoản định danh duy nhất (ví dụ: `GYM SUB 1042`).
  * Hiển thị đồng hồ đếm ngược phiên thanh toán **10 phút (600 giây)** kèm hướng dẫn: `"Mở ứng dụng ngân hàng bất kỳ để quét mã"`.
  * **Cơ chế Kích hoạt Thời gian Thực (Real-time Webhook Activation):**
    * Client lắng nghe kết quả qua SignalR / WebSocket hoặc cơ chế Polling 2 giây/lần.
    * Ngay khi nhận được tín hiệu thanh toán thành công từ Webhook ngân hàng:
      1. Tự động đóng modal QR code ngay lập tức (không cần tải lại trang).
      2. Hiệu ứng chúc mừng tinh tế (Checkmark xanh) kèm thông báo `"Kích hoạt thẻ tập thành công!"`.
      3. Cập nhật ngay trạng thái thẻ sang `ACTIVE` và cộng đủ số lượt tập.
      4. Cung cấp nút xem/tải Hóa đơn điện tử PDF.

### 2.3. Giai đoạn 3: Đặt Lịch Tập Với Huấn Luyện Viên Cá Nhân (PT Booking - UC05)
* **Lưới Chọn Khung Giờ Trực Quan (Interactive Slot Picker):**
  * Hội viên chọn Huấn luyện viên phụ trách (`AssignedPtId`) $\rightarrow$ Hệ thống hiển thị lịch tuần dạng lưới các khung giờ rảnh khả dụng (`PTAvailability`).
  * Các khung giờ đã có người đặt hoặc PT bận được làm mờ (`opacity-40 cursor-not-allowed`) kèm nhãn `"Đã kín"`.
* **Ràng Buộc Quy Tắc Hủy Ca Trước 2 Giờ (Strict 2-Hour Cancellation Rule):**
  * Khi hội viên xem danh sách ca tập đã đặt:
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$\ge 2$ giờ**: Nút `"Hủy ca tập"` hiển thị bình thường. Khi xác nhận, hoàn lại 1 lượt tập `RemainingPtSessions` về thẻ mà không phạt.
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$< 2$ giờ**: Nút `"Hủy ca tập"` đổi sang màu đỏ cảnh báo. Khi click, bật hộp thoại cảnh báo: `"Chú ý: Hủy ca tập trước dưới 2 tiếng sẽ bị tính là một buổi tập đã sử dụng theo nội quy phòng tập. Bạn có chắc chắn muốn hủy?"`.
* **Giao Diện Huấn Luyện Viên (PT View Workflow):**
  * PT đăng nhập xem được ngay: Danh sách ca dạy trong ngày theo dòng thời gian (Timeline), thông tin hội viên của ca đó, số điện thoại liên hệ nhanh, và trạng thái hội viên đã check-in hay chưa.
  * Nút chuyển đổi trạng thái ca tập: `"Bắt đầu buổi tập"` $\rightarrow$ `"Hoàn thành buổi tập"` kèm ô nhập ghi chú buổi tập (bài tập chính, mức tạ, nhận xét thể lực).

### 2.4. Giai đoạn 4: Theo Dõi Sức Khỏe & Tiến Trình InBody (Health & InBody Tracking - UC06)
* **Bảng Chỉ Số Đo Lường Cá Nhân (Personal Metric Dashboard):**
  * Hiển thị kết quả đo InBody mới nhất: Chiều cao, Cân nặng, Tỷ lệ mỡ (`BodyFatPercentage`), Khối lượng cơ (`MuscleMass`), và Chỉ số khối cơ thể tính tự động (`BMI = Weight / (Height / 100)²`).
* **Phân Loại Tình Trạng Thể Thao (Visual BMI Categorization):**
  * Sử dụng thước đo trực quan có dải màu chuẩn (Thiếu cân - Xanh dương, Chuẩn - Xanh lá, Thừa cân - Vàng, Béo phì - Đỏ).
* **Biểu Đồ Tiến Trình Theo Thời Gian (Progress Chart):**
  * Biểu đồ đường phẳng (Line chart) thể hiện xu hướng giảm mỡ, tăng cơ qua các lần đo lịch sử giúp hội viên và PT cùng đánh giá hiệu quả huấn luyện.

---

## 3. QUY CHUẨN TRẢI NGHIỆM BAN QUẢN LÝ & NHÂN VIÊN (ADMIN & STAFF WORKFLOW - WINUI 3 DESKTOP)

Ứng dụng WinUI 3 Desktop (`GymManagement.Desktop`) được thiết kế theo tư duy **Desktop-First MVVM**, phục vụ màn hình quầy lễ tân và văn phòng điều hành với tốc độ xử lý tức thì, mật độ thông tin cao và giao diện phẳng Fluent Design.

### 3.1. Phân Hệ Quầy Điểm Danh & Cổng Tự Động (Turnstile Check-in Counter - UC04)
* **Tốc Độ Xử Lý Điểm Danh < 1 Giây:**
  * Luồng check-in là luồng quan trọng bậc nhất của phòng gym, không được phép trễ nải gây ùn tắc tại cửa ra vào.
* **Giao Diện Màn Hình Điểm Danh Quầy Lễ Tân (Chia 2 Cột Rõ Ràng):**
  * **Cột Trái (Camera Scanner / Thiết bị đọc mã):** Luồng quét mã QR/mã vạch trực tiếp từ camera hoặc máy quét chuyên dụng USB, ô nhập mã thẻ luôn ở trạng thái tự động Focus để sẵn sàng nhận tín hiệu máy quét không cần click chuột.
  * **Cột Phải (Kết Quả Xác Minh Hội Viên Tức Thì):**
    * Khi mã thẻ được quét hợp lệ: Màn hình nháy viền xanh lá, phát âm thanh báo thành công ("Beep" ngắn), hiển thị to rõ: Ảnh đại diện hội viên, Họ tên, Gói tập, Số buổi còn lại (`RemainingCheckInSessions`), và tín hiệu `"ĐÃ MỞ CỔNG TURNSTILE"`.
    * Khi thẻ hết hạn hoặc không hợp lệ: Màn hình đổi nền cảnh báo đỏ, phát âm thanh cảnh báo lỗi ("Buzz"), hiển thị nguyên nhân chi tiết (ví dụ: `"Thẻ đã hết hạn từ ngày 15/10/2026"`, `"Gói tập đã hết lượt vào phòng"`, `"Thẻ đang bị đóng băng"`).
* **Nút Ghi Đè Thủ Công (Manual Receptionist Override):**
  * Trường hợp hội viên quên thẻ hoặc cổng turnstile kẹt cơ khí, lễ tân có nút `"Cho phép vào thủ công (Manual Check-in)"` kèm bắt buộc chọn lý do để lưu vào vết kiểm toán (`CheckInLog`).

### 3.2. Phân Hệ Thu Ngân, Bán Gói & Xuất Hóa Đơn (POS & Billing Workflow - UC07)
* **Tối Ưu Hóa Quy Trình Tạo Hóa Đơn (3-Step Fast Checkout):**
  1. **Bước 1 - Tìm / Chọn Hội viên:** Ô tìm kiếm thông minh hỗ trợ gõ tên, số điện thoại hoặc mã thẻ có gợi ý tự động (Auto-complete dropdown) trong $< 300\text{ms}$.
  2. **Bước 2 - Chọn Gói Tập / Dịch Vụ:** Lưới các gói tập kèm bộ lọc nhanh theo nhóm (Gym, Yoga, PT 1:1, Gói Combo). Cho phép nhập mã giảm giá (`DiscountAmount`) nếu có.
  3. **Bước 3 - Chọn Phương Thức Thanh Toán:**
     * Nút bấm lớn chuyển đổi nhanh: `Tiền mặt (Cash)`, `Chuyển khoản VietQR`, `Thẻ tín dụng / POS Card`.
* **Cơ Chế Khóa An Toàn Khi Chờ Thanh Toán VietQR:**
  * Khi chọn VietQR: Màn hình hiển thị cửa sổ bật lên (Dialog) có mã QR động lớn và tổng số tiền thanh toán.
  * Nút `"Xác nhận thu tiền thủ công"` bị vô hiệu hóa trong 10 giây đầu để khuyến khích chờ hệ thống đối soát tự động qua Webhook, tránh nhân viên bấm nhầm khi tiền chưa vào tài khoản.
* **Tự Động Sinh Hóa Đơn & Mở Khóa Dịch Vụ (ACID Integrity):**
  * Ngay khi giao dịch hoàn tất: Tạo bản ghi `Invoice`, cập nhật `MemberSubscription` mới hoặc gia hạn thêm ngày/lượt cho gói cũ trong cùng 1 transaction CSDL.
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
  * Hỗ trợ tìm kiếm, phân trang mượt mà, lọc theo trạng thái thẻ (`ACTIVE`, `FROZEN`, `EXPIRED`).
  * Nút hành động nhanh trên từng dòng: `Xem chi tiết`, `Gia hạn thẻ`, `Bảo lưu thẻ (Freeze)`, `Nhập InBody`.
* **Quy Trình Bảo Lưu Thẻ (Freeze Subscription Dialog):**
  * Nhân viên chọn Ngày bắt đầu bảo lưu (`FreezeStartDate`) và Ngày kết thúc (`FreezeEndDate`).
  * Hệ thống tự động tính số ngày bảo lưu và hiển thị ngày hết hạn mới dự kiến:
    $$\text{EndDate mới} = \text{EndDate cũ} + \text{Số ngày bảo lưu}$$
  * Có cảnh báo rõ ràng nếu số ngày bảo lưu vượt quá chính sách gói tập quy định.
* **Biểu Mẫu Nhập Chỉ Số InBody Phòng Vệ (Defensive InBody Form):**
  * Ràng buộc giới hạn hợp lệ đầu vào để tránh gõ nhầm dữ liệu y tế:
    * Chiều cao: $100\text{cm} \le \text{Height} \le 250\text{cm}$.
    * Cân nặng: $30\text{kg} \le \text{Weight} \le 250\text{kg}$.
    * Tỷ lệ mỡ: $3\% \le \text{BodyFat} \le 60\%$.
  * Tự động tính và hiển thị ngay chỉ số BMI khi người dùng vừa gõ xong Chiều cao và Cân nặng trước khi bấm nút Lưu.

### 3.5. Báo Cáo Doanh Thu, Vận Hành & Nhật Ký Kiểm Toán (Analytics & Audit Trail - UC08 & UC10)
* **Thẻ Chỉ Số KPI Trực Quan (Executive KPI Cards):**
  * 4 thẻ đầu trang: `Doanh thu hôm nay (VNĐ)`, `Tổng lượt check-in hôm nay`, `Số hội viên mới trong tháng`, `Số ca PT hoàn thành`.
* **Biểu Đồ Xu Hướng Giờ Cao Điểm (Peak Hours Density):**
  * Biểu đồ cột thể hiện phân bố lượt vào phòng theo từng khung giờ trong ngày (giúp ban quản lý nhận biết khung giờ cao điểm 17:00 - 20:00 để bố trí nhân sự tạ và máy móc).
* **Nhật Ký Kiểm Toán Không Thể Sửa (Immutable Audit Trail Log):**
  * Mọi thao tác nhạy cảm: Hủy hóa đơn, Cho check-in thủ công, Chỉnh sửa thông tin hội viên đều được lưu vết chi tiết kèm User ID của nhân viên, thời điểm thực hiện và địa chỉ IP.

---

## 4. MA TRẬN ĐỐI CHIẾU TRẢI NGHIỆM: HỘI VIÊN / PT (WEB) VS. NHÂN VIÊN / ADMIN (DESKTOP)

| Thành Phần Nghiệp Vụ | Trải Nghiệm Hội Viên & PT (Blazor Server - Mobile) | Trải Nghiệm Lễ Tân & Quản Trị (WinUI 3 - Desktop) |
| :--- | :--- | :--- |
| **Điểm danh & Ra vào (UC04)** | Mở Thẻ điện tử trên điện thoại, hiển thị mã QR động tự đổi mỗi 60 giây để quét qua turnstile. | Màn hình quầy lễ tân tiếp nhận quét trong $<1\text{s}$, đối chiếu ảnh chân dung, hiển thị trạng thái xanh/đỏ và âm thanh phản hồi. |
| **Gói tập & Thanh toán (UC03, UC07)** | Tự xem danh mục gói tập, chọn mua và quét mã VietQR tự động kích hoạt thẻ trong 3 giây. | Thu ngân POS chọn nhanh hội viên, gói tập, xuất hóa đơn in nhiệt, kiểm tra tiền về qua Webhook hoặc xác nhận thu tiền mặt. |
| **Lịch Huấn luyện viên PT (UC05)** | Xem lịch rảnh của PT trên lưới tuần, đặt ca tập chỉ với 2 click; cảnh báo phạt nếu hủy ca $<2$ tiếng. | PT xem danh sách học viên theo Timeline ca dạy; Quản trị viên xếp lịch PT, phát hiện và chặn đứng xung đột trùng giờ. |
| **Chỉ số Sức khỏe InBody (UC06)** | Xem biểu đồ trực quan biến thiên cân nặng, tỷ lệ mỡ và khối lượng cơ qua từng mốc thời gian. | Nhân viên đo nhập số liệu trên form có kiểm tra biên an toàn (Guard clauses), tự động tính chỉ số BMI ngay tức thì. |
| **Quản lý & Báo cáo (UC08, UC10)** | Nhận thông báo tự động khi thẻ sắp hết hạn hoặc đến giờ hẹn tập cùng huấn luyện viên. | Theo dõi Dashboard KPI doanh thu thời gian thực, xem mật độ giờ cao điểm, trích xuất báo cáo Excel/CSV và sổ kiểm toán. |


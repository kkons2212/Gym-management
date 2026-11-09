# VISUAL USER INTERFACE RULES (UI SYSTEM RULES)
## Hệ Thống Quản Lý Phòng Tập Thể Thao (Gym Management System)
### WinUI 3 Desktop (Admin) & Blazor Server (Member/PT Portal)

> **Mục đích tài liệu:** Tài liệu này quy định toàn bộ **Quy chuẩn Giao diện Thị giác (UI - Visual User Interface)**, Design Tokens, cấu trúc Component, bố cục lưới và hệ thống màu sắc dành cho các lập trình viên và AI Agent khi xây dựng mã nguồn XAML (`WinUI 3 Desktop`) và HTML/CSS/Tailwind/Razor (`Blazor Server Web`).
> **Triết lý Thiết kế Cốt lõi (Core Visual Philosophy):** Tối giản, Đậm chất Thể thao Năng động (Athletic Modernism), Tương phản cao chuẩn WCAG AA, Màu sắc chuẩn ngữ nghĩa, **độc tôn giao diện tối (Dark Mode 100%)** — **tuyệt đối không sử dụng hiệu ứng viền/chấm nhấp nháy, không dùng âm thanh gây phiền toái, và không hiển thị mã Use Case kỹ thuật `(UC...)` lên bất kỳ thành phần giao diện người dùng nào**.

---

## 1. HỆ THỐNG DESIGN TOKENS & NỀN TẢNG THỊ GIÁC (100% DARK MODE)

Mọi mã nguồn XAML, CSS hoặc thành phần giao diện bắt buộc tuân theo bảng định danh trực quan dưới đây:

### 1.1. Bảng Màu Chuẩn Độc Tôn Dark Mode (Color Palette & Tokens)

Hệ thống sử dụng duy nhất giao diện tối (Dark Mode 100%) đồng nhất trên cả WinUI 3 Desktop và Blazor Web. Bảng màu được chuẩn hóa tỷ lệ tương phản cao ($\ge 4.5:1$ theo tiêu chuẩn WCAG AA) trên nền tối, tách biệt rõ ràng giữa màu chữ/nhãn và màu nền khối:

| Tên Token (Role) | Giá trị Màu Chữ / Nhãn | Giá trị Nền Khối / Badge | Ràng Buộc Ứng Dụng Thực Tế |
| :--- | :--- | :--- | :--- |
| **`bg-canvas`** | — | `#0F172A` (Slate 900) | Màu nền gốc của toàn bộ ứng dụng. |
| **`bg-surface`** | — | `#1E293B` (Slate 800) | Khối thông tin gói tập, giỏ hàng POS, bảng danh sách. |
| **`border-subtle`** | — | `#334155` (Slate 700) | Đường viền thanh lịch `1px solid` phân tách các khối nội dung. |
| **`text-primary`** | `#F8FAFC` (Slate 50) | — | Văn bản chính, tiêu đề, họ tên hội viên, số liệu quan trọng. |
| **`text-secondary`** | `#94A3B8` (Slate 400) | — | Nhãn mô tả, ngày giờ, số điện thoại, ghi chú phụ. |
| **`accent-primary`** | `#0EA5E9` (Sky 500) | `#0284C7` (Sky 600) | **Đúng 1 màu Accent duy nhất** cho nút CTA chính: Quét QR, Thanh toán, Đặt lịch, Lưu thông tin. |
| **`status-success`** | `#4ADE80` (Green 400) | `#14532D` (Green 900/40) viền `#15803D` | Check-in hợp lệ, Mở cổng Turnstile, Gói tập `ACTIVE`, Hóa đơn `PAID`. |
| **`status-warning`** | `#FBBF24` (Amber 400) | `#78350F` (Amber 900/40) viền `#B45309` | Gói tập sắp hết hạn ($\le 7$ ngày), Gói tập bảo lưu `FROZEN`, Cảnh báo hủy ca $<2\text{h}$. |
| **`status-danger`** | `#F87171` (Red 400) | `#7F1D1D` (Red 900/40) viền `#B91C1C` | Gói tập hết hạn `EXPIRED`, Check-in thất bại, Trùng lịch dạy PT, Nút Hủy ca. |
| **`status-pt`** | `#A78BFA` (Purple 400) | `#4C1D95` (Purple 900/40) viền `#6D28D9` | Ca tập PT đã đặt (`PTTrainingSession`) và chỉ số InBody. |
| **`pt-slot-open`** | `#CBD5E1` (Slate 300) | `#1E293B` (Slate 800) viền `#475569` | Khung giờ rảnh PT (`PTAvailability`) — **dùng màu trung tính, không dùng màu xanh lá**. |

> [!IMPORTANT]
> **Quy định về Chip Trạng Thái & Nhãn:** Giữ nhãn trạng thái ở dạng văn bản thuần túy (ví dụ: `Đang hoạt động`, `Đang bảo lưu`, `Đã hết hạn`), **không thêm biểu tượng icon** vào chip trạng thái để giữ giao diện phẳng, tối giản.

### 1.2. Kiểu Chữ (Typography) & Tỷ Lệ Bố Cục (Scale)

* **Font chữ tiêu chuẩn:**
  * **WinUI 3 Desktop:** `Segoe UI Variable Display` (Tiêu đề) và `Segoe UI Variable Text` (Nội dung thông thường).
  * **Blazor Server Web:** `Inter`, `-apple-system`, `BlinkMacSystemFont`, `Segoe UI`, `sans-serif`.
* **Thang kích thước chữ (Type Scale):**
  * `Display 1` ($28\text{px} - 32\text{px}$, Bold): Số tiền thanh toán POS, Số lượt tập còn lại của gói tập, Đồng hồ đếm ngược.
  * `Heading 2` ($20\text{px} - 24\text{px}$, SemiBold): Tiêu đề trang, Tên hội viên tại màn hình Check-in, Tên gói tập.
  * `Body Text` ($14\text{px} - 16\text{px}$, Regular): Nội dung bảng dữ liệu, nhãn form, thông số chi tiết.
  * `Caption / Micro` ($11\text{px} - 12\text{px}$, Medium): Nhãn trạng thái, ngày hết hạn, mã định danh gói tập.
* **Quy chuẩn bo góc (Border Radius):**
  * Hộp thoại (Modal / Dialog) & Khối giao diện (Card): `rounded-xl` ($12\text{px}$).
  * Nút bấm (Button) & Ô nhập liệu (Input field): `rounded-lg` ($8\text{px}$).
  * Huy hiệu trạng thái (Status Badge / Chip): `rounded-full` ($9999\text{px}$).
* **Quy chuẩn đổ bóng (Shadows):**
  * Tối giản: Ưu tiên dùng đường viền `1px solid` (`border-subtle`) thay cho hiệu ứng bóng đổ mờ ảo đậm nét để giữ giao diện sạch và hiện đại.
* **Quy tắc hiển thị nhãn kỹ thuật:**
  * **Tuyệt đối không hiển thị mã Use Case kỹ thuật `(UC...)` lên bất kỳ thành phần giao diện người dùng nào** (tiêu đề trang, menu, nút bấm, nhãn form, popup). Mã Use Case chỉ dùng làm quy chuẩn đối chiếu nội bộ trong tài liệu kỹ thuật và mã nguồn.

---

## 2. QUY CHUẨN THÀNH PHẦN GIAO DIỆN HỘI VIÊN & PT (BLAZOR SERVER - MOBILE-FIRST)

Giao diện Web Portal (`GymManagement.Web`) được tối ưu hóa cho màn hình di động (Viewport chuẩn $375\text{px} - 430\text{px}$, canh giữa với `max-w-md mx-auto` khi xem trên máy tính).

### 2.1. Khối Thông Tin Gói Tập Số (`DigitalSubscriptionWidget.razor`)
* **Bố cục Khối Gói Tập Số (Digital Subscription Widget):**
  * Thiết kế dạng khối thẻ thông tin hiện đại trên nền tối thể thao sang trọng (`bg-slate-800`), viền kim loại mảnh `border border-slate-700`, bo góc `rounded-xl`.
  * **Góc trên bên trái:** Logo phòng gym + Tên gói tập hiện tại (`PackagePlan.Name`).
  * **Góc trên bên phải:** Chip trạng thái dạng nhãn chữ tĩnh (không nhấp nháy, không icon):
    * `ACTIVE`: Nhãn xanh lá `Đang hoạt động` (`text-green-400 bg-green-900/40 border border-green-700`).
    * `FROZEN`: Nhãn vàng hổ phách `Đang bảo lưu` (`text-amber-400 bg-amber-900/40 border border-amber-700`).
    * `EXPIRED`: Nhãn đỏ `Đã hết hạn` (`text-red-400 bg-red-900/40 border border-red-700`).
* **Hiển thị Mã QR Điểm Danh 1-Chạm:**
  * Mã QR điểm danh được tích hợp trực tiếp hoặc mở nhanh 1-chạm qua nút `"Mở mã QR điểm danh"` lớn và trực quan.
  * QR hiển thị ở kích thước tối thiểu `240px × 240px`, co giãn theo viewport nhưng không vượt quá `min(80vw, 320px)`.
  * Đặt QR đen trên nền trắng đặc, có vùng trống (quiet zone) tối thiểu 4 module xung quanh; không phủ logo, chữ, họa tiết, bo góc hoặc hiệu ứng lên vùng mã.
  * Hiển thị họ tên và mã hội viên bên ngoài vùng quét để đối chiếu. Không đưa thông tin nhạy cảm vào nội dung chuỗi QR.
  * Bên dưới QR, hiển thị thời gian còn hiệu lực và thời điểm tự làm mới mỗi 60 giây; có nút `"Làm mới mã QR"`.
  * Khi QR hết hạn hoặc gói tập không còn hợp lệ, mã cũ được ẩn đi và thay bằng thông báo giải thích rõ ràng.
* **Hàng Thông Số Buổi Tập (Session Counters):**
  * Chia 2 cột đều nhau trên khối thông tin gói tập:
    * Cột 1: `"Lượt Gym Còn Lại"` $\rightarrow$ Số lớn nổi bật (`text-2xl font-bold text-sky-400`).
    * Cột 2: `"Buổi PT Còn Lại"` $\rightarrow$ Số lớn nổi bật (`text-2xl font-bold text-purple-400`).
  * Chân khối hiển thị ngày hết hạn: `"Hết hạn: 31/12/2026"` (`text-xs text-slate-400`).

### 2.2. Danh Mục Gói Tập & Modal Thanh Toán VietQR (`PackageCatalog.razor`, `VietQrModal.razor`)
* **Lưới Danh Mục Gói Tập (Package Plan Cards):**
  * Trình bày dạng danh sách thẻ cuộn dọc (hoặc lưới 2 cột trên máy tính bảng).
  * Mỗi thẻ gói tập có: Tên gói, Giá tiền lớn kèm đơn vị VNĐ, Thời hạn (ví dụ: `30 ngày`, `90 ngày`), Số buổi tập gym, Số buổi PT đi kèm.
  * Nút CTA: `"Đăng Ký Gói Này"` màu xanh `bg-sky-500 hover:bg-sky-400 text-white font-semibold h-11 w-full rounded-lg`.
* **Modal Thanh Toán VietQR Tự Động (VietQR Pop-up Modal):**
  * Nền slate đậm, bo góc `rounded-xl`, phủ backdrop làm mờ xung quanh.
  * **Mã QR VietQR chuẩn NAPAS 247:** Kích thước cố định $220\text{px} \times 220\text{px}$, hiển thị logo ngân hàng và logo Napas ở giữa.
  * **Hộp Thông Tin Chuyển Khoản Minh Bạch:**
    * Số tiền: Hiển thị to rõ màu xanh lá (ví dụ: `800,000 đ`).
    * Nội dung chuyển khoản: Ô văn bản font đơn cách (`font-mono tracking-wider bg-slate-700 p-2 rounded`) kèm nút 1-chạm `"Sao chép"`.
  * **Thanh Đếm Ngược:** Hiển thị thời gian hiệu lực của mã QR `09:59` đếm lùi, khi hết giờ nút chuyển sang `"Tạo lại mã thanh toán"`.
  * **Nút Giả Lập Thanh Toán (Sandbox Simulation):**
    * Dưới mã QR có nút `"Giả lập thanh toán thành công"` phục vụ kiểm thử luồng tích hợp.
    * **Ràng buộc biến môi trường:** Nút này chỉ được hiển thị khi biến môi trường `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`.
    * Trong môi trường Production (`VIETQR_ENABLE_SIMULATION=false`), nút này tự động ẩn hoàn toàn khỏi giao diện người dùng.
    * Khi nhấn, nút tự động khóa (disable) ngay lập tức để tránh gửi lệnh lặp, hiển thị trạng thái `"Đang xử lý mô phỏng..."` và chỉ kích hoạt gói sau khi máy chủ xác nhận kết quả.

### 2.3. Lịch Huấn Luyện Viên & Hộp Thoại Đặt Ca (`PtCalendarView.razor`, `BookingModal.razor`)
* **Bộ Chọn Ngày Dạng Băng Chuyền Ngang (Horizontal Date Picker):**
  * Cho phép bấm chọn nhanh từng ngày trong tuần (Thứ 2 đến Chủ nhật) kèm ngày tháng rõ ràng. Ngày được chọn sáng nổi bật với nền xanh `bg-sky-500 text-white`.
* **Lưới Khung Giờ (Time Slot Grid):**
  * Các khung giờ từ 06:00 đến 21:00 trình bày dạng nút bấm phẳng 2 cột hoặc 3 cột:
    * Khung giờ rảnh: Nền Slate 800 viền Slate 600 (`bg-slate-800 text-slate-300 border border-slate-600 hover:bg-slate-700`). **Tuyệt đối không dùng màu xanh lá thành công cho slot rảnh**.
    * Khung giờ đã kín lịch: Nền Slate 900 tối (`bg-slate-900/60 text-slate-600 border border-slate-800 cursor-not-allowed line-through`).
* **Hộp Thoại Cảnh Báo Hủy Ca Muộn (< 2 Giờ):**
  * Hộp thoại bật lên cảnh báo viền đỏ/hổ phách:
    * Tiêu đề: `"Xác Nhận Hủy Buổi Tập"`.
    * Nội dung cảnh báo: `"Thời gian hủy chỉ còn [X phút]. Theo quy định phòng tập, hủy ca trước dưới 2 giờ sẽ KHÔNG ĐƯỢC HOÀN LƯỢT TẬP. Bạn có chắc chắn muốn hủy ca này không?"`.
    * 2 Nút hành động: `"Giữ Lại Ca Tập"` (Nút chính) và `"Xác Nhận Hủy"` (Nút phụ viền đỏ).

### 2.4. Bảng Tiến Trình Sức Khỏe InBody (`InBodyDashboard.razor`)
* **Hàng 4 Thẻ Chỉ Số Sức Khỏe (Metric Cards Grid - Khớp Bảng MemberBodyMetric):**
  * Cân nặng (`WeightKg` - kg), Chiều cao (`HeightCm` - cm), Tỷ lệ mỡ (`BodyFatPercentage` - %), Khối lượng cơ xương (`SkeletalMuscleMass` - kg).
* **Thanh Trực Quan Hóa Phân Loại BMI (BMI Visual Gauge):**
  * Thanh dải màu ngang chia 4 phân vùng: Xanh dương (< 18.5: Gầy), Xanh lá (18.5 - 24.9: Chuẩn), Vàng (25 - 29.9: Thừa cân), Đỏ ($\ge 30$: Béo phì).
  * Con trỏ kim vị trí chỉ chính xác số đo `Bmi` của hội viên kèm nhãn trạng thái lớn.
* **Biểu Đồ Tiến Trình Theo Lịch Sử Đo (Line Chart):**
  * Biểu đồ đường mượt mà hiển thị biến thiên cân nặng (`WeightKg`) và tỷ lệ mỡ (`BodyFatPercentage`) qua các lần đo lịch sử (`measured_at`).

---

## 3. QUY CHUẨN THÀNH PHẦN GIAO DIỆN QUẢN TRỊ (WINUI 3 DESKTOP - DÀNH CHO ADMIN)

Giao diện ứng dụng WinUI 3 Desktop (`GymManagement.Desktop`) được thiết kế theo tư duy **Desktop-First MVVM**, phục vụ màn hình máy tính làm việc của Admin với mật độ thông tin cao, thao tác bàn phím nhạy và giao diện Fluent Design phẳng độc tôn Dark Mode.

### 3.1. Khung Giao Diện Chính (App Shell & Navigation - `MainWindow.xaml`)
* **Cột Điều Hướng Trái (NavigationView):**
  * Độ rộng mở rộng `w-64` ($256\text{px}$), hỗ trợ thu gọn về dạng icon (`Compact Mode` $48\text{px}$).
  * Bố cục mục chức năng của Admin (không hiển thị mã UC):
    1. `Quầy Điểm Danh & Cổng Tự Động`
    2. `Bán Gói & Thanh Toán POS` 
    3. `Quản Lý Hội Viên & Gói Tập` 
    4. `Lịch Biểu Huấn Luyện Viên PT`
    5. `Đo & Quản Lý InBody`
    6. `Báo Cáo & Thống Kê Doanh Thu`
    7. `Cấu Hình & Sổ Kiểm Toán Hệ Thống`
* **Thanh Tiêu Đề Trên Cùng (App TitleBar):**
  * Tích hợp thanh tìm kiếm hội viên toàn cục nhanh, thông tin Admin đang đăng nhập kèm Badge vai trò (`Admin`). **Độc tôn Dark Mode, không có nút chuyển đổi Light/Dark mode**.

### 3.2. Màn Hình Quầy Điểm Danh Cửa Ra Vào (`CheckInCounterPage.xaml`)
* **Bố Cục 2 Cột Đối Xứng:**
  * **Cột Trái ($40\%$ Chiều rộng) - Khu vực Quét Mã QR:**
    * Hệ thống không dùng thẻ vật lý hay đầu đọc thẻ USB. Hội viên xuất trình mã QR trên thiết bị di động.
    * Nút bấm hành động lớn: `[Bật Camera Quét Mã QR]` kích hoạt webcam quầy để quét mã QR từ điện thoại hội viên theo thời gian thực.
    * Kèm theo ô tìm kiếm nhanh Số điện thoại (`User.Phone`).
    * Nút thao tác nhanh: `"Cho vào thủ công (Override)"` (kèm chọn lý do) và `"Mở cổng kiểm tra"`.
  * **Cột Phải ($60\%$ Chiều rộng) - Bảng Hiển Thị Kết Quả Điểm Danh Từ Backend:**
    * Khi chưa quét: Hiển thị biểu tượng camera chờ quét với thông báo *"Sẵn sàng tiếp nhận hội viên..."*.
    * Toàn bộ việc kiểm tra trạng thái gói tập (`ACTIVE`, còn hạn, còn lượt) được thực hiện bên **Backend**; Desktop hiển thị kết quả từ API phản hồi.
    * **Hoàn toàn không sử dụng âm thanh phản hồi** (không có tiếng "Beep" hay "Buzz"). Mọi phản hồi đều thể hiện qua thông báo văn bản trực quan.
    * **Hoàn toàn không yêu cầu ảnh chân dung** (không hiển thị khung ảnh bo tròn).
    * **Nội dung kết quả điểm danh & log quầy tập trung tinh gọn đúng 2 thông tin cốt lõi:**
      1. **Mã hội viên (`UserName`)**
      2. **Họ tên hội viên (`FullName`)**
    * **Khi Check-in Hợp Lệ (Backend trả về Success):**
      * Chuyển sang banner xanh tĩnh: `"CHECK-IN THÀNH CÔNG • CỔNG MỞ"` (`bg-green-600 text-white text-xl font-bold p-3 rounded-lg`). **Không sử dụng hiệu ứng nhấp nháy viền**.
      * Thông tin chi tiết: Mã hội viên (`UserName`) và Họ tên (`FullName`) in hoa to rõ (`text-2xl font-bold`).
      * Dòng thông tin gói tập & số lượt còn lại: `"Còn 18 lượt tập gym"` / `"Còn 5 buổi tập PT"`.
    * **Khi Check-in Thất Bại (Backend trả về Lỗi):**
      * Chuyển sang banner đỏ tĩnh: `"TỪ CHỐI CHECK-IN • CỔNG KHÓA"` (`bg-red-600 text-white text-xl font-bold p-3 rounded-lg`). **Không sử dụng hiệu ứng nhấp nháy viền**.
      * Thông tin chi tiết: Mã hội viên (`UserName`) và Họ tên (`FullName`).
      * Lý do từ chối do Backend trả về in chữ đậm rõ nét: *"Gói tập trong tài khoản đã hết hạn"* hoặc *"Gói tập đã hết lượt vào phòng"*.

### 3.3. Màn Hình POS Bán Gói & Xuất Hóa Đơn (`PosBillingPage.xaml`)
* **Bố Cục 2 Khối Thao Tác (Khớp 100% Thuộc Tính Bảng Invoice & PackagePlan):**
  * **Khối Danh Mục & Sản Phẩm (Bên Trái):**
    * Ô tìm kiếm gói tập nhanh theo tên hoặc mã (`PackagePlan.PlanId`, `PackagePlan.Name`).
    * Lưới thẻ gói tập (Card Grid) hiển thị đơn giá niêm yết (`PackagePlan.Price`), thời hạn (`DurationDays`), số buổi gym (`MaxCheckInSessions`), số buổi PT (`TotalPtSessions`). Click 1 chạm để nạp vào hóa đơn.
  * **Khối Chi Tiết Hóa Đơn & Thu Tiền (Bên Phải):**
    * Thông tin người mua: Ô tìm kiếm hội viên gợi ý nhanh liên kết `User.UserName`.
    * Bảng chi tiết hóa đơn thanh toán:
      * Mã gói tập: `PackagePlan.PlanId`
      * Tên gói tập: `PackagePlan.Name`
      * Thời hạn & Quyền lợi: `DurationDays` ngày, `MaxCheckInSessions` lượt gym, `TotalPtSessions` buổi PT
      * Đơn giá gói: `PackagePlan.Price` (định dạng tiền tệ VNĐ)
      * Tạm tính: `Invoice.SubtotalAmount` (gán trực tiếp bằng `PackagePlan.Price`)
      * Loại giao dịch: `Invoice.TransactionType` (`SUBSCRIPTION_PURCHASE` hoặc `SUBSCRIPTION_RENEWAL`)
    * Bảng tổng kết thanh toán:
      * **Tổng thanh toán (`Invoice.TotalAmount`):** Đồng bộ chính xác với tiền niêm yết của gói (`TotalAmount = CAST(SubtotalAmount AS INT)`).
      * **Ràng buộc nghiệp vụ CSDL:** Tuyệt đối không có ô nhập mã giảm giá, voucher hay dòng trừ chiết khấu vì bảng `Invoice` trong CSDL không có cột lưu trữ giảm giá.
      * Mã hóa đơn: `Invoice.InvoiceCode` sinh tự động duy nhất (ví dụ: `INV-20261108-001`).
      * Phương thức thanh toán: `Invoice.PaymentMethod` lưu các giá trị chuẩn (`CASH`, `VIETQR`, `CREDIT_CARD`).
      * Ngày ghi nhận thanh toán: `Invoice.paid_at` (gán ngày hiện tại khi giao dịch thành công).
      * Trường hợp hoàn tiền (nếu có): Hiển thị trường `Invoice.RefundAmount` và lý do `Invoice.RefundReason`.
* **Bộ Nút Chọn Phương Thức Thanh Toán Lớn:**
  * 3 Nút bấm kích thước lớn dễ chạm trên màn hình cảm ứng:
    * `[Tiền Mặt]`
    * `[Chuyển Khoản VietQR]` (Khuyên dùng)
    * `[Thanh Toán Thẻ Ngân Hàng (POS Terminal)]`
* **Cửa Sổ Bật Lên Thanh Toán VietQR (Dialog):**
  * Mở hộp thoại `ContentDialog` chứa mã VietQR động lớn ở trung tâm.
  * Khi chạy môi trường phát triển/kiểm thử, hiển thị nút `"Giả lập thanh toán thành công"` dưới mã QR khi biến môi trường `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`.
  * Khóa nút giả lập trong lúc gửi và sau khi giao dịch đã được xác nhận.
  * Trong môi trường Production (`VIETQR_ENABLE_SIMULATION=false`), nút này tự động ẩn hoàn toàn.
  * **Không khóa cưỡng bức thời gian:** Thu ngân được bấm nút xác nhận thu tiền thủ công ngay khi có bằng chứng chuyển khoản thành công, đi kèm hộp thoại xác nhận nhanh 2 bước để phòng ngừa bấm nhầm.

### 3.4. Bảng Lịch Huấn Luyện Viên & Hàng Rào Chống Trùng Lịch (`PtSchedulePage.xaml`)
* **Lưới Thời Gian Tuần (Weekly Timeline Timetable):**
  * Trục tung: Danh sách giờ từ 06:00 đến 22:00.
  * Trục hoành: 7 Cột biểu diễn các ngày trong tuần (hoặc từng cột đại diện cho 1 PT khi ở chế độ xem theo ngày).
  * Ca tập được biểu diễn bằng các khối chữ nhật màu tím `#A78BFA` có tên hội viên, số điện thoại và thời gian bắt đầu - kết thúc.
* **Cảnh Báo Xung Đột Trùng Lịch (Collision Guardrail Border):**
  * Nếu người dùng kéo thả hoặc xếp lịch mới đè lên khung giờ đã có ca dạy của huấn luyện viên:
    * Khối lịch lập tức chuyển sang viền đỏ rực `#F87171` kèm dòng thông báo cảnh báo rõ ràng.
    * Tooltip nổi lên giải thích chi tiết: `"Xung đột: Huấn luyện viên đã có lịch dạy ca [Tên ca khác] trong khung giờ này!"`.
    * Nút Lưu lịch bị vô hiệu hóa cho đến khi chuyển sang khung giờ hợp lệ.

### 3.5. Bảng Quản Lý Hội Viên & Hộp Thoại InBody (`MembersPage.xaml`, `InBodyDialog.xaml`)
* **DataGrid Danh Sách Hội Viên:**
  * Bảng dữ liệu phẳng, hỗ trợ cuộn ảo (Virtualization) mượt mà với hàng ngàn hội viên.
  * Cột trạng thái sử dụng các Chip màu chuẩn dạng nhãn chữ (không icon):
    * `ACTIVE`: Chip xanh lá `Đang hoạt động`.
    * `FROZEN`: Chip vàng hổ phách `Đang bảo lưu`.
    * `EXPIRED`: Chip đỏ `Đã hết hạn`.
  * Cột thao tác nhanh: Nút Xem, Gia hạn, Bảo lưu, Đo InBody.
* **Hộp Thoại Nhập Số Đo InBody (`InBodyDialog.xaml`):**
  * Giao diện nhập liệu dạng lưới 2 cột khớp các thuộc tính bảng `MemberBodyMetric`:
    * Cột 1: Chiều cao (`HeightCm` - cm), Cân nặng (`WeightKg` - kg), Tỷ lệ mỡ (`BodyFatPercentage` - %), Khối lượng cơ xương (`SkeletalMuscleMass` - kg), Ghi chú (`Notes`).
    * Cột 2: Bảng đồng hồ tính `Bmi` tự động (`Bmi = WeightKg / ((HeightCm / 100) * (HeightCm / 100))`) hiển thị ngay khi nhập xong chiều cao & cân nặng, kèm đánh giá thể trạng tức thì (Bình thường / Thừa cân / Béo phì). Tự động gán `UserName`, `measured_at` và `RecordedByPtId`.

### 3.6. Bảng Điều Khiển Thống Kê Doanh Thu (`DashboardPage.xaml`)
* **4 Thẻ Chỉ Số KPI Phẳng (Flat KPI Cards):**
  * Nền slate đậm, viền mảnh `border-subtle`, số liệu in đậm cỡ lớn (`text-3xl font-bold`).
  * Chỉ số hiển thị: `Doanh Thu Hôm Nay (VNĐ)`, `Lượt Check-in Hôm Nay`, `Gói Tập Mới Trong Tháng`, `Số Giờ PT Đã Dạy`.
* **Biểu Đồ Doanh Thu & Giờ Cao Điểm:**
  * Biểu đồ cột biểu diễn phân bố lượt khách vào phòng tập theo từng khung giờ trong ngày (nhận diện khung giờ vàng 17h - 20h).
  * Biểu đồ phân bố tỷ lệ doanh thu theo loại gói tập (Gói Tháng, Gói Năm, Gói PT).

---

## 4. MA TRẬN ĐỐI CHIẾU THÀNH PHẦN UI: WINUI 3 DESKTOP VS. BLAZOR WEB

| Thành Phần Giao Diện | Ứng Dụng WinUI 3 Desktop (Quản Trị Admin) | Ứng Dụng Blazor Server Web (Hội Viên / PT) |
| :--- | :--- | :--- |
| **Gói Tập Hội Viên & Điểm Danh** | Màn hình quét 2 cột, kết quả hiển thị tinh gọn **Mã hội viên + Họ tên**, banner tĩnh xanh/đỏ (không âm thanh, không ảnh). | Khối thông tin gói tập số di động, mã QR động tự làm mới mỗi 60s, chip đếm số buổi còn lại. |
| **Thanh Toán & Hóa Đơn** | Màn hình POS thu ngân chọn nhanh hội viên, bảng giá niêm yết, cửa sổ VietQR sandbox có nút giả lập (bật qua biến môi trường) và in nhiệt. | Danh mục gói dạng thẻ cuộn dọc, modal mã QR sandbox có đếm ngược 10 phút, nút giả lập (bật qua biến môi trường) cập nhật trạng thái sau xác nhận từ máy chủ. |
| **Lịch Tập Huấn Luyện Viên** | Bảng lịch tuần đa năng nhiều PT, kéo thả khung giờ, hàng rào viền đỏ chặn đứng xung đột trùng lịch. | Lưới chọn khung giờ rảnh của PT theo ngày (màu trung tính), modal xác nhận kèm cảnh báo mất buổi nếu hủy ca $<2$ tiếng. |
| **Chỉ Số Sức Khỏe InBody** | Biểu mẫu nhập số đo chuyên dụng có kiểm tra ngưỡng an toàn y tế và tính tự động chỉ số BMI tức thời. | Thẻ tóm tắt chỉ số thể lực cá nhân, thước đo dải màu BMI trực quan, biểu đồ đường theo dõi tiến trình giảm mỡ. |
| **Báo Cáo & Quản Trị** | Dashboard KPI doanh thu thời gian thực, biểu đồ cột giờ cao điểm, bảng dữ liệu phân trang và xuất Excel. | Trang hồ sơ cá nhân, lịch sử các ca tập đã tham gia, lịch sử các lần check-in và hóa đơn đã thanh toán. |

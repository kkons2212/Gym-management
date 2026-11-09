# USER EXPERIENCE & INTERACTION FLOW RULES (UX SYSTEM RULES)
## Hệ Thống Quản Lý Phòng Tập Thể Thao (Gym Management System)
### WinUI 3 Desktop (Admin) & Blazor Server (Member/PT Portal)

> **Mục đích tài liệu:** Tài liệu này quy định toàn bộ tiêu chuẩn **Trải nghiệm Người dùng & Luồng Tương tác Nghiệp vụ (UX - User Experience)** cho toàn bộ lập trình viên và AI Agent khi triển khai giao diện, quản lý trạng thái, xử lý sự kiện và kiểm soát ngoại lệ trên 2 ứng dụng client: **WinUI 3 Desktop App** (dành cho Admin / Lễ tân / Thu ngân) và **Blazor Server Web App** (dành cho Hội viên và Huấn luyện viên cá nhân PT).
>
> **Triết lý UX Cốt lõi (Core UX Philosophy):**
> * **Đối với Hội viên & PT (Web Portal):** Thao tác tức thì (Zero-Friction), thông tin gói tập điện tử trực quan, hiển thị mã QR nhanh chóng, minh bạch lượt tập và lịch hẹn, cảnh báo phạt hủy ca trước 2 giờ rõ ràng.
> * **Đối với Ban quản trị Admin (Desktop App):** Tốc độ quầy cực nhanh (< 1 giây điểm danh), hiển thị kết quả tinh gọn (Mã hội viên + Họ tên), an toàn giao dịch tài chính (VietQR tự động khớp, bảng giá niêm yết cố định không ô discount), kiểm soát chống gian lận và trực quan hóa xung đột lịch dạy PT.
> * **Quy tắc toàn cục về nhãn giao diện:** **Tuyệt đối không hiển thị mã Use Case kỹ thuật `(UC...)` lên bất kỳ thành phần giao diện người dùng nào** (tiêu đề trang, menu điều hướng, nút bấm, nhãn form, popup). Mã Use Case chỉ dùng làm quy chuẩn đối chiếu nội bộ trong tài liệu kỹ thuật và mã nguồn.

---

## 1. NGUYÊN TẮC TƯƠNG TÁC CỐT LÕI & QUY CHUẨN UX PHỔ QUÁT (UNIVERSAL UX PRINCIPLES)

Mọi màn hình và thành phần giao diện khi hiện thực đều phải tuân thủ nghiêm ngặt các nguyên tắc tương tác và hàng rào phòng vệ lỗi sau:

### 1.1. Bảng Mã Màu Ngữ Nghĩa Hệ Thống (Semantic Status Colors - New Colors Panel)

Hệ thống độc tôn giao diện tối (Dark Mode 100%) trên cả Desktop và Web, tối ưu cho môi trường ánh sáng phòng tập thể thao với tỷ lệ tương phản cao ($\ge 4.5:1$ theo chuẩn WCAG 2.2 AA):

| Vai Trò Token | Mã Hex / RGB | Tailwind CSS (Web) | XAML Resource (WinUI 3) | Ràng Buộc Ứng Dụng Nghiệp Vụ Bắt Buộc |
| :--- | :--- | :--- | :--- | :--- |
| **Nền Gốc (Canvas)** | `#0A0F1D` | `bg-[#0A0F1D]` | `AppBackgroundBrush` | Nền gốc toàn bộ cửa sổ ứng dụng (Obsidian Deep Dark). |
| **Bề Mặt Khối (Surface Card)** | `#1E293B` | `bg-slate-800` | `CardBackgroundBrush` | Khối thông tin gói tập, giỏ hàng POS, ca tập; viền `1px solid #334155`. |
| **Chữ Chính (Primary Text)** | `#F8FAFC` | `text-slate-50` | `TextFillColorPrimaryBrush` | Tiêu đề, số tiền POS, họ tên hội viên và thông tin quan trọng (Tương phản 14.8:1). |
| **Chữ Phụ (Secondary Text)** | `#94A3B8` | `text-slate-400` | `TextFillColorSecondaryBrush` | Nhãn form, ngày hết hạn gói, chú thích chỉ số InBody (Tương phản 5.2:1). |
| **Điểm Nhấn Hành Động (Accent)**| `#0EA5E9` | `bg-sky-500` | `AccentFillColorDefaultBrush` | **Đúng 1 màu nổi bật duy nhất** cho nút CTA chính: Quét QR, Thanh toán, Đặt lịch PT, Đăng ký gói. |
| **Trạng Thái Thành Công (Success)**| Chữ: `#4ADE80`<br>Nền: `#064E3B` viền `#059669` | `text-emerald-400 bg-emerald-950/60 border-emerald-600` | `StatusSuccessBrush` | Check-in hợp lệ, Mở cổng Turnstile, gói tập `ACTIVE`, hóa đơn `PAID`, Ca PT `COMPLETED`. |
| **Trạng Thái Cảnh Báo (Warning)** | Chữ: `#FBBF24`<br>Nền: `#78350F` viền `#D97706` | `text-amber-400 bg-amber-950/60 border-amber-600` | `StatusWarningBrush` | Gói tập sắp hết hạn ($\le 7$ ngày), Gói tập bảo lưu `FROZEN`, Cảnh báo hủy ca tập $< 2\text{h}$. |
| **Trạng Thái Lỗi / Khóa (Danger)** | Chữ: `#F87171`<br>Nền: `#7F1D1D` viền `#DC2626` | `text-red-400 bg-red-950/60 border-red-600` | `StatusDangerBrush` | Gói `EXPIRED`, `CANCELLED`, Điểm danh thất bại/Cổng khóa, Trùng lịch dạy PT, Nút Hủy gói. |
| **Trạng Thái Ca PT Đã Đặt** | Chữ: `#C084FC`<br>Nền: `#4C1D95` viền `#7C3AED` | `text-purple-400 bg-purple-950/60 border-purple-600` | `StatusPtBrush` | Ca dạy PT đã đặt (`PTTrainingSession`) và khối chỉ số InBody. |
| **Khung Giờ Rảnh PT (Open Slot)** | Chữ: `#E2E8F0`<br>Nền: `#1E293B` viền `#475569` | `text-slate-200 bg-slate-800 border-slate-600` | `PtSlotOpenBrush` | Khung giờ rảnh của PT (`PTAvailability`) — **Dùng màu trung tính, TUYỆT ĐỐI KHÔNG dùng màu xanh lá**. |

> [!IMPORTANT]
> **Quy chuẩn nhãn trạng thái & âm thanh:** Giữ nguyên các chip trạng thái ở dạng nhãn chữ thuần túy, **không thêm biểu tượng icon** vào chip trạng thái. **Hoàn toàn không sử dụng âm thanh phản hồi** (không có tiếng "Beep", "Buzz"), mọi kết quả đều được truyền tải qua thông báo văn bản trực quan.

---

### 1.2. Tự Do Nhập Liệu & Tuyệt Đối Không Chặn Dán (`Never Block Paste`)

* **Quy tắc Tự do Dán (Never Block Paste):**
  * Nghiêm cấm chặn sự kiện `paste` trên mọi ô nhập liệu (Số điện thoại, CCCD, Mã hội viên, Nội dung chuyển khoản VietQR, Ghi chú InBody). Người dùng có quyền dán nội dung từ clipboard mà không bị gián đoạn hay xóa dữ liệu.
* **Chuẩn hóa Thuộc tính `inputmode` trên Thiết bị Di động:**
  * Ô nhập Số điện thoại, CCCD, Mã PIN: Cấu hình `type="tel"` kèm `inputmode="numeric"`.
  * Ô nhập Chỉ số Thể chất InBody (Cân nặng, Chiều cao, Tỷ lệ mỡ, Khối lượng cơ): Cấu hình `type="text"` kèm `inputmode="decimal"` để bàn phím ảo hiển thị sẵn phím số và dấu chấm thập phân.
  * Ô Tìm kiếm: Cấu hình `type="search"`.
* **Thuộc tính Tự Động Điền (Autocomplete):** Khai báo tường minh `autocomplete="username"`, `autocomplete="current-password"`, `autocomplete="tel"`, `autocomplete="name"` để trình duyệt hỗ trợ điền nhanh dữ liệu người dùng.

---

### 1.3. Phản Hồi Tức Thì (< 100ms) & Khóa Nút Chống Gửi Trùng Lặp (Async Idempotency)

* **Phản hồi tức thì ($<100\text{ms}$):** Mọi tương tác click chuột hoặc chạm màn hình phải có phản hồi thị giác ngay lập tức (hiệu ứng active state hoặc thay đổi độ sáng màu).
* **Bảo vệ chống Double-Click (Idempotency Guardrail):**
  * Khi người dùng nhấn nút thực hiện tác vụ bất đồng bộ (Đăng ký hội viên mới, Xác nhận thanh toán POS, Đặt lịch PT, Lưu InBody, Giả lập VietQR):
    1. Nút bấm **lập tức chuyển sang trạng thái vô hiệu hóa (`disabled` / `IsEnabled=false`)** ngay tại khung hình đầu tiên.
    2. Nhãn nút chuyển sang trạng thái đang xử lý kèm spinner quay (ví dụ: `"Đang thanh toán…"`, `"Đang kích hoạt hội viên…"`, `"Đang lưu số đo…"`).
    3. Triệt tiêu hoàn toàn nguy cơ người dùng click lặp lại làm phát sinh 2 hóa đơn hoặc trừ 2 lượt tập.

---

### 1.4. Điều Hướng Tiêu Điểm Lỗi Biểu Mẫu (Inline Validation & Error Focus Anchoring)

* **Hiển thị lỗi nội dòng (Inline Error Display):**
  * Khi kiểm tra dữ liệu form thất bại (bỏ trống trường bắt buộc, sai định dạng SĐT/Email, trùng CCCD):
    * Trường vi phạm được khoanh viền đỏ `border-red-500` và gắn nhãn `aria-invalid="true"`.
    * Thông báo lỗi cụ thể xuất hiện ngay bên dưới trường vi phạm bằng văn bản màu đỏ (`text-red-400 text-xs`), liên kết với trường qua `aria-describedby`.
* **Tự động neo tiêu điểm (Automatic Focus Navigation):**
  * Khi nhấn submit form mà phát sinh lỗi: Hệ thống phải tự động **chuyển tiêu điểm bàn phím (Focus)** về trường lỗi đầu tiên trên màn hình để người dùng có thể chỉnh sửa ngay mà không cần dùng chuột cuộn tìm lỗi.

---

### 1.5. Hỗ Trợ Trình Đọc Màn Hình Cho Cập Nhật Thời Gian Thực (Accessible Screen Reader Announcements)

* Các biến động giao diện theo thời gian thực không tải lại trang bắt buộc phải được thông báo tới thiết bị trợ năng:
  * **Mã QR điểm danh tự làm mới mỗi 60 giây:** Vùng thông báo ẩn `aria-live="polite"` đọc *"Mã QR điểm danh đã được làm mới"*.
  * **Kết quả check-in tại quầy:** Khi cổng mở, vùng `aria-live="polite"` đọc *"Điểm danh thành công cho hội viên [Họ Tên], cổng đã mở"*. Khi bị từ chối, vùng `aria-live="assertive"` đọc *"Từ chối điểm danh: [Lý do từ chối]"*.
  * **Xác nhận thanh toán VietQR qua SignalR:** Vùng `aria-live="polite"` thông báo *"Đã nhận được thanh toán, gói tập đã được kích hoạt"*.
* Trên WinUI 3 Desktop: Sử dụng `AutomationPeer.RaiseAutomationEvent` để thông báo cho trình đọc màn hình Narrator.

---

### 1.6. Bẫy Tiêu Điểm Hộp Thoại (Modal Focus Trapping & Safe Restoration)

* Khi bất kỳ hộp thoại nào xuất hiện (`ContentDialog` trên Desktop hoặc Modal VietQR trên Web):
  1. Tiêu điểm bàn phím tự động chuyển vào phần tử tương tác đầu tiên trong hộp thoại (hoặc nút Hủy/Đóng).
  2. **Bẫy tiêu điểm (Focus Trap):** Phím `Tab` và `Shift+Tab` chỉ tuần hoàn bên trong hộp thoại, không lọt ra ngoài nền trang phía sau.
  3. Phím `Escape` đóng hộp thoại an toàn mà không thực hiện hành động nguy hiểm.
  4. Khi hộp thoại đóng lại: Tiêu điểm bàn phím tự động trả về đúng nút bấm đã mở nó trước đó.

---

### 1.7. Khả Năng Phục Hồi Kết Nối Mạng (Network Resiliency & Offline UX)

* Khi mất kết nối internet hoặc kết nối SignalR bị gián đoạn:
  * Hiển thị thanh cảnh báo mỏng cố định trên đầu màn hình: *"Mất kết nối tới máy chủ phòng tập. Đang tự động kết nối lại…"*.
  * Tạm thời khóa các nút thanh toán và gửi dữ liệu nhạy cảm để tránh mất gói tin.
  * Giữ nguyên toàn bộ dữ liệu người dùng đang nhập dở trên biểu mẫu (không xóa form).
  * Tự động đồng bộ và kích hoạt lại các nút ngay khi đường truyền phục hồi.

---

### 1.8. Quy Chuẩn Vùng Bấm (Touch Targets) & Phím Tắt Quầy Thu Ngân (POS Shortcuts)

* **Quy chuẩn vùng bấm (Touch & Click Target):**
  * Trên Web Mobile (Hội viên & PT): Mọi nút bấm, tab và mã QR phải có kích thước tối thiểu **$44\text{px} \times 44\text{px}$** với khoảng đệm tối thiểu $8\text{px}$.
  * Trên Desktop (Admin): Nút quét QR, nút bán gói lớn, dễ click chuột nhanh hoặc thao tác trên màn hình cảm ứng POS tại quầy.
* **Hỗ trợ phím tắt quầy (Admin Shortcuts):**
  * `F2`: Mở hóa đơn bán hàng POS mới.
  * `F3`: Đưa con trỏ vào ô tìm kiếm nhanh hội viên.
  * `F4` hoặc `Ctrl+N`: Mở hộp thoại Đăng ký hội viên mới (`RegisterMemberWizard.xaml`).
  * `F5`: Làm mới bảng lịch PT và danh sách ra/vào.
  * `Enter`: Xác nhận thanh toán / In hóa đơn / Thực hiện điểm danh thủ công.
  * `Esc`: Đóng hộp thoại hoặc hủy thao tác.
  * **Tuyệt đối không dùng phím tắt `F1` cho điểm danh.**

---

## 2. QUY CHUẨN TRẢI NGHIỆM HỘI VIÊN & HUẤN LUYỆN VIÊN (MEMBER & PT JOURNEY - BLAZOR WEB)

Cổng thông tin Web Portal (`GymManagement.Web`) được thiết kế theo tư duy **Mobile-First Responsive**, tối ưu cho hội viên và huấn luyện viên sử dụng trên điện thoại di động.

### 2.1. Giai đoạn 1: Đăng nhập & Định tuyến Phân quyền (Khớp UC01)
* **Luồng đăng nhập mượt mà:**
  * Hội viên và PT nhập Tên đăng nhập / Email và Mật khẩu.
  * Khi xác thực thành công: Hệ thống cấp JWT Token, tự động điều hướng: PT vào giao diện Lịch dạy (`/pt/schedule`), Hội viên vào Cổng tự phục vụ (`/member/dashboard`).
  * Khi nhập sai mật khẩu: Thông báo lỗi *"Tên đăng nhập hoặc mật khẩu không chính xác"*, giữ nguyên tên đăng nhập và xóa trắng ô mật khẩu để người dùng gõ lại ngay.
  * Khi tài khoản bị khóa (`status == 'LOCKED'`): Hiển thị thông báo *"Tài khoản đã bị khóa. Vui lòng liên hệ ban quản lý phòng gym."*.

---

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
* **Tra Cứu Lịch Sử Tập Luyện (UC 9.2):**
  * Dòng thời gian kết hợp giữa các lần quét QR vào phòng và các ca tập hoàn thành cùng PT, hỗ trợ lọc theo Tuần này, Tháng này hoặc Tùy chọn.

---

### 2.3. Giai đoạn 3: Khám Phá Gói Tập & Thanh Toán VietQR Tự Động (Khớp UC05 & UC02)
* **Bảng Giá & So Sánh Gói Tập Minh Bạch:**
  * Liệt kê danh mục gói tập dạng thẻ trực quan khớp bảng `PackagePlan` (`PlanId`, `Name`, `Description`).
  * Hiển thị rõ đơn giá niêm yết (`PackagePlan.Price`), thời hạn sử dụng (`DurationDays`), số buổi tập gym (`MaxCheckInSessions`), số buổi tập PT đi kèm (`TotalPtSessions`).
  * **Chính sách Giá Niêm Yết Cố Định:** Giá hóa đơn hiển thị đúng bằng giá niêm yết của gói (`Invoice.TotalAmount = CAST(PackagePlan.Price AS INT)`). Hệ thống không áp dụng chiết khấu/giảm giá vì mô hình CSDL vật lý không lưu trữ giảm giá.
* **Thanh Toán VietQR Không Cần Nhập Liệu:**
  * Khi hội viên chọn `"Đăng ký gói"` $\rightarrow$ Hệ thống gọi API sinh mã thanh toán VietQR chuẩn NAPAS 247 có sẵn số tiền chính xác và nội dung chuyển khoản định danh duy nhất (ví dụ: `GYM SUB 1042`).
  * Hiển thị đồng hồ đếm ngược phiên thanh toán **10 phút (600 giây)** kèm hướng dẫn: *"Mở ứng dụng ngân hàng bất kỳ để quét mã"*.
  * **Nút Giả Lập Thanh Toán Sandbox:**
    * Dưới mã QR có nút `"Giả lập thanh toán thành công"` phục vụ kiểm thử luồng tích hợp.
    * Nút chỉ hiển thị khi biến môi trường `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`.
    * Trong môi trường Production (`VIETQR_ENABLE_SIMULATION=false`), nút này tự động ẩn hoàn toàn khỏi giao diện người dùng.
    * Khóa nút ngay sau khi nhấn để tránh gửi lệnh trùng lặp.
  * **Cơ chế Kích hoạt Thời gian Thực:**
    * Client lắng nghe kết quả qua SignalR / WebSocket hoặc cơ chế Polling 2 giây/lần.
    * Ngay khi nhận được tín hiệu thanh toán thành công:
      1. Tự động đóng modal QR code ngay lập tức (không cần tải lại trang).
      2. Thông báo biểu thị trạng thái: *"Kích hoạt gói tập thành công!"*.
      3. Cập nhật ngay trạng thái gói tập sang `ACTIVE` và cộng đủ số lượt tập.
      4. Cung cấp nút xem/tải Hóa đơn điện tử PDF.

---

### 2.4. Giai đoạn 4: Cổng Tác Nghiệp Huấn Luyện Viên PT (Khớp UC08)
* **PT Đăng Ký Khung Giờ Rảnh (`PtAvailabilityGrid.razor` - UC 8.1.1):**
  * PT xem lưới thời gian các ngày trong tuần và chạm chọn nhanh các slot giờ làm việc rảnh cho tuần tới.
  * Khung giờ rảnh hiển thị màu trung tính (`slot-pt-open`).
  * **Ràng buộc bảo vệ:** PT không thể hủy bỏ khung giờ rảnh nếu khung giờ đó đã có học viên đặt lịch; hiển thị thông báo: *"Khung giờ đã có học viên đặt lịch, không thể hủy."*.
* **Thời Khóa Biểu Tuần Của PT (`PtWeeklySchedule.razor` - UC 8.1.2 & UC 8.2.3):**
  * Dòng thời gian cá nhân hiển thị chi tiết các ca dạy trong tuần: Khung giờ, Tên học viên, SĐT gọi nhanh (`tel:`), Trạng thái ca (`Sắp diễn ra`, `COMPLETED`, `Đã hủy`) và ghi chú bài tập.
  * Nút bấm lớn 1-chạm: `"Xác Nhận Hoàn Thành Ca Tập"` trên ca đang/đã diễn ra (khóa khi chưa đến giờ); bấm xong ca đổi sang `COMPLETED`, tự động trừ 1 buổi của học viên và ghi nhận KPI công dạy.
* **Theo Dõi Học Viên & Nhập InBody (UC 8.2.1 & UC 8.2.2):**
  * Danh sách học viên phụ trách hiển thị số buổi PT còn lại.
  * Form nhập số đo InBody nhanh: Chiều cao, Cân nặng, Tỷ lệ mỡ, Khối lượng cơ; hệ thống tự động tính chỉ số BMI hiển thị tức thì.

---

### 2.5. Giai đoạn 5: Đặt Lịch PT, Hủy Ca & Đánh Giá Dịch Vụ (Khớp UC10)
* **Quy trình Đặt Lịch PT 3 Bước (UC 10.1):**
  * Hội viên xem thông tin PT phụ trách cố định (`AssignedPtId`) và số buổi PT còn lại.
  * Bước 1: Chọn ngày trong tuần trên băng chuyền ngang.
  * Bước 2: Chọn khung giờ rảnh của PT (màu trung tính `slot-pt-open`). Các ca đã kín lịch hiển thị làm mờ gạch ngang.
  * Bước 3: Modal xác nhận đặt lịch $\rightarrow$ Chuyển ca sang trạng thái "Đã đặt" (màu tím), tạm giữ 1 buổi tập và gửi thông báo cho PT.
* **Ràng Buộc Quy Tắc Hủy Ca Trước 2 Giờ Chuẩn PRD (UC 10.2):**
  * Khi hội viên xem danh sách ca tập đã đặt:
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$\ge 2$ giờ**: Nút `"Hủy ca tập"` hiển thị bình thường. Khi xác nhận, hoàn lại 1 lượt tập `RemainingPtSessions` về gói tập mà không phạt.
    * Nếu thời gian hiện tại cách giờ bắt đầu ca tập **$< 2$ giờ**: Nút `"Hủy ca tập"` đổi sang màu đỏ cảnh báo. Khi click, bật hộp thoại cảnh báo: *"Chú ý: Hủy ca tập trước dưới 2 tiếng sẽ bị tính là một buổi tập đã sử dụng theo nội quy phòng tập. Bạn có chắc chắn muốn hủy ca này không?"* với 2 lựa chọn: `"Giữ Lại Ca Tập"` (Nút chính) hoặc `"Xác Nhận Hủy (Mất buổi)"` (Nút phụ viền đỏ).
* **Đánh Giá Chất Lượng Ca Tập (UC 10.3):**
  * Khi ca tập chuyển sang `COMPLETED`, thẻ ca tập hiển thị nút `"Đánh giá buổi tập"`.
  * Cho phép chọn từ 1 đến 5 sao và nhập nội dung nhận xét (tùy chọn). Gửi thành công thì khóa ca tập không cho đánh giá lại và tự động cập nhật điểm trung bình của PT.
* **Đổi Lịch Sang Ca Trống Khác (UC 10.4):**
  * Bấm "Đổi lịch" trên ca tập sắp tới $\rightarrow$ Hiển thị các ca trống khác của cùng PT $\rightarrow$ Bấm xác nhận đổi lịch, hệ thống giải phóng ca cũ và giữ chỗ ca mới trong cùng 1 Transaction CSDL.

---

## 3. QUY CHUẨN TRẢI NGHIỆM DÀNH CHO ADMIN (ADMIN WORKFLOW - WINUI 3 DESKTOP)

Ứng dụng WinUI 3 Desktop (`GymManagement.Desktop`) được thiết kế theo tư duy **Desktop-First MVVM**, phục vụ màn hình làm việc của Admin tại quầy tiếp đón với tốc độ xử lý tức thì, mật độ thông tin cao và giao diện phẳng Fluent Design độc tôn Dark Mode.

### 3.1. Phân Hệ Đăng Nhập Quản Trị & Bảo Vệ Tài Khoản (Khớp UC01)
* **Đăng nhập phân quyền Claims JWT:**
  * Admin nhập Tên đăng nhập/Email và Mật khẩu, nhấn Enter hoặc bấm `"Đăng Nhập"`.
  * Thời gian phản hồi xác thực $< 1$ giây.
  * Nếu đăng nhập thành công: Lưu phiên làm việc và điều hướng trực tiếp vào không gian làm việc quản trị.
  * Nếu tài khoản bị khóa: Báo lỗi chi tiết *"Tài khoản đã bị khóa. Vui lòng liên hệ Super Admin."*, từ chối truy cập.

---

### 3.2. Phân Hệ Quầy Điểm Danh & Cổng Tự Động (Khớp UC03)
* **Tốc Độ Xử Lý Điểm Danh < 1 Giây:** Luồng check-in là luồng quan trọng bậc nhất của phòng gym, không được phép trễ nải gây ùn tắc tại cửa ra vào.
* **Giao Diện Màn Hình Điểm Danh Quầy Tiếp Đón (Chia 2 Cột Rõ Ràng):**
  * **Cột Trái (Webcam Quét Mã QR & Tra Cứu Thủ Công):**
    * Hệ thống không dùng thẻ vật lý, không có đầu đọc thẻ USB, và không dùng máy quét barcode cầm tay. Tuyệt đối không cung cấp ô text để gõ chuỗi mã QR. Không dùng phím tắt `F1`.
    * Hội viên xuất trình mã QR từ ứng dụng Web/Mobile trước webcam tại quầy; webcam nhận diện và giải mã tự động theo thời gian thực.
    * Kèm theo ô nhập nhanh để tra cứu thủ công bằng **Mã hội viên (`UserName`)** hoặc **Số điện thoại (`User.Phone`)** khi hội viên quên điện thoại hoặc hết pin. Phím `Enter` kích hoạt điểm danh ngay.
    * **Nút `"Cho vào thủ công (Override)"`:** Bắt buộc chọn lý do (Quên điện thoại, Máy hết pin, Cổng turnstile kẹt cơ khí, Lý do khác) để lưu vào vết kiểm toán (`CheckInLog`, ghi nhận `VerifiedByAdminId`).
    * **Nút `"Mở cổng kiểm tra"`:** Thao tác thử nghiệm kỹ thuật.
  * **Cột Phải (Kết Quả Xác Minh Từ Backend Tinh Gọn & Giám Sát Ra/Vào):**
    * Desktop App đóng vai trò Presentation: Gửi yêu cầu lên Backend xử lý tập trung trong transaction (`POST /api/v1/checkin`).
    * **Hoàn toàn không sử dụng âm thanh phản hồi** (không phát tiếng "Beep" hay "Buzz").
    * **Hoàn toàn không yêu cầu ảnh chân dung** (không hiển thị khung ảnh bo tròn).
    * **Bảng kết quả tập trung tinh gọn đúng 2 thông tin cốt lõi:**
      1. **Mã hội viên (`UserName`)**
      2. **Họ tên hội viên (`FullName`)**
    * **Khi Check-in Hợp Lệ:** Màn hình hiển thị banner xanh tĩnh: `"CHECK-IN THÀNH CÔNG • CỔNG MỞ"`, to rõ Mã hội viên (`UserName`), Họ tên (`FullName`), Tên gói tập và số buổi gym còn lại (`RemainingCheckInSessions`). Đồng hồ 7 giây đếm lùi để trở về trạng thái chờ. **Không nhấp nháy viền**.
    * **Khi Gói Tập Không Hợp Lệ:** Màn hình hiển thị banner đỏ tĩnh: `"TỪ CHỐI CHECK-IN • CỔNG KHÓA"`, to rõ Mã hội viên (`UserName`), Họ tên (`FullName`), và dòng lý do chi tiết do Backend phản hồi (*"Gói tập trong tài khoản đã hết hạn"*, *"Gói tập đã hết số lượt vào phòng"*, *"Gói tập đang tạm ngưng bảo lưu"*). **Không nhấp nháy viền**.
  * **Bảng Giám sát Ra/Vào Hôm Nay (UC 3.3):**
    * Nửa dưới cột phải hiển thị danh sách các lượt check-in hôm nay theo thời gian thực (lượt mới nhất xếp trên cùng).
    * Bộ lọc theo ca trực (Sáng/Chiều/Tối); Hiển thị số lượng hội viên hiện diện trong phòng.

---

### 3.3. Phân Hệ Quản Lý Hội Viên & Vòng Đời Gói Tập (Khớp UC02)
* **Vị Trí Tạo Hội Viên Mới Chuẩn Nghiệp Vụ (UC 02.1):**
  * Nút CTA chính **`"+ Đăng Ký Hội Viên Mới"`** đặt nổi bật tại góc trên bên phải thanh Header của màn hình [`MembersPage.xaml`](file:///c:/Users/nguye/Desktop/Gym-management/src/Client/GymManagement.Desktop/Views/MembersPage.xaml) (phím tắt `Ctrl+N` hoặc `F4`).
  * Khi bấm: Mở hộp thoại **Wizard Đăng ký 2 bước (`RegisterMemberWizard.xaml`)**:
    * **Bước 1 - Thông tin cá nhân:** Nhập Họ tên, SĐT (`inputmode="numeric"`), Email, CCCD, Ngày sinh, Giới tính, Địa chỉ. Báo lỗi trùng SĐT/CCCD tức thời dưới trường dữ liệu.
    * **Bước 2 - Chọn Gói & Thu tiền:** Lưới thẻ gói tập `PackagePlan` hiển thị Đơn giá niêm yết (`tabular-nums`), Thời hạn, Số buổi gym/PT; Chọn ngày bắt đầu; Bảng tính tổng tiền bằng đúng giá niêm yết; Bộ 3 nút chọn thanh toán (`Tiền mặt`, `VietQR`, `Thẻ POS`); Bấm hoàn tất để kích hoạt hội viên và gói tập trong cùng 1 Transaction CSDL.
* **Cập Nhật Hồ Sơ Hội Viên (UC 02.2):** Tìm kiếm nhanh theo SĐT/Tên/Mã trong $<1$s; Cho phép chỉnh sửa SĐT, Email, Địa chỉ; Khóa không cho sửa Mã hội viên và CCCD.
* **Gia Hạn hoặc Nâng Cấp Gói Tập (UC 02.3):** Tự động tính ngày hết hạn mới (kéo dài `EndDate` cũ nếu còn hạn, hoặc kích hoạt mới từ hôm nay nếu đã hết hạn); Bảng tính tiền chênh lệch tự động.
* **Quy Trình Bảo Lưu Gói Tập (UC 02.4 - `FreezeSubscriptionDialog.xaml`):**
  * Chọn Ngày bắt đầu bảo lưu (`FreezeStartDate`) và Ngày kết thúc (`FreezeEndDate`).
  * Hệ thống tự động tính số ngày bảo lưu và hiển thị ngày hết hạn mới dự kiến:
    $$\text{EndDate mới} = \text{EndDate cũ} + \text{Số ngày bảo lưu}$$
  * Có cảnh báo rõ ràng nếu số ngày bảo lưu vượt quá hạn mức tối đa của gói. Bắt buộc nhập lý do bảo lưu.
* **Hủy Đăng Ký Gói Tập (UC 02.5 - `CancelSubscriptionDialog.xaml`):** Modal cảnh báo viền đỏ; bắt buộc nhập lý do hủy; chuyển trạng thái gói sang `CANCELLED`, vô hiệu hóa quyền check-in nhưng bảo toàn toàn bộ lịch sử sử dụng trong CSDL.

---

### 3.4. Phân Hệ Bán Gói POS, In Hóa Đơn & Hoàn Tiền (Khớp UC05)
* **Quy Trình Bán Hàng POS 3 Bước Nhanh (`PosBillingPage.xaml` - UC 05.1):**
  1. **Bước 1 - Tìm / Chọn Hội viên:** Ô tìm kiếm thông minh gợi ý tự động trong $<300\text{ms}$ kèm **nút shortcut `[+] Khách mới`** để tạo nhanh hội viên khi khách chưa có hồ sơ.
  2. **Bước 2 - Chọn Gói Tập Niêm Yết:** Lưới thẻ gói tập hiển thị giá niêm yết (`PackagePlan.Price`), thời hạn và số buổi. Hệ thống tự động gán `Invoice.SubtotalAmount = PackagePlan.Price` và `Invoice.TotalAmount = CAST(PackagePlan.Price AS INT)`.
     * **Ràng Buộc Tuyệt Đối Về Giá:** Không có trường giảm giá hay mã ưu đãi vì bảng `Invoice` trong CSDL vật lý không lưu trữ giảm giá.
  3. **Bước 3 - Chọn Phương Thức Thanh Toán:** 3 nút bấm lớn: `Tiền mặt (CASH)`, `Chuyển khoản VietQR (VIETQR)`, `Quẹt thẻ ngân hàng (CREDIT_CARD)`.
* **Cơ Chế Thanh Toán VietQR & Nút Giả Lập Sandbox:**
  * Mở cửa sổ popup có mã VietQR NAPAS 247 động lớn.
  * Nút `"Giả lập thanh toán thành công"` chỉ hiển thị khi `VIETQR_ENABLE_SIMULATION=true` hoặc `ASPNETCORE_ENVIRONMENT=Development`. Tự ẩn hoàn toàn trên Production. Khóa nút ngay sau khi bấm.
  * Thu ngân được bấm nút xác nhận thu tiền thủ công ngay khi có bằng chứng chuyển khoản thành công, đi kèm hộp thoại xác nhận nhanh 2 bước để phòng ngừa bấm nhầm.
* **Tra Cứu Lịch Sử & In Lại Hóa Đơn (`InvoiceHistoryPage.xaml` - UC 05.2):** Tra cứu theo mã `INV-...`/SĐT/ngày; Cung cấp nút in nhiệt chuẩn K80 tức thì và nút xuất file PDF trong $<2$s.
* **Xử Lý Hủy Hóa Đơn & Hoàn Tiền (`RefundDialog.xaml` - UC 05.3):** Nhập số tiền hoàn ($\le$ gốc), hình thức hoàn và lý do hoàn tiền bắt buộc; chuyển trạng thái hóa đơn sang `REFUNDED`, thu hồi gói dịch vụ và ghi nhận dòng tiền âm vào sổ quỹ doanh thu.
* **Báo Cáo Doanh Thu Tài Chính (UC 05.4):** Bộ lọc mốc thời gian linh hoạt; tính tổng doanh thu thực thu (Gộp - Hoàn) và cung cấp nút xuất file Excel/PDF.

---

### 3.5. Phân Hệ Quản Lý PT, Phân Công 1-1 & Xếp Lịch Chống Trùng (Khớp UC04)
* **Thêm Mới & Cập Nhật Hồ Sơ PT (UC 04.1 & UC 04.3):** Nhập thông tin chuyên môn, mức lương, kinh nghiệm. Ràng buộc an toàn: Ngăn chặn chuyển trạng thái PT sang `INACTIVE` nếu PT đang còn học viên phụ trách hoặc ca dạy chưa hoàn tất.
* **Phân Công PT Phụ Trách Cố Định Cho Học Viên (UC 04.4 - `PtAssignDialog.xaml`):** Gán cố định quan hệ 1-1 cho hội viên có gói PT, kiểm tra định mức học viên tối đa của PT.
* **Rà Soát Ca Dạy Ngoại Lệ Phía Admin (UC 04.5 - `SessionReviewDialog.xaml`):** Xử lý nhanh các ca quá giờ chưa hoàn thành: `Hoàn thành` (trừ buổi, ghi nhận công PT), `Học viên vắng` (trừ buổi), `PT vắng` (hoàn buổi cho khách), `Hoãn ca` (hoàn buổi kèm lý do).
* **Xếp Lịch Làm Việc & Hàng Rào Chống Trùng Giờ (UC 04.6 - `WeeklyCalendarDispatcher.xaml`):**
  * Lưới thời gian tuần (06:00 đến 22:00).
  * **Hàng rào chống trùng lịch:** Nếu xếp lịch đè lên ca đã có hoặc vi phạm định mức $\rightarrow$ Viền đỏ rực `#F87171` kèm tooltip giải thích chi tiết, vô hiệu hóa nút Lưu.
  * Cấm xóa khung giờ đã có học viên đặt lịch thành công.

---

### 3.6. Phân Hệ Quản Trị Nhân Sự RBAC & Danh Mục Gói Tập (Khớp UC07)
* **Quản Lý Nhân Sự & Phân Quyền RBAC (UC 7.1):**
  * Tạo tài khoản nhân viên, gán vai trò (`Admin`, `PT`, `Member`).
  * Switch Toggle khóa/mở khóa tài khoản nhân viên.
  * **Hàng rào an toàn chống tự phá vỡ quyền quản trị:**
    * Ngăn chặn Super Admin tự thay đổi vai trò quản trị của chính tài khoản mình.
    * Ngăn chặn Quản trị viên tự khóa tài khoản của chính mình.
* **Quản Lý Danh Mục Gói Tập (UC 7.2):**
  * Thêm gói tập mới vào danh mục kinh doanh (Tên, Đơn giá > 0, Thời hạn, Lượt gym, Buổi PT, Số ngày bảo lưu tối đa).
  * Chỉnh sửa gói tập (áp dụng cho lượt mua mới, giữ nguyên cam kết gói cũ).
  * Xóa gói tập: Nếu gói đã có hội viên đăng ký hoặc phát sinh hóa đơn $\rightarrow$ Từ chối xóa vật lý, chuyển cờ trạng thái sang `DISCONTINUED` (Ngừng kinh doanh) để bảo toàn quyền lợi khách hàng.

---

### 3.7. Phân Hệ Báo Cáo BI Analytics Thời Gian Thực (Khớp UC06)
* **Dashboard Quản Trị Đa Chiều (US 06.1):**
  * 4 thẻ KPI lớn đầu trang: `Doanh thu hôm nay (VNĐ)`, `Tổng lượt check-in hôm nay`, `Số hội viên mới trong tháng`, `Số ca PT hoàn thành`.
  * Biểu đồ cột phân bố lưu lượng theo từng khung giờ trong ngày (nhận diện khung giờ cao điểm 17:00 - 20:00).
  * Biểu đồ tỷ trọng cơ cấu doanh thu theo gói và bảng xếp hạng KPI đội ngũ PT.
* **Xuất Dữ Liệu Báo Cáo (US 06.2):** Nút xuất Excel (.xlsx) và PDF tải về trong $<3$ giây qua cơ chế Streaming MemoryStream tránh nghẽn RAM máy chủ.

---

## 4. MA TRẬN ĐỐI CHIẾU TRẢI NGHIỆM: HỘI VIÊN / PT (WEB) VS. QUẢN TRỊ VIÊN ADMIN (DESKTOP)

| Phân Hệ Nghiệp Vụ | Trải Nghiệm Hội Viên & PT (Blazor Server - Mobile) | Trải Nghiệm Quản Trị Viên (WinUI 3 - Desktop) |
| :--- | :--- | :--- |
| **Điểm danh & Ra vào** | Mở mã QR gói tập trên điện thoại, hiển thị mã QR tĩnh chứa mã hội viên để quét qua quầy check-in. | Màn hình Admin tiếp nhận quét camera trong $<1\text{s}$, kết quả hiển thị tinh gọn **Mã hội viên + Họ tên**, banner tĩnh xanh/đỏ (không âm thanh, không ảnh). |
| **Gói tập & Thanh toán** | Tự xem danh mục gói tập, chọn mua và quét mã VietQR tự động kích hoạt gói tập trong 3 giây. Hỗ trợ nút giả lập thanh toán sandbox qua biến môi trường. | Admin chọn nhanh hội viên, gói tập, xuất hóa đơn in nhiệt K80, kiểm tra tiền về qua Webhook hoặc xác nhận thu tiền mặt (không khóa cưỡng bức thời gian). Hỗ trợ nút giả lập sandbox qua biến môi trường. |
| **Lịch Huấn luyện viên PT** | Xem lịch rảnh của PT trên lưới tuần (màu trung tính), đặt ca tập chỉ với 2 click; cảnh báo phạt nếu hủy ca $<2$ tiếng. | PT xem danh sách học viên theo Timeline ca dạy; Quản trị viên xếp lịch PT, phát hiện và chặn đứng xung đột trùng giờ. |
| **Chỉ số Sức khỏe InBody** | Xem biểu đồ trực quan biến thiên cân nặng, tỷ lệ mỡ và khối lượng cơ qua từng mốc thời gian. | Nhân viên đo nhập số liệu trên form có kiểm tra biên an toàn, tự động tính chỉ số BMI ngay tức thì. |
| **Quản lý & Báo cáo** | Nhận thông báo tự động khi gói tập sắp hết hạn hoặc đến giờ hẹn tập cùng huấn luyện viên. | Theo dõi Dashboard KPI doanh thu thời gian thực, xem mật độ giờ cao điểm, trích xuất báo cáo Excel/CSV và sổ kiểm toán. |

# Hệ Thống Quản Lý Phòng Tập Thể Thao (Gym Management System)

<p align="center">
  <img src="https://github.com/user-attachments/assets/ed4d18a3-9994-4d0e-8752-2a8a88a752eb" alt="Sơ đồ Use Case Tổng Quát" style="max-width: 100%; height: auto;" />
</p>

---

## 1. Giới thiệu & Bối cảnh Nghiệp vụ

Hệ thống Quản lý Phòng tập thể thao vận hành như một mạng lưới tương tác đa bên liên đới chặt chẽ giữa ba tác nhân chính:
* **Hội viên (Member):** Người sử dụng dịch vụ tập luyện và các tiện ích tại phòng tập.
* **Nhân viên vận hành & Quản lý (Admin / Lễ tân / Thu ngân):** Tiếp nhận thông tin, quản lý hồ sơ, thẻ tập, lập hóa đơn thanh toán và điều phối tại cơ sở.
* **Huấn luyện viên cá nhân (Personal Trainer - PT):** Trực tiếp cung cấp dịch vụ chuyên môn, hướng dẫn và theo dõi tiến độ tập luyện của học viên.

Trọng tâm của hệ thống là chuyển đổi các yêu cầu tập luyện và hợp đồng dịch vụ thành các luồng nghiệp vụ hợp lệ, đảm bảo kiểm soát quyền ra vào phòng tập chuẩn xác, điều phối lịch dạy – lịch tập không xung đột, quản lý dòng tiền minh bạch và cung cấp báo cáo hỗ trợ ra quyết định kinh doanh.

---

## 2. Mô tả Luồng Nghiệp vụ Xuyên suốt

### 2.1. Cấu hình Nền tảng & Tiếp nhận Hội viên
Hành trình nghiệp vụ bắt đầu từ khâu thiết lập nền tảng và phân quyền hệ thống:
* **Quản trị viên (Admin):** Cấu hình danh mục các gói tập (thời hạn, đơn giá, số buổi), thiết lập các tham số vận hành chung (thời gian hoạt động, quy định bảo lưu, thời gian tối thiểu cho phép hủy lịch hẹn), đồng thời tạo tài khoản và phân quyền truy cập cho nhân sự theo từng vai trò cụ thể.
* **Lễ tân:** Khi có khách hàng mới, Lễ tân tiếp nhận thông tin, tạo hồ sơ hội viên và đăng ký gói tập phù hợp.

### 2.2. Thanh toán & Kích hoạt Dịch vụ
Khâu thanh toán và lập hóa đơn tiếp nối ngay sau đó:
* Hệ thống tự động tính toán tổng chi phí, áp dụng ưu đãi, ghi nhận hình thức thanh toán (tiền mặt, chuyển khoản VietQR, quẹt thẻ POS) và phát hành hóa đơn.
* Khi giao dịch hoàn tất thành công, gói tập được kích hoạt, mã định danh (mã thẻ / mã QR) được sinh ra và tài khoản cá nhân được cấp để hội viên có thể chủ động theo dõi thông tin gói tập của mình trên ứng dụng di động.

### 2.3. Tuyến Vận hành Hằng ngày: Điểm danh (Check-in)
Nghiệp vụ điểm danh (Check-in) đóng vai trò là chốt chặn kiểm soát ra vào:
* Lễ tân quét mã QR trên ứng dụng của hội viên, hoặc quẹt thẻ từ / nhập số điện thoại tra cứu tại quầy tiếp đón.
* Hệ thống lập tức tra cứu thông tin thẻ và kiểm tra tính hợp lệ trong thời gian thực:
  * Trạng thái thẻ có đang hoạt động (`ACTIVE`) hay không.
  * Còn hạn sử dụng hoặc còn số buổi tập hay không.
  * Có đang bị bảo lưu (`FROZEN`) hoặc khóa thẻ (`LOCKED`) hay không.
  * Có đi đúng khung giờ đăng ký hay không.
* **Kết quả:**
  * **Hợp lệ:** Xác nhận check-in thành công, tự động trừ 1 buổi tập (đối với gói tính theo lượt), lưu vết vào lịch sử ra vào trong ngày.
  * **Không hợp lệ:** Hệ thống phát tín hiệu từ chối và hiển thị rõ lý do vi phạm.

### 2.4. Điều phối Dịch vụ Huấn luyện viên Cá nhân (PT)
Đối với các dịch vụ tập luyện có huấn luyện viên cá nhân, hệ thống đảm nhiệm vai trò cầu nối điều phối giữa PT và Hội viên:
* **Phân công & Đăng ký lịch:** Admin thực hiện phân công PT phụ trách cho hội viên có gói tập PT tương ứng. PT chủ động truy cập vào hệ thống để theo dõi lịch dạy theo tuần và đăng ký các khung giờ làm việc rảnh (khả dụng).
* **Đặt lịch hẹn:** Dựa trên lịch rảnh của PT, Hội viên có thể tra cứu và chủ động đặt lịch tập, hoặc gửi yêu cầu đổi/hủy lịch hẹn khi có thay đổi kế hoạch.
* **Kiểm soát chống xung đột:** Hệ thống liên tục rà soát nhằm ngăn chặn tuyệt đối tình trạng trùng lịch giữa PT và học viên, đồng thời kiểm tra điều kiện số buổi PT còn lại trong gói.
* **Ghi nhận kết quả & Đánh giá:** Khi ca tập diễn ra, PT cập nhật các chỉ số phát triển thể chất của học viên (như cân nặng, chỉ số InBody định kỳ, ghi chú bài tập) và xác nhận hoàn thành ca tập (hệ thống tự động trừ 1 buổi tập). Sau buổi tập, hội viên có quyền chấm điểm số sao (1–5 sao) và để lại nhận xét đánh giá chất lượng của PT để nâng cao tính minh bạch dịch vụ.

### 2.5. Vòng đời Thẻ tập & Xử lý Ngoại lệ
Trong suốt vòng đời của gói tập, hệ thống linh hoạt hỗ trợ các nghiệp vụ phát sinh và xử lý ngoại lệ:
* **Gia hạn / Nâng cấp:** Khi thẻ sắp hoặc đã hết hạn, Lễ tân có thể thực hiện gia hạn hoặc nâng cấp thẻ sang gói mới và thu khoản phí chênh lệch.
* **Bảo lưu thẻ (Freeze):** Khi hội viên có nhu cầu tạm dừng do công tác hoặc ốm đau, tính năng bảo lưu cho phép tạm ngưng thẻ và tự động kéo dài ngày hết hạn tương ứng theo đúng chính sách cho phép.
* **Khóa / Hủy thẻ:** Trường hợp có vi phạm hoặc sự cố mất thẻ, Admin có thể khóa hoặc hủy thẻ để thu hồi quyền ra vào.
* **Hoàn tiền / Hủy hóa đơn:** Khi có phát sinh khiếu nại hoặc hủy dịch vụ hợp lệ, chức năng hoàn tiền/hủy hóa đơn sẽ điều chỉnh lại trạng thái thẻ và ghi nhận số tiền hoàn vào nhật ký dòng tiền của hệ thống.

### 2.6. Báo cáo Quản trị & An toàn Hệ thống
* **Báo cáo BI:** Cung cấp số liệu tổng thể phục vụ đánh giá hiệu quả kinh doanh. Admin có thể tra cứu và phân tích doanh thu theo khoảng thời gian, theo dõi lưu lượng điểm danh theo từng ca trong ngày, thống kê biến động số lượng hội viên mới/tái gia hạn, cũng như đánh giá hiệu suất giảng dạy và điểm hài lòng của đội ngũ PT, hỗ trợ xuất file Excel hoặc PDF để lưu trữ.
* **Bảo mật & Phân quyền (RBAC):** Mọi tác nhân chỉ được phép xem và thao tác trên dữ liệu thuộc phạm vi của mình, tài khoản có thể bị khóa khi cần thiết và toàn bộ các thao tác chỉnh sửa thông tin, điểm danh hay giao dịch tài chính đều được lưu vết phục vụ việc đối soát, vận hành ổn định.

---

## 3. Tóm tắt Luồng Nghiệp vụ Cốt lõi (B1 → B5)

| Bước | Tên giai đoạn | Luồng xử lý chi tiết |
| :---: | :--- | :--- |
| **B1** | **Cấu hình & Tiếp nhận hội viên** | Admin thiết lập danh mục gói tập, tham số vận hành và phân quyền tài khoản $\rightarrow$ Lễ tân tiếp nhận khách hàng, tạo hồ sơ hội viên mới. |
| **B2** | **Thanh toán & Kích hoạt gói tập** | Lập hóa đơn $\rightarrow$ Thu tiền (Tiền mặt / VietQR / Thẻ POS) $\rightarrow$ Hệ thống kích hoạt gói tập, tạo mã định danh (QR) và cấp tài khoản cho hội viên. |
| **B3** | **Điểm danh Check-in** | Quét mã QR hoặc nhập SĐT $\rightarrow$ Hệ thống kiểm tra điều kiện (hạn dùng, số buổi, khung giờ, trạng thái bảo lưu) $\rightarrow$ Ghi nhận lượt vào phòng và cập nhật lịch sử ra/vào. |
| **B4** | **Điều phối & Thực hiện dịch vụ PT** | Admin phân công PT $\rightarrow$ PT đăng ký lịch rảnh theo tuần $\rightarrow$ Hội viên đặt lịch tập (chống trùng lịch) $\rightarrow$ Tiến hành buổi tập, PT cập nhật chỉ số InBody và xác nhận hoàn thành (trừ 1 buổi tập) $\rightarrow$ Hội viên gửi đánh giá chất lượng PT. |
| **B5** | **Quản trị vòng đời thẻ & Báo cáo** | Thực hiện gia hạn, nâng cấp, bảo lưu hoặc khóa/hủy thẻ khi phát sinh $\rightarrow$ Xử lý hoàn tiền/hủy hóa đơn nếu cần $\rightarrow$ Tổng hợp và xuất báo cáo doanh thu, lưu lượng check-in và hiệu suất PT. |

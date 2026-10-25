# Sơ đồ Use Case Tổng Quát

<p align="center">
  <img src="https://github.com/user-attachments/assets/ed4d18a3-9994-4d0e-8752-2a8a88a752eb" alt="Sơ đồ Use Case Tổng Quát" style="max-width: 100%; height: auto;" />
</p>

---

## 1. Ma trận Phân quyền Tác nhân (Actor Matrix)

| STT | Use Case ID | Tên Use Case | Hội viên (Member) | PT (Trainer) | Quản trị viên (Admin) |
| :---: | :---: | :--- | :---: | :---: | :---: |
| 1.0 | **UC1** | Đăng nhập | x | x | x |
| 2.0 | **UC2** | Quản lý hội viên và gói tập | | | x |
| 3.0 | **UC3** | Thực hiện check-in điểm danh | | | x |
| 4.0 | **UC4** | Quản lý PT & ca dạy | | | x |
| 5.0 | **UC5** | Quản lý thanh toán & hóa đơn | | | x |
| 6.0 | **UC6** | Quản lý báo cáo & thống kê | | | x |
| 7.0 | **UC7** | Cài đặt & phân quyền | | | x |
| 8.0 | **UC8** | Quản lý lịch dạy & Học viên | | x | |
| 9.0 | **UC9** | Quản lý thông tin cá nhân & Gói tập | x | | |
| 10.0 | **UC10** | Đặt lịch tập với PT | x | | |

---

## 2. Danh mục 10 Phân hệ Use Case Tổng Quát

| STT | Mã Use Case | Tên Use Case | Mô tả chi tiết phạm vi nghiệp vụ | Tài liệu chi tiết |
| :---: | :---: | :--- | :--- | :---: |
| 1.0 | **UC1** | **Đăng nhập** | Cho phép người dùng đăng nhập vào hệ thống bằng tài khoản đã đăng ký. | [UC01.md](UC01.md) |
| 2.0 | **UC2** | **Quản lý hội viên và gói tập** | Cho phép Admin quản trị toàn diện vòng đời hội viên và dịch vụ tập luyện, bao gồm: đăng ký mới, cập nhật hồ sơ cá nhân, gia hạn/nâng cấp, bảo lưu và hủy đăng ký gói tập; đồng thời đồng bộ trạng thái sử dụng dịch vụ và ghi nhận lịch sử giao dịch trong hệ thống. | [UC02.md](UC02.md) |
| 3.0 | **UC3** | **Thực hiện check-in điểm danh** | Cho phép Admin thực hiện và kiểm soát quá trình điểm danh ra/vào của hội viên bằng mã QR hoặc mã định danh thủ công, đồng thời theo dõi toàn bộ lịch sử check-in trong ngày. | [UC03.md](UC03.md) |
| 4.0 | **UC4** | **Quản lý PT & ca dạy** | Cung cấp các chức năng giúp Admin quản lý hồ sơ nhân sự PT, phân công PT cho hội viên, thiết lập/tra cứu lịch dạy và kiểm soát, xác nhận kết quả các buổi tập luyện trong hệ thống. | [UC04.md](UC04.md) |
| 5.0 | **UC5** | **Quản lý thanh toán & hóa đơn** | Cung cấp các chức năng giúp Admin thực hiện lập hóa đơn thu tiền, tra cứu/in lại hóa đơn, xử lý hủy/hoàn tiền và theo dõi, xuất báo cáo thống kê doanh thu phòng gym. | [UC05.md](UC05.md) |
| 6.0 | **UC6** | **Quản lý báo cáo & thống kê** | Admin xem các báo cáo và số liệu thống kê của phòng tập theo các tiêu chí lọc và khoảng thời gian xác định. | [UC06.md](UC06.md) |
| 7.0 | **UC7** | **Cài đặt & phân quyền** | Cung cấp các chức năng giúp Quản trị viên cấp cao quản lý tài khoản và phân quyền nhân sự, cài đặt các tham số quy định vận hành, và quản lý danh mục gói tập của phòng gym. | [UC07.md](UC07.md) |
| 8.0 | **UC8** | **Quản lý lịch dạy & Học viên** | Cung cấp các chức năng cho Huấn luyện viên (PT) quản lý lịch biểu cá nhân, theo dõi học viên phụ trách, cập nhật tiến độ thể hình và xác nhận kết quả ca tập. | [UC08.md](UC08.md) |
| 9.0 | **UC9** | **Quản lý thông tin cá nhân & Gói tập** | Cung cấp các chức năng tự phục vụ giúp Hội viên xem/cập nhật thông tin cá nhân, tra cứu trạng thái gói tập và theo dõi chi tiết lịch sử điểm danh, tập luyện tại phòng gym. | [UC09.md](UC09.md) |
| 10.0 | **UC10** | **Đặt lịch tập với PT** | Cung cấp các chức năng cho phép Hội viên chủ động đặt lịch, theo dõi/hủy đổi ca tập với PT phụ trách và gửi phản hồi, đánh giá chất lượng sau mỗi buổi tập. | [UC10.md](UC10.md) |

---

## 3. Cây Phân Rã Toàn Bộ 37 Use Cases Chi Tiết

* **1.0 Phân hệ Xác thực:**
  * [UC01: Đăng nhập](UC01.md)
* **2.0 Phân hệ Quản lý Hội viên & Gói tập (Admin):**
  * [UC02.1: Đăng ký hội viên mới cùng gói tập](UC02.md)
  * [UC02.2: Cập nhật thông tin hội viên](UC02.md)
  * [UC02.3: Gia hạn / Nâng cấp gói tập](UC02.md)
  * [UC02.4: Bảo lưu gói tập](UC02.md)
  * [UC02.5: Hủy đăng ký gói tập](UC02.md)
* **3.0 Phân hệ Điểm danh & Check-in (Admin):**
  * [UC 3.1: Quét mã QR](UC03.md)
  * [UC 3.2: Nhập mã hội viên thủ công](UC03.md)
  * [UC 3.3: Tra cứu lịch sử ra/vào](UC03.md)
* **4.0 Phân hệ Quản lý PT & Ca dạy (Admin):**
  * [UC 4.1: Thêm PT mới](UC04.md)
  * [UC 4.2: Tra cứu lịch dạy](UC04.md)
  * [UC 4.3: Cập nhật thông tin PT](UC04.md)
  * [UC 4.4: Phân công cho PT](UC04.md)
  * [UC 4.5: Xác nhận kết quả buổi tập](UC04.md)
  * [UC 4.6: Xếp lịch dạy](UC04.md)
* **5.0 Phân hệ Quản lý Thanh toán & Hóa đơn (Admin):**
  * [UC5.1: Lập hóa đơn và Thu tiền](UC05.md)
  * [UC5.2: Tra cứu và in lại hóa đơn](UC05.md)
  * [UC5.3: Xử lý Hủy / Hoàn tiền hóa đơn](UC05.md)
  * [UC5.4: Xem và xuất báo cáo doanh thu](UC05.md)
* **6.0 Phân hệ Báo cáo & Thống kê (Admin):**
  * [UC06: Xem báo cáo và thống kê](UC06.md)
* **7.0 Phân hệ Cài đặt & Phân quyền (Super Admin):**
  * [UC07.1.1: Tạo mới tài khoản và phân quyền](UC07.md)
  * [UC 7.1.2: Cập nhật tài khoản và phân quyền](UC07.md)
  * [UC 7.1.3: Khóa / Mở khóa tài khoản](UC07.md)
  * [UC 7.2: Cài đặt tham số vận hành hệ thống](UC07.md)
  * [UC 7.3.1: Thêm gói tập](UC07.md)
  * [UC 7.3.2: Sửa gói tập](UC07.md)
  * [UC 7.3.3: Xóa gói tập](UC07.md)
* **8.0 Phân hệ Huấn luyện viên (PT):**
  * [UC 8.1.1: Đăng ký lịch rảnh](UC08.md)
  * [UC 8.1.2: Xem lịch dạy theo tuần](UC08.md)
  * [UC 8.2.1: Xem danh sách học viên](UC08.md)
  * [UC 8.2.2: Ghi chú tiến độ / Chỉ số (InBody)](UC08.md)
  * [UC 8.2.3: Xác nhận hoàn thành ca tập](UC08.md)
* **9.0 Phân hệ Cá nhân Hội viên (Member):**
  * [UC 9.1: Quản lý thông tin cá nhân](UC09.md)
  * [UC 9.2: Xem lịch sử tập luyện](UC09.md)
* **10.0 Phân hệ Đặt lịch & Đánh giá (Member):**
  * [UC 10.1: Đặt lịch tập](UC10.md)
  * [UC 10.2: Quản lý lịch tập](UC10.md)
  * [UC 10.3: Đánh giá](UC10.md)

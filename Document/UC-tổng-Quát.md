# Sơ đồ Use Case Tổng Quát

<p align="center">
  <img width="2655" height="3200" alt="image" src="https://github.com/user-attachments/assets/0864437d-f1b3-478b-8126-b3a33dd9daa9" />

</p>

---

## 1. Ma trận Phân quyền Tác nhân (Actor Matrix)

| STT | Use Case ID | Tên Use Case Phân Hệ | Hội viên (Member) | Huấn luyện viên (PT) | Quản trị viên (Admin / Staff) |
| :---: | :---: | :--- | :---: | :---: | :---: |
| 1.0 | **UC1** | Đăng nhập và Xác thực | x | x | x |
| 2.0 | **UC2** | Quản lý hội viên và gói tập | | | x |
| 3.0 | **UC3** | Thực hiện check-in điểm danh | | | x |
| 4.0 | **UC4** | Quản lý PT & ca dạy | | | x |
| 5.0 | **UC5** | Quản lý thanh toán & hóa đơn | | | x |
| 6.0 | **UC6** | Quản lý báo cáo & thống kê | | | x |
| 7.0 | **UC7** | Quản lý tài khoản & Danh mục gói tập | | | x |
| 8.0 | **UC8** | Quản lý lịch dạy & Học viên | | x | |
| 9.0 | **UC9** | Quản lý thông tin cá nhân & Gói tập | x | | |
| 10.0 | **UC10** | Đặt lịch tập với PT & Đánh giá | x | | |

---

## 2. Danh mục 10 Phân hệ Use Case Cấp cao

| STT | Mã Phân Hệ | Tên Phân Hệ | Tác nhân chính | Mô tả phạm vi nghiệp vụ cốt lõi | Tài liệu chi tiết |
| :---: | :---: | :--- | :---: | :--- | :---: |
| 1.0 | **UC1** | **Đăng nhập và Xác thực** | Member, PT, Admin | Xác thực danh tính người dùng bằng tài khoản đã cấp, kiểm tra trạng thái khóa tài khoản và điều hướng vào đúng không gian làm việc theo vai trò. | [UC01.md](UC01.md) |
| 2.0 | **UC2** | **Quản lý hội viên và gói tập** | Admin / Lễ tân | Quản trị trọn vẹn vòng đời hội viên: tiếp nhận đăng ký mới, cập nhật hồ sơ, gia hạn/nâng cấp gói, bảo lưu thời hạn và hủy đăng ký gói tập. | [UC02.md](UC02.md) |
| 3.0 | **UC3** | **Thực hiện check-in điểm danh** | Admin / Lễ tân | Kiểm soát quyền vào phòng tập thông qua quét mã QR hoặc nhập mã/SĐT thủ công; kiểm tra hạn dùng, khung giờ, trừ buổi tập và lưu vết lịch sử ra/vào. | [UC03.md](UC03.md) |
| 4.0 | **UC4** | **Quản lý PT & ca dạy** | Admin | Quản lý hồ sơ chuyên môn PT, phân công PT phụ trách cố định cho hội viên, xếp lịch làm việc theo ca và xử lý xác nhận kết quả các ca tập ngoại lệ. | [UC04.md](UC04.md) |
| 5.0 | **UC5** | **Quản lý thanh toán & hóa đơn** | Admin / Thu ngân | Lập hóa đơn thu tiền đa phương thức (Tiền mặt, QR, Thẻ POS), tra cứu/in hóa đơn, xử lý hoàn tiền/hủy hóa đơn và theo dõi báo cáo doanh thu tài chính. | [UC05.md](UC05.md) |
| 6.0 | **UC6** | **Quản lý báo cáo & thống kê** | Admin | Cung cấp các báo cáo quản trị trực quan đa chiều: doanh thu, biến động hội viên, lưu lượng check-in theo ca/giờ, hiệu suất làm việc của đội ngũ PT. | [UC06.md](UC06.md) |
| 7.0 | **UC7** | **Quản lý tài khoản & Danh mục gói tập** | Super Admin | Quản lý tài khoản nhân viên, phân quyền hạn truy cập (RBAC), khóa/mở khóa tài khoản; quản lý danh mục gói tập (thêm, sửa, chuyển trạng thái ngừng kinh doanh). | [UC07.md](UC07.md) |
| 8.0 | **UC8** | **Quản lý lịch dạy & Học viên** | Huấn luyện viên (PT) | Cổng tác nghiệp dành riêng cho PT: đăng ký khung giờ làm việc rảnh, theo dõi thời khóa biểu tuần, quản lý học viên phụ trách, cập nhật chỉ số InBody và xác nhận hoàn thành ca dạy. | [UC08.md](UC08.md) |
| 9.0 | **UC9** | **Quản lý thông tin cá nhân & Gói tập** | Hội viên (Member) | Cổng tự phục vụ dành cho hội viên: xem thông tin cá nhân, kiểm tra thời hạn và số buổi còn lại của gói tập, cập nhật thông tin liên hệ và tra cứu lịch sử tập luyện. | [UC09.md](UC09.md) |
| 10.0 | **UC10** | **Đặt lịch tập với PT & Đánh giá** | Hội viên (Member) | Cho phép hội viên tra cứu lịch rảnh của PT phụ trách, chủ động đặt lịch tập, đổi ca tập, hủy lịch hẹn theo chính sách thời gian và chấm điểm sao, nhận xét chất lượng sau buổi tập. | [UC10.md](UC10.md) |

---

## 3. Cây Phân Rã Chi Tiết 37 Use Cases

* **1.0 Phân hệ Xác thực:**
  * [UC01: Đăng nhập](UC01.md)
* **2.0 Phân hệ Quản lý Hội viên & Gói tập (Admin / Lễ tân):**
  * [UC02.1: Đăng ký hội viên mới cùng gói tập](UC02.md)
  * [UC02.2: Cập nhật thông tin hội viên](UC02.md)
  * [UC02.3: Gia hạn / Nâng cấp gói tập](UC02.md)
  * [UC02.4: Bảo lưu gói tập](UC02.md)
  * [UC02.5: Hủy đăng ký gói tập](UC02.md)
* **3.0 Phân hệ Điểm danh & Check-in (Admin / Lễ tân):**
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
* **5.0 Phân hệ Quản lý Thanh toán & Hóa đơn (Admin / Thu ngân):**
  * [UC5.1: Lập hóa đơn và Thu tiền](UC05.md)
  * [UC5.2: Tra cứu và in lại hóa đơn](UC05.md)
  * [UC5.3: Xử lý Hủy / Hoàn tiền hóa đơn](UC05.md)
  * [UC5.4: Xem và xuất báo cáo doanh thu](UC05.md)
* **6.0 Phân hệ Báo cáo & Thống kê (Admin):**
  * [UC06: Xem báo cáo và thống kê](UC06.md)
* **7.0 Phân hệ Quản lý Tài khoản & Gói tập (Super Admin):**
  * [UC 7.1.1: Tạo mới tài khoản và phân quyền](UC07.md)
  * [UC 7.1.2: Cập nhật tài khoản và phân quyền](UC07.md)
  * [UC 7.1.3: Khóa / Mở khóa tài khoản](UC07.md)
  * [UC 7.2.1: Thêm gói tập](UC07.md)
  * [UC 7.2.2: Sửa gói tập](UC07.md)
  * [UC 7.2.3: Xóa gói tập](UC07.md)
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
  * [UC 10.2: Hủy lịch tập](UC10.md)
  * [UC 10.2: Hủy lịch tập](UC10.md)
  * [UC 10.3: Đánh giá](UC10.md)
  * [UC 10.4: Đổi lịch tập](UC10.md)

---

## 4. Bảng Đặc Tả Tóm Tắt Toàn Bộ 37 Use Cases Chi Tiết

| STT | Mã Use Case | Tên Use Case | Tác nhân | Tóm tắt mục tiêu nghiệp vụ | Tiền điều kiện | Hậu điều kiện chính |
| :---: | :---: | :--- | :---: | :--- | :--- | :--- |
| 1 | **UC01** | Đăng nhập | All | Người dùng đăng nhập vào hệ thống bằng tài khoản được cấp. | Đã có tài khoản trong hệ thống. | Cấp JWT/phiên làm việc, điều hướng đúng phân hệ. |
| 2 | **UC02.1** | Đăng ký hội viên mới cùng gói tập | Admin/Lễ tân | Tạo hồ sơ khách hàng, chọn gói tập, thu tiền và kích hoạt thẻ. | Đã đăng nhập, vào màn hình quản lý hội viên. | Thẻ kích hoạt trạng thái `ACTIVE`, gửi thông báo chào mừng. |
| 3 | **UC02.2** | Cập nhật thông tin hội viên | Admin | Tìm kiếm và điều chỉnh thông tin liên lạc, địa chỉ của hội viên. | Hội viên đã tồn tại trong CSDL. | Hồ sơ hội viên được cập nhật dữ liệu mới. |
| 4 | **UC02.3** | Gia hạn / Nâng cấp gói tập | Admin | Tăng thêm thời hạn hoặc chuyển gói tập lên hạng cao hơn. | Hội viên đã có gói tập trong hệ thống. | Ngày hết hạn mới được dời, cập nhật gói mới, ghi nhận thu tiền. |
| 5 | **UC02.4** | Bảo lưu gói tập | Admin | Tạm ngưng hiệu lực thẻ theo số ngày hợp lệ và bù thời hạn sau. | Thẻ đủ điều kiện bảo lưu theo quy chế gói. | Trạng thái chuyển `FROZEN`, ngày hết hạn được kéo dài tương ứng. |
| 6 | **UC02.5** | Hủy đăng ký gói tập | Admin | Chấm dứt hiệu lực gói tập hiện tại của hội viên khi có vi phạm/yêu cầu. | Thẻ đang hoạt động trong hệ thống. | Trạng thái thẻ chuyển `Đã hủy` (không xóa gói khỏi danh mục). |
| 7 | **UC 3.1** | Quét mã QR | Admin/Lễ tân | Quét mã QR trên ứng dụng hội viên để kiểm soát vào phòng tập. | Màn hình check-in kết nối camera sẵn sàng. | Ghi nhận lượt check-in, trừ 1 buổi (nếu tính theo lượt). |
| 8 | **UC 3.2** | Nhập mã hội viên thủ công | Admin/Lễ tân | Nhập SĐT hoặc mã hội viên để điểm danh khi khách không mang điện thoại. | Màn hình check-in mở sẵn sàng. | Tra cứu thông tin, đối chiếu khuôn mặt, ghi nhận check-in. |
| 9 | **UC 3.3** | Tra cứu lịch sử ra/vào | Admin/Lễ tân | Theo dõi danh sách điểm danh trong ngày theo ca, tên, trạng thái. | Đã đăng nhập hệ thống. | Danh sách lượt check-in hiển thị theo bộ lọc thời gian thực. |
| 10 | **UC 4.1** | Thêm PT mới | Admin | Tạo mới hồ sơ nhân sự PT kèm chuyên môn và ảnh đại diện. | Có quyền quản trị nhân sự. | Lưu hồ sơ PT mới vào CSDL, sẵn sàng phân công ca dạy. |
| 11 | **UC 4.2** | Tra cứu lịch dạy | Admin | Lọc và tra cứu tình trạng ca dạy của PT (trống, đã đặt, hoàn thành). | Đã có dữ liệu PT và lịch dạy. | Hiển thị chi tiết lịch làm việc và danh sách học viên đặt lịch. |
| 12 | **UC 4.3** | Cập nhật thông tin PT | Admin | Chỉnh sửa hồ sơ chuyên môn hoặc chuyển trạng thái hoạt động của PT. | PT đã tồn tại trong hệ thống. | Cập nhật thông tin và phản ánh lên lịch làm việc. |
| 13 | **UC 4.4** | Phân công cho PT | Admin | Gán cố định một PT phụ trách cho hội viên có gói tập PT. | Hội viên có gói PT, PT còn khả năng nhận thêm học viên. | Gán PT cố định vào hồ sơ hội viên; hội viên chỉ đặt lịch với PT này. |
| 14 | **UC 4.5** | Xác nhận kết quả buổi tập | Admin | Xử lý các ca tập đến/quá giờ (Hoàn thành, Vắng mặt, Hoãn) để trừ/hoàn buổi. | Tồn tại buổi tập đã đến hoặc quá thời gian diễn ra. | Cập nhật trạng thái ca tập, tự động trừ hoặc hoàn trả buổi tập. |
| 15 | **UC 4.6** | Xếp lịch dạy | Admin | Thiết lập khung giờ làm việc theo ca tuần/tháng cho PT. | PT đang hoạt động; phòng tập còn chỗ trống. | Lưu các ca làm việc khả dụng để mở cho học viên đặt lịch. |
| 16 | **UC5.1** | Lập hóa đơn và Thu tiền | Admin/Thu ngân | Lập phiếu thu tiền gói tập, dịch vụ PT, phụ kiện qua Tiền mặt/QR/Thẻ. | Hội viên và dịch vụ thanh toán hợp lệ. | Lưu hóa đơn `PAID`, kích hoạt gói dịch vụ, gửi hóa đơn điện tử. |
| 17 | **UC5.2** | Tra cứu và in lại hóa đơn | Admin/Thu ngân | Tìm kiếm lịch sử hóa đơn, thực hiện in nhiệt hoặc xuất file PDF. | Hóa đơn đã tồn tại trong hệ thống. | Xuất file PDF hoặc gửi lệnh in nhiệt thành công. |
| 18 | **UC5.3** | Xử lý Hủy / Hoàn tiền hóa đơn | Admin | Xử lý hoàn tiền cho hội viên theo đúng chính sách hoàn hủy của cơ sở. | Hóa đơn hợp lệ và còn trong thời hạn hoàn tiền. | Chuyển trạng thái `Đã hủy/Hoàn tiền`, ghi giảm doanh thu trong sổ quỹ. |
| 19 | **UC5.4** | Xem và xuất báo cáo doanh thu | Admin | Tổng hợp dữ liệu doanh thu theo mốc thời gian và xuất file Excel/PDF. | Đã đăng nhập vào hệ thống. | Báo cáo doanh thu hiển thị trực quan và tải file về máy. |
| 20 | **UC06** | Xem báo cáo và thống kê | Admin | Xem các báo cáo kinh doanh: doanh thu, hội viên, điểm danh, PT. | Có quyền truy cập báo cáo quản trị. | Hiển thị bảng số liệu chi tiết kèm biểu đồ trực quan hóa. |
| 21 | **UC 7.1.1** | Tạo mới tài khoản và phân quyền | Super Admin | Cấp tài khoản nhân viên mới và gán vai trò quyền hạn (RBAC). | Đăng nhập bằng tài khoản Quản trị cấp cao nhất. | Lưu tài khoản nhân viên mới vào CSDL với quyền hạn tương ứng. |
| 22 | **UC 7.1.2** | Cập nhật tài khoản và phân quyền | Super Admin | Chỉnh sửa quyền hoặc thông tin nhân sự; chặn tự tước quyền của chính mình. | Tài khoản nhân sự cần sửa đã tồn tại. | Thông tin và quyền hạn mới được cập nhật vào CSDL. |
| 23 | **UC 7.1.3** | Khóa / Mở khóa tài khoản | Super Admin | Khóa truy cập tài khoản vi phạm hoặc mở khóa; ngăn tự khóa tài khoản mình. | Tài khoản nhân sự đã tồn tại. | Cập nhật trạng thái `ACTIVE`/`LOCKED` trong CSDL. |
| 24 | **UC 7.2.1** | Thêm gói tập | Super Admin | Tạo mới gói tập vào danh mục (đơn giá, thời hạn, số buổi, quyền lợi). | Đăng nhập bằng quyền Quản trị cấp cao nhất. | Gói tập mới hiển thị trên danh mục bán lẻ của phòng gym. |
| 25 | **UC 7.2.2** | Sửa gói tập | Super Admin | Điều chỉnh giá bán, thời hạn hoặc đổi trạng thái kinh doanh của gói. | Gói tập cần sửa đã tồn tại trong danh mục. | Thông tin gói tập mới được cập nhật trong CSDL. |
| 26 | **UC 7.2.3** | Xóa gói tập | Super Admin | Xóa gói tập chưa phát sinh giao dịch; nếu đã có hội viên thì chuyển ngừng bán. | Gói tập tồn tại trong danh mục. | Xóa khỏi CSDL hoặc chuyển trạng thái `Ngừng kinh doanh`. |
| 27 | **UC 8.1.1** | Đăng ký lịch rảnh | PT | PT chủ động đăng ký các ca rảnh trong tuần; chặn hủy ca đã có người đặt. | Đăng nhập tài khoản PT hợp lệ. | Lịch rảnh được lưu vào CSDL để học viên phụ trách nhìn thấy. |
| 28 | **UC 8.1.2** | Xem lịch dạy theo tuần | PT | Tra cứu lịch làm việc biểu diễn theo lưới thời gian các ngày trong tuần. | Đăng nhập tài khoản PT hợp lệ. | Lịch dạy chi tiết hiển thị trực quan theo tuần. |
| 29 | **UC 8.2.1** | Xem danh sách học viên | PT | Theo dõi học viên đang trực tiếp phụ trách, số buổi còn lại và lịch sử tập. | Đăng nhập tài khoản PT hợp lệ. | Danh sách học viên và thông tin gói tập hiển thị đầy đủ. |
| 30 | **UC 8.2.2** | Ghi chú tiến độ / Chỉ số (InBody) | PT | Nhập số đo InBody định kỳ (cân nặng, % mỡ, cơ) và ghi chú buổi tập. | Đã chọn học viên trong danh sách phụ trách. | Bản ghi InBody mới được lưu vào CSDL theo thời gian. |
| 31 | **UC 8.2.3** | Xác nhận hoàn thành ca tập | PT | Xác nhận ca tập đã/đang diễn ra, hệ thống tự động trừ 1 buổi tập của học viên. | Ca tập đã đến giờ hoặc đang diễn ra. | Trạng thái ca chuyển `Hoàn thành`, trừ 1 buổi trong gói học viên. |
| 32 | **UC 9.1** | Quản lý thông tin cá nhân | Member | Hội viên tra cứu tình trạng gói tập và tự sửa thông tin liên hệ cho phép. | Đăng nhập tài khoản hội viên hợp lệ. | Thông tin cá nhân mới được cập nhật; xem rõ số buổi và hạn dùng. |
| 33 | **UC 9.2** | Xem lịch sử tập luyện | Member | Xem lại toàn bộ lịch sử điểm danh check-in và ca tập PT đã tham gia. | Đăng nhập tài khoản hội viên hợp lệ. | Dòng thời gian lịch sử điểm danh và ca tập hiển thị theo bộ lọc. |
| 34 | **UC 10.1** | Đặt lịch tập | Member | Tra cứu ca trống của PT phụ trách cố định và gửi yêu cầu giữ chỗ. | Gói PT còn buổi; đã được gán PT phụ trách cố định. | Lưu lịch hẹn mới; ca dạy PT chuyển sang trạng thái đã đặt. |
| 35 | **UC 10.2** | Hủy lịch tập | Member | Hủy ca tập đã đặt; hoàn buổi nếu trước giờ quy định, cảnh báo phạt nếu trễ. | Có lịch tập sắp tới ở trạng thái đã đặt. | Chuyển ca sang `Đã hủy`, mở lại ca cho PT; hoàn buổi nếu đúng hạn. |
| 36 | **UC 10.3** | Đánh giá | Member | Chấm điểm sao (1–5) và nhận xét chất lượng ca tập sau khi hoàn tất. | Buổi tập ở trạng thái `Hoàn thành` chưa được đánh giá. | Lưu đánh giá vào CSDL, cập nhật điểm đánh giá trung bình của PT. |
| 37 | **UC 10.4** | Đổi lịch tập | Member | Chuyển ca tập đã đặt sang một ca trống khác của cùng PT phụ trách. | Có lịch tập sắp tới đã đặt thành công. | Giải phóng ca cũ, chuyển thông tin lịch tập sang ca mới. |

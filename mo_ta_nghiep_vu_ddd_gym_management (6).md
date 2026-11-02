# Mô Tả Nghiệp Vụ & Thiết Kế ERD Hệ Thống Quản Lý Phòng Tập Thể Thao (Chuẩn DDD & Modular Monolith)

> Tài liệu này chuẩn hóa lại toàn bộ mô tả nghiệp vụ và thiết kế cơ sở dữ liệu tinh gọn (ERD) của **Hệ thống Quản lý Phòng tập thể thao (Gym Management System)** theo phương pháp luận **Domain-Driven Design (DDD)** và kiến trúc **Modular Monolith**, tham chiếu trực tiếp cách tiếp cận của bộ khung mẫu [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd).

## 1. Mô Tả Nghiệp Vụ Tổng Quan (Ubiquitous Language Narrative)

### 1.1. Bối cảnh Miền nghiệp vụ (Domain Context)

Miền nghiệp vụ **Quản lý Phòng tập thể thao (Gym Management Domain)** vận hành như một mạng lưới tương tác đa bên liên đới chặt chẽ giữa **3 tác nhân nghiệp vụ chính** (đều kế thừa từ khái niệm định danh `User` trong ngữ cảnh kiểm soát truy cập):

* **Quản trị viên (`Administrator` / `Admin`)**: Tác nhân điều phối và vận hành toàn diện tại cơ sở — trực tiếp cấu hình danh mục gói tập, tiếp nhận khách hàng mới (tạo tài khoản hội viên đồng thời đăng ký gói tập), xử lý thanh toán – hóa đơn, kiểm soát điểm danh (`CheckIn`), phân công huấn luyện viên, quản trị vòng đời gói tập (gia hạn, nâng cấp, bảo lưu, khóa/hủy gói tập, hoàn tiền) và truy xuất báo cáo kinh doanh.
* **Huấn luyện viên Cá nhân (`PersonalTrainer` / `PT`)**: Tác nhân cung cấp dịch vụ chuyên môn — chủ động đăng ký lịch làm việc rảnh theo tuần, trực tiếp hướng dẫn tập luyện, cập nhật chỉ số thể chất (`InBodyMetrics`) và xác nhận hoàn thành ca tập trên hệ thống.
* **Hội viên (`Member`)**: Tác nhân sử dụng dịch vụ — sử dụng tài khoản được cấp để theo dõi thông tin gói tập và mã QR trên ứng dụng cá nhân, thực hiện điểm danh ra vào phòng tập, chủ động đặt/đổi/hủy lịch tập cùng PT và chấm điểm, đánh giá chất lượng dịch vụ sau mỗi buổi tập.

Trọng tâm của **Domain Model** là chuyển hóa các yêu cầu tập luyện và đăng ký gói tập thành các luồng trạng thái hợp lệ thông qua việc thực thi nghiêm ngặt các **Quy tắc nghiệp vụ bất biến (`Business Rules` / `Invariants`)** ngay bên trong các **Aggregate Root**. Kiến trúc phân tách hệ thống thành các **Bounded Context (Modules)** độc lập về dữ liệu, giao tiếp bất đồng bộ qua **Integration Events** nhằm đảm bảo kiểm soát quyền ra vào chuẩn xác, điều phối lịch dạy – lịch tập không xung đột, quản lý dòng tiền minh bạch và cung cấp mô hình đọc (**CQRS Read Model**) phục vụ báo cáo quản trị.

### 1.2. Mô tả Nghiệp vụ theo từng Bounded Context (Theo chuẩn Section 2.1 của `kgrzybek/modular-monolith-with-ddd`)

#### A. Quản trị Danh mục Gói tập & Phân công PT (`Administration` Bounded Context)

Thực thể nghiệp vụ trung tâm trong ngữ cảnh này là **Danh mục Gói tập (`PackagePlan`)**:

* **`Administrator`** chịu trách nhiệm thiết lập và duy trì danh mục các **`PackagePlan`**. Mỗi `PackagePlan` định nghĩa rõ các **Value Objects** gồm: thời hạn hiệu lực (`PackageDuration` tính theo ngày), đơn giá niêm yết (`MoneyValue`), tổng số buổi tập cho phép (`SessionQuota` — áp dụng cho gói tính theo lượt), cờ phân loại dịch vụ có kèm huấn luyện viên (`IncludesPersonalTrainer`) và số buổi PT đi kèm (`PtSessionQuota`).
* Đối với các hội viên đăng ký gói tập có dịch vụ PT, **`Administrator`** thực hiện nghiệp vụ phân công **`PersonalTrainer`** phụ trách cho **`Member`**.

#### B. Quản lý Định danh & Kiểm soát Truy cập (`UserAccess` Bounded Context)

* Mỗi **`Administrator`**, **`PersonalTrainer`** và **`Member`** trong hệ thống đều là một **`User`** được định danh duy nhất.
* **`Administrator`** khởi tạo tài khoản và phân quyền truy cập theo từng vai trò cụ thể (**`Role`**: `Administrator`, `PersonalTrainer`, `Member`). Mỗi `Role` sở hữu tập quyền hạn (**`Permission`**) xác định rõ người dùng có được phép thực thi một **`Command`** hoặc truy vấn một **`Query`** cụ thể hay không (**RBAC - Role-Based Access Control**).
* Khi tiếp nhận khách hàng mới, **`Administrator`** trực tiếp tạo tài khoản cá nhân cho **`Member`** (cấp quyền truy cập ứng dụng hội viên) đồng thời thực hiện đăng ký gói tập cho hội viên ngay trong quy trình tiếp nhận.
* Mọi **`User`** chỉ được phép xem và thao tác trên phạm vi dữ liệu thuộc thẩm quyền của mình. **`Administrator`** có quyền khóa tài khoản (`LockUserAccount`) khi cần thiết.

#### C. Thanh toán & Quản lý Dòng tiền (`Payments` / `Billing` Bounded Context)

Các thực thể nghiệp vụ chính bao gồm **Hóa đơn (`Invoice`)** và các bút toán thanh toán / hoàn tiền:

* Ngay sau khi **`Administrator`** tạo tài khoản và đăng ký gói tập mới (hoặc thực hiện gia hạn, nâng cấp gói tập) cho **`Member`**, hệ thống tự động khởi tạo một **`Invoice`**. Aggregate `Invoice` tính toán tổng chi phí dựa trên đơn giá `PackagePlan`, áp dụng ưu đãi/khuyến mãi (`DiscountAmount`) hoặc tính toán khoản phí chênh lệch khi nâng cấp gói (`UpgradePriceDifference`).
* **`Administrator`** ghi nhận thanh toán theo hình thức thực tế (`PaymentMethod`: Tiền mặt `Cash`, Chuyển khoản `QrTransfer`, hoặc Quẹt thẻ ngân hàng/POS `BankCard`) và phát hành hóa đơn.
* Khi giao dịch hoàn tất thành công (`Paid`), hệ thống phát sinh sự kiện miền **`InvoicePaidDomainEvent`** và xuất bản sự kiện tích hợp **`InvoicePaidIntegrationEvent`** qua **Events Bus** để kích hoạt gói tập đã đăng ký của hội viên bên module `Memberships`.
* Khi phát sinh khiếu nại hoặc hủy dịch vụ hợp lệ, **`Administrator`** thực thi chức năng hoàn tiền / hủy hóa đơn (`ProcessRefundCommand`). Hệ thống ghi nhận số tiền hoàn (`RefundAmount`) và phát sự kiện **`PaymentRefundedIntegrationEvent`** để điều chỉnh lại trạng thái gói tập tương ứng.

#### D. Quản lý Hội viên, Vòng đời Gói tập & Kiểm soát Ra vào (`Memberships` & `CheckIn` Bounded Context)

Đây là ngữ cảnh nghiệp vụ cốt lõi quản lý **Gói tập đã đăng ký của Hội viên (`MemberPackageSubscription`)** và **Lượt Điểm danh (`CheckInRecord`)**:

* **Tiếp nhận, Đăng ký & Kích hoạt Gói tập**: Khi có khách hàng mới, **`Administrator`** tiếp nhận thông tin, tạo tài khoản **`User` (`Member`)** đồng thời đăng ký gói tập phù hợp (**`MemberPackageSubscription`** khởi tạo ở trạng thái chờ thanh toán `PendingPayment`). Ngay khi nhận được sự kiện thanh toán hoàn tất từ module `Payments`, `MemberPackageSubscription` được kích hoạt sang trạng thái hoạt động (`Active`), đồng thời sinh mã định danh **QR (`QrCode`)** gắn với gói tập để hội viên chủ động theo dõi thông tin gói tập và sử dụng điểm danh trên ứng dụng cá nhân.
* **Kiểm soát Điểm danh (`CheckIn`) thời gian thực**: Ở tuyến vận hành hằng ngày, **`Administrator`** quét mã `QrCode` trên ứng dụng của hội viên hoặc nhập số điện thoại / thông tin định danh của **`Member`** tại quầy tiếp đón. Hệ thống lập tức tra cứu gói tập của hội viên và thực thi phương thức `CheckIn()` trên Aggregate **`MemberPackageSubscription`**, kiểm tra đồng thời các **Business Rules** trong thời gian thực:
  1. Gói tập của hội viên có đang ở trạng thái hoạt động (`Active`) hay không (`SubscriptionMustBeActiveRule`).
  2. Gói tập có đang bị bảo lưu (`Frozen`) hoặc bị khóa/hủy (`Suspended` / `Cancelled`) hay không (`SubscriptionMustNotBeFrozenOrSuspendedRule`).
  3. Gói tập còn hạn sử dụng (`ExpirationDate`) và còn số buổi tập khả dụng (`RemainingSessions > 0` đối với gói tính theo lượt) hay không (`SubscriptionMustHaveRemainingValidityAndQuotaRule`).
* Nếu đáp ứng đầy đủ điều kiện, hệ thống xác nhận check-in thành công, tự động trừ `1` buổi tập (đối với gói tính theo lượt) và lưu vết bản ghi **`CheckInRecord`** vào lịch sử ra vào trong ngày. Nếu vi phạm bất kỳ quy tắc nào, Domain Model ném ngoại lệ nghiệp vụ (`BusinessRuleValidationException`) và hiển thị cảnh báo từ chối kèm lý do rõ ràng.
* **Quản trị Vòng đời Gói tập & Xử lý Ngoại lệ**:
  * **Gia hạn (`Renew`) & Nâng cấp (`Upgrade`)**: Khi gói tập sắp hoặc đã hết hạn, **`Administrator`** thực hiện gia hạn hoặc nâng cấp sang gói tập mới (`PackagePlan`) và thu khoản phí gia hạn (hoặc phí chênh lệch) thông qua luồng thanh toán.
  * **Bảo lưu (`Freeze`)**: Khi **`Member`** có nhu cầu tạm dừng tập luyện do công tác hoặc ốm đau, **`Administrator`** thực hiện nghiệp vụ bảo lưu (`FreezeSubscription`) để tạm ngưng gói tập sang trạng thái `Frozen` và tự động kéo dài ngày hết hạn (`ExpirationDate`) tương ứng với số ngày bảo lưu.
  * **Khóa / Hủy Gói tập (`Suspend` / `Cancel`)**: Trường hợp hội viên vi phạm nội quy hoặc phát sinh sự cố cần thu hồi quyền truy cập, **`Administrator`** thực thi lệnh khóa hoặc hủy gói tập để vô hiệu hóa mã `QrCode` và thu hồi quyền ra vào phòng tập ngay lập tức.

#### E. Điều phối & Thực hiện Dịch vụ Huấn luyện viên Cá nhân (`PersonalTraining` Bounded Context)

Ngữ cảnh này đảm nhiệm vai trò cầu nối điều phối giữa **`PersonalTrainer` (PT)** và **`Member`** dưới sự phân công của **`Administrator`**, xoay quanh các thực thể **Lịch làm việc khả dụng (`TrainerAvailabilitySchedule`)**, **Buổi tập PT (`TrainingSession`)**, **Chỉ số Thể chất (`BodyMetricsRecord` / `InBody`)** và **Đánh giá Chất lượng PT (`TrainerReview`)**:

* **Đăng ký Lịch rảnh**: **`PersonalTrainer`** chủ động truy cập hệ thống để theo dõi lịch dạy theo tuần và đăng ký các khung giờ làm việc rảnh (`AvailableTimeSlot`) trên Aggregate **`TrainerAvailabilitySchedule`**.
* **Đặt lịch, Đổi & Hủy lịch hẹn**: Dựa trên lịch rảnh của PT phụ trách, **`Member`** tra cứu và chủ động đặt lịch tập (`BookTrainingSession`), hoặc gửi yêu cầu đổi/hủy lịch hẹn (`Reschedule` / `CancelTrainingSession`) khi có thay đổi kế hoạch. Lớp quy tắc kiểm soát lịch của Domain Model liên tục rà soát các **Invariants**:
  1. Ngăn chặn tuyệt đối tình trạng xung đột giờ dạy — không trùng lịch giữa PT và học viên (`NoOverlappingTrainingSessionRule`).
  2. Kiểm tra điều kiện số buổi PT còn lại trong gói tập của hội viên phải lớn hơn 0 (`MemberMustHaveAvailablePtSessionsRule`).
* **Thực hiện Ca tập & Ghi nhận InBody**: Khi buổi tập diễn ra, **`PersonalTrainer`** cập nhật các chỉ số phát triển thể chất của học viên (cân nặng, chỉ số `InBody` định kỳ, ghi chú bài tập `WorkoutNotes`) và thực hiện xác nhận hoàn thành ca tập (`CompleteTrainingSession`). Hành động này phát sự kiện **`TrainingSessionCompletedIntegrationEvent`** để tự động trừ `1` buổi tập trong gói tập PT của hội viên.
* **Đánh giá Chất lượng PT**: Sau mỗi buổi tập đã hoàn tất (`Completed`), **`Member`** có quyền chấm điểm số sao (`StarRating` từ 1–5) và để lại nhận xét đánh giá chất lượng của PT (`TrainerReview`) để nâng cao tính minh bạch dịch vụ.

#### F. Báo cáo & Thống kê Quản trị (`Reporting` Bounded Context / CQRS Read Model)

* Tuân thủ nguyên tắc **CQRS (Command Query Responsibility Segregation)** từ `kgrzybek/modular-monolith-with-ddd`, các báo cáo ở tầng quản trị tách biệt hoàn toàn khỏi Write Domain Model và truy vấn trực tiếp trên các **Database Views / Read Models** giúp **`Administrator`** đánh giá hiệu quả kinh doanh:
  * Tra cứu và phân tích doanh thu theo khoảng thời gian.
  * Theo dõi lưu lượng điểm danh (`CheckIn` traffic) theo từng ca trong ngày.
  * Thống kê biến động số lượng hội viên đăng ký mới và hội viên tái gia hạn/nâng cấp gói tập.
  * Đánh giá hiệu suất giảng dạy (số buổi hoàn tất) và điểm hài lòng trung bình (`AverageRating`) của đội ngũ **`PersonalTrainer`**.
* Hỗ trợ kết xuất dữ liệu báo cáo ra định dạng tệp **Excel (`.xlsx`)** hoặc **PDF (`.pdf`)** để lưu trữ.

## 2. Kiến Trúc Phân Rã Module & Ma Trận Ánh Xạ DDD (Modular Monolith)

| Bounded Context (Module) | DB Schema | Tác nhân tương tác (`Role`) | Aggregate Roots | Value Objects & Entities tiêu biểu | Domain Rules (Invariants) trọng tâm |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`UserAccess`** | `[users]` | `Administrator`, `PersonalTrainer`, `Member` | `User`, `Role` | `UserId`, `RoleId`, `RoleName` (`ADMIN`, `PT`, `MEMBER`), `AccountStatus` | `UserAccountMustBeActiveToAuthenticateRule`, `OnlyAuthorizedRoleCanExecuteCommandRule` |
| **`Administration`** | `[administration]` | `Administrator` | `PackagePlan` | `PackagePlanId`, `MoneyValue`, `PackageDuration`, `SessionQuota` | `PackagePriceMustBePositiveRule`, `PtAssignmentRequiresPtEligiblePackageRule` |
| **`Payments`** | `[payments]` | `Administrator`, `Member` *(xem HĐ)* | `Invoice` | `InvoiceId`, `PaymentMethod` (`Cash`, `QrTransfer`, `BankCard`), `DiscountValue` | `InvoiceCanOnlyBePaidOnceRule`, `RefundAmountCannotExceedPaidAmountRule` |
| **`Memberships`** | `[memberships]` | `Administrator`, `Member` | `MemberPackageSubscription`, `CheckInRecord` | `SubscriptionId`, `QrCodeIdentity`, `SubscriptionStatus`, `FreezePeriod` | `SubscriptionMustBeActiveForCheckInRule`, `SubscriptionMustNotBeFrozenOrSuspendedRule`, `SubscriptionMustHaveRemainingValidityAndQuotaRule` |
| **`PersonalTraining`** | `[training]` | `Administrator`, `PersonalTrainer`, `Member` | `TrainerAvailabilitySchedule`, `TrainingSession` | `SessionId`, `TimeSlot`, `InBodyMetrics`, `WorkoutNote`, `StarRating` | `NoOverlappingTrainingSessionRule`, `MemberMustHaveRemainingPtQuotaRule`, `ReviewOnlyAllowedForCompletedSessionRule` |
| **`Reporting`** | `[reporting]` *(Views)* | `Administrator` | *(CQRS Read-Only Projections)* | `RevenueReportDto`, `ShiftCheckInTrafficDto`, `MemberRetentionDto`, `PtPerformanceDto` | *(Truy vấn qua Raw SQL / Dapper trên Database Views, xuất file Excel/PDF)* |

## 3. Tóm Tắt Luồng Nghiệp Vụ Cốt Lõi Theo Chuẩn DDD & Event Storming (B1 → B5)

### B1: Cấu hình Gói tập, Tạo Tài khoản & Đăng ký Gói tập cho Hội viên (`Administration`, `UserAccess` & `Memberships`)

* **Thiết lập Danh mục Gói tập & Phân quyền (`Administration` & `UserAccess`)**:
  * `Administrator` thực thi `CreatePackagePlanCommand` để cấu hình danh mục gói tập (`PackagePlan`: thời hạn, đơn giá, số buổi tập, số buổi PT).
  * `Administrator` thực thi `CreateUserCommand` để tạo tài khoản và gán vai trò (`Role`: `Administrator`, `PersonalTrainer`, `Member`).
* **Tiếp nhận Hội viên, Cấp Tài khoản & Đăng ký Gói tập (`UserAccess` & `Memberships`)**:
  * Khi có khách hàng mới, `Administrator` tiếp nhận thông tin, thực thi `OnboardNewMemberCommand`: tạo tài khoản `User` (gán `RoleId` của `Member`) đồng thời đăng ký gói tập `MemberPackageSubscription` ở trạng thái chờ thanh toán (`PendingPayment`).
  * Phát sinh sự kiện tích hợp **`MemberPackageRegisteredIntegrationEvent`** gửi sang module `Payments` để lập hóa đơn.

### B2: Thanh toán & Kích hoạt Gói tập (`Payments` → `Memberships`)

* **Lập Hóa đơn & Thu tiền (`Payments`)**:
  * Khi nhận `MemberPackageRegisteredIntegrationEvent`, module `Payments` tự động khởi tạo `Invoice` (tính tổng chi phí gói tập, áp dụng ưu đãi).
  * `Administrator` thực thi `RecordPaymentCommand` ghi nhận hình thức thanh toán (`Cash`, `QrTransfer` hoặc `BankCard`) và phát hành hóa đơn. Aggregate `Invoice` chuyển sang trạng thái `Paid` và xuất bản **`InvoicePaidIntegrationEvent`** vào Outbox.
* **Kích hoạt Gói tập & Sinh Mã QR (`Memberships`)**:
  * Module `Memberships` tiêu thụ `InvoicePaidIntegrationEvent` và chạy `ActivateMemberPackageSubscriptionInternalCommand`: chuyển trạng thái `MemberPackageSubscription` sang `Active`, thiết lập thời hạn sử dụng (`ExpirationDate`), số buổi tập (`RemainingSessions`) và sinh mã định danh **`QrCodeIdentity` (`QrCode`)** để `Member` đăng nhập vào tài khoản đã được cấp và theo dõi gói tập trên ứng dụng.

### B3: Điểm danh Kiểm soát Ra vào Thời gian thực (`Memberships` / `CheckIn`)

* **Thực thi Điểm danh**:
  * `Administrator` quét mã `QrCode` trên ứng dụng của hội viên hoặc nhập số điện thoại của `Member` tại quầy tiếp đón, gửi `PerformCheckInCommand` đến module `Memberships`.
* **Kiểm tra Quy tắc Nghiệp vụ (`Domain Invariants`)**:
  * Aggregate `MemberPackageSubscription` kiểm tra tức thời trong thời gian thực: (1) Gói tập đang ở trạng thái `Active`, (2) Gói tập không bị bảo lưu (`Frozen`) hoặc bị khóa/hủy (`Suspended`/`Cancelled`), (3) Còn hạn sử dụng (`ExpirationDate`) và còn số buổi tập (`RemainingSessions > 0` với gói theo lượt).
* **Hệ quả Miền (`Side Effects`)**:
  * Nếu đáp ứng đủ điều kiện: Xác nhận check-in thành công, tự động trừ `1` buổi tập (đối với gói tính theo lượt), lưu vết `CheckInRecord` vào lịch sử ra vào trong ngày.
  * Nếu vi phạm: Ném ngoại lệ `BusinessRuleValidationException` và hiển thị cảnh báo từ chối kèm lý do rõ ràng.

### B4: Điều phối & Thực hiện Dịch vụ PT (`Administration` → `PersonalTraining` → `Memberships`)

* **Phân công & Đăng ký Lịch rảnh**:
  * `Administrator` thực thi `AssignPersonalTrainerCommand` để phân công `PersonalTrainer` phụ trách cho `Member` có gói tập PT tương ứng.
  * `PersonalTrainer` chủ động thực thi `RegisterTrainerAvailabilityCommand` để cập nhật các khung giờ làm việc rảnh (khả dụng) theo tuần trên `TrainerAvailabilitySchedule`.
* **Đặt / Đổi / Hủy Lịch tập**:
  * Dựa trên lịch rảnh của PT, `Member` thực thi `BookTrainingSessionCommand` để đặt lịch tập, hoặc gửi `RescheduleTrainingSessionCommand` / `CancelTrainingSessionCommand` khi thay đổi kế hoạch.
  * Aggregate `TrainingSession` rà soát các quy tắc: chống xung đột giờ dạy giữa PT và học viên (`NoOverlappingTrainingSessionRule`) và kiểm tra số buổi PT còn lại trong gói tập (`MemberMustHaveRemainingPtQuotaRule`).
* **Thực hiện Buổi tập, Cập nhật InBody & Đánh giá**:
  * Khi buổi tập diễn ra, `PersonalTrainer` thực thi `UpdateMemberBodyMetricsCommand` (cập nhật cân nặng, chỉ số `InBody` định kỳ, ghi chú bài tập) và `CompleteTrainingSessionCommand` để xác nhận hoàn thành ca tập.
  * Hệ thống phát **`TrainingSessionCompletedIntegrationEvent`** sang module `Memberships` để tự động trừ `1` buổi tập PT trong gói tập của `Member`.
  * Sau khi buổi tập hoàn tất, `Member` thực thi `SubmitTrainerReviewCommand` để chấm điểm số sao (`StarRating`) và để lại nhận xét đánh giá chất lượng của PT.

### B5: Quản trị Vòng đời Gói tập & Báo cáo (`Memberships`, `Payments`, `Reporting`)

* **Gia hạn, Nâng cấp, Bảo lưu & Khóa/Hủy Gói tập (`Memberships` & `Payments`)**:
  * *Gia hạn / Nâng cấp*: Khi gói tập sắp hoặc đã hết hạn, `Administrator` thực thi `RenewPackageSubscriptionCommand` hoặc `UpgradePackageSubscriptionCommand` → Thu phí gia hạn hoặc khoản phí chênh lệch qua module `Payments` → Cập nhật hạn dùng/gói tập mới cho hội viên.
  * *Bảo lưu (`Freeze`)*: Khi `Member` tạm dừng do công tác hoặc ốm đau, `Administrator` thực thi `FreezePackageSubscriptionCommand` → Tạm ngưng gói tập sang trạng thái `Frozen` và tự động kéo dài ngày hết hạn (`ExpirationDate`) tương ứng.
  * *Khóa / Hủy Gói tập*: Khi có vi phạm nội quy, `Administrator` thực thi `SuspendPackageSubscriptionCommand` hoặc `CancelPackageSubscriptionCommand` để vô hiệu hóa gói tập và thu hồi quyền ra vào.
* **Hoàn tiền / Hủy Hóa đơn (`Payments` & `Memberships`)**:
  * Khi có khiếu nại hoặc hủy dịch vụ hợp lệ, `Administrator` thực thi `RefundInvoiceCommand` → Ghi nhận số tiền hoàn trên hóa đơn → Phát **`PaymentRefundedIntegrationEvent`** để điều chỉnh lại trạng thái gói tập bên module `Memberships`.
* **Tổng hợp & Xuất Báo cáo Quản trị (`Reporting` - CQRS Read Stack)**:
  * `Administrator` thực thi các `IQuery<TResult>` (`GetRevenueAnalyticsQuery`, `GetShiftCheckInTrafficQuery`, `GetMembershipGrowthQuery`, `GetTrainerPerformanceQuery`) để phân tích doanh thu, lưu lượng check-in theo ca, biến động hội viên mới/tái gia hạn, hiệu suất và điểm hài lòng PT, đồng thời kết xuất ra file **Excel / PDF**.

## 4. Thiết Kế Cơ Sở Dữ Liệu Quan Hệ Tinh Gọn (ERD - 9 Bảng Cốt Lõi)

Sau khi loại bỏ bảng cấu hình tham số hệ thống không cần thiết (UC 7.2), tối ưu hóa các mối quan hệ 1-1 dư thừa và **tách riêng bảng `Role` tối giản (chỉ gồm `RoleId` và `RoleName`) để chuẩn hóa phân quyền RBAC**, toàn bộ hệ thống gồm **9 bảng cốt lõi** đáp ứng trọn vẹn 100% luồng nghiệp vụ từ B1 đến B5:

### 4.1. Tổng quan 9 Bảng trong ERD

| STT | Tên bảng (Table) | Module (Bounded Context) | Vai trò nghiệp vụ cốt lõi | Quan hệ chính |
| :--- | :--- | :--- | :--- | :--- |
| **1** | **`Role`** | `UserAccess` | Danh mục vai trò chuẩn hóa trong hệ thống (`ADMIN`, `PT`, `MEMBER`) phục vụ phân quyền RBAC. | `1 - N` với `User` |
| **2** | **`User`** | `UserAccess` | Lưu tài khoản, thông tin cá nhân của người dùng và liên kết tới vai trò `Role`. | `N - 1` với `Role`; `1 - N` với `MemberSubscription`, `Invoice`, `CheckInLog`, `PTAvailability`, `PTTrainingSession`, `MemberBodyMetric` |
| **3** | **`PackagePlan`** | `Administration` | Danh mục các gói tập do Admin thiết lập (giá, thời hạn ngày, số buổi tập, số buổi PT). | `1 - N` với `MemberSubscription` |
| **4** | **`MemberSubscription`** | `Memberships` | Gói tập hội viên đã đăng ký (vòng đời gói, mã QR, hạn dùng, số buổi còn lại, PT được phân công, bảo lưu). | `N - 1` với `User`, `PackagePlan`; `1 - N` với `Invoice`, `CheckInLog`, `PTTrainingSession` |
| **5** | **`Invoice`** | `Payments` | Hóa đơn & giao dịch dòng tiền (thu tiền đăng ký mới/gia hạn/nâng cấp, hình thức thanh toán, hoàn tiền). | `N - 1` với `MemberSubscription`, `User` |
| **6** | **`CheckInLog`** | `Memberships` | Lịch sử điểm danh ra/vào phòng tập hằng ngày bằng mã QR hoặc SĐT. | `N - 1` với `MemberSubscription`, `User` |
| **7** | **`PTAvailability`** | `PersonalTraining` | Khung giờ làm việc rảnh theo tuần do PT đăng ký để hội viên tra cứu và đặt lịch. | `N - 1` với `User` (`PT`) |
| **8** | **`PTTrainingSession`** | `PersonalTraining` | Lịch hẹn & ca tập PT (đặt/đổi/hủy lịch, chống trùng lịch, ghi chú bài tập, hoàn thành trừ buổi & chấm sao đánh giá PT). | `N - 1` với `MemberSubscription`, `User` (`Member` & `PT`), `PTAvailability` |
| **9** | **`MemberBodyMetric`** | `PersonalTraining` | Chỉ số phát triển thể chất (`InBody`, cân nặng, cơ - mỡ) của hội viên do PT cập nhật định kỳ qua các buổi tập. | `N - 1` với `User` (`Member`), `PTTrainingSession` |

### 4.2. Chi tiết Các Trường Chính (Columns) của Từng Bảng

#### 1. Bảng `Role` (Danh mục Vai trò & Phân quyền RBAC)
* `RoleId` (INT, **PK**): Mã định danh vai trò.
* `RoleName` (NVARCHAR(100), **UNIQUE**): Tên định danh vai trò (`'ADMIN'`, `'PT'`, `'MEMBER'`).

#### 2. Bảng `User` (Người dùng & Tài khoản - Gộp chung Admin, PT, Hội viên)
* `UserId` (INT, **PK**): Mã định danh người dùng.
* `RoleId` (INT, **FK** -> `Role.RoleId`): Khóa ngoại xác định vai trò của người dùng.
* `FullName` (NVARCHAR(100)): Họ và tên.
* `Phone` (NVARCHAR(20), **UNIQUE**): Số điện thoại (dùng để đăng nhập và tra cứu điểm danh nhanh tại quầy).
* `Email` (NVARCHAR(100), **UNIQUE**, Nullable): Địa chỉ email.
* `PasswordHash` (NVARCHAR(255)): Mật khẩu mã hóa.
* `Gender` (NVARCHAR(10), Nullable), `DateOfBirth` (DATETIME, Nullable): Thông tin cá nhân hội viên/PT.
* `Status` (NVARCHAR(20)): Trạng thái tài khoản (`'ACTIVE'`, `'LOCKED'`).
* `created_at` (DATETIME): Thời điểm tạo tài khoản.

#### 3. Bảng `PackagePlan` (Danh mục Gói tập)
* `PlanId` (INT, **PK**): Mã gói tập danh mục.
* `Name` (NVARCHAR(100)): Tên gói tập (VD: *Gói Gym 3 Tháng*, *Gói PT 1-1 24 Buổi*).
* `Description` (NVARCHAR(MAX), Nullable): Mô tả quyền lợi gói tập.
* `DurationDays` (INT): Thời hạn sử dụng tính theo ngày (VD: 30, 90, 365).
* `Price` (DECIMAL(12,2)): Đơn giá niêm yết.
* `MaxCheckInSessions` (INT, Nullable): Tổng số buổi tập cho phép (`NULL` nếu là gói tập tự do không giới hạn số lượt ra vào).
* `is_PtIncluded` (BIT): Cờ đánh dấu gói có kèm Huấn luyện viên cá nhân (`1` = có, `0` = không).
* `TotalPtSessions` (INT, Default `0`): Tổng số buổi tập cùng PT trong gói.
* `is_Active` (BIT): Trạng thái đang mở bán hay ngừng kinh doanh (`1` = mở bán, `0` = ngừng).

#### 4. Bảng `MemberSubscription` (Gói tập đã đăng ký của Hội viên)
* `SubscriptionId` (INT, **PK**): Mã đăng ký gói tập của hội viên.
* `UserId` (INT, **FK** -> `User.UserId`): Hội viên sở hữu gói tập.
* `PlanId` (INT, **FK** -> `PackagePlan.PlanId`): Gói tập hiện tại.
* `AssignedPtId` (INT, **FK** -> `User.UserId`, Nullable): PT được Admin phân công phụ trách (nếu là gói có PT).
* `QrCode` (NVARCHAR(100), **UNIQUE**, Nullable): Mã định danh QR sinh ra khi kích hoạt gói tập để quét Check-in.
* `StartDate` (DATETIME, Nullable): Ngày bắt đầu kích hoạt.
* `EndDate` (DATETIME, Nullable): Ngày hết hạn (tự động cộng dồn thêm ngày khi thực hiện bảo lưu `Freeze`).
* `RemainingCheckInSessions` (INT, Nullable): Số buổi tập tự do còn lại (trừ `1` mỗi lần check-in thành công nếu gói tính theo lượt).
* `RemainingPtSessions` (INT, Default `0`): Số buổi tập PT còn lại (trừ `1` mỗi khi PT xác nhận hoàn thành ca tập).
* `FreezeStartDate` (DATETIME, Nullable), `FreezeEndDate` (DATETIME, Nullable): Khoảng thời gian bảo lưu gần nhất.
* `Status` (NVARCHAR(20)): Trạng thái vòng đời gói tập (`'PENDING_PAYMENT'`, `'ACTIVE'`, `'FROZEN'`, `'EXPIRED'`, `'SUSPENDED'`, `'CANCELLED'`).
* `created_at` (DATETIME): Thời điểm đăng ký.

#### 5. Bảng `Invoice` (Hóa đơn & Dòng tiền Thanh toán / Hoàn tiền)
* `InvoiceId` (INT, **PK**): Mã hóa đơn.
* `InvoiceCode` (NVARCHAR(50), **UNIQUE**): Mã hóa đơn hiển thị (VD: `INV-20261025-001`).
* `SubscriptionId` (INT, **FK** -> `MemberSubscription.SubscriptionId`): Gói tập đăng ký liên quan.
* `UserId` (INT, **FK** -> `User.UserId`): Hội viên thanh toán.
* `CreatedByAdminId` (INT, **FK** -> `User.UserId`): Admin lập hóa đơn / thu tiền.
* `TransactionType` (NVARCHAR(50)): Loại giao dịch (`'NEW_REGISTRATION'`, `'RENEWAL'`, `'UPGRADE'`).
* `SubtotalAmount` (DECIMAL(12,2)): Chi phí gốc (hoặc phí chênh lệch khi nâng cấp).
* `TotalAmount` (DECIMAL(12,2)): Tổng tiền thực thu.
* `PaymentMethod` (NVARCHAR(30)): Hình thức thanh toán (`'CASH'`, `'QR_TRANSFER'`, `'BANK_CARD'`).
* `Status` (NVARCHAR(20)): Trạng thái hóa đơn (`'PENDING'`, `'PAID'`, `'REFUNDED'`, `'CANCELLED'`).
* `RefundAmount` (DECIMAL(12,2), Default `0`): Số tiền hoàn lại cho khách (nếu hủy dịch vụ / hoàn tiền).
* `RefundReason` (NVARCHAR(MAX), Nullable): Lý do hoàn tiền / hủy hóa đơn.
* `paid_at` (DATETIME, Nullable): Thời điểm hoàn tất thanh toán.

#### 6. Bảng `CheckInLog` (Lịch sử Điểm danh Ra/Vào)
* `CheckInId` (INT, **PK**): Mã lượt điểm danh.
* `SubscriptionId` (INT, **FK** -> `MemberSubscription.SubscriptionId`): Gói tập được kiểm tra khi check-in.
* `UserId` (INT, **FK** -> `User.UserId`): Hội viên điểm danh.
* `VerifiedByAdminId` (INT, **FK** -> `User.UserId`, Nullable): Admin thực hiện quét QR / tra cứu tại quầy.
* `check_in_at` (DATETIME): Thời điểm quét mã ra/vào.
* `Status` (NVARCHAR(20)): Kết quả kiểm tra (`'SUCCESS'`, `'DENIED'`).
* `DenialReason` (NVARCHAR(255), Nullable): Lý do từ chối nếu vi phạm (VD: hết hạn, hết số buổi, đang bảo lưu, gói bị khóa).

#### 7. Bảng `PTAvailability` (Lịch rảnh khả dụng của PT)
* `AvailabilityId` (INT, **PK**): Mã khung giờ rảnh.
* `PtId` (INT, **FK** -> `User.UserId`): Huấn luyện viên đăng ký lịch.
* `AvailableDate` (DATETIME): Ngày làm việc.
* `StartTime` (TIME): Giờ bắt đầu ca rảnh.
* `EndTime` (TIME): Giờ kết thúc ca rảnh.
* `is_Booked` (BIT, Default `0`): Đánh dấu khung giờ đã được hội viên đặt lịch hay chưa.

#### 8. Bảng `PTTrainingSession` (Buổi tập PT & Đánh giá sau buổi tập)
* `PTSessionId` (INT, **PK**): Mã buổi tập PT.
* `SubscriptionId` (INT, **FK** -> `MemberSubscription.SubscriptionId`): Gói tập PT của hội viên (dùng để trừ 1 buổi khi hoàn thành).
* `MemberId` (INT, **FK** -> `User.UserId`): Hội viên đặt lịch.
* `PtId` (INT, **FK** -> `User.UserId`): PT giảng dạy.
* `AvailabilityId` (INT, **FK** -> `PTAvailability.AvailabilityId`, Nullable): Khung giờ rảnh gốc đã chọn.
* `SessionStartTime` (DATETIME): Thời gian bắt đầu buổi tập.
* `SessionEndTime` (DATETIME): Thời gian kết thúc buổi tập.
* `Status` (NVARCHAR(20)): Trạng thái ca tập (`'BOOKED'`, `'RESCHEDULED'`, `'CANCELLED'`, `'COMPLETED'`).
* `WorkoutNotes` (NVARCHAR(MAX), Nullable): Ghi chú bài tập do PT cập nhật trong/sau buổi tập.
* `RatingStars` (INT, Nullable): Điểm đánh giá của hội viên từ `1` đến `5` sao (chỉ cho phép cập nhật khi `status = 'COMPLETED'`).
* `ReviewComment` (NVARCHAR(MAX), Nullable): Nhận xét chất lượng PT của hội viên.
* `completed_at` (DATETIME, Nullable): Thời điểm PT xác nhận hoàn thành ca tập.

#### 9. Bảng `MemberBodyMetric` (Chỉ số Thể chất & InBody của Hội viên)
* `BodyMetricId` (INT, **PK**): Mã bản ghi chỉ số thể chất.
* `UserId` (INT, **FK** -> `User.UserId`): Hội viên được đo chỉ số.
* `PTSessionId` (INT, **FK** -> `PTTrainingSession.PTSessionId`, Nullable): Buổi tập PT gắn với lần đo (nếu đo trong buổi tập).
* `RecordedByPtId` (INT, **FK** -> `User.UserId`): PT thực hiện cập nhật.
* `WeightKg` (DECIMAL(5,2)): Cân nặng (kg).
* `HeightCm` (DECIMAL(5,2), Nullable): Chiều cao (cm).
* `SkeletalMuscleMass` (DECIMAL(5,2), Nullable): Khối lượng cơ xương (kg - chỉ số InBody).
* `BodyFatPercentage` (DECIMAL(5,2), Nullable): Tỷ lệ mỡ cơ thể (% - chỉ số InBody).
* `Bmi` (DECIMAL(4,2), Nullable): Chỉ số BMI.
* `Notes` (NVARCHAR(MAX), Nullable): Nhận xét thể trạng định kỳ của PT.
* `measured_at` (DATETIME): Ngày đo chỉ số.

### 4.3. Mã DBML (dbdiagram.io) Mức Vật Lý - Chuẩn SQL Server

```dbml
// ====================================================================
// ERD VẬT LÝ (PHYSICAL DATA MODEL) - CHUẨN SQL SERVER (9 BẢNG)
// Kiểu dữ liệu: int, nvarchar, datetime, bit, decimal | Có đầy đủ FK
// ====================================================================

TableGroup UserAccess {
  Role
  User
}

TableGroup Administration {
  PackagePlan
}

TableGroup Memberships {
  MemberSubscription
  CheckInLog
}

TableGroup Payments {
  Invoice
}

TableGroup PersonalTraining {
  PTAvailability
  PTTrainingSession
  MemberBodyMetric
}

Table Role {
  RoleId int [pk, increment]
  RoleName nvarchar(100) [unique, not null]
}

Table User {
  UserId int [pk, increment]
  RoleId int [not null]
  FullName nvarchar(100) [not null]
  Phone nvarchar(20) [unique, not null]
  Email nvarchar(100) [unique]
  PasswordHash nvarchar(255) [not null]
  Gender nvarchar(10)
  DateOfBirth datetime
  Status nvarchar(20) [not null, default: 'ACTIVE']
  created_at datetime [not null, default: `getdate()`]
}

Table PackagePlan {
  PlanId int [pk, increment]
  Name nvarchar(100) [not null]
  Description nvarchar(max)
  DurationDays int [not null]
  Price decimal(12,2) [not null]
  MaxCheckInSessions int
  is_PtIncluded bit [not null, default: 0]
  TotalPtSessions int [not null, default: 0]
  is_Active bit [not null, default: 1]
}

Table MemberSubscription {
  SubscriptionId int [pk, increment]
  UserId int [not null]
  PlanId int [not null]
  AssignedPtId int
  QrCode nvarchar(100) [unique]
  StartDate datetime
  EndDate datetime
  RemainingCheckInSessions int
  RemainingPtSessions int [not null, default: 0]
  FreezeStartDate datetime
  FreezeEndDate datetime
  Status nvarchar(20) [not null, default: 'PENDING_PAYMENT']
  created_at datetime [not null, default: `getdate()`]
}

Table Invoice {
  InvoiceId int [pk, increment]
  InvoiceCode nvarchar(50) [unique, not null]
  SubscriptionId int [not null]
  UserId int [not null]
  CreatedByAdminId int [not null]
  TransactionType nvarchar(50) [not null]
  SubtotalAmount decimal(12,2) [not null]
  TotalAmount decimal(12,2) [not null]
  PaymentMethod nvarchar(30) [not null]
  Status nvarchar(20) [not null, default: 'PENDING']
  RefundAmount decimal(12,2) [not null, default: 0]
  RefundReason nvarchar(max)
  paid_at datetime
}

Table CheckInLog {
  CheckInId int [pk, increment]
  SubscriptionId int [not null]
  UserId int [not null]
  VerifiedByAdminId int
  check_in_at datetime [not null, default: `getdate()`]
  Status nvarchar(20) [not null]
  DenialReason nvarchar(255)
}

Table PTAvailability {
  AvailabilityId int [pk, increment]
  PtId int [not null]
  AvailableDate datetime [not null]
  StartTime time [not null]
  EndTime time [not null]
  is_Booked bit [not null, default: 0]
}

Table PTTrainingSession {
  PTSessionId int [pk, increment]
  SubscriptionId int [not null]
  MemberId int [not null]
  PtId int [not null]
  AvailabilityId int
  SessionStartTime datetime [not null]
  SessionEndTime datetime [not null]
  Status nvarchar(20) [not null, default: 'BOOKED']
  WorkoutNotes nvarchar(max)
  RatingStars int
  ReviewComment nvarchar(max)
  completed_at datetime
}

Table MemberBodyMetric {
  BodyMetricId int [pk, increment]
  UserId int [not null]
  PTSessionId int
  RecordedByPtId int [not null]
  WeightKg decimal(5,2) [not null]
  HeightCm decimal(5,2)
  SkeletalMuscleMass decimal(5,2)
  BodyFatPercentage decimal(5,2)
  Bmi decimal(4,2)
  Notes nvarchar(max)
  measured_at datetime [not null, default: `getdate()`]
}

// ====================================================================
// THAM CHIẾU KHÓA NGOẠI (TỪ FK > TRỎ ĐẾN PK)
// ====================================================================

Ref: User.RoleId > Role.RoleId
Ref: MemberSubscription.UserId > User.UserId
Ref: MemberSubscription.PlanId > PackagePlan.PlanId
Ref: MemberSubscription.AssignedPtId > User.UserId
Ref: Invoice.SubscriptionId > MemberSubscription.SubscriptionId
Ref: Invoice.UserId > User.UserId
Ref: Invoice.CreatedByAdminId > User.UserId
Ref: CheckInLog.SubscriptionId > MemberSubscription.SubscriptionId
Ref: CheckInLog.UserId > User.UserId
Ref: CheckInLog.VerifiedByAdminId > User.UserId
Ref: PTAvailability.PtId > User.UserId
Ref: PTTrainingSession.SubscriptionId > MemberSubscription.SubscriptionId
Ref: PTTrainingSession.MemberId > User.UserId
Ref: PTTrainingSession.PtId > User.UserId
Ref: PTTrainingSession.AvailabilityId > PTAvailability.AvailabilityId
Ref: MemberBodyMetric.UserId > User.UserId
Ref: MemberBodyMetric.PTSessionId > PTTrainingSession.PTSessionId
Ref: MemberBodyMetric.RecordedByPtId > User.UserId
```
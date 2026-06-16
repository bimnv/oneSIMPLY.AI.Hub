Cẩm Nang Tác Chiến & Bộ Quy Tắc Điều Phối Cursor (oneSIMPLY AI Hub)
Tài liệu Master định hướng Kiến trúc, Database và Lộ trình code dứt điểm

I. TỔNG QUAN HỆ THỐNG (SYSTEM OVERVIEW)

Dự án oneSIMPLY AI Hub (Growth AI) là một hệ thống SaaS B2B giúp các chủ doanh nghiệp và chủ shop tự động hóa 100% quy trình Marketing: từ khâu phân tích kịch bản chuẩn tâm lý ➔ sinh nội dung hàng loạt ➔ duyệt nhanh ➔ PUSH 1-click đăng trực tiếp lên Facebook Fanpage & TikTok.

Quy tắc thiết kế lõi:

Không dùng Prompt phức tạp ở Frontend: Người dùng chỉ điền vào form ngắn (Tên sản phẩm, đặc tính, giọng điệu). Hệ thống tự bọc Prompt chuẩn tâm lý ở Backend trước khi gửi lên AI.

Hybrid Routing (Tối ưu chi phí): Hệ thống tự động phân loại tác vụ. Viết content thông thường -> gọi DeepSeek V3 (giá siêu rẻ). Phân tích chiến lược doanh nghiệp -> gọi GPT-4o-mini (OpenAI).

Thanh toán VietQR khép kín: Khách quét mã chuyển khoản ngân hàng -> Webhook tự động bắt cú pháp -> Cộng hạn mức tự động mà không cần con người phê duyệt thủ công.

II. KIẾN TRÚC KỸ THUẬT & THƯ MỤC CHUẨN (TECH STACK)

1. Backend Core (ASP.NET Core 8.0/9.0 Web API)

Thư viện lõi: Entity Framework Core (SQL Server), JWT Bearer Authentication, HttpClient.

Cấu trúc thư mục chuẩn:

oneSIMPLY.AIHub.Api/
├── Controllers/           # Chứa các Controller xử lý Request (Chat, Auth, Payment)
├── Models/                # Định nghĩa các Entity Database (User, Log, Subscription)
├── Data/                  # DbContext và các file cấu hình EF Core Migrations
├── Services/              # Logic gọi API bên thứ ba (OpenAIService, DeepSeekService)
├── DTOs/                  # Data Transfer Objects cho Request và Response
└── Program.cs             # Khởi tạo DI Container, Middleware, Auth Config


2. Frontend Web App (React Vite + Tailwind CSS)

Thư viện lõi: Axios, Lucide React (Icons), Tailwind CSS.

Cấu trúc thư mục chuẩn:

oneSIMPLY.AIHub.Web/
├── src/
│   ├── assets/            # Lưu trữ logo_onesimply, hình ảnh tĩnh
│   ├── components/        # Các UI component dùng chung (Sidebar, Navbar, Layout, Button)
│   ├── pages/             # Các trang chính (Dashboard, CopywritingForm, Analytics, Billing)
│   ├── services/          # Thư viện gọi API (apiClient.js sử dụng Axios, authService.js)
│   ├── App.jsx            # Điều phối Router và cấu trúc trang chính
│   └── main.jsx           # Điểm khởi tạo dự án React


III. THIẾT KẾ CƠ SỞ DỮ LIỆU (DATABASE SCHEMA)

Cursor bắt buộc phải tuân thủ cấu trúc các bảng dữ liệu (SQL Server) sau khi thiết lập Entity Framework Core:

[Users] (1) ── (1) [UserSubscriptions] (N) ── (1) [Subscriptions]
   │
   ├─── (N) [UsageLogs] (Nhật ký gọi AI)
   │
   └─── (N) [Transactions] (Nhật ký quét mã VietQR)


User: Id (int, PK), Email (string), PasswordHash (string - PBKDF2), CreatedAt (DateTime), IsActive (bool).

Subscription: Id (int, PK), Name (string), Price (decimal), MaxRequests (int), DurationDays (int).

UserSubscription: Id (int, PK), UserId (int, FK), SubscriptionId (int, FK), StartDate (DateTime), EndDate (DateTime), IsActive (bool), RemainingRequests (int).

UsageLog: Id (int, PK), UserId (int, FK), ModelUsed (string), PromptTokens (int), CompletionTokens (int), CreatedAt (DateTime), TaskType (string).

Transaction: Id (int, PK), UserId (int, FK), Amount (decimal), Status (string - Pending/Success), Code (string - Mã nội dung chuyển khoản độc nhất), CreatedAt (DateTime).

IV. BỘ NGUYÊN TẮC PHÁT TRIỂN DÀNH CHO CURSOR (CURSORDEV RULES)

Khi làm việc với dự án này, Cursor phải tuân thủ nghiêm ngặt các quy tắc sau:

Rule of One: Chỉ tập trung viết/sửa duy nhất một file hoặc một tính năng nhỏ trong mỗi câu lệnh. Không tự ý viết thêm code không liên quan làm loãng Context Window.

Strict Context: Luôn kiểm tra sự tồn tại của các class trong @Program.cs trước khi viết các file Controller hoặc Service mới.

No Placeholders: Khi viết code sửa đổi, không được viết các đoạn comment rút gọn như // ... code cũ giữ nguyên. Phải cung cấp đầy đủ đoạn code hoàn chỉnh để người dùng dễ dàng copy-paste.

Secure Keys: Không bao giờ hardcode API Key hay chuỗi bảo mật JWT vào code. Luôn đọc từ appsettings.json hoặc môi trường hệ thống.

V. LỘ TRÌNH 15 NGÀY CHIẾN THẦN (DETAILED SPRINT PLAN)

GIAI ĐOẠN 1: THIẾT LẬP BACKEND CORE & CẤU HÌNH HỆ THỐNG (NGÀY 1 - 3)

💬 Prompt 1.1: Tạo file cấu hình môi trường bảo mật appsettings.json

Nội dung dán vào Cursor:
"Hãy tạo cho tôi file cấu hình appsettings.json đặt ở thư mục gốc của project Web API oneSIMPLY.AIHub.Api. File cần chứa đầy đủ các thông tin:

Chuỗi kết nối SQL Server LocalDB (DefaultConnection) tới DB oneSimplyAIHubDb.

Cấu hình JWT: Secret (sử dụng một chuỗi ký tự dài phức tạp), Issuer và Audience.

Cấu hình các API Key đối tác AI: OpenAIKey và DeepSeekKey (để trống giá trị để tôi điền sau).
Hãy viết code định dạng JSON chuẩn chỉnh, không cắt bớt."

💬 Prompt 1.2: Cấu hình dữ liệu mẫu (Seed Data) tự động nạp Gói cước

Nội dung dán vào Cursor:
"Hãy chỉnh sửa file Program.cs tại khu vực khởi tạo Database (db.Database.EnsureCreated()) để tự động tạo dữ liệu mẫu (Seed Data) cho bảng Subscriptions nếu bảng này chưa có dữ liệu. Tôi muốn nạp sẵn 3 gói cước tiêu chuẩn:

Gói 'Free Trial' - Giá: 0 VNĐ - Hạn mức: 10 requests - Thời hạn: 30 ngày.

Gói 'Starter' - Giá: 390,000 VNĐ - Hạn mức: 50 requests - Thời hạn: 30 ngày.

Gói 'Professional' - Giá: 990,000 VNĐ - Hạn mức: 200 requests - Thời hạn: 30 ngày.
Hãy cung cấp đoạn code C# chuẩn xác và chỉ rõ vị trí cần chèn trong Program.cs của tôi."

GIAI ĐOẠN 2: THIẾT LẬP THƯ VIỆN FRONTEND & CẤU HÌNH TÔNG LOGO THIÊN LONG (NGÀY 4 - 6)

💬 Prompt 2.1: Cấu hình Tailwind CSS & dải màu nhận diện thương hiệu

Nội dung dán vào Cursor:
"Tôi đang ở dự án Frontend React Vite. Tôi đã cài đặt Tailwind CSS.

Hãy cấu hình lại file tailwind.config.js để định nghĩa dải màu thương hiệu Thiên Long Software Solutions:

brandBlue: Màu xanh chủ đạo #00a2e8

brandOrange: Màu cam chủ đạo #ff6600

Hãy viết lại toàn bộ nội dung file src/index.css để import đầy đủ các directive của Tailwind CSS (@tailwind base;, @tailwind components;, @tailwind utilities;) và thiết lập kiểu chữ mặc định mượt mà."

GIAI ĐOẠN 3: GIAO DIỆN CHÍNH "REVIEW & PUSH ĐA KÊNH" (NGÀY 7 - 10)

💬 Prompt 3.1: Code trang giao diện điều phối tác vụ chính

Nội dung dán vào Cursor:
"Hãy sử dụng React (Vite) + Tailwind CSS để tạo cho tôi file src/App.jsx hoạt động như một Dashboard Marketing chuyên nghiệp:

Thanh Sidebar bên trái: Có logo Thiên Long Software Solutions (sử dụng file ảnh logo_onesimply.png, nếu lỗi thì hiển thị fallback text 'oneSIMPLY AI Hub' đẹp mắt). Có các menu: 'Viết bài bán hàng (Copywriting)', 'Gói cước & Nạp tiền (Billing)', 'Lịch sử sử dụng'.

Trang Viết bài (Giao diện chính chia đôi):

Cột trái (Form nhập liệu): Các ô điền thông tin: Tên sản phẩm, các đặc tính (textarea), chọn giọng văn, chọn công thức tâm lý (AIDA/PAS). Nút bấm 'Tạo bài viết AI' màu cam #ff6600.

Cột phải (Duyệt bài & Push): Hiển thị chữ chạy thời gian thực (Streaming) vào một khung soạn thảo (textarea) cho phép sửa nhanh. Bên dưới có checkbox chọn kênh đăng: 'Facebook Page', 'TikTok'. Có nút bấm '🚀 PUSH ĐĂNG NGAY' màu xanh #00a2e8 để tự động đẩy bài lên các API đã chọn.
Hãy viết code hoàn chỉnh, không viết tắt, hỗ trợ đầy đủ Axios kết nối lên Backend cổng 5000."

GIAI ĐOẠN 4: THANH TOÁN QUÉT MÃ VIETQR ĐỘNG TỰ ĐỘNG (NGÀY 11 - 13)

💬 Prompt 4.1: Xây dựng trang Thanh toán & Quét mã VietQR

Nội dung dán vào Cursor:
"Hãy tạo cho tôi một React Component đặt tên là src/components/BillingPage.jsx để xử lý việc mua gói cước:

Hiển thị danh sách 3 gói cước dưới dạng các thẻ đẹp mắt (Free Trial, Starter, Professional) kèm mức giá và hạn mức.

Khi người dùng bấm mua gói Starter hoặc Professional, gọi API POST /api/payment/generate-qr của Backend để lấy link QR và mã chuyển khoản (transactionCode).

Hiển thị một khung Pop-up Modal chứa ảnh mã QR VietQR động để khách mở ứng dụng ngân hàng quét, hiển thị số tiền và nội dung chuyển khoản rõ ràng. Có hiệu ứng đếm ngược thời gian chờ ngân hàng báo Webhook để tự động kích hoạt gói cước."

GIAI ĐOẠN 5: CẤU HÌNH CORS & DEPLOY DOCKER CONTAINER (NGÀY 14 - 15)

💬 Prompt 5.1: Cấu hình bẫy lỗi CORS ở Backend C#

Nội dung dán vào Cursor:
"Ứng dụng Frontend React của tôi chạy ở cổng http://localhost:5173 đang bị lỗi CORS chặn kết nối khi gọi lên Web API ở cổng http://localhost:5000.
Hãy viết cho tôi đoạn code cấu hình CORS bổ sung vào Program.cs ở Backend để cho phép nhận request từ nguồn cổng 5173, hỗ trợ đầy đủ các phương thức HTTP (GET, POST, PUT, DELETE) và cho phép truyền tải Headers bảo mật."
# BÀI THI GIỮA KỲ – QUẢN LÝ MÓN ĂN – BIT240149

**Công nghệ:** ASP.NET Core MVC (.NET 8), Entity Framework Core 8, SQL Server LocalDB.  
**MSSV:** `BIT240149`.  
**Tên project:** `BIT240149_FoodManagement`.  
**Tên database:** `BIT240149_MID`.

## A. Mở và chạy trên Windows bằng Visual Studio 2022

1. Giải nén `BIT240149_FoodManagement.zip`.
2. Mở **Visual Studio 2022** (cài workload **ASP.NET and web development**, SDK **.NET 8**).
3. Chọn **Open a project or solution** → mở file `BIT240149_FoodManagement.sln` ở thư mục đã giải nén.
4. Đảm bảo đã cài **SQL Server Express LocalDB**. Nếu Visual Studio đã có LocalDB thì thường không cần làm gì thêm.
5. Kiểm tra `appsettings.json`, dòng `DefaultConnection`:
   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BIT240149_MID;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   ```
   Nếu máy chỉ có SQL Server Express, có thể đổi **Server** thành `Server=.\\SQLEXPRESS;` (giữ nguyên `Database=BIT240149_MID`). Nếu dùng tên máy chủ khác, thay đúng tên server của máy.
6. Visual Studio tự restore package NuGet. Nếu chưa xong, nhấp phải project → **Restore NuGet Packages**.
7. Nhấn **Ctrl+Shift+B** để build, sau đó **F5** hoặc **Ctrl+F5** để chạy.
8. Lần chạy đầu tiên: ứng dụng dùng `EnsureCreatedAsync()` để tự tạo database và 3 bảng, sau đó `DemoSeeder.SeedAsync()` để thêm **3 loại món, 6 món và hình ảnh mẫu**. Không cần chạy lệnh SQL hay migration trước.
9. Trình duyệt mở danh sách món ăn. Ngôn ngữ mặc định là **English**.
10. Nếu muốn chạy bằng terminal tại thư mục dự án: `dotnet restore`, rồi `dotnet run --launch-profile http` và vào `http://localhost:5164`.

### Khi bị lỗi kết nối cơ sở dữ liệu

- Thông báo **LocalDB not installed** hoặc **SQL Server not found**: cài SQL Server Express LocalDB hoặc sửa connection string trỏ đến SQL Server thật sự đã cài.
- Lỗi **Login failed**: kiểm tra Windows Authentication, user và server.
- Lỗi **Cannot open database**: kiểm tra quyền tạo database, kiểm tra SQL Server có khởi động không.
- SQL Server Express LocalDB chỉ chạy trên Windows. Nếu chạy Linux/macOS phải thay connection string trỏ tới một SQL Server ngoài.
- Nếu chạy lại: dữ liệu đã nhập vẫn lưu trong SQL Server, không mất khi đóng Visual Studio.
- Không gọi `Add-Migration` hay `Update-Database` trên DB này: bài dùng `EnsureCreated`, không dùng migrations. Muốn chuyển sang migrations thì cần tạo database mới và bỏ `EnsureCreated`.

## B. Kiểm tra trên SQL Server Object Explorer

1. Visual Studio → **View** → **SQL Server Object Explorer**.
2. Mở `(localdb)\\MSSQLLocalDB` → **Databases** → `BIT240149_MID` → **Tables**.
3. Có đúng các bảng:
   - `dbo.BIT240149_DishCategories`
   - `dbo.BIT240149_Dishes`
   - `dbo.BIT240149_DishImages`
4. Mở cột của từng bảng: mọi cột vật lý đều có tiền tố `BIT240149_`.
5. Dùng cửa sổ truy vấn của database, chạy SQL sau để chứng minh:

```sql
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
  'BIT240149_DishCategories',
  'BIT240149_Dishes',
  'BIT240149_DishImages'
)
ORDER BY TABLE_NAME, ORDINAL_POSITION;

SELECT COUNT(*) AS SoLoai FROM dbo.BIT240149_DishCategories;
SELECT COUNT(*) AS SoMon FROM dbo.BIT240149_Dishes;

SELECT TOP (10)
    d.BIT240149_Id,
    d.BIT240149_Name,
    d.BIT240149_Price,
    d.BIT240149_PreparationTime,
    d.BIT240149_IsAvailable,
    c.BIT240149_Name AS LoaiMon
FROM dbo.BIT240149_Dishes AS d
JOIN dbo.BIT240149_DishCategories AS c
    ON d.BIT240149_DishCategoryId = c.BIT240149_Id;
```

## C. Chức năng theo đề

| Tiêu chí | Nơi triển khai / Cách demo |
|---|---|
| Tên DB, model, bảng, cột | `appsettings.json`, `Models/*.cs`, `Data/AppDbContext.cs`, SQL Server Object Explorer |
| Quan hệ 1-n | `AppDbContext.OnModelCreating`: category→dish (Restrict), dish→image (Cascade) |
| Validation | `DishesController.ValidateFormAsync`, `CategoriesController.ValidateAsync`, chỉ mục unique và SQL CHECK |
| Create / Edit | Menu Dishes → Add dish / Edit, chọn Category từ database |
| Lọc kết hợp | Trang Dishes: search, category, status, min/max price, sort; `IQueryable.Where`, `OrderBy`, `ToListAsync` sau cùng |
| Phân loại thời gian | `PreparationTime <= 15` → Quick / Nhanh, >15 → Regular / Thông thường; tính trên View, KHÔNG thêm cột |
| Quản lý ảnh | Trang Details → Add image, Set as thumbnail, Delete; nhiều ảnh; DB filtered unique index bảo đảm tối đa 1 thumbnail |
| Không xóa category đang dùng | Categories → Delete loại đã có món → lỗi dễ hiểu; DB FK Restrict |
| Ngừng bán vẫn hiện | Món Bún chả đã ngừng bán vẫn hiện ở All statuses; chọn Available sẽ ẩn nó |
| Xử lý lỗi | Vào `/Dishes/Details/99999`, `/Dishes/Edit/99999`, `/Categories/Edit/99999`; invalid range, missing images, no search results |
| Đa ngôn ngữ | Dropdown góc trên English → Tiếng Việt, cookie lưu lựa chọn; giữ nguyên query string khi đổi trên danh sách |

## D. Kịch bản quay video demo (3–5 phút)

1. Mở web trong cửa sổ ẩn danh để chắc chắn **English** mặc định. Quay màn hình trang Dishes (hiện 6 món).
2. Vào SQL Server Object Explorer: tên DB `BIT240149_MID`, 3 bảng, tên cột đều `BIT240149_`; trong Solution Explorer cho thấy 3 Model đúng hậu tố.
3. Dishes → thử tìm **Phở**, lọc **Món chính**, status **Available**, giá từ 50.000 đến 70.000, sắp xếp giá giảm; cho thấy các điều kiện vẫn hiển thị.
4. Quay Quick / Regular, Available / Unavailable; chuyển sang **Unavailable** để thấy Bún chả.
5. Create → tạo món mới và chọn loại; Edit → thay đổi giá/thời gian; thử gửi giá bằng 0 (tắt HTML validation bằng công cụ trình duyệt nếu muốn minh họa server-side).
6. Details của Phở bò → cho thấy 2 ảnh → chọn ảnh thứ 2 làm thumbnail → thêm URL ảnh mới → xóa thử ảnh.
7. Categories → thử Delete **Món chính** (bị chặn); mở `/Dishes/Details/99999` (hiện trang 404 dễ hiểu).
8. Chuyển English sang Tiếng Việt, đi qua Dishes → Create → Edit → Details; quay các nhãn, lỗi, trạng thái tiếng Việt. Quay lúc đang lọc và đổi ngôn ngữ để chứng minh query vẫn còn.

**Lưu ý:** Các ảnh seed dùng URL Unsplash nên cần Internet để trình duyệt tải ảnh, còn database/app có thể chạy mà không cần mạng sau khi NuGet đã restore.

## E. Giải thích nếu giảng viên hỏi

- **ORM là gì?** Entity Framework Core cho phép thao tác với đối tượng C# thay vì tự viết hầu hết câu lệnh SQL; LINQ được dịch thành SQL để truy vấn.
- **Database lưu ở đâu?** SQL Server LocalDB trên Windows, tên `BIT240149_MID`, không lưu trong file `.cs` hay trong View. Visual Studio chỉ dùng SQL Server Object Explorer để quản lý/quan sát database.
- **Vì sao đặt tên cột khác Model?** `HasColumnName("BIT240149_Name")` ánh xạ thuộc tính C# `Name` sang cột thật `BIT240149_Name` theo đề.
- **Quan hệ 1-n ở đâu?** Các `HasMany().WithOne().HasForeignKey()` trong `AppDbContext`.
- **Vì sao Delete Category bị chặn?** Kiểm tra `AnyAsync` ở controller và FK `DeleteBehavior.Restrict` ở database.
- **Có lọc SQL hay tải toàn bộ?** Điều kiện `Where` được xây dựng trên `IQueryable`; EF Core chỉ gọi `ToListAsync()` sau khi áp dụng các điều kiện, do đó SQL Server thực hiện lọc.
- **Chống trùng món theo loại ở đâu?** `AnyAsync` so tên + mã loại trong controller, và `HasIndex(new { DishCategoryId, Name }).IsUnique()` ở database.
- **Phân loại thời gian lưu SQL không?** Không; ở Views, `<= 15` = Quick / Nhanh, còn lại Regular / Thông thường.
- **Ảnh đại diện chỉ 1 ảnh như nào?** Mỗi `DishImage` có `IsThumbnail` và `DishId`; cập nhật các ảnh trong transaction, kết hợp chỉ mục unique filtered trên `BIT240149_DishId` với điều kiện `BIT240149_IsThumbnail = 1`.
- **Vì sao English mặc định?** `DefaultRequestCulture` là `en`; khi đổi ngôn ngữ lưu cookie qua `LanguageController.Switch`.
- **Tại sao giữ được bộ lọc khi đổi ngôn ngữ?** Switch nhận và quay lại `Path` + `QueryString` hiện tại, không xóa tham số search/category/status/price/sort.

## F. Nộp bài

Theo file đề, cần **mã nguồn** và **video demo**. Ảnh chụp trang EDUX có thể giới hạn loại file 5 MB; hãy kiểm tra yêu cầu nộp thực tế và hình thức được giảng viên cho phép. Zip này chứa mã nguồn nhưng không chứa video demo. Nếu EDUX không nhận ZIP/MP4, cần hỏi giảng viên kênh nộp mã nguồn và video phù hợp. Không ghi đã nộp nếu chưa thực sự tải đủ tài liệu lên.

## G. Lưu ý khi mở lại / làm mới database

- Dữ liệu mẫu sẽ chỉ được seed nếu bảng loại món đang trống.
- Nếu muốn làm lại từ đầu, xóa database `BIT240149_MID` trong SQL Server Object Explorer rồi chạy app lại (sẽ **mất dữ liệu cũ**).
- Project này không có file `*.mdf` đi kèm vì SQL Server LocalDB tạo database lần đầu; dùng Visual Studio trên máy có SQL Server/LocalDB.
- Đây là project tham khảo có thể chỉnh sửa để giải thích bằng kiến thức của mình; cần tự quay video trên máy thực tế.

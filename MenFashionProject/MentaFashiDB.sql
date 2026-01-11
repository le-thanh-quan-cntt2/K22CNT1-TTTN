CREATE DATABASE MenFashionDB;
GO
USE MenFashionDB;
GO

-- 1. Bảng Danh mục (Categories) - Ví dụ: Áo, Quần, Phụ kiện
-- Có ParentId để phân cấp (VD: Áo -> Áo sơ mi)
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Alias NVARCHAR(100), -- Link thân thiện (ao-so-mi)
    Description NVARCHAR(500),
    ParentId INT NULL, -- Danh mục cha
    Image NVARCHAR(250),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 2. Bảng Sản phẩm (Products)
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(250) NOT NULL,
    Alias NVARCHAR(250),
    CategoryId INT FOREIGN KEY REFERENCES Categories(CategoryId),
    Description NVARCHAR(MAX),
    Detail NVARCHAR(MAX), -- Chi tiết (chất liệu, form dáng) [cite: 17]
    Image NVARCHAR(250), -- Ảnh đại diện chính
    Price DECIMAL(18,2) NOT NULL, -- Giá bán
    PriceSale DECIMAL(18,2), -- Giá khuyến mãi
    Quantity INT DEFAULT 0, -- Tổng tồn kho
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1, -- Hiển thị/Ẩn
    IsHome BIT DEFAULT 0, -- Hiển thị trang chủ (Hot)
    IsHot BIT DEFAULT 0
);

-- 3. Bảng Ảnh chi tiết sản phẩm (ProductImages) - Một áo có nhiều góc chụp
CREATE TABLE ProductImages (
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Url NVARCHAR(250),
    IsDefault BIT DEFAULT 0 -- Ảnh chính
);

-- 4. Bảng thuộc tính Size và Color (Để quản lý biến thể giống Torano)
CREATE TABLE ProductAttributes (
    AttributeId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Size NVARCHAR(10) NOT NULL, -- S, M, L, XL [cite: 17]
    Color NVARCHAR(50) NOT NULL, -- Trắng, Đen, Xanh
    Quantity INT DEFAULT 0 -- Số lượng tồn kho cho từng Size/Màu
);

-- 5. Bảng Tài khoản (Users/Customers) [cite: 29]
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100),
    UserName NVARCHAR(50) NOT NULL,
    Password NVARCHAR(100) NOT NULL, -- Lưu mã hóa MD5/SHA
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(250),
    Role INT DEFAULT 0, -- 0: Khách hàng, 1: Admin
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 6. Bảng Đơn hàng (Orders)
CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId), -- Nếu khách đăng nhập
    Code NVARCHAR(50) NOT NULL, -- Mã đơn hàng (VD: DH001)
    CustomerName NVARCHAR(100) NOT NULL, -- Người nhận
    Phone NVARCHAR(20) NOT NULL,
    Address NVARCHAR(250) NOT NULL,
    TotalAmount DECIMAL(18,2),
    PaymentMethod INT DEFAULT 1, -- 1: COD, 2: Chuyển khoản
    Status INT DEFAULT 1, -- 1: Chờ xác nhận, 2: Đóng gói, 3: Đang giao, 4: Hoàn thành [cite: 28]
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 7. Bảng Chi tiết đơn hàng (OrderDetails)
CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT FOREIGN KEY REFERENCES Orders(OrderId),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL
);

-- 8. Bảng Tin tức/Bài viết (Hỗ trợ SEO và Marketing)
CREATE TABLE Posts (
    PostId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(250) NOT NULL,
    Content NVARCHAR(MAX),
    Image NVARCHAR(250),
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO



-- 1. Thêm danh mục
INSERT INTO Categories (CategoryName, Alias, Description) 
VALUES 
(N'Áo Sơ Mi', 'ao-so-mi', N'Phong cách lịch lãm'),
(N'Quần Âu', 'quan-au', N'Chuẩn men công sở'),
(N'Áo Polo', 'ao-polo', N'Năng động, trẻ trung');

-- 2. Thêm sản phẩm mẫu (Lưu ý: Bạn cần copy ảnh vào folder wwwroot/images sau)
INSERT INTO Products (ProductName, Alias, CategoryId, Price, PriceSale, Image, IsActive, IsHome) 
VALUES 
(N'Áo Sơ Mi Trắng Slimfit', 'ao-so-mi-trang', 1, 450000, 390000, 'sp1.jpg', 1, 1),
(N'Quần Âu Đen Cao Cấp', 'quan-au-den', 2, 550000, 500000, 'sp2.jpg', 1, 1),
(N'Áo Polo Xanh Navy', 'ao-polo-xanh', 3, 350000, 0, 'sp3.jpg', 1, 1),
(N'Áo Vest Nam Thời Thượng', 'ao-vest-nam', 1, 1200000, 0, 'sp4.jpg', 1, 1);
GO


-- 1. Thêm Size và Màu cho sản phẩm (Giả sử sản phẩm ID = 1 là Áo Sơ Mi)
INSERT INTO ProductAttributes (ProductId, Size, Color, Quantity) VALUES 
(1, 'S', N'Trắng', 10),
(1, 'M', N'Trắng', 15),
(1, 'L', N'Trắng', 5),
(1, 'XL', N'Trắng', 2);

-- 2. Thêm Size cho Quần (ID = 2)
INSERT INTO ProductAttributes (ProductId, Size, Color, Quantity) VALUES 
(2, '29', N'Đen', 10),
(2, '30', N'Đen', 10),
(2, '31', N'Đen', 10),
(2, '32', N'Đen', 10);

-- 3. Thêm ảnh chi tiết (Nhiều góc chụp) cho Áo sơ mi (ID = 1)
INSERT INTO ProductImages (ProductId, Url, IsDefault) VALUES 
(1, 'sp1.jpg', 1),
(1, 'sp1_sau.jpg', 0), -- Giả sử bạn có ảnh mặt sau
(1, 'sp1_can.jpg', 0); -- Giả sử bạn có ảnh cận cảnh
GO
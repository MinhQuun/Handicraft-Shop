CREATE DATABASE QL_MYNGHE;
GO
USE QL_MYNGHE;
GO

-- Tạo bảng CHATLIEU
CREATE TABLE CHATLIEU
(	
    MACHATLIEU VARCHAR(10) NOT NULL,
    TENCHATLIEU NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_CHATLIEU PRIMARY KEY(MACHATLIEU)
);
--Tạo bảng DANHMUCSANPHAM
CREATE TABLE DANHMUCSANPHAM
(
    MADANHMUC INT IDENTITY(1,1) PRIMARY KEY, -- ID danh mục tự tăng
    TENDANHMUC NVARCHAR(100) NOT NULL, -- Tên danh mục
    MOTA NVARCHAR(255), -- Mô tả
    MADANHMUCCHA INT NULL, -- ID danh mục cha (nếu có)
    CONSTRAINT FK_DANHMUC_CHA FOREIGN KEY(MADANHMUCCHA) REFERENCES DANHMUCSANPHAM(MADANHMUC) -- Khóa ngoại liên kết chính bảng này
);

-- Tạo bảng KHUYENMAI
CREATE TABLE KHUYENMAI
(
    MAKHUYENMAI VARCHAR(10) NOT NULL,
    LOAIKHUYENMAI VARCHAR(50),
    TENKHUYENMAI NVARCHAR(100),
    NGAYBATDAU DATETIME,
    NGAYKETTHUC DATETIME,
    GIAMGIA DECIMAL(5,2), 
    CONSTRAINT PK_KHUYENMAI PRIMARY KEY(MAKHUYENMAI)
);

-- Tạo bảng NHACUNGCAP
CREATE TABLE NHACUNGCAP
(
    MANHACUNGCAP INT IDENTITY(1,1) PRIMARY KEY,
    TENNHACUNGCAP NVARCHAR(100) NOT NULL,
    DTHOAI NVARCHAR(20),
    DIACHI NVARCHAR(255)
);
-- Tạo bảng SANPHAM
CREATE TABLE SANPHAM
(
    MASANPHAM VARCHAR(10) NOT NULL,
    TENSANPHAM NVARCHAR(50) NOT NULL,
    HINHANH NVARCHAR(255),
    GIABAN DECIMAL(18,0),
    SOLUONGTON INT,
    MOTA NVARCHAR(1000), -- Mở rộng cột MOTA
    MACHATLIEU VARCHAR(10),
    MAKHUYENMAI VARCHAR(10),
    MANHACUNGCAP INT,
    MADANHMUC INT,
    CONSTRAINT PK_SANPHAM PRIMARY KEY(MASANPHAM),
    CONSTRAINT FK_SANPHAM_CHATLIEU FOREIGN KEY(MACHATLIEU) REFERENCES CHATLIEU(MACHATLIEU),
    CONSTRAINT FK_SANPHAM_KHUYENMAI FOREIGN KEY(MAKHUYENMAI) REFERENCES KHUYENMAI(MAKHUYENMAI),
    CONSTRAINT FK_SANPHAM_NHACUNGCAP FOREIGN KEY(MANHACUNGCAP) REFERENCES NHACUNGCAP(MANHACUNGCAP),
    CONSTRAINT FK_SANPHAM_DANHMUC FOREIGN KEY(MADANHMUC) REFERENCES DANHMUCSANPHAM(MADANHMUC)
);

-- Tạo bảng KHACHHANG
CREATE TABLE KHACHHANG
(
    MAKHACHHANG INT IDENTITY(1,1) NOT NULL,
    HOTEN NVARCHAR(50) NOT NULL,
    NGAYSINH DATETIME,
    GIOITINH NVARCHAR(10),
    SODIENTHOAI VARCHAR(10),
    TAIKHOAN VARCHAR(255),
    MATKHAU NVARCHAR(50),
    EMAIL VARCHAR(255),
    CONSTRAINT PK_KHACHHANG PRIMARY KEY(MAKHACHHANG)
);

-- Tạo bảng DIACHI_GIAOHANG
CREATE TABLE DIACHI_GIAOHANG
(
    MADIACHI INT IDENTITY(1,1) PRIMARY KEY,
    MAKHACHHANG INT NOT NULL,
    DUONG NVARCHAR(255),
    QUAN_HUYEN NVARCHAR(255),
    TINH_THANH NVARCHAR(255),
    FOREIGN KEY (MAKHACHHANG) REFERENCES KHACHHANG(MAKHACHHANG)
);

-- Tạo bảng DONHANG
CREATE TABLE DONHANG
(
    MADONHANG INT IDENTITY(1,1) NOT NULL,
    MAKHACHHANG INT NOT NULL,
    NGAYGIAO DATETIME NULL,
    NGAYDAT DATETIME NULL,
    HINHTHUCTHANHTOAN NVARCHAR(255) NULL,
    GHICHU NVARCHAR(255) NULL,
    TONGSLHANG INT NULL,
    TONGTHANHTIEN DECIMAL(18, 0) NULL,
    TRANGTHAI NVARCHAR(50) DEFAULT 'Chờ xử lý', -- Thêm cột trạng thái đơn hàng
    CONSTRAINT PK_DONHANG PRIMARY KEY(MADONHANG),
    CONSTRAINT FK_DONHANG_KHACHHANG FOREIGN KEY(MAKHACHHANG) REFERENCES KHACHHANG(MAKHACHHANG)
);

-- Tạo bảng CHITIETDONHANG
CREATE TABLE CHITIETDONHANG
(
    MADONHANG INT NOT NULL,
    MASANPHAM VARCHAR(10) NOT NULL,
    SOLUONG INT,
    DONGIA DECIMAL(18,0),
    CONSTRAINT PK_CHITIETDONHANG PRIMARY KEY(MADONHANG, MASANPHAM),
    CONSTRAINT FK_CHITIETDONHANG_DONHANG FOREIGN KEY(MADONHANG) REFERENCES DONHANG(MADONHANG),
    CONSTRAINT FK_CHITIETDONHANG_SANPHAM FOREIGN KEY(MASANPHAM) REFERENCES SANPHAM(MASANPHAM)
);

-- Tạo bảng TINH
CREATE TABLE TINH
(
    MATINH INT NOT NULL,
    TENTINH NVARCHAR(255) NOT NULL,
    CONSTRAINT PK_TINH PRIMARY KEY(MATINH)
);

-- Tạo bảng GIAOHANG
CREATE TABLE GIAOHANG
(
    MASANPHAM VARCHAR(10) NOT NULL,
    MATINH INT NOT NULL,
    CONSTRAINT PK_GIAOHANG PRIMARY KEY(MASANPHAM, MATINH),
    CONSTRAINT FK_GIAOHANG_SANPHAM FOREIGN KEY(MASANPHAM) REFERENCES SANPHAM(MASANPHAM),
    CONSTRAINT FK_GIAOHANG_TINH FOREIGN KEY(MATINH) REFERENCES TINH(MATINH)
);




-- Tạo bảng SANPHAM_KHUYENMAI (Bảng nối để quản lý nhiều-nhiều giữa sản phẩm và khuyến mãi)
CREATE TABLE SANPHAM_KHUYENMAI
(
    MASANPHAM VARCHAR(10) NOT NULL,
    MAKHUYENMAI VARCHAR(10) NOT NULL,
    PRIMARY KEY (MASANPHAM, MAKHUYENMAI),
    FOREIGN KEY (MASANPHAM) REFERENCES SANPHAM(MASANPHAM),
    FOREIGN KEY (MAKHUYENMAI) REFERENCES KHUYENMAI(MAKHUYENMAI)
);

-- Tạo bảng DANHGIA để lưu nhận xét và đánh giá sản phẩm
CREATE TABLE DANHGIA
(
    MADANHGIA INT IDENTITY(1,1) PRIMARY KEY,
    MAKHACHHANG INT NOT NULL,
    MASANPHAM VARCHAR(10) NOT NULL,
    DIEMSO INT CHECK(DIEMSO >= 1 AND DIEMSO <= 5),
    NHANXET NVARCHAR(1000),
    NGAYDANHGIA DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MAKHACHHANG) REFERENCES KHACHHANG(MAKHACHHANG),
    FOREIGN KEY (MASANPHAM) REFERENCES SANPHAM(MASANPHAM)
);

-- Trigger để tự động cập nhật số lượng tồn kho sau khi tạo chi tiết đơn hàng
GO
CREATE TRIGGER trg_UpdateInventory
ON CHITIETDONHANG
AFTER INSERT
AS
BEGIN
    UPDATE SANPHAM
    SET SOLUONGTON = SOLUONGTON - i.SOLUONG
    FROM inserted i
    WHERE SANPHAM.MASANPHAM = i.MASANPHAM;
END;

--BẢNG CHẤT LIỆU
INSERT INTO CHATLIEU (MACHATLIEU, TENCHATLIEU)
VALUES
-- SẢN PHẨM CHẤT LIỆU: MÂY --
('MAY', N'Mây'),

-- SẢN PHẨM CHẤT LIỆU: TRE --
('TR', N'Tre'),

-- SẢN PHẨM CHẤT LIỆU: CÓI --
('COI', N'Cói'),

-- SẢN PHẨM CHẤT LIỆU: GUỘT --
('G', N'Guột'),

-- SẢN PHẨM CHẤT LIỆU: LỤC BÌNH--
('LB', N'Lục bình')

---- CÁC SẢN PHẨM KHÁC: QUÀ TỐT NGHIỆP --
--('TN', N'Quà tốt nghiệp'),


-- -- CÁC SẢN PHẨM KHÁC: TRANH SƠN MÀI --
--('TSM', N'Tranh sơn mài'),


---- CÁC SẢN PHẨM KHÁC: PHIN CÀ PHÊ --
--('CP', N'Phin cà phê')
;

--Bảng DANHMUCSANPHAM (Danh mục sản phẩm)
INSERT INTO DANHMUCSANPHAM (TENDANHMUC, MOTA, MADANHMUCCHA)
VALUES
    (N'Chất liệu', N'Sản phẩm theo chất liệu', NULL),             --1
    (N'Lồng bàn', N'Danh mục các loại lồng bàn', NULL),           --2
    (N'Giỏ đựng đồ', N'Danh mục các loại giỏ đựng đồ', NULL),     --3 
    (N'Giỏ quà', N'Danh mục các loại giỏ quà', NULL),             --4
    (N'Đèn', N'Danh mục các loại đèn', NULL),                     --5
	(N'Sản phẩm khác', N'Các sản phẩm khác', NULL),               --6 
    (N'Mây', N'Sản phẩm chất liệu mây', 1),
    (N'Tre', N'Sản phẩm chất liệu tre', 1),
    (N'Cói', N'Sản phẩm chất liệu cói', 1),
    (N'Guột', N'Sản phẩm chất liệu guột', 1),

--Chưa có sản phẩm
	(N'Lục bình', N'Sản phẩm chất liệu lục bình', 1),

	(N'Lồng bàn lưới', N'Sản phẩm lồng bàn lưới', 2),
	(N'Lồng bàn truyền thống', N'Sản phẩm lồng truyền thống', 2),

	(N'Giỏ to', N'Sản phẩm giỏ to', 3),
	(N'Giỏ nhỏ', N'Sản phẩm giỏ nhỏ', 3),
	(N'Giỏ picnic', N'Sản phẩm giỏ picnic', 3),

	(N'Giỏ hoa quả', N'Sản phẩm giỏ hoa quả', 4),
	(N'Giỏ tết & trung thu', N'Sản phẩm giỏ tết & trung thu', 4),

	(N'Đèn trang trí', N'Sản phẩm đèn trang trí', 5),

	(N'Lồng thú cưng', N'Sản phẩm lồng thú cưng', 6),
	(N'Túi xách', N'Túi xách', 6);



--BẢNG SẢN PHẨM
INSERT INTO SANPHAM (MASANPHAM, TENSANPHAM, GIABAN, HINHANH, MOTA, SOLUONGTON, MACHATLIEU) 
VALUES 
-- SẢN PHẨM CHẤT LIỆU: MÂY --
('SP001', N'Giỏ chữ nhật nhỏ', CAST(134000 AS Decimal(18, 0)), 'giochunhatnho.jpg', 
 N'Giỏ cói đựng đồ chữ nhật size nhỏ được đan từ sợi cói với thiết kế đơn giản, nhỏ xinh, được ứng dụng để đựng những món đồ nhỏ nhắn như chìa khóa xe, chìa khóa nhà, các món phụ kiện nhỏ như vòng, khuyên tai hay điều khiển TV, điều khiển điều hòa, giúp cho căn phòng gọn gàng, tinh tế hơn ', 
 50, 'MAY'),
('SP002', N'Khay đũa thìa', CAST(120000 AS Decimal(18, 0)), 'khayduathia.jpg', 
 N'Sản phẩm được làm 100% từ ván tre ép, mà chất liệu tre là một chất liệu bền, ít bị hư hỏng trong quá trình sử dụng. Sản phẩm được làm bán thủ công do đôi bàn tay Việt Nam thực hiện nên sản phẩm được làm một cách tỉ mỉ với những mối nối góc đầy tinh tế. Bề mặt sản phẩm còn được sơn hai lớp giúp chống mối, mốc ngay cả khi tiếp xúc với nước.', 
 10, 'MAY'),
('SP003', N'Giỏ 4 ngăn', CAST(250000 AS Decimal(18, 0)), 'gio4ngan.jpg', 
 N'Giỏ 4 ngăn có kích thước 18x18x13cm, thích hợp để đựng thìa, đũa và các vật dụng nhỏ khác. Sản phẩm được làm thủ công với chất liệu mây, mang lại vẻ đẹp mộc mạc và độ bền cao. Đây là hàng thủ công xuất khẩu.', 
 7, 'MAY'),
('SP004', N'Giỏ Picnic Oval', CAST(360000 AS Decimal(18, 0)), 'giopicnicoval.jpg', 
 N'Giỏ Picnic Oval với kích thước 46x31xH19(37cm) là lựa chọn lý tưởng cho các buổi dã ngoại và picnic. Thiết kế hình oval mang đến sự tiện lợi và phong cách, phù hợp để đựng thức ăn và đồ uống.', 
 5, 'MAY'),
('SP005', N'Hộp giấy ăn', CAST(180000 AS Decimal(18, 0)), 'hopgiayan.jpg', 
 N'Hộp giấy ăn được chế tác từ các nghệ nhân làng nghề truyền thống, với thiết kế tinh xảo và hiện đại. Kích thước 27x14xh9cm, sản phẩm được xử lý chống mối mọt và chống mốc, phù hợp với không gian xanh và tự nhiên.', 
 0, 'MAY'),

-- SẢN PHẨM CHẤT LIỆU: TRE --
('SP006', N'Lồng bàn truyền thống', CAST(185000 AS Decimal(18, 0)), 'longbantruyenthong.jpg', 
 N'Lồng bàn truyền thống có đường kính 60cm và 65cm, được làm từ tre tự nhiên, giúp bảo vệ thực phẩm và tạo ra không khí truyền thống. Được thiết kế để sử dụng trong các bữa ăn gia đình hoặc lễ hội.', 
 12, 'TR'),
('SP007', N'Lồng bàn có khay', CAST(200000 AS Decimal(18, 0)), 'longbancokhay.jpg',
N'Lồng bàn có khay với đường kính 33cm, mang đến sự tiện lợi trong việc bày trí thực phẩm. Sản phẩm làm từ tre tự nhiên, có thiết kế đẹp mắt và dễ dàng vệ sinh, phù hợp cho các bữa tiệc hoặc sự kiện.', 
 6, 'TR'),
('SP008', N'Bát tre có đế', CAST(50000 AS Decimal(18, 0)), 'battrecode.jpg', 
 N'Bát tre có đế làm từ tre tự nhiên với keo đá giúp tạo khuôn chắc chắn. Bề mặt được sơn lớp mỏng hệ nước, đảm bảo an toàn thực phẩm và dễ vệ sinh. Thích hợp cho các món ăn truyền thống.', 
 4, 'TR'),
('SP009', N'Bình tre cuốn', CAST(150000 AS Decimal(18, 0)), 'binhtrecuon.jpg', 
 N'Bình tre cuốn làm từ tre tự nhiên, với lớp sơn mỏng hệ nước bảo vệ bề mặt. Sản phẩm mang đến vẻ đẹp truyền thống và tính năng bảo quản tốt. Được sản xuất tại Việt Nam với sự chăm sóc tỉ mỉ.', 
 5, 'TR'),
('SP010', N'Khay tre có tay cầm', CAST(100000 AS Decimal(18, 0)), 'khaytrecotaycam.jpg', 
 N'Khay tre cuốn hình chữ nhật có tay nắm tiện lợi, được làm từ tre mang lại cảm giác gần gũi với tự nhiên. Sản phẩm thích hợp cho việc trình bày món ăn và đồ uống theo kiểu truyền thống và châu Á.', 
 7, 'TR'),

-- SẢN PHẨM CHẤT LIỆU: CÓI --
('SP011', N'Dép cói', CAST(70000 AS Decimal(18, 0)), 'depcoi.jpg', 
 N'Dép cói thiết kế đơn giản, nhẹ nhàng, rất phù hợp cho mùa hè hoặc các ngày thời tiết ấm áp. Được làm từ sợi cói tự nhiên, sản phẩm mang lại sự thoải mái và vẻ ngoài thời trang.', 
 10, 'COI'),
('SP012', N'Hộp đèn điện trang trí nội thất', CAST(20000 AS Decimal(18, 0)), 'hopdendien.jpg', 
 N'Hộp đèn điện trang trí nội thất có khung đèn làm từ cói, tạo ra vẻ đẹp mộc mạc và ấm cúng. Phù hợp cho việc trang trí phòng khách hoặc các không gian sống, mang lại ánh sáng nhẹ nhàng và tinh tế.', 
 15, 'COI'),
('SP013', N'Hộp đựng đồ nữ', CAST(158000 AS Decimal(18, 0)), 'hopdungdonu.jpg', 
 N'Hộp đựng đồ nữ với thiết kế đơn giản nhưng tinh tế, bề mặt dệt từ sợi cói tạo nên vẻ đẹp mộc mạc và sang trọng. Bên trong lót bằng vải mềm hoặc nhung, lý tưởng để bảo vệ trang sức như dây chuyền, bông tai và nhẫn.', 
 8, 'COI'),
('SP014', N'Túi xách nữ', CAST(285000 AS Decimal(18, 0)), 'tuixachnu.jpg', 
 N'Túi xách nữ có kiểu dáng đơn giản, nhẹ nhàng và tự nhiên. Được làm từ cói, sản phẩm rất thích hợp cho mùa hè, mang lại sự thoải mái và phong cách thanh lịch.', 
 38, 'COI'),
('SP015', N'Vỏ trang trí nậm rượu', CAST(147000 AS Decimal(18, 0)), 'votrangtrinamruou.jpg',
N'Vỏ trang trí nậm rượu bằng cói là lớp bọc bảo vệ nậm rượu, được thiết kế để tạo ra vẻ đẹp mộc mạc và tinh tế. Sản phẩm mang đến sự độc đáo và bảo vệ cho nậm rượu.', 
 28, 'COI'),

-- SẢN PHẨM CHẤT LIỆU: GUỘT --
('SP016', N'Hộp giấy guột cao cấp', CAST(350000 AS Decimal(18, 0)), 'hopgiayguotcaocap.jpg', 
 N'Hộp giấy Guột cao cấp với kích thước 23x15xH10.5cm, được thiết kế riêng biệt với hộp giấy thông dụng của Việt Nam. Chất liệu Guột mang lại sự sang trọng và độ bền cao, phù hợp để đựng giấy ăn hoặc các vật phẩm khác.', 
 50, 'G'),
('SP017', N'Giỏ guột tròn có nắp', CAST(250000 AS Decimal(18, 0)), 'gioguottronconap.jpg', 
 N'Giỏ ruột tròn có nắp được làm từ mây tre đan guột, với thiết kế tỉ mỉ và đẹp mắt. Vẻ đẹp mộc mạc và tự nhiên của sản phẩm phù hợp cho việc lưu trữ và trang trí, tạo điểm nhấn cho không gian sống.', 
 19, 'G');



--KHÁCH HÀNG
INSERT INTO KHACHHANG(HOTEN, NGAYSINH, GIOITINH, SODIENTHOAI, TAIKHOAN, MATKHAU, EMAIL, DIACHI)
VALUES
(N'Võ Nguyễn Minh Quân', '2004-05-20', N'Nam', '0966546750', 'a', '123', 'vnmq@gmail.com', N'33 Phan Huy Ích' ),
(N'Võ Huỳnh Sơn', '2004-06-20', N'Nam', '0123456789', 'b', '456', 'vhs@gmail.com', N'140 Lê Trọng Tấn' ),
(N'Trương Quang Như Đoan', '2004-07-20', N'Nam', '0966546888', 'c', '123', 'tqnd@gmail.com', N'35 Phan Huy Ích' );


--TỈNH
INSERT INTO TINH(MATINH, TENTINH)
VALUES
(1, N'TP Hồ Chí Minh'),
(2, N'Khánh Hòa'),
(3, N'Hà Nội'),
(4, N'Huế'),
(5, N'Đà Nẵng'),
(6, N'Cà Mau');

--GIAO HÀNG
INSERT INTO GIAOHANG (MASANPHAM, MATINH)
VALUES
('SP001', 1),
('SP001', 2),
('SP001', 3),
('SP002', 1),
('SP003', 2),
('SP004', 6),
('SP005', 4),
('SP006', 3),
('SP010', 6),
('SP015', 5),
('SP020', 4),
('SP019', 3),
('SP012', 2),
('SP015', 3),
('SP022', 4),
('SP032', 5),
('SP025', 6);

--ĐƠN HÀNG
INSERT INTO DONHANG(MADONHANG, MAKHACHHANG, NGAYGIAO, NGAYDAT, HINHTHUCTHANHTOAN, GHICHU, TONGSLHANG, TONGTHANHTIEN)
VALUES
(1, 1, '2024-05-20', '2024-05-12', N'Trực tiếp', N'', NULL, NULL),
(2, 2, '2024-06-20', '2024-06-12', N'Chuyển khoản', N'Yêu cầu gấp', NULL, NULL),
(3, 3, '2024-07-20', '2024-07-12', NULL, NULL, NULL, NULL),
(4, 3, '2024-08-20', '2024-08-12', NULL, NULL, 3, CAST(402000 AS Decimal(18, 0))),
(5, 4, '2024-09-20', '2024-09-12', NULL, NULL, 2, CAST(240000 AS Decimal(18, 0)));

--NHÀ CUNG CẤP
INSERT INTO NHACUNGCAP (TENNHACUNGCAP, DTHOAI, DIACHI)
VALUES
    (N'Công ty Tre Việt', '0934567890', N'123 Đường Tre, TP Hồ Chí Minh'),
    (N'Công ty Cói Hà Nội', '0945678901', N'456 Phố Cói, Hà Nội'),
	(N'Công ty Guột Huế', '0967890123', N'12 Đường Guột, Huế'),
    (N'Công ty Lục Bình Đà Nẵng', '0956789012', N'789 Đường Lục Bình, Đà Nẵng');


-- Giả sử bạn có cột LOAISANPHAM để xác định loại sản phẩm
--SELECT *
--FROM SANPHAM
--WHERE MACHATLIEU = (SELECT MACHATLIEU FROM SANPHAM WHERE MASANPHAM = @ProductID)
--AND MASANPHAM != @ProductID;







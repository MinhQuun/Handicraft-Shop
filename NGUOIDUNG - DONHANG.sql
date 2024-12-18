USE QL_MYNGHE;

INSERT INTO QUYEN (MAQUYEN, TENQUYEN) VALUES 
('Q01', N'Admin'),
('Q02', N'NhanVien'),
('Q03', N'KhachHang'),
('Q04', N'NhanVienKhoHang');

INSERT INTO NGUOIDUNG(MANGUOIDUNG, TENNGUOIDUNG, TAIKHOAN, MATKHAU, EMAIL, SODIENTHOAI) VALUES
('ND01', N'Minh Quân', 'minhquun', '123', 'vnmq@gmail.com', '0966546750'),
('ND02', N'Minh Lu', 'lucon', '123', 'lu@gmail.com', '0966546000'),
('ND03', N'Trường Duy', 'duy', '123', 'duy@gmail.com', '0966546111'),
('ND04', N'Như Đoan', 'doan', '123', 'doan@gmail.com', '0966546222'),
('ND05', N'Minh Trí', 'tri', '123', 'tri@gmail.com', '0966546567'),
('ND06', N'Huỳnh Giáp', 'giap', '123', 'giap@gmail.com', '0966546489'),
('ND07', N'Huỳnh Sơn', 'son', '123', 'son@gmail.com', '0966556689'),
('ND08', N'Anh Tú', 'tu', '123', 'tu@gmail.com', '0226556689'),
('ND09', N'Tuấn Khoa', 'khoa', '123', 'khoa@gmail.com', '0919879902'),
('ND10', N'Minh Luân', 'luan', '123', 'minhluan@gmail.com', '0979879902');

INSERT INTO QUYEN_NGUOIDUNG(MANGUOIDUNG, MAQUYEN) VALUES
('ND01', 'Q01'),
('ND02', 'Q02'),
('ND03', 'Q03'),
('ND04', 'Q03'),
('ND05', 'Q03'),
('ND06', 'Q03'),
('ND07', 'Q03'),
('ND08', 'Q03'),
('ND09', 'Q03'),
('ND10', 'Q04');

-- Thêm dữ liệu mẫu vào bảng KHACHHANG chỉ cho những người dùng có quyền 'KhachHang'
INSERT INTO KHACHHANG (MANGUOIDUNG, HOTEN, SODIENTHOAI, EMAIL) VALUES
('ND03', N'Trường Duy', '0966546111', 'duy@gmail.com'),
('ND04', N'Như Đoan', '0966546222', 'doan@gmail.com'),
('ND05', N'Minh Trí', '0966546567', 'tri@gmail.com'),
('ND06', N'Huỳnh Giáp', '0966546489', 'tri@gmail.com'),
('ND07', N'Huỳnh Sơn', '0966556689', 'son@gmail.com'),
('ND08', N'Anh Tú', '0226556689', 'tu@gmail.com'),
('ND09', N'Tuấn Khoa', '0919879902', 'khoa@gmail.com');


INSERT INTO HINHTHUCTT(MATT, LOAITT) VALUES 
('HT01', 'ATM Banking'),
('HT02', 'Tiền mặt');


INSERT INTO DIACHI_GIAOHANG (MAKHACHHANG, DIACHI) VALUES
(1, N'Số 1, Cầu Đá, Nha Trang, Khánh Hòa'),
(2, N'Số 46, Đường Hoàng Việt, Quận Tân Bình, TP. HCM'),
(3, N'Số 35, Đường Phan Huy Ích, Quận Tân Bình, TP. HCM'),
(4, N'Số 140, Đường Lê Trọng Tấn, Quận Tân Phú, TP. HCM'),
(5, N'19 Đ. Nguyễn Hữu Thọ, Tân Hưng, Quận 7, Hồ Chí Minh'),
(6, N'227 Đ. Nguyễn Văn Cừ, Phường 4, Quận 5, Hồ Chí Minh');


INSERT INTO DONHANG (MAKHACHHANG, MADIACHI, NGAYDAT, NGAYGIAO, MATT, GHICHU, TONGSLHANG, TONGTHANHTIEN, TRANGTHAI) VALUES
(1, 1, '2024-11-01', NULL, 'HT01', N'Giao nhanh trong tuần', 5, 638000, N'Đang xử lý'),
(2, 2, '2024-11-02', NULL, 'HT02', N'Giao vào buổi sáng', 3, 840000, N'Đang xử lý'),
(3, 3, '2024-11-03', NULL, 'HT01', N'Không giao vào cuối tuần', 7, 1132000, N'Đang xử lý'),
(4, 4, '2024-11-04', NULL, 'HT02', N'Giao trước 5 giờ chiều', 4, 1010000, N'Đang giao'),
(5, 5, '2024-10-29', NULL, 'HT02', N'Giao sau cổng trường', 10, 12840000, N'Đang xử lý'),
(6, 6, '2024-05-20', NULL, 'HT01', N'Để hàng ở chung cư', 14, 16510000, N'Đang giao');



--SP045 ban chay nhat
INSERT INTO CHITIETDONHANG (MADONHANG, MASANPHAM, SOLUONG, DONGIA) VALUES
-- Chi tiết cho Đơn Hàng 1: TONGSLHANG = 5, TONGTHANHTIEN = 638,000
(1, 'SP001', 2, 134000),
(1, 'SP002', 1, 120000),
(1, 'SP003', 2, 250000),

-- Chi tiết cho Đơn Hàng 2: TONGSLHANG = 3, TONGTHANHTIEN = 840,000
(2, 'SP002', 1, 120000),
(2, 'SP004', 2, 360000),

-- Chi tiết cho Đơn Hàng 3: TONGSLHANG = 7, TONGTHANHTIEN = 1,132,000
(3, 'SP001', 3, 134000),
(3, 'SP005', 2, 180000),
(3, 'SP006', 2, 185000),

-- Chi tiết cho Đơn Hàng 4: TONGSLHANG = 4, TONGTHANHTIEN = 1,010,000
(4, 'SP003', 1, 250000),
(4, 'SP004', 1, 360000),
(4, 'SP007', 2, 200000),

--Chi tiết cho Đơn Hàng 5: TONGSLHANG = 10, TONGTHANHTIEN = 12,840,000
(5, 'SP041', 5, 1800000),
(5, 'SP045', 2, 1100000),
(5, 'SP038', 2, 480000),
(5, 'SP039', 1, 680000),

--Chi tiết cho Đơn Hàng 6: TONGSLHANG = 14, TONGTHANHTIEN = 16,510,000
(6, 'SP044', 1, 1700000),
(6, 'SP062', 3, 1700000),
(6, 'SP045', 8, 1100000),
(6, 'SP075', 2, 455000);
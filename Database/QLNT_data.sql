-- ============================================================
-- QLNT - Du lieu mau (chay SAU QLNT_v2.sql)
-- File UTF-8 co dau. Neu chuoi tieng Viet bi loi font: mo/chay file bang
-- encoding UTF-8 (MySQL: ket noi charset utf8mb4; SQL Server: giu tien to N'...').
--
-- Dac diem du lieu de test:
--   * Hoa don trai deu 4 quy nam 2026, du 4 nhan vien, nhieu khach hang + khach le (MAKH NULL)
--   * Moi hoa don chi ap dung toi da 1 loai giam gia cap hoa don (voucher HOAC moc KM_HOADON,
--     lay loai co so tien giam lon hon)
--   * THUOC.SOLUONG = tong nhap - tong ban (khop voi CTPN/CTHD)
--   * TH0008: sap het han (HSD 2026-11-15) + dang khuyen mai 30%
--   * TH0010: DA het han va ton thap (4)  -> test chan ban thuoc het han
--   * KH0008, NCC0004: chua phat sinh giao dich -> test xoa thanh cong
--   * Xoa nhan vien/khach/thuoc da co giao dich -> phai bi chan (khoa ngoai)
-- ============================================================

-- LOAITHUOC
INSERT INTO LOAITHUOC (MALOAI, TENLOAI) VALUES
    ('LT0001', N'Giảm đau - hạ sốt'),
    ('LT0002', N'Kháng sinh'),
    ('LT0003', N'Vitamin và khoáng chất'),
    ('LT0004', N'Tiêu hóa'),
    ('LT0005', N'Dị ứng - hô hấp');

-- THUOC (SOLUONG = tong nhap - tong ban)
INSERT INTO THUOC (MATHUOC, TENTHUOC, HSD, DONGIA, SOLUONG, MALOAI) VALUES
    ('TH0001', 'Paracetamol 500mg', '2028-06-30', 25000, 6, 'LT0001'),
    ('TH0002', 'Ibuprofen 400mg', '2027-12-31', 35000, 8, 'LT0001'),
    ('TH0003', 'Amoxicillin 500mg', '2027-08-31', 60000, 2, 'LT0002'),
    ('TH0004', 'Azithromycin 250mg', '2027-03-31', 85000, 3, 'LT0002'),
    ('TH0005', 'Vitamin C 1000mg', '2028-01-31', 70000, 5, 'LT0003'),
    ('TH0006', N'Vitamin tổng hợp Multivit', '2027-10-31', 150000, 2, 'LT0003'),
    ('TH0007', 'Omeprazole 20mg', '2027-05-31', 45000, 2, 'LT0004'),
    ('TH0008', 'Berberin 100mg', '2026-11-15', 12000, 4, 'LT0004'),
    ('TH0009', 'Cetirizine 10mg', '2027-09-30', 30000, 0, 'LT0005'),
    ('TH0010', 'Siro ho Prospan', '2026-09-30', 95000, 4, 'LT0005');

-- NHANVIEN
INSERT INTO NHANVIEN (MANV, HOTENNV, CHUCVU, LUONG) VALUES
    ('NV0001', N'Nguyễn Văn An', N'Quản lý', 15000000),
    ('NV0002', N'Trần Thị Bình', N'Dược sĩ', 12000000),
    ('NV0003', N'Lê Minh Châu', N'Dược sĩ', 12000000),
    ('NV0004', N'Phạm Quốc Dũng', N'Thu ngân', 8000000);

-- KHACHHANG
INSERT INTO KHACHHANG (MAKH, HOTEN, SDT, DIACHI) VALUES
    ('KH0001', N'Lê Thị Hoa', '0901234567', N'12 Nguyễn Huệ, Quận 1, TP.HCM'),
    ('KH0002', N'Trần Văn Bình', '0912345678', N'45 Hàng Bài, Hoàn Kiếm, Hà Nội'),
    ('KH0003', N'Nguyễn Thị Cẩm', '0923456789', N'88 Lê Lợi, Quận 3, TP.HCM'),
    ('KH0004', N'Phạm Văn Dũng', '0934567890', N'21 Trần Phú, Hải Châu, Đà Nẵng'),
    ('KH0005', N'Hoàng Thị Em', '0945678901', N'7 Cầu Giấy, Cầu Giấy, Hà Nội'),
    ('KH0006', N'Võ Văn Phúc', '0956789012', N'150 Ninh Kiều, Ninh Kiều, Cần Thơ'),
    ('KH0007', N'Đặng Thị Giang', '0967890123', N'33 Võ Văn Tần, Quận 3, TP.HCM'),
    ('KH0008', N'Bùi Văn Hải', '0978901234', N'9 Nguyễn Trãi, Thanh Xuân, Hà Nội');

-- NHACUNGCAP
INSERT INTO NHACUNGCAP (MANCC, TENNCC, DIACHI, SDT) VALUES
    ('NCC0001', N'Công ty Dược phẩm Minh Châu', N'25 Lý Thường Kiệt, Quận 10, TP.HCM', '02838111111'),
    ('NCC0002', N'Công ty TNHH Dược Việt', N'18 Láng Hạ, Đống Đa, Hà Nội', '02439222222'),
    ('NCC0003', N'Công ty CP Dược Sài Gòn', N'60 Điện Biên Phủ, Bình Thạnh, TP.HCM', '02838333333'),
    ('NCC0004', N'Công ty Dược phẩm An Khang', N'5 Nguyễn Văn Linh, Hải Châu, Đà Nẵng', '02363444444');

-- CTKHUYENMAI
INSERT INTO CTKHUYENMAI (MACTKM, TENCTKM, NGAYBATDAU, NGAYKETTHUC) VALUES
    ('KM0001', N'Vitamin mùa hè', '2026-06-01', '2026-08-31'),
    ('KM0002', N'Khách hàng thân thiết', '2026-01-01', '2026-12-31'),
    ('KM0003', N'Mua nhiều giảm nhiều', '2026-01-01', '2026-12-31'),
    ('KM0004', N'Xả hàng thuốc sắp hết hạn', '2026-10-15', '2026-11-15');

-- KM_SANPHAM
INSERT INTO KM_SANPHAM (MACTKM, MATHUOC, PHANTRAMGIAM) VALUES
    ('KM0001', 'TH0005', 10),
    ('KM0001', 'TH0006', 15),
    ('KM0004', 'TH0008', 30);

-- KM_VOUCHER (CHUNG5 khong gan khach hang)
INSERT INTO KM_VOUCHER (MAVOUCHER, MACTKM, MAKH, PHANTRAMGIAM) VALUES
    ('VIP10', 'KM0002', 'KH0001', 10),
    ('VIP05', 'KM0002', 'KH0002', 5),
    ('CHUNG5', 'KM0002', NULL, 5);

-- KM_HOADON
INSERT INTO KM_HOADON (MACTKM, MUCTIENHD, PHANTRAMGIAM) VALUES
    ('KM0003', 300000, 3),
    ('KM0003', 500000, 5),
    ('KM0003', 1000000, 8);

-- HOADON (TONGTIEN = tong thanh tien - TIENGIAM)
INSERT INTO HOADON (MAHD, MANV, NGAYLAP, TONGTIEN, MAKH, MAVOUCHER, MACTKM_HD, MUCTIENHD, TIENGIAM) VALUES
    ('HD0001', 'NV0003', '2026-01-15', 396000, 'KH0001', 'VIP10', NULL, NULL, 44000),
    ('HD0002', 'NV0004', '2026-02-03', 436500, NULL, NULL, 'KM0003', 300000, 13500),
    ('HD0003', 'NV0001', '2026-03-20', 893000, 'KH0003', NULL, 'KM0003', 500000, 47000),
    ('HD0004', 'NV0002', '2026-04-20', 456000, 'KH0002', 'VIP05', NULL, NULL, 24000),
    ('HD0005', 'NV0004', '2026-05-18', 760000, 'KH0004', NULL, 'KM0003', 500000, 40000),
    ('HD0006', 'NV0004', '2026-06-10', 1512480, 'KH0005', NULL, 'KM0003', 1000000, 131520),
    ('HD0007', 'NV0002', '2026-07-12', 891000, 'KH0001', 'VIP10', NULL, NULL, 99000),
    ('HD0008', 'NV0003', '2026-08-05', 456000, NULL, 'CHUNG5', NULL, NULL, 24000),
    ('HD0009', 'NV0004', '2026-08-22', 1166100, 'KH0006', NULL, 'KM0003', 1000000, 101400),
    ('HD0010', 'NV0001', '2026-09-14', 2005600, 'KH0007', NULL, 'KM0003', 1000000, 174400),
    ('HD0011', 'NV0003', '2026-10-06', 527250, 'KH0003', NULL, 'KM0003', 500000, 27750),
    ('HD0012', 'NV0001', '2026-10-12', 503500, 'KH0002', NULL, 'KM0003', 500000, 26500),
    ('HD0013', 'NV0002', '2026-10-20', 661200, NULL, NULL, 'KM0003', 500000, 34800),
    ('HD0014', 'NV0003', '2026-10-28', 1026000, 'KH0001', 'VIP10', NULL, NULL, 114000);

-- CTHD (THANHTIEN = SOLUONG * DONGIA * (1 - % KM san pham dang hieu luc))
INSERT INTO CTHD (MAHD, MATHUOC, SOLUONG, DONGIA, THANHTIEN) VALUES
    ('HD0001', 'TH0001', 8, 25000, 200000),
    ('HD0001', 'TH0003', 4, 60000, 240000),
    ('HD0002', 'TH0002', 6, 35000, 210000),
    ('HD0002', 'TH0008', 20, 12000, 240000),
    ('HD0003', 'TH0004', 6, 85000, 510000),
    ('HD0003', 'TH0007', 4, 45000, 180000),
    ('HD0003', 'TH0001', 10, 25000, 250000),
    ('HD0004', 'TH0003', 6, 60000, 360000),
    ('HD0004', 'TH0009', 4, 30000, 120000),
    ('HD0005', 'TH0006', 3, 150000, 450000),
    ('HD0005', 'TH0005', 5, 70000, 350000),
    ('HD0006', 'TH0005', 8, 70000, 504000),
    ('HD0006', 'TH0010', 12, 95000, 1140000),
    ('HD0007', 'TH0006', 4, 150000, 510000),
    ('HD0007', 'TH0003', 8, 60000, 480000),
    ('HD0008', 'TH0009', 10, 30000, 300000),
    ('HD0008', 'TH0007', 4, 45000, 180000),
    ('HD0009', 'TH0005', 10, 70000, 630000),
    ('HD0009', 'TH0006', 5, 150000, 637500),
    ('HD0010', 'TH0010', 20, 95000, 1900000),
    ('HD0010', 'TH0002', 8, 35000, 280000),
    ('HD0011', 'TH0001', 12, 25000, 300000),
    ('HD0011', 'TH0004', 3, 85000, 255000),
    ('HD0012', 'TH0002', 10, 35000, 350000),
    ('HD0012', 'TH0009', 6, 30000, 180000),
    ('HD0013', 'TH0008', 40, 12000, 336000),
    ('HD0013', 'TH0007', 8, 45000, 360000),
    ('HD0014', 'TH0006', 6, 150000, 900000),
    ('HD0014', 'TH0003', 4, 60000, 240000);

-- PHIEUNHAP
INSERT INTO PHIEUNHAP (MAPN, MANCC, MANV, NGAYNHAP, TONGTIEN) VALUES
    ('PN0001', 'NCC0001', 'NV0001', '2026-01-10', 2121000),
    ('PN0002', 'NCC0002', 'NV0002', '2026-04-12', 3878000),
    ('PN0003', 'NCC0003', 'NV0001', '2026-07-08', 2660000),
    ('PN0004', 'NCC0001', 'NV0002', '2026-10-05', 1802000);

-- CTPN
INSERT INTO CTPN (MATHUOC, MAPN, SOLUONG, DONGIA) VALUES
    ('TH0001', 'PN0001', 20, 17500),
    ('TH0002', 'PN0001', 16, 24500),
    ('TH0003', 'PN0001', 12, 42000),
    ('TH0004', 'PN0001', 8, 59500),
    ('TH0007', 'PN0001', 6, 31500),
    ('TH0008', 'PN0001', 30, 7000),
    ('TH0005', 'PN0002', 14, 49000),
    ('TH0006', 'PN0002', 6, 105000),
    ('TH0009', 'PN0002', 8, 21000),
    ('TH0010', 'PN0002', 36, 66500),
    ('TH0003', 'PN0003', 12, 42000),
    ('TH0005', 'PN0003', 14, 49000),
    ('TH0007', 'PN0003', 12, 31500),
    ('TH0009', 'PN0003', 12, 21000),
    ('TH0006', 'PN0003', 8, 105000),
    ('TH0001', 'PN0004', 16, 18000),
    ('TH0002', 'PN0004', 16, 25000),
    ('TH0004', 'PN0004', 4, 60000),
    ('TH0006', 'PN0004', 6, 106000),
    ('TH0008', 'PN0004', 34, 7000);

-- THAMSO da co 5 dong mau trong QLNT_v2.sql

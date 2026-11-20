using Microsoft.Data.SqlClient;

internal static partial class Program
{
    private static void KiemTraDonDatWeb()
    {
        var conn = new MyConnection();
        conn.ThucThi("INSERT INTO THUOC (MATHUOC, TENTHUOC, HSD, DONGIA, SOLUONG, MALOAI) " +
            "VALUES ('TH0999', N'Thuốc kiểm thử web', '2030-12-31', 200, 10, 'LT0001')");
        conn.ThucThi("INSERT INTO KM_SANPHAM VALUES ('KM0001', 'TH0999', 10)");
        var bus = new DatThuocBUS();
        var hoaDonBUS = new HoaDonBUS();
        var dongBa = new List<DongDatThuoc> { new() { MaThuoc = "TH0999", SoLuong = 3 } };
        var (gia, loi) = bus.TinhGia(dongBa, "WEB15");
        Check(gia != null && gia.TongGoc == 600m && gia.GiamSanPham == 60m &&
              gia.GiamHoaDon == 108m && gia.TongThanhToan == 432m &&
              gia.MaVoucher == "WEB15" && gia.MaCTKMHD == "KM0001" &&
              gia.PhanTramGiamTheoThuoc["TH0999"] == 10m,
            "Web tinh giam san pham truoc va cong dong voucher voi moc hoa don: " + loi);
        var (giaKhongVoucher, _) = bus.TinhGia(dongBa, null);
        Check(giaKhongVoucher?.MaCTKMHD == "KM0001" && giaKhongVoucher.MucTienHD == 500m &&
              giaKhongVoucher.GiamHoaDon == 27m,
            "Web tu ap dung khuyen mai hoa don theo moc khi khong co voucher");
        Check(bus.TinhGia(dongBa, "VC0001").Gia == null,
            "Web tu choi voucher rieng cua khach hang chua dang nhap");

        DonDatThuoc TaoDon(string phuongThuc, string? voucher = null) => new()
        {
            HoTen = "Khách kiểm thử web", SDT = "0912345678", DiaChi = "Địa chỉ kiểm thử web",
            MaVoucher = voucher, PhuongThucThanhToan = phuongThuc
        };
        int Ton() => Convert.ToInt32(conn.LayGiaTri("SELECT SOLUONG FROM THUOC WHERE MATHUOC='TH0999'"));
        string MaHD(int maDon) => Convert.ToString(conn.LayGiaTri(
            "SELECT MAHD FROM DONDATTHUOC WHERE MADON=@ma", new SqlParameter("@ma", maDon)))!;

        KetQuaDatThuoc datTaiQuay = bus.Dat(TaoDon(PhuongThucThanhToanCode.TaiCuaHang, "WEB15"), dongBa);
        Check(datTaiQuay.ThanhCong && datTaiQuay.MaDon.HasValue && Ton() == 7,
            "Dat web tai quay luu don va tru ton mot lan");
        int maTaiQuay = datTaiQuay.MaDon!.Value;
        Check(Convert.ToDecimal(conn.LayGiaTri("SELECT THANHTIEN FROM CTDONDATTHUOC WHERE MADON=@ma",
                  new SqlParameter("@ma", maTaiQuay))) == 540m,
            "Don web luu thanh tien dong sau giam san pham");
        Check(!bus.XacNhan(maTaiQuay, "NV0001", 0).ThanhCong,
            "Tra tai quay khong tao hoa don neu tien khach dua thieu");
        Check(bus.XacNhan(maTaiQuay, "NV0001", 432m).ThanhCong && Ton() == 7,
            "Xac nhan don web tao hoa don khong tru ton lan hai");
        string hdTaiQuay = MaHD(maTaiQuay);
        Check(Convert.ToDecimal(conn.LayGiaTri(        "SELECT TONGTIEN FROM HOADON WHERE MAHD=@ma",
              new SqlParameter("@ma", hdTaiQuay))) == 432m &&
              Convert.ToDecimal(conn.LayGiaTri("SELECT THANHTIEN FROM CTHD WHERE MAHD=@ma",
                  new SqlParameter("@ma", hdTaiQuay))) == 540m,
            "Hoa don WinForms giu dung gia va khuyen mai da chot tren web");
        Check(hoaDonBUS.HuyHoaDon(hdTaiQuay).ThanhCong && Ton() == 10 &&
              Convert.ToString(conn.LayGiaTri("SELECT TRANGTHAI FROM DONDATTHUOC WHERE MADON=@ma",
                  new SqlParameter("@ma", maTaiQuay))) == "DaHuy",
            "Huy hoa don tu don web hoan ton va doi trang thai don");

        var dongMot = new List<DongDatThuoc> { new() { MaThuoc = "TH0999", SoLuong = 1 } };
        decimal thuTruocCOD = new QLNT.DAO.ThongKeDAO().TongThu(DateTime.Today, DateTime.Today);
        KetQuaDatThuoc datCOD = bus.Dat(TaoDon(PhuongThucThanhToanCode.COD), dongMot);
        Check(datCOD.ThanhCong && datCOD.MaDon.HasValue && Ton() == 9,
            "Dat COD giu thuoc trong kho");
        int maCOD = datCOD.MaDon!.Value;
        Check(bus.XacNhan(maCOD, "NV0001", 0).ThanhCong && Ton() == 9,
            "COD tao hoa don cho thu tien khong tru ton hai lan");
        string hdCOD = MaHD(maCOD);
        Check(Convert.ToString(conn.LayGiaTri("SELECT TRANGTHAITT FROM HOADON WHERE MAHD=@ma",
                  new SqlParameter("@ma", hdCOD))) == "ChoThuTien" &&
              new QLNT.DAO.ThongKeDAO().TongThu(DateTime.Today, DateTime.Today) == thuTruocCOD,
            "COD chua thu khong tinh vao doanh thu");
        Check(!bus.XacNhanThuCOD(maCOD, 100m).ThanhCong &&
              bus.XacNhanThuCOD(maCOD, 180m).ThanhCong &&
              !bus.XacNhanThuCOD(maCOD, 180m).ThanhCong,
            "COD chi ghi nhan thu mot lan khi nhan du tien");
        Check(new QLNT.DAO.ThongKeDAO().TongThu(DateTime.Today, DateTime.Today) == thuTruocCOD + 180m,
            "COD da thu duoc tinh vao doanh thu");
        Check(hoaDonBUS.HuyHoaDon(hdCOD).ThanhCong && Ton() == 10,
            "Huy hoa don COD hoan lai ton kho");

        KetQuaDatThuoc datOnline = bus.Dat(TaoDon(PhuongThucThanhToanCode.Online), dongMot);
        Check(datOnline.ThanhCong && datOnline.MaDon.HasValue && Ton() == 9,
            "Dat online giu thuoc trong kho");
        int maOnline = datOnline.MaDon!.Value;
        Check(!bus.XacNhan(maOnline, "NV0001", 0).ThanhCong &&
              bus.XacNhan(maOnline, "NV0001", 0, true).ThanhCong && Ton() == 9,
            "Online can doi chieu truoc khi tao hoa don");
        Check(hoaDonBUS.HuyHoaDon(MaHD(maOnline)).ThanhCong && Ton() == 10,
            "Huy hoa don online hoan ton kho");

        KetQuaDatThuoc datHuy = bus.Dat(TaoDon(PhuongThucThanhToanCode.COD), dongMot);
        Check(datHuy.ThanhCong && datHuy.MaDon.HasValue && Ton() == 9 &&
              bus.Huy(datHuy.MaDon!.Value).ThanhCong && Ton() == 10,
            "Huy don web chua xu ly hoan ton kho");
    }
}

using System.Collections;
using System.Data;

public class HoaDonBUS
{
    private readonly HoaDonDAO dao = new();
    private readonly ThuocBUS thuocBUS = new();
    private readonly KMSanPhamBUS kmSanPhamBUS = new();
    private readonly KMVoucherBUS voucherBUS = new();
    private readonly KMHoaDonBUS kmHoaDonBUS = new();
    private ArrayList? danhSachHoaDon;

    public ArrayList LayTatCa()
    {
        if (danhSachHoaDon != null)
            return new ArrayList(danhSachHoaDon);

        DataTable bang = dao.LayTatCa();
        danhSachHoaDon = new ArrayList();
        foreach (DataRow dong in bang.Rows)
        {
            danhSachHoaDon.Add(new HoaDon
            {
                MaHD = dong["MAHD"].ToString() ?? string.Empty,
                MaNV = dong["MANV"].ToString() ?? string.Empty,
                TenNhanVien = dong.IsNull("HOTENNV") ? string.Empty : dong["HOTENNV"].ToString() ?? string.Empty,
                NgayLap = Convert.ToDateTime(dong["NGAYLAP"]),
                TongTien = Convert.ToDecimal(dong["TONGTIEN"]),
                MaKH = dong.IsNull("MAKH") ? null : dong["MAKH"].ToString(),
                TenKhachHang = dong.IsNull("TENKHACHHANG") ? "Khách lẻ" : dong["TENKHACHHANG"].ToString() ?? "Khách lẻ",
                MaVoucher = dong.IsNull("MAVOUCHER") ? null : dong["MAVOUCHER"].ToString(),
                MaCTKMHD = dong.IsNull("MACTKM_HD") ? null : dong["MACTKM_HD"].ToString(),
                MucTienHD = dong.IsNull("MUCTIENHD") ? null : Convert.ToDecimal(dong["MUCTIENHD"]),
                TienGiam = dong.IsNull("TIENGIAM") ? 0 : Convert.ToDecimal(dong["TIENGIAM"])
            });
        }

        return new ArrayList(danhSachHoaDon);
    }

    public void TaiLai() => danhSachHoaDon = null;

    public ArrayList TimKiem(string tuKhoa)
    {
        ArrayList tatCa = LayTatCa();
        if (string.IsNullOrWhiteSpace(tuKhoa))
            return tatCa;

        string tuKhoaChuan = tuKhoa.Trim();
        var ketQua = new ArrayList();
        foreach (HoaDon hoaDon in tatCa)
        {
            if (hoaDon.MaHD.Contains(tuKhoaChuan, StringComparison.OrdinalIgnoreCase) ||
                hoaDon.MaNV.Contains(tuKhoaChuan, StringComparison.OrdinalIgnoreCase) ||
                hoaDon.TenNhanVien.Contains(tuKhoaChuan, StringComparison.OrdinalIgnoreCase) ||
                (hoaDon.MaKH?.Contains(tuKhoaChuan, StringComparison.OrdinalIgnoreCase) ?? false) ||
                hoaDon.TenKhachHang.Contains(tuKhoaChuan, StringComparison.OrdinalIgnoreCase))
                ketQua.Add(hoaDon);
        }

        return ketQua;
    }

    public ArrayList LayChiTiet(string maHD)
    {
        if (string.IsNullOrWhiteSpace(maHD))
            throw new ArgumentException("Mã hóa đơn không được để trống.", nameof(maHD));

        DataTable bang = dao.LayChiTiet(maHD.Trim());
        var danhSach = new ArrayList();
        foreach (DataRow dong in bang.Rows)
        {
            danhSach.Add(new CTHoaDon
            {
                MaHD = dong["MAHD"].ToString() ?? string.Empty,
                MaThuoc = dong["MATHUOC"].ToString() ?? string.Empty,
                TenThuoc = dong["TENTHUOC"].ToString() ?? string.Empty,
                SoLuong = Convert.ToInt32(dong["SOLUONG"]),
                DonGia = Convert.ToDecimal(dong["DONGIA"]),
                ThanhTien = Convert.ToDecimal(dong["THANHTIEN"])
            });
        }

        return danhSach;
    }

    public KetQuaTinhHoaDon TinhTienDuThao(
        IReadOnlyCollection<DongHoaDonDuThao> chiTiet,
        DateTime ngayLap,
        string? maKH,
        string? maVoucher,
        IReadOnlyDictionary<string, int>? tonKhoCongLai = null)
    {
        if (chiTiet.Count == 0)
            return KetQuaTinhHoaDon.Loi("Hóa đơn cần có ít nhất một thuốc.");

        var tongSoLuongTheoMa = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (DongHoaDonDuThao dong in chiTiet)
        {
            if (dong.SoLuong <= 0)
                return KetQuaTinhHoaDon.Loi($"Số lượng của thuốc {dong.Thuoc.MaThuoc} phải lớn hơn 0.");

            tongSoLuongTheoMa.TryGetValue(dong.Thuoc.MaThuoc, out long soLuongDaCo);
            tongSoLuongTheoMa[dong.Thuoc.MaThuoc] = soLuongDaCo + (long)dong.SoLuong;
        }

        foreach ((string maThuoc, long soLuong) in tongSoLuongTheoMa)
        {
            Thuoc? thuoc = thuocBUS.LayTheoMa(maThuoc);
            if (thuoc == null)
                return KetQuaTinhHoaDon.Loi($"{maThuoc} - Không tìm thấy thuốc.");
            if (thuoc.HSD.HasValue && thuoc.HSD.Value.Date < ngayLap.Date)
                return KetQuaTinhHoaDon.Loi($"{maThuoc} - Thuốc đã hết hạn.");

            int soLuongDuocHoan = 0;
            if (tonKhoCongLai != null)
                tonKhoCongLai.TryGetValue(maThuoc, out soLuongDuocHoan);
            if (soLuong > (long)thuoc.SoLuong + soLuongDuocHoan)
                return KetQuaTinhHoaDon.Loi($"{maThuoc} - Thuốc không đủ tồn kho.");
        }

        decimal tongGoc = 0;
        decimal tongSauGiamSanPham = 0;
        foreach (DongHoaDonDuThao dong in chiTiet)
        {
            decimal phanTramGiam = kmSanPhamBUS.LayPhanTramGiam(dong.Thuoc.MaThuoc, ngayLap);
            if (phanTramGiam is < 0 or > 100)
                return KetQuaTinhHoaDon.Loi($"Phần trăm giảm của thuốc {dong.Thuoc.MaThuoc} không hợp lệ.");

            dong.PhanTramGiam = phanTramGiam;
            decimal thanhTienGoc = dong.Thuoc.DonGia * dong.SoLuong;
            dong.ThanhTien = Math.Round(thanhTienGoc * (100 - phanTramGiam) / 100m, 2, MidpointRounding.AwayFromZero);
            tongGoc += thanhTienGoc;
            tongSauGiamSanPham += dong.ThanhTien;
        }

        string? maVoucherChuan = string.IsNullOrWhiteSpace(maVoucher) ? null : maVoucher.Trim();
        decimal phanTramVoucher = 0;
        if (maVoucherChuan != null)
        {
            KetQua kiemTraVoucher = voucherBUS.KiemTraHopLe(maVoucherChuan, maKH, ngayLap);
            if (!kiemTraVoucher.ThanhCong)
                return KetQuaTinhHoaDon.Loi(kiemTraVoucher.ThongBao);

            KMVoucher voucher = voucherBUS.LayTheoMa(maVoucherChuan)!;
            phanTramVoucher = voucher.PhanTramGiam;
            if (phanTramVoucher is < 0 or > 100)
                return KetQuaTinhHoaDon.Loi("Phần trăm giảm của voucher không hợp lệ.");
        }

        KMHoaDon? mocApDung = kmHoaDonBUS.LayMocApDung(tongSauGiamSanPham, ngayLap);
        if (mocApDung?.PhanTramGiam is < 0 or > 100)
            return KetQuaTinhHoaDon.Loi("Phần trăm giảm của mốc hóa đơn không hợp lệ.");
        decimal tienGiamVoucher = Math.Round(tongSauGiamSanPham * phanTramVoucher / 100m, 2, MidpointRounding.AwayFromZero);
        decimal tienGiamMoc = mocApDung == null
            ? 0
            : Math.Round(tongSauGiamSanPham * mocApDung.PhanTramGiam / 100m, 2, MidpointRounding.AwayFromZero);

        bool dungVoucher = maVoucherChuan != null && tienGiamVoucher >= tienGiamMoc;
        decimal tienGiamHoaDon = dungVoucher ? tienGiamVoucher : tienGiamMoc;
        decimal tongTien = Math.Max(0, tongSauGiamSanPham - tienGiamHoaDon);

        return KetQuaTinhHoaDon.Ok(
            tongGoc,
            tongGoc - tongSauGiamSanPham,
            tongSauGiamSanPham,
            tienGiamHoaDon,
            dungVoucher ? phanTramVoucher : mocApDung?.PhanTramGiam ?? 0,
            tongTien,
            dungVoucher ? maVoucherChuan : null,
            !dungVoucher ? mocApDung?.MaCTKM : null,
            !dungVoucher ? mocApDung?.MucTienHD : null);
    }

    public KetQuaLuuHoaDon TaoHoaDon(
        NhanVien nhanVien,
        KhachHang? khachHang,
        DateTime ngayLap,
        string? maVoucher,
        IReadOnlyCollection<DongHoaDonDuThao> chiTiet)
    {
        ThuocBUS.TaiLai();
        return LuuHoaDon(null, nhanVien, khachHang, ngayLap, maVoucher, chiTiet, null);
    }

    public KetQuaLuuHoaDon SuaHoaDon(
        HoaDon hoaDonCu,
        NhanVien nhanVien,
        KhachHang? khachHang,
        DateTime ngayLap,
        string? maVoucher,
        IReadOnlyCollection<DongHoaDonDuThao> chiTiet)
    {
        ThuocBUS.TaiLai();
        try
        {
            ArrayList chiTietCu = LayChiTiet(hoaDonCu.MaHD);
            var tonKhoCongLai = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (CTHoaDon dong in chiTietCu)
            {
                tonKhoCongLai.TryGetValue(dong.MaThuoc, out int soLuong);
                tonKhoCongLai[dong.MaThuoc] = checked(soLuong + dong.SoLuong);
            }

            return LuuHoaDon(
                hoaDonCu,
                nhanVien,
                khachHang,
                ngayLap,
                maVoucher,
                chiTiet,
                tonKhoCongLai);
        }
        catch (Exception ex)
        {
            return KetQuaLuuHoaDon.Loi(ex.Message);
        }
    }

    public KetQuaLuuHoaDon HuyHoaDon(string maHD)
    {
        try
        {
            dao.Huy(maHD);
            TaiLai();
            TaiLaiDuLieuBanHang();
            return KetQuaLuuHoaDon.Ok(maHD, "Đã hủy hóa đơn và hoàn lại tồn kho.");
        }
        catch (Exception ex)
        {
            return KetQuaLuuHoaDon.Loi(ex.Message);
        }
    }

    private KetQuaLuuHoaDon LuuHoaDon(
        HoaDon? hoaDonCu,
        NhanVien nhanVien,
        KhachHang? khachHang,
        DateTime ngayLap,
        string? maVoucher,
        IReadOnlyCollection<DongHoaDonDuThao> chiTiet,
        IReadOnlyDictionary<string, int>? tonKhoCongLai)
    {
        if (string.IsNullOrWhiteSpace(nhanVien.MaNV))
            return KetQuaLuuHoaDon.Loi("Hãy chọn nhân viên lập hóa đơn.");

        KetQuaTinhHoaDon tinhTien;
        try
        {
            tinhTien = TinhTienDuThao(
                chiTiet,
                ngayLap,
                khachHang?.MaKH,
                maVoucher,
                tonKhoCongLai);
            if (!tinhTien.ThanhCong)
                return KetQuaLuuHoaDon.Loi(tinhTien.ThongBao);

            var hoaDon = new HoaDon
            {
                MaHD = hoaDonCu?.MaHD ?? string.Empty,
                MaNV = nhanVien.MaNV.Trim(),
                TenNhanVien = nhanVien.HoTenNV,
                NgayLap = ngayLap.Date,
                TongTien = tinhTien.TongTien,
                MaKH = khachHang?.MaKH,
                TenKhachHang = khachHang?.HoTen ?? "Khách lẻ",
                MaVoucher = tinhTien.MaVoucher,
                MaCTKMHD = tinhTien.MaCTKMHD,
                MucTienHD = tinhTien.MucTienHD,
                TienGiam = tinhTien.TienGiamHoaDon
            };
            List<CTHoaDon> chiTietLuu = chiTiet.Select(dong => new CTHoaDon
            {
                MaHD = hoaDon.MaHD,
                MaThuoc = dong.Thuoc.MaThuoc,
                TenThuoc = dong.Thuoc.TenThuoc,
                SoLuong = dong.SoLuong,
                DonGia = dong.Thuoc.DonGia,
                ThanhTien = dong.ThanhTien
            }).ToList();

            if (hoaDonCu == null)
                dao.Them(hoaDon, chiTietLuu);
            else
                dao.Sua(hoaDon, chiTietLuu);

            TaiLai();
            TaiLaiDuLieuBanHang();
            return KetQuaLuuHoaDon.Ok(
                hoaDon.MaHD,
                hoaDonCu == null ? "Đã lưu hóa đơn và trừ tồn kho." : "Đã cập nhật hóa đơn và tồn kho.");
        }
        catch (Exception ex)
        {
            return KetQuaLuuHoaDon.Loi(ex.Message);
        }
    }

    public static void TaiLaiDuLieuBanHang()
    {
        ThuocBUS.TaiLai();
        KMSanPhamBUS.TaiLai();
        KMVoucherBUS.TaiLai();
        KMHoaDonBUS.TaiLai();
        CTKhuyenMaiBUS.TaiLai();
    }
}

public sealed class DongHoaDonDuThao
{
    public Thuoc Thuoc { get; }
    public int SoLuong { get; set; } = 1;
    public decimal PhanTramGiam { get; set; }
    public decimal ThanhTien { get; set; }

    public DongHoaDonDuThao(Thuoc thuoc)
    {
        Thuoc = thuoc;
    }
}

public sealed class KetQuaTinhHoaDon
{
    public bool ThanhCong { get; private init; }
    public string ThongBao { get; private init; } = string.Empty;
    public decimal TongGoc { get; private init; }
    public decimal TienGiamSanPham { get; private init; }
    public decimal TongTienHang { get; private init; }
    public decimal TienGiamHoaDon { get; private init; }
    public decimal PhanTramGiamHoaDon { get; private init; }
    public decimal TongTien { get; private init; }
    public string? MaVoucher { get; private init; }
    public string? MaCTKMHD { get; private init; }
    public decimal? MucTienHD { get; private init; }

    public static KetQuaTinhHoaDon Loi(string thongBao) => new() { ThongBao = thongBao };

    public static KetQuaTinhHoaDon Ok(
        decimal tongGoc,
        decimal tienGiamSanPham,
        decimal tongTienHang,
        decimal tienGiamHoaDon,
        decimal phanTramGiamHoaDon,
        decimal tongTien,
        string? maVoucher,
        string? maCTKMHD,
        decimal? mucTienHD) => new()
        {
            ThanhCong = true,
            TongGoc = tongGoc,
            TienGiamSanPham = tienGiamSanPham,
            TongTienHang = tongTienHang,
            TienGiamHoaDon = tienGiamHoaDon,
            PhanTramGiamHoaDon = phanTramGiamHoaDon,
            TongTien = tongTien,
            MaVoucher = maVoucher,
            MaCTKMHD = maCTKMHD,
            MucTienHD = mucTienHD
        };
}

public sealed class KetQuaLuuHoaDon
{
    public bool ThanhCong { get; private init; }
    public string ThongBao { get; private init; } = string.Empty;
    public string MaHD { get; private init; } = string.Empty;

    public static KetQuaLuuHoaDon Loi(string thongBao) => new() { ThongBao = thongBao };

    public static KetQuaLuuHoaDon Ok(string maHD, string thongBao) =>
        new() { ThanhCong = true, MaHD = maHD, ThongBao = thongBao };
}

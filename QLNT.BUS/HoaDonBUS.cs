using System.Collections;
using System.Data;
using System.Globalization;

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
                TenKhuyenMaiVoucher = dong.IsNull("TENKM_VOUCHER") ? null : dong["TENKM_VOUCHER"].ToString(),
                MaCTKMHD = dong.IsNull("MACTKM_HD") ? null : dong["MACTKM_HD"].ToString(),
                TenKhuyenMaiHoaDon = dong.IsNull("TENKM_HOADON") ? null : dong["TENKM_HOADON"].ToString(),
                MucTienHD = dong.IsNull("MUCTIENHD") ? null : Convert.ToDecimal(dong["MUCTIENHD"]),
                TienGiam = dong.IsNull("TIENGIAM") ? 0 : Convert.ToDecimal(dong["TIENGIAM"]),
                TienKhachDua = dong.IsNull("TIENKHACHDUA") ? null : Convert.ToDecimal(dong["TIENKHACHDUA"]),
                PhuongThucThanhToan = Convert.ToString(dong["PHUONGTHUCTT"]) ?? PhuongThucThanhToanCode.TaiCuaHang,
                TrangThaiThanhToan = Convert.ToString(dong["TRANGTHAITT"]) ?? "DaThanhToan"
            });
        }

        return new ArrayList(danhSachHoaDon);
    }

    public void TaiLai() => danhSachHoaDon = null;

    public string MoTaKhuyenMaiDaApDung(HoaDon hoaDon, decimal tienHangSauGiamSanPham)
    {
        if (string.IsNullOrWhiteSpace(hoaDon.MaVoucher) && string.IsNullOrWhiteSpace(hoaDon.MaCTKMHD))
            return "Không áp dụng khuyến mãi hóa đơn";

        decimal phanTramThucTe = tienHangSauGiamSanPham > 0
            ? hoaDon.TienGiam * 100m / tienHangSauGiamSanPham
            : 0m;
        string mucGiam = $"giảm {phanTramThucTe:0.##}% tiền hàng";

        bool coVoucher = !string.IsNullOrWhiteSpace(hoaDon.MaVoucher);
        bool coKMHoaDon = !string.IsNullOrWhiteSpace(hoaDon.MaCTKMHD);

        if (coVoucher && coKMHoaDon)
        {
            string ten = string.IsNullOrWhiteSpace(hoaDon.TenKhuyenMaiVoucher)
                ? "Voucher" : hoaDon.TenKhuyenMaiVoucher;
            string tenMoc = string.IsNullOrWhiteSpace(hoaDon.TenKhuyenMaiHoaDon)
                ? "Khuyến mãi hóa đơn" : hoaDon.TenKhuyenMaiHoaDon;
            string mucTien = hoaDon.MucTienHD.HasValue
                ? hoaDon.MucTienHD.Value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))
                : "0";
            return $"{ten} - voucher {hoaDon.MaVoucher} + {tenMoc} ({hoaDon.MaCTKMHD}): {mucGiam}; mốc {mucTien} đ";
        }

        if (coVoucher)
        {
            string ten = string.IsNullOrWhiteSpace(hoaDon.TenKhuyenMaiVoucher)
                ? "Voucher" : hoaDon.TenKhuyenMaiVoucher;
            return $"{ten} - voucher {hoaDon.MaVoucher}: {mucGiam}";
        }

        string tenMocCuoi = string.IsNullOrWhiteSpace(hoaDon.TenKhuyenMaiHoaDon)
            ? "Khuyến mãi hóa đơn" : hoaDon.TenKhuyenMaiHoaDon;
        string mucTienCuoi = hoaDon.MucTienHD.HasValue
            ? hoaDon.MucTienHD.Value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))
            : "0";
        return $"{tenMocCuoi} ({hoaDon.MaCTKMHD}): {mucGiam}; mốc {mucTienCuoi} đ";
    }

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

    public ArrayList TimKiemNangCao(BoLocHoaDon loc)
    {
        ArgumentNullException.ThrowIfNull(loc);
        if (loc.Noi is not (PhepNoi.And or PhepNoi.Or))
            throw new ArgumentException("Chỉ được kết hợp điều kiện bằng AND hoặc OR.");
        if (loc.TuNgay.HasValue && loc.DenNgay.HasValue && loc.TuNgay.Value.Date > loc.DenNgay.Value.Date)
            throw new ArgumentException("Từ ngày không được sau Đến ngày.");
        if (loc.TienTu < 0 || loc.TienDen < 0)
            throw new ArgumentException("Khoảng tổng tiền không được âm.");
        if (loc.TienTu.HasValue && loc.TienDen.HasValue && loc.TienTu > loc.TienDen)
            throw new ArgumentException("Tiền từ không được lớn hơn Tiền đến.");

        ArrayList tatCa = LayTatCa();
        var nhomKetQua = new List<ArrayList>();

        void ThemChuoi(string tenTruong, string? giaTri)
        {
            if (!string.IsNullOrWhiteSpace(giaTri))
                nhomKetQua.Add(BoLoc.Loc(tatCa, [new DieuKien
                {
                    TenTruong = tenTruong, Phep = PhepSoSanh.Chua, GiaTri = giaTri.Trim()
                }], PhepNoi.And));
        }

        ArrayList LocNhieuTruong(string giaTri, params string[] truong)
        {
            var dieuKien = truong.Select(ten => new DieuKien
            {
                TenTruong = ten, Phep = PhepSoSanh.Chua, GiaTri = giaTri.Trim()
            }).ToList();
            return BoLoc.Loc(tatCa, dieuKien, PhepNoi.Or);
        }

        if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
            nhomKetQua.Add(LocNhieuTruong(loc.TuKhoa,
                nameof(HoaDon.MaHD), nameof(HoaDon.MaNV), nameof(HoaDon.TenNhanVien),
                nameof(HoaDon.MaKH), nameof(HoaDon.TenKhachHang)));
        ThemChuoi(nameof(HoaDon.MaHD), loc.MaHD);
        if (!string.IsNullOrWhiteSpace(loc.NhanVien))
            nhomKetQua.Add(LocNhieuTruong(loc.NhanVien, nameof(HoaDon.MaNV), nameof(HoaDon.TenNhanVien)));
        if (!string.IsNullOrWhiteSpace(loc.KhachHang))
            nhomKetQua.Add(LocNhieuTruong(loc.KhachHang, nameof(HoaDon.MaKH), nameof(HoaDon.TenKhachHang)));

        var ngay = new List<DieuKien>();
        if (loc.TuNgay.HasValue)
            ngay.Add(new DieuKien { TenTruong = nameof(HoaDon.NgayLap), Phep = PhepSoSanh.LonHoacBang, GiaTri = loc.TuNgay.Value.Date });
        if (loc.DenNgay.HasValue)
            ngay.Add(new DieuKien { TenTruong = nameof(HoaDon.NgayLap), Phep = PhepSoSanh.NhoHoacBang, GiaTri = loc.DenNgay.Value.Date });
        if (ngay.Count > 0)
            nhomKetQua.Add(BoLoc.Loc(tatCa, ngay, PhepNoi.And));

        var tien = new List<DieuKien>();
        if (loc.TienTu.HasValue)
            tien.Add(new DieuKien { TenTruong = nameof(HoaDon.TongTien), Phep = PhepSoSanh.LonHoacBang, GiaTri = loc.TienTu.Value });
        if (loc.TienDen.HasValue)
            tien.Add(new DieuKien { TenTruong = nameof(HoaDon.TongTien), Phep = PhepSoSanh.NhoHoacBang, GiaTri = loc.TienDen.Value });
        if (tien.Count > 0)
            nhomKetQua.Add(BoLoc.Loc(tatCa, tien, PhepNoi.And));

        if (loc.DieuKienThem != null)
        {
            string[] choPhep = [nameof(HoaDon.MaHD), nameof(HoaDon.MaNV), nameof(HoaDon.TenNhanVien),
                nameof(HoaDon.MaKH), nameof(HoaDon.TenKhachHang), nameof(HoaDon.NgayLap),
                nameof(HoaDon.TongTien), nameof(HoaDon.TienGiam), nameof(HoaDon.TienKhachDua),
                nameof(HoaDon.MaVoucher), nameof(HoaDon.MaCTKMHD)];
            if (!choPhep.Contains(loc.DieuKienThem.TenTruong))
                throw new ArgumentException("Trường tìm kiếm bổ sung không hợp lệ.");
            nhomKetQua.Add(BoLoc.Loc(tatCa, [loc.DieuKienThem], PhepNoi.And));
        }

        if (nhomKetQua.Count == 0)
            return tatCa;
        var ketQua = new ArrayList();
        foreach (HoaDon hoaDon in tatCa)
            if (loc.Noi == PhepNoi.And
                ? nhomKetQua.All(nhom => nhom.Contains(hoaDon))
                : nhomKetQua.Any(nhom => nhom.Contains(hoaDon)))
                ketQua.Add(hoaDon);
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

        decimal tienGiamHoaDon = Math.Min(
            tongSauGiamSanPham,
            tienGiamVoucher + tienGiamMoc);
        decimal phanTramGiamHoaDon = tongSauGiamSanPham > 0
            ? Math.Round(tienGiamHoaDon * 100m / tongSauGiamSanPham, 2, MidpointRounding.AwayFromZero)
            : 0m;
        decimal tongTien = Math.Max(0, tongSauGiamSanPham - tienGiamHoaDon);

        return KetQuaTinhHoaDon.Ok(
            tongGoc,
            tongGoc - tongSauGiamSanPham,
            tongSauGiamSanPham,
            tienGiamHoaDon,
            phanTramGiamHoaDon,
            tongTien,
            maVoucherChuan,
            mocApDung?.MaCTKM,
            mocApDung?.MucTienHD,
            maVoucherChuan,
            mocApDung?.MaCTKM,
            mocApDung?.MucTienHD,
            tienGiamVoucher,
            mocApDung == null ? 0 : tienGiamMoc,
            phanTramVoucher,
            mocApDung?.PhanTramGiam ?? 0);
    }

    public KetQuaLuuHoaDon TaoHoaDon(
        NhanVien nhanVien,
        KhachHang? khachHang,
        DateTime ngayLap,
        string? maVoucher,
        IReadOnlyCollection<DongHoaDonDuThao> chiTiet,
        decimal? tienKhachDua = null)
    {
        ThuocBUS.TaiLai();
        return LuuHoaDon(null, nhanVien, khachHang, ngayLap, maVoucher, chiTiet, null, tienKhachDua);
    }

    public KetQuaLuuHoaDon SuaHoaDon(
        HoaDon hoaDonCu,
        NhanVien nhanVien,
        KhachHang? khachHang,
        DateTime ngayLap,
        string? maVoucher,
        IReadOnlyCollection<DongHoaDonDuThao> chiTiet,
        decimal? tienKhachDuaThem = null)
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
                tonKhoCongLai,
                tienKhachDuaThem);
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
        IReadOnlyDictionary<string, int>? tonKhoCongLai,
        decimal? tienKhachDua)
    {
        if (string.IsNullOrWhiteSpace(nhanVien.MaNV))
            return KetQuaLuuHoaDon.Loi("Hãy chọn nhân viên lập hóa đơn.");
        if (tienKhachDua < 0)
            return KetQuaLuuHoaDon.Loi("Tiền khách đưa không được âm.");

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
            decimal canThuThem = hoaDonCu == null
                ? tinhTien.TongTien
                : Math.Max(0, tinhTien.TongTien - hoaDonCu.TongTien);
            if (tienKhachDua is decimal daNhan && daNhan < canThuThem)
                return KetQuaLuuHoaDon.Loi("Tiền khách đưa chưa đủ để thanh toán hóa đơn.");

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
                TienGiam = tinhTien.TienGiamHoaDon,
                TienKhachDua = hoaDonCu?.TienKhachDua is decimal daDua
                    ? checked(daDua + (tienKhachDua ?? 0))
                    : tienKhachDua is > 0 ? tienKhachDua : null
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

            if (hoaDonCu != null)
            {
                hoaDonCu.TienKhachDua = hoaDon.TienKhachDua;
                hoaDonCu.TongTien = hoaDon.TongTien;
            }

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
    public string? MaVoucherDuKien { get; private init; }
    public string? MaCTKMHDDuKien { get; private init; }
    public decimal? MucTienHDDuKien { get; private init; }
    public decimal TienGiamVoucherDuKien { get; private init; }
    public decimal TienGiamHoaDonDuKien { get; private init; }
    public decimal PhanTramVoucherDuKien { get; private init; }
    public decimal PhanTramHoaDonDuKien { get; private init; }

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
        decimal? mucTienHD,
        string? maVoucherDuKien = null,
        string? maCTKMHDDuKien = null,
        decimal? mucTienHDDuKien = null,
        decimal tienGiamVoucherDuKien = 0,
        decimal tienGiamHoaDonDuKien = 0,
        decimal phanTramVoucherDuKien = 0,
        decimal phanTramHoaDonDuKien = 0) => new()
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
            MucTienHD = mucTienHD,
            MaVoucherDuKien = maVoucherDuKien,
            MaCTKMHDDuKien = maCTKMHDDuKien,
            MucTienHDDuKien = mucTienHDDuKien,
            TienGiamVoucherDuKien = tienGiamVoucherDuKien,
            TienGiamHoaDonDuKien = tienGiamHoaDonDuKien,
            PhanTramVoucherDuKien = phanTramVoucherDuKien,
            PhanTramHoaDonDuKien = phanTramHoaDonDuKien
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

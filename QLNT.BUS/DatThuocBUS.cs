using System.Collections;
using System.Data;

public sealed class DatThuocBUS
{
    private readonly ThuocBUS thuocBUS = new();
    private readonly DonDatThuocDAO dao = new();
    private ArrayList? donDaTai;

    public void TaiLaiDon() => donDaTai = null;

    public ArrayList TimThuoc(string? tuKhoa)
    {
        ArrayList danhSach = thuocBUS.TimKiem(tuKhoa ?? string.Empty);
        var ketQua = new ArrayList();
        foreach (Thuoc thuoc in danhSach)
            if (thuoc.SoLuong > 0 && (!thuoc.HSD.HasValue || thuoc.HSD.Value.Date >= DateTime.Today))
                ketQua.Add(thuoc);
        return ketQua;
    }

    public (GiaDonDatThuoc? Gia, string ThongBao) TinhGia(IEnumerable<DongDatThuoc> cacDong, string? maVoucher)
    {
        try
        {
            ThuocBUS.TaiLai();
            KMSanPhamBUS.TaiLai();
            KMHoaDonBUS.TaiLai();
            KMVoucherBUS.TaiLai();
            CTKhuyenMaiBUS.TaiLai();
            var chiTiet = new List<DongHoaDonDuThao>();
            var maDaCo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DongDatThuoc dong in cacDong)
            {
                if (dong.SoLuong is < 1 or > 999 || !maDaCo.Add(dong.MaThuoc))
                    return (null, "Số lượng hoặc mã thuốc trong giỏ không hợp lệ.");
                Thuoc? thuoc = thuocBUS.LayTheoMa(dong.MaThuoc);
                if (thuoc == null) return (null, $"Không tìm thấy thuốc {dong.MaThuoc}.");
                chiTiet.Add(new DongHoaDonDuThao(thuoc) { SoLuong = dong.SoLuong });
            }
            if (chiTiet.Count is < 1 or > 30) return (null, "Chọn từ 1 đến 30 loại thuốc.");
            KetQuaTinhHoaDon tinh = new HoaDonBUS().TinhTienDuThao(chiTiet, DateTime.Today, null, maVoucher);
            if (!tinh.ThanhCong) return (null, tinh.ThongBao);
            return (new GiaDonDatThuoc
            {
                TongGoc = tinh.TongGoc, GiamSanPham = tinh.TienGiamSanPham,
                GiamHoaDon = tinh.TienGiamHoaDon,
                PhanTramGiamHoaDon = tinh.PhanTramGiamHoaDon,
                TongThanhToan = tinh.TongTien,
                MaVoucher = tinh.MaVoucher, MaCTKMHD = tinh.MaCTKMHD, MucTienHD = tinh.MucTienHD,
                ThanhTienTheoThuoc = chiTiet.ToDictionary(d => d.Thuoc.MaThuoc, d => d.ThanhTien,
                    StringComparer.OrdinalIgnoreCase),
                PhanTramGiamTheoThuoc = chiTiet.ToDictionary(d => d.Thuoc.MaThuoc, d => d.PhanTramGiam,
                    StringComparer.OrdinalIgnoreCase)
            }, string.Empty);
        }
        catch (Exception ex)
        {
            return (null, $"Chưa tính được giá đơn: {ex.Message}");
        }
    }

    public KetQuaDatThuoc Dat(DonDatThuoc don, IEnumerable<DongDatThuoc> cacDong)
    {
        ArgumentNullException.ThrowIfNull(don);
        ArgumentNullException.ThrowIfNull(cacDong);
        don.HoTen = don.HoTen?.Trim() ?? string.Empty;
        don.SDT = don.SDT?.Trim() ?? string.Empty;
        don.DiaChi = don.DiaChi?.Trim() ?? string.Empty;
        don.GhiChu = don.GhiChu?.Trim();
        don.MaVoucher = don.MaVoucher?.Trim();
        if (don.HoTen.Length is < 2 or > 100)
            return KetQuaDatThuoc.Loi("Họ tên cần từ 2 đến 100 ký tự.");
        if (don.SDT.Length is < 9 or > 15 || !don.SDT.All(char.IsDigit))
            return KetQuaDatThuoc.Loi("Số điện thoại cần 9 đến 15 chữ số.");
        if (don.DiaChi.Length is < 5 or > 200)
            return KetQuaDatThuoc.Loi("Địa chỉ cần từ 5 đến 200 ký tự.");
        if (don.GhiChu?.Length > 500)
            return KetQuaDatThuoc.Loi("Ghi chú tối đa 500 ký tự.");
        if (don.MaVoucher?.Length > 20)
            return KetQuaDatThuoc.Loi("Mã voucher tối đa 20 ký tự.");
        if (don.PhuongThucThanhToan is not (PhuongThucThanhToanCode.Online or
            PhuongThucThanhToanCode.TaiCuaHang or PhuongThucThanhToanCode.COD))
            return KetQuaDatThuoc.Loi("Hãy chọn phương thức thanh toán hợp lệ.");

        List<DongDatThuoc> chiTiet = cacDong.ToList();
        if (chiTiet.Count == 0 || chiTiet.Count > 30)
            return KetQuaDatThuoc.Loi("Chọn từ 1 đến 30 loại thuốc.");
        if (chiTiet.Any(d => string.IsNullOrWhiteSpace(d.MaThuoc) || d.SoLuong < 1 || d.SoLuong > 999))
            return KetQuaDatThuoc.Loi("Số lượng mỗi thuốc cần từ 1 đến 999.");
        if (chiTiet.Select(d => d.MaThuoc).Distinct(StringComparer.OrdinalIgnoreCase).Count() != chiTiet.Count)
            return KetQuaDatThuoc.Loi("Một loại thuốc chỉ được chọn một lần.");

        try
        {
            (GiaDonDatThuoc? gia, string thongBao) = TinhGia(chiTiet, don.MaVoucher);
            if (gia == null) return KetQuaDatThuoc.Loi(thongBao);
            int maDon = dao.Them(don, chiTiet, gia);
            ThuocBUS.TaiLai();
            return KetQuaDatThuoc.Ok(maDon);
        }
        catch (InvalidOperationException ex)
        {
            return KetQuaDatThuoc.Loi($"Không gửi được đơn: {ex.Message}");
        }
        catch
        {
            return KetQuaDatThuoc.Loi("Không gửi được đơn lúc này. Vui lòng thử lại hoặc liên hệ nhà thuốc.");
        }
    }

    public ArrayList LayDon()
    {
        if (donDaTai != null) return new ArrayList(donDaTai);
        DataTable bang = dao.LayDonMoi();
        var ketQua = new ArrayList();
        foreach (DataRow dong in bang.Rows)
            ketQua.Add(new DonDatThuoc
            {
                MaDon = Convert.ToInt32(dong["MADON"]),
                HoTen = Convert.ToString(dong["HOTEN"]) ?? string.Empty,
                SDT = Convert.ToString(dong["SDT"]) ?? string.Empty,
                DiaChi = Convert.ToString(dong["DIACHI"]) ?? string.Empty,
                GhiChu = dong.IsNull("GHICHU") ? null : Convert.ToString(dong["GHICHU"]),
                NgayDat = Convert.ToDateTime(dong["NGAYDAT"]),
                TrangThai = Convert.ToString(dong["TRANGTHAI"]) ?? "Moi",
                MaVoucher = dong.IsNull("MAVOUCHER") ? null : Convert.ToString(dong["MAVOUCHER"]),
                TienGiam = Convert.ToDecimal(dong["TIENGIAM"]),
                GiamSanPham = Convert.ToDecimal(dong["GIAMSANPHAM"]),
                MaCTKMHD = dong.IsNull("MACTKM_HD") ? null : Convert.ToString(dong["MACTKM_HD"]),
                MucTienHD = dong.IsNull("MUCTIENHD") ? null : Convert.ToDecimal(dong["MUCTIENHD"]),
                TongTamTinh = Convert.ToDecimal(dong["TONGTAMTINH"]),
                DaTruTon = Convert.ToBoolean(dong["DATRUTON"]),
                MaHD = dong.IsNull("MAHD") ? null : Convert.ToString(dong["MAHD"]),
                PhuongThucThanhToan = Convert.ToString(dong["PHUONGTHUCTT"]) ?? PhuongThucThanhToanCode.TaiCuaHang,
                TrangThaiThanhToan = dong.IsNull("TRANGTHAITT") ? null : Convert.ToString(dong["TRANGTHAITT"])
            });
        donDaTai = ketQua;
        return new ArrayList(ketQua);
    }

    public ArrayList TimDon(string? tuKhoa, bool chiDonMoi)
    {
        string noiDung = tuKhoa?.Trim() ?? string.Empty;
        var ketQua = new ArrayList();
        foreach (DonDatThuoc don in LayDon())
        {
            if (chiDonMoi && don.TrangThai != "Moi") continue;
            if (noiDung.Length == 0 || don.MaDon.ToString().Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                don.HoTen.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                don.SDT.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                don.DiaChi.Contains(noiDung, StringComparison.OrdinalIgnoreCase))
                ketQua.Add(don);
        }
        return ketQua;
    }

    public Dictionary<int, List<DongDatThuoc>> LayChiTietDon()
    {
        var ketQua = new Dictionary<int, List<DongDatThuoc>>();
        foreach (DataRow dong in dao.LayChiTiet().Rows)
        {
            int maDon = Convert.ToInt32(dong["MADON"]);
            if (!ketQua.TryGetValue(maDon, out List<DongDatThuoc>? danhSach))
                ketQua[maDon] = danhSach = [];
            danhSach.Add(new DongDatThuoc
            {
                MaDon = maDon,
                MaThuoc = Convert.ToString(dong["MATHUOC"]) ?? string.Empty,
                TenThuoc = Convert.ToString(dong["TENTHUOC"]) ?? string.Empty,
                SoLuong = Convert.ToInt32(dong["SOLUONG"]),
                DonGiaDuKien = Convert.ToDecimal(dong["DONGIADUKIEN"]),
                ThanhTien = Convert.ToDecimal(dong["THANHTIEN"])
            });
        }
        return ketQua;
    }

    public (bool HopLe, decimal PhanTram, string ThongBao) XemVoucher(string? maVoucher)
    {
        if (string.IsNullOrWhiteSpace(maVoucher))
            return (false, 0, "Nhập mã voucher để kiểm tra.");
        if (maVoucher.Trim().Length > 20)
            return (false, 0, "Mã voucher tối đa 20 ký tự.");
        try
        {
            KMVoucherBUS.TaiLai();
            CTKhuyenMaiBUS.TaiLai();
            var voucherBUS = new KMVoucherBUS();
            KetQua hopLe = voucherBUS.KiemTraHopLe(maVoucher.Trim(), null, DateTime.Today);
            if (!hopLe.ThanhCong)
                return (false, 0, hopLe.ThongBao);
            KMVoucher voucher = voucherBUS.LayTheoMa(maVoucher.Trim())!;
            return (true, voucher.PhanTramGiam, $"Voucher hợp lệ: giảm {voucher.PhanTramGiam:N0}%.");
        }
        catch
        {
            return (false, 0, "Chưa kiểm tra được voucher lúc này.");
        }
    }

    public KetQua XacNhan(int maDon, string maNV, decimal tienKhachDua, bool daDoiChieuOnline = false)
    {
        if (maDon <= 0) return KetQua.Loi("Mã đơn không hợp lệ.");
        if (string.IsNullOrWhiteSpace(maNV)) return KetQua.Loi("Hãy chọn nhân viên lập hóa đơn.");
        if (tienKhachDua < 0) return KetQua.Loi("Tiền khách đưa không được âm.");
        try
        {
            NhanVienBUS.TaiLai();
            if (new NhanVienBUS().LayTheoMa(maNV.Trim()) == null)
                return KetQua.Loi("Không tìm thấy nhân viên lập hóa đơn.");
            string maHD = dao.TaoHoaDonTuDon(maDon, maNV.Trim(), tienKhachDua, daDoiChieuOnline);
            TaiLaiDon();
            HoaDonBUS.TaiLaiDuLieuBanHang();
            KhachHangBUS.TaiLai();
            return KetQua.Ok($"Đã tạo hóa đơn {maHD} từ đơn web #{maDon}; tồn kho không bị trừ lần hai.");
        }
        catch (Exception ex)
        {
            return KetQua.Loi($"Chưa tạo được hóa đơn: {ex.Message}");
        }
    }

    public KetQua XacNhanThuCOD(int maDon, decimal tienKhachDua)
    {
        if (maDon <= 0 || tienKhachDua < 0) return KetQua.Loi("Đơn hoặc số tiền không hợp lệ.");
        try
        {
            DonDatThuoc? don = LayDon().Cast<DonDatThuoc>().FirstOrDefault(d => d.MaDon == maDon);
            if (don?.MaHD == null || don.PhuongThucThanhToan != PhuongThucThanhToanCode.COD ||
                don.TrangThaiThanhToan != "ChoThuTien")
                return KetQua.Loi("Đơn không ở trạng thái chờ thu COD.");
            if (tienKhachDua < don.TongTamTinh)
                return KetQua.Loi("Tiền đã thu chưa đủ tổng tiền hóa đơn.");
            if (dao.XacNhanThuCOD(don.MaHD, tienKhachDua) != 1)
                return KetQua.Loi("Hóa đơn không còn chờ thu COD hoặc số tiền chưa đủ.");
            TaiLaiDon();
            return KetQua.Ok($"Đã ghi nhận thu COD cho hóa đơn {don.MaHD}.");
        }
        catch (Exception ex)
        {
            return KetQua.Loi($"Chưa ghi nhận được tiền COD: {ex.Message}");
        }
    }

    public KetQua Huy(int maDon)
    {
        if (maDon <= 0) return KetQua.Loi("Mã đơn không hợp lệ.");
        try
        {
            if (dao.CapNhatTrangThai(maDon, "DaHuy") != 1)
                return KetQua.Loi("Đơn không tồn tại hoặc không còn ở trạng thái mới.");
            ThuocBUS.TaiLai();
            TaiLaiDon();
            return KetQua.Ok($"Đã hủy đơn #{maDon} và hoàn tồn kho.");
        }
        catch
        {
            return KetQua.Loi("Chưa hủy được đơn. Kiểm tra SQL Server.");
        }
    }
}

public sealed class KetQuaDatThuoc
{
    public bool ThanhCong { get; private init; }
    public int? MaDon { get; private init; }
    public string ThongBao { get; private init; } = string.Empty;

    public static KetQuaDatThuoc Loi(string thongBao) => new() { ThongBao = thongBao };
    public static KetQuaDatThuoc Ok(int maDon) => new()
    {
        ThanhCong = true,
        MaDon = maDon,
        ThongBao = $"Đã đặt đơn #{maDon} và giữ thuốc trong kho. Nhà thuốc sẽ liên hệ với bạn."
    };
}

public sealed class DonDatThuoc
{
    public int MaDon { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string SDT { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public DateTime NgayDat { get; set; }
    public string TrangThai { get; set; } = "Moi";
    public string? MaVoucher { get; set; }
    public decimal TienGiam { get; set; }
    public decimal GiamSanPham { get; set; }
    public string? MaCTKMHD { get; set; }
    public decimal? MucTienHD { get; set; }
    public decimal TongTamTinh { get; set; }
    public bool DaTruTon { get; set; }
    public string? MaHD { get; set; }
    public string PhuongThucThanhToan { get; set; } = PhuongThucThanhToanCode.TaiCuaHang;
    public string? TrangThaiThanhToan { get; set; }
}

public static class PhuongThucThanhToanCode
{
    public const string Online = "Online";
    public const string TaiCuaHang = "TaiCuaHang";
    public const string COD = "COD";
}

public sealed class DongDatThuoc
{
    public int MaDon { get; set; }
    public string MaThuoc { get; set; } = string.Empty;
    public string TenThuoc { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGiaDuKien { get; set; }
    public decimal ThanhTien { get; set; }
}

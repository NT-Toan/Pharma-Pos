public class HoaDon
{
    public string MaHD { get; set; } = string.Empty;
    public string MaNV { get; set; } = string.Empty;
    public string TenNhanVien { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; }
    public decimal TongTien { get; set; }
    public string? MaKH { get; set; }
    public string TenKhachHang { get; set; } = "Khách lẻ";
    public string? MaVoucher { get; set; }
    public string? MaCTKMHD { get; set; }
    public decimal? MucTienHD { get; set; }
    public decimal TienGiam { get; set; }
}

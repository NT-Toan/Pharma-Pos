public sealed class BoLocHoaDon
{
    public string? TuKhoa { get; set; }
    public string? MaHD { get; set; }
    public string? NhanVien { get; set; }
    public string? KhachHang { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public decimal? TienTu { get; set; }
    public decimal? TienDen { get; set; }
    public PhepNoi Noi { get; set; } = PhepNoi.And;
    public DieuKien? DieuKienThem { get; set; }
}

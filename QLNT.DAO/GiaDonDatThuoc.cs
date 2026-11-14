public sealed class GiaDonDatThuoc
{
    public decimal TongGoc { get; init; }
    public decimal GiamSanPham { get; init; }
    public decimal GiamHoaDon { get; init; }
    public decimal PhanTramGiamHoaDon { get; init; }
    public decimal TongThanhToan { get; init; }
    public string? MaVoucher { get; init; }
    public string? MaCTKMHD { get; init; }
    public decimal? MucTienHD { get; init; }
    public IReadOnlyDictionary<string, decimal> ThanhTienTheoThuoc { get; init; } =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, decimal> PhanTramGiamTheoThuoc { get; init; } =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
}

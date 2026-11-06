public class CTPhieuNhap
{
    public string MaPN { get; set; } = string.Empty;
    public string MaThuoc { get; set; } = string.Empty;
    public string TenThuoc { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }

    // Bảng CTPN không có cột THANHTIEN; tính từ số lượng và đơn giá nhập.
    public decimal ThanhTien => SoLuong * DonGia;
}

public class KMSanPham
{
    public string MaCTKM { get; set; } = string.Empty;
    public string MaThuoc { get; set; } = string.Empty;
    public decimal PhanTramGiam { get; set; }

    public KMSanPham() { }

    public KMSanPham(string maKM, string maThuoc, decimal phanTram)
    {
        MaCTKM = maKM;
        MaThuoc = maThuoc;
        PhanTramGiam = phanTram;
    }
}
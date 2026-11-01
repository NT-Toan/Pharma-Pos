public class KMHoaDon
{
    public string MaCTKM { get; set; } = string.Empty;
    public decimal MucTienHD { get; set; }
    public decimal PhanTramGiam { get; set; }

    public KMHoaDon() { }

    public KMHoaDon(string maKM, decimal mucTien, decimal phanTram)
    {
        MaCTKM = maKM;
        MucTienHD = mucTien;
        PhanTramGiam = phanTram;
    }
}
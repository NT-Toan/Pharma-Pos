public class KMVoucher
{
    public string MaVoucher { get; set; } = string.Empty;
    public string MaCTKM { get; set; } = string.Empty;
    public string? MaKH { get; set; }
    public decimal PhanTramGiam { get; set; }

    public KMVoucher() { }

    public KMVoucher(string maVoucher, string maKM, string? maKH, decimal phanTram)
    {
        MaVoucher = maVoucher;
        MaCTKM = maKM;
        MaKH = maKH;
        PhanTramGiam = phanTram;
    }
}
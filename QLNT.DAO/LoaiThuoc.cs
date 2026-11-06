public class LoaiThuoc
{
    public string MaLoai { get; set; }
    public string TenLoai { get; set; }

    public LoaiThuoc(string maLoai, string tenLoai)
    {
        MaLoai = maLoai;
        TenLoai = tenLoai;
    }
}

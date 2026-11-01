public class KetQua
{
    public bool ThanhCong;
    public string ThongBao = string.Empty;

    public static KetQua Ok(string thongBao = "")
    {
        return new KetQua { ThanhCong = true, ThongBao = thongBao };
    }

    public static KetQua Loi(string thongBao)
    {
        return new KetQua { ThanhCong = false, ThongBao = thongBao };
    }
}
using System.Data;

public class NhanVienDAO
{
    public DataTable LayTatCa()
    {
        const string sql = "SELECT MANV, HOTENNV, CHUCVU FROM NHANVIEN ORDER BY HOTENNV, MANV";
        return new MyConnection().LayBang(sql);
    }
}

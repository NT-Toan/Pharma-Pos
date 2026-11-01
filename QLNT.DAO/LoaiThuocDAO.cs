using System.Data;

public class LoaiThuocDAO
{
    public DataTable LayTatCa()
    {
        MyConnection myConnection = new MyConnection();
        return myConnection.LayBang("SELECT MALOAI, TENLOAI FROM LOAITHUOC");
    }
}

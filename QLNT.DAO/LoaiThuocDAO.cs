using System.Data;
using Microsoft.Data.SqlClient;
public class LoaiThuocDAO
{
    public DataTable LayTatCa()
    {
        MyConnection myConnection = new MyConnection();
        return myConnection.LayBang("SELECT MALOAI, TENLOAI FROM LOAITHUOC");
    }
    public int Them(LoaiThuoc loaithuoc)
    {
        const string sql = "INSERT INTO LOAITHUOC (MALOAI, TENLOAI) VALUES (@ma, @ten) ";

        var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
        {
            Value = loaithuoc.MaLoai
        };

        var ten = new SqlParameter("@ten", SqlDbType.NVarChar, 100)
        {
            Value = loaithuoc.TenLoai
        };
        return new MyConnection().ThucThi(sql, ma, ten);
    }
    public int Sua(LoaiThuoc loaiThuoc)
    {
        const string sql = "UPDATE LOAITHUOC SET TENLOAI = @ten WHERE MALOAI = @ma";
        var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
        {
            Value = loaiThuoc.MaLoai
        };
        var ten = new SqlParameter("@ten", SqlDbType.NVarChar, 100)
        {
            Value = loaiThuoc.TenLoai
        };
        return new MyConnection().ThucThi(sql, ma, ten);
    }

    public int Xoa(string maLoai)
    {
        const string sql = "DELETE FROM LOAITHUOC WHERE MALOAI = @ma";
        var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
        {
            Value = maLoai
        };
        return new MyConnection().ThucThi(sql, ma);
    }
}


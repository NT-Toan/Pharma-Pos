using System.Data;
using Microsoft.Data.SqlClient;

public class ThamSoDAO
{
    public DataTable LayTatCa()
    {
        const string sql =
            "SELECT MATHAMSO, GIATRI FROM THAMSO ORDER BY MATHAMSO";

        return new MyConnection().LayBang(sql);
    }

    public int Them(ThamSo thamSo)
    {
        const string sql =
            "INSERT INTO THAMSO (MATHAMSO, GIATRI) VALUES (@ma, @giaTri)";

        return new MyConnection().ThucThi(sql,
            new SqlParameter("@ma", thamSo.MaThamSo),
            new SqlParameter("@giaTri", thamSo.GiaTri));
    }

    public int Sua(ThamSo thamSo)
    {
        const string sql =
            "UPDATE THAMSO SET GIATRI = @giaTri WHERE MATHAMSO = @ma";

        return new MyConnection().ThucThi(sql,
            new SqlParameter("@ma", thamSo.MaThamSo),
            new SqlParameter("@giaTri", thamSo.GiaTri));
    }

    public int Xoa(string maThamSo)
    {
        const string sql =
            "DELETE FROM THAMSO WHERE MATHAMSO = @ma";

        return new MyConnection().ThucThi(sql,
            new SqlParameter("@ma", maThamSo));
    }
}
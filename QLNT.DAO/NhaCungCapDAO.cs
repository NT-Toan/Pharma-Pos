using System.Data;
using Microsoft.Data.SqlClient;
public class NhaCungCapDAO
{
    public DataTable LayTatCa()
    {
        const string sql =
            "SELECT MANCC, TENNCC, DIACHI, SDT FROM NHACUNGCAP";

        return new MyConnection().LayBang(sql);
    }

    public int Them(NhaCungCap ncc)
{
    const string sql =
        "INSERT INTO NHACUNGCAP (MANCC, TENNCC, DIACHI, SDT) " +
        "VALUES (@ma, @ten, @diaChi, @sdt)";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = ncc.MaNCC
    };
    var ten = new SqlParameter("@ten", SqlDbType.NVarChar, 100)
    {
        Value = ncc.TenNCC
    };
    var diaChi = new SqlParameter("@diaChi", SqlDbType.NVarChar, 200)
    {
        Value = string.IsNullOrWhiteSpace(ncc.DiaChi)
            ? (object)DBNull.Value : ncc.DiaChi.Trim()
    };
    var sdt = new SqlParameter("@sdt", SqlDbType.VarChar, 15)
    {
        Value = string.IsNullOrWhiteSpace(ncc.SDT)
            ? (object)DBNull.Value : ncc.SDT.Trim()
    };

    return new MyConnection().ThucThi(sql, ma, ten, diaChi, sdt);
}

public int Sua(NhaCungCap ncc)
{
    const string sql =
        "UPDATE NHACUNGCAP SET TENNCC = @ten, " +
        "DIACHI = @diaChi, SDT = @sdt WHERE MANCC = @ma";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = ncc.MaNCC
    };
    var ten = new SqlParameter("@ten", SqlDbType.NVarChar, 100)
    {
        Value = ncc.TenNCC
    };
    var diaChi = new SqlParameter("@diaChi", SqlDbType.NVarChar, 200)
    {
        Value = string.IsNullOrWhiteSpace(ncc.DiaChi)
            ? (object)DBNull.Value : ncc.DiaChi.Trim()
    };
    var sdt = new SqlParameter("@sdt", SqlDbType.VarChar, 15)
    {
        Value = string.IsNullOrWhiteSpace(ncc.SDT)
            ? (object)DBNull.Value : ncc.SDT.Trim()
    };

    return new MyConnection().ThucThi(sql, ten, diaChi, sdt, ma);
}

public int Xoa(string maNCC)
{
    const string sql = "DELETE FROM NHACUNGCAP WHERE MANCC = @ma";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = maNCC
    };

    return new MyConnection().ThucThi(sql, ma);
}

public DataTable LayTheoMa(string maNCC)
{
    const string sql =
        "SELECT MANCC, TENNCC, DIACHI, SDT " +
        "FROM NHACUNGCAP WHERE MANCC = @ma";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = maNCC
    };

    return new MyConnection().LayBang(sql, ma);
}
}
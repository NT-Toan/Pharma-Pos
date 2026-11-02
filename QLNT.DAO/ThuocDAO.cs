using System.Data;
using Microsoft.Data.SqlClient;
public class ThuocDAO
{
    public DataTable LayTatCa()
    {
        const string sql =
            "SELECT MATHUOC, TENTHUOC, HSD, DONGIA, SOLUONG, MALOAI FROM THUOC";

        return new MyConnection().LayBang(sql);
    }
    
    public DataTable LayTheoMa(string maThuoc)
{
    const string sql =
        "SELECT MATHUOC, TENTHUOC, HSD, DONGIA, SOLUONG, MALOAI " +
        "FROM THUOC WHERE MATHUOC = @ma";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = maThuoc
    };

    return new MyConnection().LayBang(sql, ma);
}

public int Them(Thuoc thuoc)
{
    const string sql =
        "INSERT INTO THUOC (MATHUOC, TENTHUOC, HSD, DONGIA, MALOAI) " +
        "VALUES (@ma, @ten, @hsd, @donGia, @maLoai)";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = thuoc.MaThuoc
    };
    var ten = new SqlParameter("@ten", SqlDbType.NVarChar, 100)
    {
        Value = thuoc.TenThuoc
    };
    var hsd = new SqlParameter("@hsd", SqlDbType.Date)
    {
        Value = (object?)thuoc.HSD?.Date ?? DBNull.Value
    };
    var donGia = new SqlParameter("@donGia", SqlDbType.Decimal)
    {
        Precision = 18,
        Scale = 2,
        Value = thuoc.DonGia
    };
    var maLoai = new SqlParameter("@maLoai", SqlDbType.VarChar, 10)
    {
        Value = thuoc.MaLoai
    };

    return new MyConnection().ThucThi(sql, ma, ten, hsd, donGia, maLoai);
}

public int Sua(Thuoc thuoc)
{
    const string sql =
        "UPDATE THUOC SET TENTHUOC = @ten, HSD = @hsd, " +
        "DONGIA = @donGia, MALOAI = @maLoai " +
        "WHERE MATHUOC = @ma";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = thuoc.MaThuoc
    };
    var ten = new SqlParameter("@ten", SqlDbType.NVarChar, 100)
    {
        Value = thuoc.TenThuoc
    };
    var hsd = new SqlParameter("@hsd", SqlDbType.Date)
    {
        Value = (object?)thuoc.HSD?.Date ?? DBNull.Value
    };
    var donGia = new SqlParameter("@donGia", SqlDbType.Decimal)
    {
        Precision = 18,
        Scale = 2,
        Value = thuoc.DonGia
    };
    var maLoai = new SqlParameter("@maLoai", SqlDbType.VarChar, 10)
    {
        Value = thuoc.MaLoai
    };

    return new MyConnection().ThucThi(
        sql, ten, hsd, donGia, maLoai, ma);
}

public int Xoa(string maThuoc)
{
    const string sql = "DELETE FROM THUOC WHERE MATHUOC = @ma";

    var ma = new SqlParameter("@ma", SqlDbType.VarChar, 10)
    {
        Value = maThuoc
    };

    return new MyConnection().ThucThi(sql, ma);
}
}
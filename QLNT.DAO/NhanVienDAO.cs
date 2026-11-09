using System.Data;
using Microsoft.Data.SqlClient;

public class NhanVienDAO
{
    public DataTable LayTatCa()
    {
        const string sql = "SELECT MANV, HOTENNV, CHUCVU, LUONG FROM NHANVIEN ORDER BY HOTENNV, MANV";
        return new MyConnection().LayBang(sql);
    }
    public int Them(NhanVien nv)
{
    const string sql = """
        INSERT INTO NHANVIEN (MANV, HOTENNV, CHUCVU, LUONG)
        VALUES (@ma, @ten, @chucVu, @luong)
        """;

    return new MyConnection().ThucThi(sql,
        new SqlParameter("@ma", nv.MaNV),
        new SqlParameter("@ten", nv.HoTenNV),
        new SqlParameter("@chucVu",
            string.IsNullOrWhiteSpace(nv.ChucVu) ? DBNull.Value : nv.ChucVu),
        new SqlParameter("@luong", (object?)nv.Luong ?? DBNull.Value));
}
public int Sua(NhanVien nv)
{
    const string sql = """
        UPDATE NHANVIEN
        SET HOTENNV = @ten, CHUCVU = @chucVu, LUONG = @luong
        WHERE MANV = @ma
        """;

    return new MyConnection().ThucThi(sql,
        new SqlParameter("@ma", nv.MaNV),
        new SqlParameter("@ten", nv.HoTenNV),
        new SqlParameter("@chucVu",
            string.IsNullOrWhiteSpace(nv.ChucVu) ? DBNull.Value : nv.ChucVu),
        new SqlParameter("@luong", (object?)nv.Luong ?? DBNull.Value));
}
public int Xoa(string maNV)
{
    const string sql = "DELETE FROM NHANVIEN WHERE MANV = @ma";

    return new MyConnection().ThucThi(
        sql,
        new SqlParameter("@ma", maNV));
}
}

using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
public class ThuocDAO
{
    public int ThemHangLoat(ArrayList danhSach)
    {
        ArgumentNullException.ThrowIfNull(danhSach);
        if (danhSach.Count == 0) throw new ArgumentException("Danh sách nhập trống.");
        return new MyConnection().ChayGiaoDich((ketNoi, giaoDich) =>
        {
            int soLonNhat = 0;
            using (var lenh = new SqlCommand("SELECT MATHUOC FROM THUOC WITH (UPDLOCK, HOLDLOCK)", ketNoi, giaoDich))
            using (var reader = lenh.ExecuteReader())
                while (reader.Read())
                {
                    string ma = reader.GetString(0);
                    if (ma.Length == 6 && ma.StartsWith("TH", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(ma.AsSpan(2), out int so)) soLonNhat = Math.Max(soLonNhat, so);
                }
            if ((long)soLonNhat + danhSach.Count > 9999)
                throw new InvalidOperationException("Không còn đủ mã thuốc theo định dạng TH0001.");
            foreach (Thuoc thuoc in danhSach)
            {
                using var lenh = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM THUOC WHERE TENTHUOC = @ten AND MALOAI = @loai " +
                    "AND (HSD = @hsd OR (HSD IS NULL AND @hsd IS NULL))) " +
                    "THROW 50001, N'Thuốc trùng tên, loại và hạn dùng. Toàn bộ lô nhập bị hủy.', 1; " +
                    "INSERT INTO THUOC (MATHUOC, TENTHUOC, HSD, DONGIA, SOLUONG, MALOAI) " +
                    "VALUES (@ma, @ten, @hsd, @gia, 0, @loai)", ketNoi, giaoDich);
                lenh.Parameters.Add(new SqlParameter("@ma", SqlDbType.VarChar, 10) { Value = "TH" + (++soLonNhat).ToString("D4") });
                lenh.Parameters.Add(new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = thuoc.TenThuoc });
                lenh.Parameters.Add(new SqlParameter("@hsd", SqlDbType.Date) { Value = (object?)thuoc.HSD?.Date ?? DBNull.Value });
                lenh.Parameters.Add(new SqlParameter("@gia", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = thuoc.DonGia });
                lenh.Parameters.Add(new SqlParameter("@loai", SqlDbType.VarChar, 10) { Value = thuoc.MaLoai });
                if (lenh.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("Lệnh thêm thuốc không tác động đúng 1 dòng; toàn bộ lô nhập bị hủy.");
            }
            return danhSach.Count;
        });
    }

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

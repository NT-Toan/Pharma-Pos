using System.Data;
using Microsoft.Data.SqlClient;

public class KhachHangDAO
{
    public DataTable LayTatCa()
    {
        const string sql = "SELECT MAKH, HOTEN, SDT, DIACHI FROM KHACHHANG ORDER BY HOTEN, MAKH";
        return new MyConnection().LayBang(sql);
    }

    public string Them(KhachHang khachHang)
    {
        return new MyConnection().ChayGiaoDich((ketNoi, giaoDich) =>
        {
            using var lenhMa = new SqlCommand(
                "SELECT MAKH FROM KHACHHANG WITH (UPDLOCK, HOLDLOCK)",
                ketNoi,
                giaoDich);
            using SqlDataReader reader = lenhMa.ExecuteReader();
            int soLonNhat = 0;
            while (reader.Read())
            {
                string ma = reader.GetString(0);
                if (ma.StartsWith("KH", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(ma.AsSpan(2), out int so))
                    soLonNhat = Math.Max(soLonNhat, so);
            }
            reader.Close();

            if (soLonNhat >= 9999)
                throw new InvalidOperationException("Đã hết mã khách hàng theo định dạng KH0001.");

            string maMoi = "KH" + (soLonNhat + 1).ToString("D4");
            using var lenhThem = new SqlCommand(
                "INSERT INTO KHACHHANG (MAKH, HOTEN, SDT, DIACHI) " +
                "VALUES (@ma, @hoTen, @sdt, @diaChi)",
                ketNoi,
                giaoDich);
            lenhThem.Parameters.Add(new SqlParameter("@ma", SqlDbType.VarChar, 10) { Value = maMoi });
            lenhThem.Parameters.Add(new SqlParameter("@hoTen", SqlDbType.NVarChar, 100) { Value = khachHang.HoTen.Trim() });
            lenhThem.Parameters.Add(new SqlParameter("@sdt", SqlDbType.VarChar, 15)
            {
                Value = string.IsNullOrWhiteSpace(khachHang.SDT) ? DBNull.Value : khachHang.SDT.Trim()
            });
            lenhThem.Parameters.Add(new SqlParameter("@diaChi", SqlDbType.NVarChar, 200)
            {
                Value = string.IsNullOrWhiteSpace(khachHang.DiaChi) ? DBNull.Value : khachHang.DiaChi.Trim()
            });

            if (lenhThem.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("Không thêm được khách hàng.");
            return maMoi;
        });
    }
}

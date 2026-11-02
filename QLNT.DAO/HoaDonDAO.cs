using System.Data;
using Microsoft.Data.SqlClient;

public class HoaDonDAO
{
    private readonly MyConnection conn = new();

    public DataTable LayTatCa()
    {
        const string sql =
            "SELECT hd.MAHD, hd.MANV, nv.HOTENNV, hd.NGAYLAP, hd.TONGTIEN, " +
            "hd.MAKH, kh.HOTEN AS TENKHACHHANG, hd.MAVOUCHER, hd.MACTKM_HD, " +
            "hd.MUCTIENHD, hd.TIENGIAM " +
            "FROM HOADON hd " +
            "LEFT JOIN NHANVIEN nv ON nv.MANV = hd.MANV " +
            "LEFT JOIN KHACHHANG kh ON kh.MAKH = hd.MAKH " +
            "ORDER BY hd.NGAYLAP DESC, hd.MAHD DESC";

        return conn.LayBang(sql);
    }

    public DataTable LayChiTiet(string maHD)
    {
        const string sql =
            "SELECT ct.MAHD, ct.MATHUOC, t.TENTHUOC, ct.SOLUONG, ct.DONGIA, ct.THANHTIEN " +
            "FROM CTHD ct " +
            "INNER JOIN THUOC t ON t.MATHUOC = ct.MATHUOC " +
            "WHERE ct.MAHD = @maHD " +
            "ORDER BY t.TENTHUOC, ct.MATHUOC";

        var thamSo = new SqlParameter("@maHD", SqlDbType.VarChar, 10)
        {
            Value = maHD
        };
        return conn.LayBang(sql, thamSo);
    }

    public void Them(HoaDon hoaDon, IReadOnlyCollection<CTHoaDon> chiTiet)
    {
        conn.ChayGiaoDich((ketNoi, giaoDich) =>
        {
            hoaDon.MaHD = TaoMaHoaDonTrongGiaoDich(ketNoi, giaoDich);
            using (var lenh = new SqlCommand(
                "INSERT INTO HOADON (MAHD, MANV, NGAYLAP, TONGTIEN, MAKH, MAVOUCHER, MACTKM_HD, MUCTIENHD, TIENGIAM) " +
                "VALUES (@maHD, @maNV, @ngayLap, @tongTien, @maKH, @maVoucher, @maCTKM, @mucTien, @tienGiam)",
                ketNoi, giaoDich))
            {
                ThemThamSoHoaDon(lenh, hoaDon);
                KiemTraSoDong(lenh.ExecuteNonQuery(), 1, "Không thêm được hóa đơn.");
            }

            GhiChiTietVaTruTon(ketNoi, giaoDich, hoaDon, chiTiet);
            return true;
        });
    }

    public void Sua(HoaDon hoaDon, IReadOnlyCollection<CTHoaDon> chiTietMoi)
    {
        conn.ChayGiaoDich((ketNoi, giaoDich) =>
        {
            KiemTraHoaDonTonTai(ketNoi, giaoDich, hoaDon.MaHD);
            List<CTHoaDon> chiTietCu = LayChiTietTrongGiaoDich(ketNoi, giaoDich, hoaDon.MaHD);
            if (chiTietCu.Count == 0)
                throw new InvalidOperationException("Không tìm thấy chi tiết hóa đơn cần sửa.");

            CongTon(ketNoi, giaoDich, chiTietCu);
            using (var lenh = new SqlCommand("DELETE FROM CTHD WHERE MAHD = @maHD", ketNoi, giaoDich))
            {
                lenh.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = hoaDon.MaHD });
                KiemTraSoDong(lenh.ExecuteNonQuery(), chiTietCu.Count, "Không xóa đủ chi tiết hóa đơn cũ.");
            }

            using (var lenh = new SqlCommand(
                "UPDATE HOADON SET MANV = @maNV, NGAYLAP = @ngayLap, TONGTIEN = @tongTien, " +
                "MAKH = @maKH, MAVOUCHER = @maVoucher, MACTKM_HD = @maCTKM, " +
                "MUCTIENHD = @mucTien, TIENGIAM = @tienGiam WHERE MAHD = @maHD",
                ketNoi, giaoDich))
            {
                ThemThamSoHoaDon(lenh, hoaDon);
                KiemTraSoDong(lenh.ExecuteNonQuery(), 1, "Không tìm thấy hóa đơn cần sửa.");
            }

            GhiChiTietVaTruTon(ketNoi, giaoDich, hoaDon, chiTietMoi);
            return true;
        });
    }

    public void Huy(string maHD)
    {
        conn.ChayGiaoDich((ketNoi, giaoDich) =>
        {
            KiemTraHoaDonTonTai(ketNoi, giaoDich, maHD);
            List<CTHoaDon> chiTiet = LayChiTietTrongGiaoDich(ketNoi, giaoDich, maHD);
            if (chiTiet.Count == 0)
                throw new InvalidOperationException("Không tìm thấy chi tiết hóa đơn cần hủy.");

            CongTon(ketNoi, giaoDich, chiTiet);
            using (var lenh = new SqlCommand("DELETE FROM CTHD WHERE MAHD = @maHD", ketNoi, giaoDich))
            {
                lenh.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });
                KiemTraSoDong(lenh.ExecuteNonQuery(), chiTiet.Count, "Không xóa đủ chi tiết hóa đơn.");
            }

            using (var lenh = new SqlCommand("DELETE FROM HOADON WHERE MAHD = @maHD", ketNoi, giaoDich))
            {
                lenh.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });
                KiemTraSoDong(lenh.ExecuteNonQuery(), 1, "Không tìm thấy hóa đơn cần hủy.");
            }

            return true;
        });
    }

    private static void GhiChiTietVaTruTon(
        SqlConnection ketNoi,
        SqlTransaction giaoDich,
        HoaDon hoaDon,
        IReadOnlyCollection<CTHoaDon> chiTiet)
    {
        foreach (CTHoaDon dong in chiTiet.OrderBy(dong => dong.MaThuoc, StringComparer.OrdinalIgnoreCase))
        {
            using (var lenh = new SqlCommand(
                "UPDATE THUOC WITH (UPDLOCK, ROWLOCK) " +
                "SET SOLUONG = SOLUONG - @soLuong " +
                "WHERE MATHUOC = @maThuoc AND SOLUONG >= @soLuong " +
                "AND (HSD IS NULL OR HSD >= @ngayLap)",
                ketNoi, giaoDich))
            {
                lenh.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
                lenh.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                lenh.Parameters.Add(new SqlParameter("@ngayLap", SqlDbType.Date) { Value = hoaDon.NgayLap.Date });
                KiemTraSoDong(
                    lenh.ExecuteNonQuery(),
                    1,
                    $"Không thể bán {dong.MaThuoc}: thuốc không còn đủ tồn, đã hết hạn hoặc không còn tồn tại.");
            }

            using (var lenh = new SqlCommand(
                "INSERT INTO CTHD (MAHD, MATHUOC, SOLUONG, DONGIA, THANHTIEN) " +
                "VALUES (@maHD, @maThuoc, @soLuong, @donGia, @thanhTien)",
                ketNoi, giaoDich))
            {
                lenh.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = hoaDon.MaHD });
                lenh.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                lenh.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
                lenh.Parameters.Add(TaoTien("@donGia", dong.DonGia));
                lenh.Parameters.Add(TaoTien("@thanhTien", dong.ThanhTien));
                KiemTraSoDong(lenh.ExecuteNonQuery(), 1, $"Không lưu được chi tiết thuốc {dong.MaThuoc}.");
            }
        }
    }

    private static void CongTon(SqlConnection ketNoi, SqlTransaction giaoDich, IEnumerable<CTHoaDon> chiTiet)
    {
        foreach (CTHoaDon dong in chiTiet.OrderBy(dong => dong.MaThuoc, StringComparer.OrdinalIgnoreCase))
        {
            using var lenh = new SqlCommand(
                "UPDATE THUOC WITH (UPDLOCK, ROWLOCK) SET SOLUONG = SOLUONG + @soLuong WHERE MATHUOC = @maThuoc",
                ketNoi, giaoDich);
            lenh.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
            lenh.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
            KiemTraSoDong(lenh.ExecuteNonQuery(), 1, $"Không thể hoàn tồn cho thuốc {dong.MaThuoc}.");
        }
    }

    private static string TaoMaHoaDonTrongGiaoDich(SqlConnection ketNoi, SqlTransaction giaoDich)
    {
        using var lenh = new SqlCommand("SELECT MAHD FROM HOADON WITH (UPDLOCK, HOLDLOCK)", ketNoi, giaoDich);
        using SqlDataReader reader = lenh.ExecuteReader();
        int soLonNhat = 0;
        while (reader.Read())
        {
            string ma = reader.GetString(0);
            if (!ma.StartsWith("HD", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(ma.AsSpan(2), out int so))
                continue;
            soLonNhat = Math.Max(soLonNhat, so);
        }

        if (soLonNhat >= 9999)
            throw new InvalidOperationException("Đã hết mã hóa đơn theo định dạng HD0001.");
        return "HD" + (soLonNhat + 1).ToString("D4");
    }

    private static List<CTHoaDon> LayChiTietTrongGiaoDich(
        SqlConnection ketNoi,
        SqlTransaction giaoDich,
        string maHD)
    {
        using var lenh = new SqlCommand(
            "SELECT MAHD, MATHUOC, SOLUONG, DONGIA, THANHTIEN " +
            "FROM CTHD WITH (UPDLOCK, HOLDLOCK) WHERE MAHD = @maHD",
            ketNoi, giaoDich);
        lenh.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });
        using SqlDataReader reader = lenh.ExecuteReader();
        var chiTiet = new List<CTHoaDon>();
        while (reader.Read())
        {
            chiTiet.Add(new CTHoaDon
            {
                MaHD = reader.GetString(0),
                MaThuoc = reader.GetString(1),
                SoLuong = reader.GetInt32(2),
                DonGia = reader.GetDecimal(3),
                ThanhTien = reader.GetDecimal(4)
            });
        }

        return chiTiet;
    }

    private static void KiemTraHoaDonTonTai(SqlConnection ketNoi, SqlTransaction giaoDich, string maHD)
    {
        using var lenh = new SqlCommand(
            "SELECT MAHD FROM HOADON WITH (UPDLOCK, HOLDLOCK) WHERE MAHD = @maHD",
            ketNoi, giaoDich);
        lenh.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });
        if (lenh.ExecuteScalar() == null)
            throw new InvalidOperationException("Không tìm thấy hóa đơn cần thao tác.");
    }

    private static void ThemThamSoHoaDon(SqlCommand lenh, HoaDon hoaDon)
    {
        lenh.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = hoaDon.MaHD });
        lenh.Parameters.Add(new SqlParameter("@maNV", SqlDbType.VarChar, 10) { Value = hoaDon.MaNV });
        lenh.Parameters.Add(new SqlParameter("@ngayLap", SqlDbType.Date) { Value = hoaDon.NgayLap.Date });
        lenh.Parameters.Add(TaoTien("@tongTien", hoaDon.TongTien));
        lenh.Parameters.Add(new SqlParameter("@maKH", SqlDbType.VarChar, 10) { Value = (object?)hoaDon.MaKH ?? DBNull.Value });
        lenh.Parameters.Add(new SqlParameter("@maVoucher", SqlDbType.VarChar, 20) { Value = (object?)hoaDon.MaVoucher ?? DBNull.Value });
        lenh.Parameters.Add(new SqlParameter("@maCTKM", SqlDbType.VarChar, 10) { Value = (object?)hoaDon.MaCTKMHD ?? DBNull.Value });
        lenh.Parameters.Add(new SqlParameter("@mucTien", SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 2,
            Value = (object?)hoaDon.MucTienHD ?? DBNull.Value
        });
        lenh.Parameters.Add(TaoTien("@tienGiam", hoaDon.TienGiam));
    }

    private static SqlParameter TaoTien(string ten, decimal giaTri) => new(ten, SqlDbType.Decimal)
    {
        Precision = 18,
        Scale = 2,
        Value = giaTri
    };

    private static void KiemTraSoDong(int thucTe, int mongDoi, string thongBao)
    {
        if (thucTe != mongDoi)
            throw new InvalidOperationException(thongBao);
    }
}

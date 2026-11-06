using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;

public class PhieuNhapDAO
{
    private readonly MyConnection conn = new();

    public DataTable LayTatCa()
    {
        const string sql =
            "SELECT pn.MAPN, pn.MANCC, ncc.TENNCC, pn.MANV, nv.HOTENNV, " +
            "pn.NGAYNHAP, pn.TONGTIEN " +
            "FROM PHIEUNHAP pn " +
            "INNER JOIN NHACUNGCAP ncc ON ncc.MANCC = pn.MANCC " +
            "INNER JOIN NHANVIEN nv ON nv.MANV = pn.MANV " +
            "ORDER BY pn.NGAYNHAP DESC, pn.MAPN DESC";

        return conn.LayBang(sql);
    }

    public DataTable LayChiTiet(string maPN)
    {
        const string sql =
            "SELECT ct.MAPN, ct.MATHUOC, t.TENTHUOC, ct.SOLUONG, ct.DONGIA, " +
            "ct.SOLUONG * ct.DONGIA AS THANHTIEN " +
            "FROM CTPN ct " +
            "INNER JOIN THUOC t ON t.MATHUOC = ct.MATHUOC " +
            "WHERE ct.MAPN = @maPN " +
            "ORDER BY t.TENTHUOC, ct.MATHUOC";

        var thamSo = new SqlParameter("@maPN", SqlDbType.VarChar, 10)
        {
            Value = maPN
        };

        return conn.LayBang(sql, thamSo);
    }

    public string Them(PhieuNhap phieu, ArrayList chiTiet)
    {
        ArgumentNullException.ThrowIfNull(phieu);
        ArgumentNullException.ThrowIfNull(chiTiet);
        if (chiTiet.Count == 0)
            throw new InvalidOperationException("Phiếu nhập cần có ít nhất một thuốc.");

        return conn.ChayGiaoDich((ketNoi, giaoDich) =>
        {
            string maPN = TaoMaTrongGiaoDich(ketNoi, giaoDich);
            using (var lenh = new SqlCommand(
                "INSERT INTO PHIEUNHAP (MAPN, MANCC, MANV, NGAYNHAP, TONGTIEN) " +
                "VALUES (@maPN, @maNCC, @maNV, @ngayNhap, @tongTien)", ketNoi, giaoDich))
            {
                lenh.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = maPN });
                lenh.Parameters.Add(new SqlParameter("@maNCC", SqlDbType.VarChar, 10) { Value = phieu.MaNCC });
                lenh.Parameters.Add(new SqlParameter("@maNV", SqlDbType.VarChar, 10) { Value = phieu.MaNV });
                lenh.Parameters.Add(new SqlParameter("@ngayNhap", SqlDbType.Date) { Value = phieu.NgayNhap.Date });
                lenh.Parameters.Add(TaoTien("@tongTien", phieu.TongTien));
                KiemTraMotDong(lenh.ExecuteNonQuery(), "Không thêm được phiếu nhập.");
            }

            // Cùng thứ tự khóa thuốc với luồng bán hàng để giảm nguy cơ deadlock.
            foreach (CTPhieuNhap dong in chiTiet.Cast<CTPhieuNhap>()
                         .OrderBy(d => d.MaThuoc, StringComparer.OrdinalIgnoreCase))
            {
                if (dong.SoLuong <= 0 || dong.DonGia < 0)
                    throw new InvalidOperationException("Số lượng hoặc đơn giá nhập không hợp lệ.");

                using (var lenh = new SqlCommand(
                    "UPDATE THUOC WITH (UPDLOCK, ROWLOCK) SET SOLUONG = SOLUONG + @soLuong " +
                    "WHERE MATHUOC = @maThuoc AND SOLUONG <= @tonToiDa", ketNoi, giaoDich))
                {
                    lenh.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
                    lenh.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                    lenh.Parameters.Add(new SqlParameter("@tonToiDa", SqlDbType.Int) { Value = int.MaxValue - dong.SoLuong });
                    KiemTraMotDong(lenh.ExecuteNonQuery(),
                        $"Không tăng được tồn kho thuốc {dong.MaThuoc}: thuốc không còn tồn tại hoặc vượt giới hạn số lượng.");
                }

                using (var lenh = new SqlCommand(
                    "INSERT INTO CTPN (MAPN, MATHUOC, SOLUONG, DONGIA) " +
                    "VALUES (@maPN, @maThuoc, @soLuong, @donGia)", ketNoi, giaoDich))
                {
                    lenh.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = maPN });
                    lenh.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                    lenh.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
                    lenh.Parameters.Add(TaoTien("@donGia", dong.DonGia));
                    KiemTraMotDong(lenh.ExecuteNonQuery(), $"Không thêm được chi tiết thuốc {dong.MaThuoc}.");
                }
            }

            return maPN;
        });
    }

    public DuLieuPhieuNhap LayDeSua(string maPN) => conn.ChayGiaoDich(
        (ketNoi, giaoDich) => DocTrongGiaoDich(ketNoi, giaoDich, maPN));

    public void Sua(PhieuNhap phieu, ArrayList chiTiet, DuLieuPhieuNhap? banGoc = null)
    {
        ArgumentNullException.ThrowIfNull(phieu);
        ArgumentNullException.ThrowIfNull(chiTiet);
        if (chiTiet.Count == 0)
            throw new InvalidOperationException("Phiếu nhập cần có ít nhất một thuốc.");
        conn.ChayGiaoDich((ketNoi, giaoDich) =>
        {
            DuLieuPhieuNhap hienTai = DocTrongGiaoDich(ketNoi, giaoDich, phieu.MaPN);
            KiemTraBanGoc(hienTai, banGoc);
            DieuChinhTon(ketNoi, giaoDich, hienTai.ChiTiet, chiTiet);

            XoaChiTiet(ketNoi, giaoDich, phieu.MaPN, hienTai.ChiTiet.Count);
            foreach (CTPhieuNhap dong in chiTiet)
            {
                using var lenh = new SqlCommand(
                    "INSERT INTO CTPN (MAPN, MATHUOC, SOLUONG, DONGIA) " +
                    "VALUES (@maPN, @maThuoc, @soLuong, @donGia)", ketNoi, giaoDich);
                lenh.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = phieu.MaPN });
                lenh.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                lenh.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
                lenh.Parameters.Add(TaoTien("@donGia", dong.DonGia));
                KiemTraMotDong(lenh.ExecuteNonQuery(), $"Không lưu được chi tiết thuốc {dong.MaThuoc}.");
            }

            using var sua = new SqlCommand(
                "UPDATE PHIEUNHAP SET MANCC = @maNCC, MANV = @maNV, " +
                "NGAYNHAP = @ngayNhap, TONGTIEN = @tongTien WHERE MAPN = @maPN", ketNoi, giaoDich);
            sua.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = phieu.MaPN });
            sua.Parameters.Add(new SqlParameter("@maNCC", SqlDbType.VarChar, 10) { Value = phieu.MaNCC });
            sua.Parameters.Add(new SqlParameter("@maNV", SqlDbType.VarChar, 10) { Value = phieu.MaNV });
            sua.Parameters.Add(new SqlParameter("@ngayNhap", SqlDbType.Date) { Value = phieu.NgayNhap.Date });
            sua.Parameters.Add(TaoTien("@tongTien", phieu.TongTien));
            KiemTraMotDong(sua.ExecuteNonQuery(), "Phiếu nhập không còn tồn tại.");
            return true;
        });
    }

    public void Xoa(string maPN, DuLieuPhieuNhap? banGoc = null)
    {
        conn.ChayGiaoDich((ketNoi, giaoDich) =>
        {
            DuLieuPhieuNhap hienTai = DocTrongGiaoDich(ketNoi, giaoDich, maPN);
            KiemTraBanGoc(hienTai, banGoc);
            DieuChinhTon(ketNoi, giaoDich, hienTai.ChiTiet, new ArrayList());
            XoaChiTiet(ketNoi, giaoDich, maPN, hienTai.ChiTiet.Count);
            using var lenh = new SqlCommand("DELETE FROM PHIEUNHAP WHERE MAPN = @maPN", ketNoi, giaoDich);
            lenh.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = maPN });
            KiemTraMotDong(lenh.ExecuteNonQuery(), "Không xóa được phiếu nhập.");
            return true;
        });
    }

    private static DuLieuPhieuNhap DocTrongGiaoDich(SqlConnection ketNoi, SqlTransaction giaoDich, string maPN)
    {
        var duLieu = new DuLieuPhieuNhap();
        using (var lenh = new SqlCommand(
            "SELECT MAPN, MANCC, MANV, NGAYNHAP, TONGTIEN FROM PHIEUNHAP WITH (UPDLOCK, HOLDLOCK) " +
            "WHERE MAPN = @maPN", ketNoi, giaoDich))
        {
            lenh.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = maPN });
            using var reader = lenh.ExecuteReader();
            if (!reader.Read())
                throw new InvalidOperationException("Phiếu nhập không còn tồn tại. Hãy tải lại danh sách.");
            duLieu.Phieu = new PhieuNhap
            {
                MaPN = reader.GetString(0), MaNCC = reader.GetString(1), MaNV = reader.GetString(2),
                NgayNhap = reader.GetDateTime(3), TongTien = reader.GetDecimal(4)
            };
        }
        using (var lenh = new SqlCommand(
            "SELECT MATHUOC, SOLUONG, DONGIA FROM CTPN WITH (UPDLOCK, HOLDLOCK) WHERE MAPN = @maPN",
            ketNoi, giaoDich))
        {
            lenh.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = maPN });
            using var reader = lenh.ExecuteReader();
            while (reader.Read())
                duLieu.ChiTiet.Add(new CTPhieuNhap
                {
                    MaPN = maPN, MaThuoc = reader.GetString(0), SoLuong = reader.GetInt32(1), DonGia = reader.GetDecimal(2)
                });
        }
        return duLieu;
    }

    private static void KiemTraBanGoc(DuLieuPhieuNhap hienTai, DuLieuPhieuNhap? banGoc)
    {
        if (banGoc == null) return;
        PhieuNhap a = hienTai.Phieu, b = banGoc.Phieu;
        bool giong = a.MaPN == b.MaPN && a.MaNV == b.MaNV && a.MaNCC == b.MaNCC &&
                     a.NgayNhap == b.NgayNhap && a.TongTien == b.TongTien &&
                     hienTai.ChiTiet.Count == banGoc.ChiTiet.Count;
        foreach (CTPhieuNhap dong in hienTai.ChiTiet)
            giong &= banGoc.ChiTiet.Cast<CTPhieuNhap>().Any(c =>
                c.MaThuoc.Equals(dong.MaThuoc, StringComparison.OrdinalIgnoreCase) &&
                c.SoLuong == dong.SoLuong && c.DonGia == dong.DonGia);
        if (!giong)
            throw new InvalidOperationException("Phiếu đã được người khác thay đổi. Hãy đóng và mở lại phiếu trước khi sửa/xóa.");
    }

    private static void DieuChinhTon(SqlConnection ketNoi, SqlTransaction giaoDich, ArrayList cu, ArrayList moi)
    {
        var cacMa = new ArrayList();
        foreach (CTPhieuNhap dong in cu.Cast<CTPhieuNhap>().Concat(moi.Cast<CTPhieuNhap>()))
            if (!cacMa.Cast<string>().Contains(dong.MaThuoc, StringComparer.OrdinalIgnoreCase))
                cacMa.Add(dong.MaThuoc);
        foreach (string ma in cacMa.Cast<string>().OrderBy(m => m, StringComparer.OrdinalIgnoreCase))
        {
            long soCu = cu.Cast<CTPhieuNhap>().Where(d => d.MaThuoc.Equals(ma, StringComparison.OrdinalIgnoreCase)).Sum(d => (long)d.SoLuong);
            long soMoi = moi.Cast<CTPhieuNhap>().Where(d => d.MaThuoc.Equals(ma, StringComparison.OrdinalIgnoreCase)).Sum(d => (long)d.SoLuong);
            long chenhLech = soMoi - soCu;
            if (chenhLech == 0) continue;
            using var lenh = new SqlCommand(
                "UPDATE THUOC WITH (UPDLOCK, ROWLOCK) SET SOLUONG = CONVERT(int, CONVERT(bigint, SOLUONG) + @chenhLech) " +
                "WHERE MATHUOC = @ma AND CONVERT(bigint, SOLUONG) + @chenhLech BETWEEN 0 AND 2147483647",
                ketNoi, giaoDich);
            lenh.Parameters.Add(new SqlParameter("@ma", SqlDbType.VarChar, 10) { Value = ma });
            lenh.Parameters.Add(new SqlParameter("@chenhLech", SqlDbType.BigInt) { Value = chenhLech });
            KiemTraMotDong(lenh.ExecuteNonQuery(),
                $"Không điều chỉnh được tồn thuốc {ma}: không đủ tồn để hoàn phiếu, thuốc không tồn tại hoặc vượt giới hạn số lượng.");
        }
    }

    private static void XoaChiTiet(SqlConnection ketNoi, SqlTransaction giaoDich, string maPN, int soDongCu)
    {
        using var lenh = new SqlCommand("DELETE FROM CTPN WHERE MAPN = @maPN", ketNoi, giaoDich);
        lenh.Parameters.Add(new SqlParameter("@maPN", SqlDbType.VarChar, 10) { Value = maPN });
        if (lenh.ExecuteNonQuery() != soDongCu)
            throw new InvalidOperationException("Không xóa đủ chi tiết cũ; toàn bộ giao dịch sẽ được hủy.");
    }

    private static string TaoMaTrongGiaoDich(SqlConnection ketNoi, SqlTransaction giaoDich)
    {
        using var lenh = new SqlCommand(
            "SELECT MAPN FROM PHIEUNHAP WITH (UPDLOCK, HOLDLOCK)", ketNoi, giaoDich);
        using SqlDataReader reader = lenh.ExecuteReader();
        int soLonNhat = 0;
        while (reader.Read())
        {
            string ma = reader.GetString(0);
            if (ma.Length == 6 && ma.StartsWith("PN", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(ma.AsSpan(2), out int so))
                soLonNhat = Math.Max(soLonNhat, so);
        }

        if (soLonNhat >= 9999)
            throw new InvalidOperationException("Đã hết mã phiếu nhập theo định dạng PN0001.");

        return "PN" + (soLonNhat + 1).ToString("D4");
    }

    private static SqlParameter TaoTien(string ten, decimal giaTri) => new(ten, SqlDbType.Decimal)
    {
        Precision = 18,
        Scale = 2,
        Value = giaTri
    };

    private static void KiemTraMotDong(int soDong, string thongBao)
    {
        if (soDong != 1)
            throw new InvalidOperationException(thongBao);
    }
}

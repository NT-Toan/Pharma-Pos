using System.Data;
using Microsoft.Data.SqlClient;

public sealed class DonDatThuocDAO
{
    public int Them(DonDatThuoc don, IReadOnlyList<DongDatThuoc> chiTiet, GiaDonDatThuoc giaDon)
    {
        return new MyConnection().ChayGiaoDich((ketNoi, giaoDich) =>
        {
            decimal tong = 0;
            var cacDong = new List<DongDatThuoc>();
            foreach (DongDatThuoc dong in chiTiet.OrderBy(d => d.MaThuoc, StringComparer.OrdinalIgnoreCase))
            {
                using var layThuoc = new SqlCommand(
                    "SELECT TENTHUOC, DONGIA, SOLUONG, HSD FROM dbo.THUOC WITH (UPDLOCK, HOLDLOCK) WHERE MATHUOC = @ma",
                    ketNoi, giaoDich);
                layThuoc.Parameters.Add(new SqlParameter("@ma", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                using SqlDataReader doc = layThuoc.ExecuteReader();
                if (!doc.Read())
                    throw new InvalidOperationException($"Không tìm thấy thuốc {dong.MaThuoc}.");
                string ten = doc.GetString(0);
                decimal gia = doc.GetDecimal(1);
                int ton = doc.GetInt32(2);
                DateTime? han = doc.IsDBNull(3) ? null : doc.GetDateTime(3);
                doc.Close();
                if (han.HasValue && han.Value.Date < DateTime.Today)
                    throw new InvalidOperationException($"{ten} đã hết hạn.");
                if (dong.SoLuong > ton)
                    throw new InvalidOperationException($"{ten} chỉ còn {ton} sản phẩm.");
                tong = checked(tong + gia * dong.SoLuong);
                cacDong.Add(new DongDatThuoc
                {
                    MaThuoc = dong.MaThuoc, TenThuoc = ten,
                    SoLuong = dong.SoLuong, DonGiaDuKien = gia
                });
            }

            if (tong != giaDon.TongGoc ||
                giaDon.ThanhTienTheoThuoc.Count != cacDong.Count ||
                giaDon.ThanhTienTheoThuoc.Values.Sum() != tong - giaDon.GiamSanPham ||
                giaDon.TongThanhToan != tong - giaDon.GiamSanPham - giaDon.GiamHoaDon)
                throw new InvalidOperationException("Giá thuốc hoặc khuyến mãi đã thay đổi. Hãy tải lại giỏ hàng.");

            using var themDon = new SqlCommand(
                "INSERT INTO dbo.DONDATTHUOC (HOTEN, SDT, DIACHI, GHICHU, TONGTAMTINH, MAVOUCHER, TIENGIAM, " +
                "GIAMSANPHAM, MACTKM_HD, MUCTIENHD, DATRUTON, PHUONGTHUCTT) " +
                "OUTPUT INSERTED.MADON VALUES (@ten, @sdt, @diaChi, @ghiChu, @tong, @voucher, @giam, " +
                "@giamSP, @maCTKM, @mucTien, 1, @phuongThuc)", ketNoi, giaoDich);
            themDon.Parameters.Add(new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = don.HoTen });
            themDon.Parameters.Add(new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = don.SDT });
            themDon.Parameters.Add(new SqlParameter("@diaChi", SqlDbType.NVarChar, 200) { Value = don.DiaChi });
            themDon.Parameters.Add(new SqlParameter("@ghiChu", SqlDbType.NVarChar, 500)
            { Value = string.IsNullOrWhiteSpace(don.GhiChu) ? DBNull.Value : don.GhiChu });
            themDon.Parameters.Add(new SqlParameter("@tong", SqlDbType.Decimal)
            { Precision = 18, Scale = 2, Value = giaDon.TongThanhToan });
            themDon.Parameters.Add(new SqlParameter("@voucher", SqlDbType.VarChar, 20)
            { Value = (object?)giaDon.MaVoucher ?? DBNull.Value });
            themDon.Parameters.Add(new SqlParameter("@giam", SqlDbType.Decimal)
            { Precision = 18, Scale = 2, Value = giaDon.GiamHoaDon });
            themDon.Parameters.Add(new SqlParameter("@giamSP", SqlDbType.Decimal)
            { Precision = 18, Scale = 2, Value = giaDon.GiamSanPham });
            themDon.Parameters.Add(new SqlParameter("@maCTKM", SqlDbType.VarChar, 10)
            { Value = (object?)giaDon.MaCTKMHD ?? DBNull.Value });
            themDon.Parameters.Add(new SqlParameter("@mucTien", SqlDbType.Decimal)
            { Precision = 18, Scale = 2, Value = (object?)giaDon.MucTienHD ?? DBNull.Value });
            themDon.Parameters.Add(new SqlParameter("@phuongThuc", SqlDbType.VarChar, 12) { Value = don.PhuongThucThanhToan });
            int maDon = Convert.ToInt32(themDon.ExecuteScalar());

            foreach (DongDatThuoc dong in cacDong)
            {
                using var truTon = new SqlCommand(
                    "UPDATE dbo.THUOC SET SOLUONG = SOLUONG - @soLuong " +
                    "WHERE MATHUOC = @thuoc AND SOLUONG >= @soLuong " +
                    "AND (HSD IS NULL OR HSD >= @homNay)", ketNoi, giaoDich);
                truTon.Parameters.Add(new SqlParameter("@thuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                truTon.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
                truTon.Parameters.Add(new SqlParameter("@homNay", SqlDbType.Date) { Value = DateTime.Today });
                if (truTon.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException($"{dong.TenThuoc} không còn đủ tồn hoặc đã hết hạn.");

                using var themDong = new SqlCommand(
                    "INSERT INTO dbo.CTDONDATTHUOC (MADON, MATHUOC, SOLUONG, DONGIADUKIEN, THANHTIEN) " +
                    "VALUES (@don, @thuoc, @soLuong, @gia, @thanhTien)", ketNoi, giaoDich);
                themDong.Parameters.Add(new SqlParameter("@don", SqlDbType.Int) { Value = maDon });
                themDong.Parameters.Add(new SqlParameter("@thuoc", SqlDbType.VarChar, 10) { Value = dong.MaThuoc });
                themDong.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = dong.SoLuong });
                themDong.Parameters.Add(new SqlParameter("@gia", SqlDbType.Decimal)
                { Precision = 18, Scale = 2, Value = dong.DonGiaDuKien });
                themDong.Parameters.Add(new SqlParameter("@thanhTien", SqlDbType.Decimal)
                { Precision = 18, Scale = 2, Value = giaDon.ThanhTienTheoThuoc[dong.MaThuoc] });
                if (themDong.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException($"Không lưu được chi tiết {dong.TenThuoc}.");
            }
            return maDon;
        });
    }

    public DataTable LayDonMoi()
    {
        return new MyConnection().LayBang(
            "SELECT d.MADON, d.HOTEN, d.SDT, d.DIACHI, d.GHICHU, d.NGAYDAT, d.TRANGTHAI, " +
            "d.TONGTAMTINH, d.MAVOUCHER, d.TIENGIAM, d.DATRUTON, d.MAHD, d.PHUONGTHUCTT, h.TRANGTHAITT, " +
            "d.GIAMSANPHAM, d.MACTKM_HD, d.MUCTIENHD " +
            "FROM dbo.DONDATTHUOC d LEFT JOIN dbo.HOADON h ON h.MAHD = d.MAHD " +
            "ORDER BY d.NGAYDAT DESC, d.MADON DESC");
    }

    public DataTable LayChiTiet()
    {
        return new MyConnection().LayBang(
            "SELECT ct.MADON, ct.MATHUOC, t.TENTHUOC, ct.SOLUONG, ct.DONGIADUKIEN, ct.THANHTIEN " +
            "FROM dbo.CTDONDATTHUOC ct JOIN dbo.THUOC t ON t.MATHUOC = ct.MATHUOC " +
            "ORDER BY ct.MADON DESC, t.TENTHUOC");
    }

    public string TaoHoaDonTuDon(int maDon, string maNV, decimal tienKhachDua, bool daDoiChieuOnline)
    {
        return new MyConnection().ChayGiaoDich((ketNoi, giaoDich) =>
        {
            using var layDon = new SqlCommand(
                "SELECT HOTEN, SDT, DIACHI, TRANGTHAI, DATRUTON, TONGTAMTINH, TIENGIAM, MAVOUCHER, MAHD, PHUONGTHUCTT, MACTKM_HD, MUCTIENHD " +
                "FROM dbo.DONDATTHUOC WITH (UPDLOCK, HOLDLOCK) WHERE MADON = @maDon",
                ketNoi, giaoDich);
            layDon.Parameters.Add(new SqlParameter("@maDon", SqlDbType.Int) { Value = maDon });
            string hoTen, sdt, diaChi, trangThai;
            bool daTruTon;
            decimal tongTien, tienGiam;
            string? voucher, maHDTruoc, maCTKMHD;
            decimal? mucTienHD;
            string phuongThuc;
            using (SqlDataReader doc = layDon.ExecuteReader())
            {
                if (!doc.Read()) throw new InvalidOperationException("Không tìm thấy đơn web.");
                hoTen = doc.GetString(0);
                sdt = doc.GetString(1);
                diaChi = doc.GetString(2);
                trangThai = doc.GetString(3);
                daTruTon = doc.GetBoolean(4);
                tongTien = doc.GetDecimal(5);
                tienGiam = doc.GetDecimal(6);
                voucher = doc.IsDBNull(7) ? null : doc.GetString(7);
                maHDTruoc = doc.IsDBNull(8) ? null : doc.GetString(8);
                phuongThuc = doc.GetString(9);
                maCTKMHD = doc.IsDBNull(10) ? null : doc.GetString(10);
                mucTienHD = doc.IsDBNull(11) ? null : doc.GetDecimal(11);
            }
            if (trangThai != "Moi" || maHDTruoc != null)
                throw new InvalidOperationException("Đơn đã được xử lý; không thể tạo hóa đơn lần nữa.");
            if (phuongThuc == PhuongThucThanhToanCode.TaiCuaHang && tienKhachDua < tongTien)
                throw new InvalidOperationException("Tiền khách đưa chưa đủ tổng tiền đơn.");
            if (phuongThuc != PhuongThucThanhToanCode.TaiCuaHang && tienKhachDua != 0)
                throw new InvalidOperationException("Đơn online/COD không nhập tiền khách đưa tại quầy.");
            if (phuongThuc == PhuongThucThanhToanCode.Online && !daDoiChieuOnline)
                throw new InvalidOperationException("Hãy đối chiếu khoản thanh toán online trước khi tạo hóa đơn.");

            using var layDong = new SqlCommand(
                "SELECT MATHUOC, SOLUONG, DONGIADUKIEN, THANHTIEN FROM dbo.CTDONDATTHUOC " +
                "WHERE MADON = @maDon ORDER BY MATHUOC", ketNoi, giaoDich);
            layDong.Parameters.Add(new SqlParameter("@maDon", SqlDbType.Int) { Value = maDon });
            var cacDong = new List<(string MaThuoc, int SoLuong, decimal DonGia, decimal ThanhTien)>();
            using (SqlDataReader doc = layDong.ExecuteReader())
                while (doc.Read()) cacDong.Add((doc.GetString(0), doc.GetInt32(1), doc.GetDecimal(2), doc.GetDecimal(3)));
            if (cacDong.Count == 0) throw new InvalidOperationException("Đơn không có chi tiết thuốc.");

            string maKH = LayHoacTaoKhachHang(ketNoi, giaoDich, hoTen, sdt, diaChi);
            string maHD = HoaDonDAO.TaoMaHoaDonTrongGiaoDich(ketNoi, giaoDich);

            if (!daTruTon)
                foreach ((string maThuoc, int soLuong, _, _) in cacDong)
                {
                    using var truTon = new SqlCommand(
                        "UPDATE dbo.THUOC WITH (UPDLOCK, ROWLOCK) SET SOLUONG = SOLUONG - @soLuong " +
                        "WHERE MATHUOC = @maThuoc AND SOLUONG >= @soLuong " +
                        "AND (HSD IS NULL OR HSD >= @homNay)", ketNoi, giaoDich);
                    truTon.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = soLuong });
                    truTon.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = maThuoc });
                    truTon.Parameters.Add(new SqlParameter("@homNay", SqlDbType.Date) { Value = DateTime.Today });
                    if (truTon.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException($"Thuốc {maThuoc} không còn đủ tồn hoặc đã hết hạn.");
                }

            using var themHoaDon = new SqlCommand(
                "INSERT INTO dbo.HOADON (MAHD, MANV, NGAYLAP, TONGTIEN, MAKH, MAVOUCHER, " +
                "MACTKM_HD, MUCTIENHD, TIENGIAM, TIENKHACHDUA, PHUONGTHUCTT, TRANGTHAITT) " +
                "VALUES (@maHD, @maNV, @ngay, @tong, @maKH, @voucher, @maCTKM, @mucTien, @giam, @khachDua, @phuongThuc, @trangThaiTT)",
                ketNoi, giaoDich);
            themHoaDon.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });
            themHoaDon.Parameters.Add(new SqlParameter("@maNV", SqlDbType.VarChar, 10) { Value = maNV });
            themHoaDon.Parameters.Add(new SqlParameter("@ngay", SqlDbType.Date) { Value = DateTime.Today });
            themHoaDon.Parameters.Add(new SqlParameter("@tong", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = tongTien });
            themHoaDon.Parameters.Add(new SqlParameter("@maKH", SqlDbType.VarChar, 10) { Value = maKH });
            themHoaDon.Parameters.Add(new SqlParameter("@voucher", SqlDbType.VarChar, 20) { Value = (object?)voucher ?? DBNull.Value });
            themHoaDon.Parameters.Add(new SqlParameter("@maCTKM", SqlDbType.VarChar, 10) { Value = (object?)maCTKMHD ?? DBNull.Value });
            themHoaDon.Parameters.Add(new SqlParameter("@mucTien", SqlDbType.Decimal)
            { Precision = 18, Scale = 2, Value = (object?)mucTienHD ?? DBNull.Value });
            themHoaDon.Parameters.Add(new SqlParameter("@giam", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = tienGiam });
            themHoaDon.Parameters.Add(new SqlParameter("@khachDua", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = tienKhachDua });
            themHoaDon.Parameters.Add(new SqlParameter("@phuongThuc", SqlDbType.VarChar, 12) { Value = phuongThuc });
            themHoaDon.Parameters.Add(new SqlParameter("@trangThaiTT", SqlDbType.VarChar, 15)
            { Value = phuongThuc == PhuongThucThanhToanCode.COD ? "ChoThuTien" : "DaThanhToan" });
            if (themHoaDon.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("Không lưu được hóa đơn từ đơn web.");

            foreach ((string maThuoc, int soLuong, decimal donGia, decimal thanhTien) in cacDong)
            {
                using var themChiTiet = new SqlCommand(
                    "INSERT INTO dbo.CTHD (MAHD, MATHUOC, SOLUONG, DONGIA, THANHTIEN) " +
                    "VALUES (@maHD, @maThuoc, @soLuong, @donGia, @thanhTien)", ketNoi, giaoDich);
                themChiTiet.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });
                themChiTiet.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = maThuoc });
                themChiTiet.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = soLuong });
                themChiTiet.Parameters.Add(new SqlParameter("@donGia", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = donGia });
                themChiTiet.Parameters.Add(new SqlParameter("@thanhTien", SqlDbType.Decimal)
                { Precision = 18, Scale = 2, Value = thanhTien });
                if (themChiTiet.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException($"Không lưu được thuốc {maThuoc} vào hóa đơn.");
            }

            using var capNhatDon = new SqlCommand(
                "UPDATE dbo.DONDATTHUOC SET TRANGTHAI = 'DaXacNhan', DATRUTON = 1, MAHD = @maHD " +
                "WHERE MADON = @maDon AND TRANGTHAI = 'Moi' AND MAHD IS NULL", ketNoi, giaoDich);
            capNhatDon.Parameters.Add(new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });
            capNhatDon.Parameters.Add(new SqlParameter("@maDon", SqlDbType.Int) { Value = maDon });
            if (capNhatDon.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("Đơn đã được xử lý trong lúc tạo hóa đơn.");
            return maHD;
        });
    }

    public int XacNhanThuCOD(string maHD, decimal tienKhachDua)
    {
        return new MyConnection().ThucThi(
            "UPDATE dbo.HOADON SET TRANGTHAITT = 'DaThanhToan', TIENKHACHDUA = @tien " +
            "WHERE MAHD = @maHD AND PHUONGTHUCTT = 'COD' AND TRANGTHAITT = 'ChoThuTien' " +
            "AND TONGTIEN <= @tien",
            new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD },
            new SqlParameter("@tien", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = tienKhachDua });
    }

    private static string LayHoacTaoKhachHang(SqlConnection ketNoi, SqlTransaction giaoDich,
        string hoTen, string sdt, string diaChi)
    {
        using var layKhach = new SqlCommand(
            "SELECT MAKH FROM dbo.KHACHHANG WITH (UPDLOCK, HOLDLOCK) " +
            "WHERE HOTEN = @ten AND SDT = @sdt ORDER BY MAKH", ketNoi, giaoDich);
        layKhach.Parameters.Add(new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = hoTen });
        layKhach.Parameters.Add(new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = sdt });
        if (layKhach.ExecuteScalar() is string maDaCo) return maDaCo;

        using var layMa = new SqlCommand("SELECT MAKH FROM dbo.KHACHHANG WITH (UPDLOCK, HOLDLOCK)", ketNoi, giaoDich);
        int soLonNhat = 0;
        using (SqlDataReader doc = layMa.ExecuteReader())
            while (doc.Read())
            {
                string ma = doc.GetString(0);
                if (ma.StartsWith("KH", StringComparison.OrdinalIgnoreCase) && int.TryParse(ma.AsSpan(2), out int so))
                    soLonNhat = Math.Max(soLonNhat, so);
            }
        if (soLonNhat >= 9999) throw new InvalidOperationException("Đã hết mã khách hàng KH0001–KH9999.");
        string maMoi = "KH" + (soLonNhat + 1).ToString("D4");
        using var themKhach = new SqlCommand(
            "INSERT INTO dbo.KHACHHANG (MAKH, HOTEN, SDT, DIACHI) VALUES (@ma, @ten, @sdt, @diaChi)",
            ketNoi, giaoDich);
        themKhach.Parameters.Add(new SqlParameter("@ma", SqlDbType.VarChar, 10) { Value = maMoi });
        themKhach.Parameters.Add(new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = hoTen });
        themKhach.Parameters.Add(new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = sdt });
        themKhach.Parameters.Add(new SqlParameter("@diaChi", SqlDbType.NVarChar, 200) { Value = diaChi });
        if (themKhach.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Không lưu được khách hàng của đơn web.");
        return maMoi;
    }

    public int CapNhatTrangThai(int maDon, string trangThai)
    {
        if (trangThai != "DaHuy")
            throw new ArgumentException("Trạng thái đơn không hợp lệ.", nameof(trangThai));
        return new MyConnection().ChayGiaoDich((ketNoi, giaoDich) =>
        {
            using var doiTrangThai = new SqlCommand(
                "UPDATE dbo.DONDATTHUOC SET TRANGTHAI = @trangThai OUTPUT INSERTED.DATRUTON " +
                "WHERE MADON = @maDon AND TRANGTHAI = 'Moi'",
                ketNoi, giaoDich);
            doiTrangThai.Parameters.Add(new SqlParameter("@trangThai", SqlDbType.VarChar, 20) { Value = trangThai });
            doiTrangThai.Parameters.Add(new SqlParameter("@maDon", SqlDbType.Int) { Value = maDon });
            object? daTruTon = doiTrangThai.ExecuteScalar();
            if (daTruTon is null) return 0;
            if (!Convert.ToBoolean(daTruTon)) return 1;

            using var layChiTiet = new SqlCommand(
                "SELECT MATHUOC, SOLUONG FROM dbo.CTDONDATTHUOC WHERE MADON = @maDon ORDER BY MATHUOC",
                ketNoi, giaoDich);
            layChiTiet.Parameters.Add(new SqlParameter("@maDon", SqlDbType.Int) { Value = maDon });
            var cacDong = new List<(string MaThuoc, int SoLuong)>();
            using (SqlDataReader doc = layChiTiet.ExecuteReader())
                while (doc.Read()) cacDong.Add((doc.GetString(0), doc.GetInt32(1)));
            foreach ((string maThuoc, int soLuong) in cacDong)
            {
                using var hoanTon = new SqlCommand(
                    "UPDATE dbo.THUOC SET SOLUONG = SOLUONG + @soLuong WHERE MATHUOC = @maThuoc",
                    ketNoi, giaoDich);
                hoanTon.Parameters.Add(new SqlParameter("@soLuong", SqlDbType.Int) { Value = soLuong });
                hoanTon.Parameters.Add(new SqlParameter("@maThuoc", SqlDbType.VarChar, 10) { Value = maThuoc });
                if (hoanTon.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException($"Không hoàn tồn được thuốc {maThuoc}.");
            }
            return 1;
        });
    }
}

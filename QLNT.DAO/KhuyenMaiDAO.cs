using System.Data;
using Microsoft.Data.SqlClient;

public class KhuyenMaiDAO
{
    private readonly MyConnection conn = new MyConnection();

    // ================= CTKHUYENMAI =================
    public DataTable LayTatCaCTKM()
    {
        return conn.LayBang("SELECT MACTKM, TENCTKM, NGAYBATDAU, NGAYKETTHUC FROM CTKHUYENMAI");
    }

    public int ThemCTKM(CTKhuyenMai km)
    {
        string sql = "INSERT INTO CTKHUYENMAI (MACTKM, TENCTKM, NGAYBATDAU, NGAYKETTHUC) VALUES (@ma, @ten, @bd, @kt)";
        return conn.ThucThi(sql,
            new SqlParameter("@ma", km.MaCTKM),
            new SqlParameter("@ten", km.TenCTKM),
            new SqlParameter("@bd", km.NgayBatDau ?? (object)DBNull.Value),
            new SqlParameter("@kt", km.NgayKetThuc ?? (object)DBNull.Value));
    }

    public int SuaCTKM(CTKhuyenMai km)
    {
        string sql = "UPDATE CTKHUYENMAI SET TENCTKM = @ten, NGAYBATDAU = @bd, NGAYKETTHUC = @kt WHERE MACTKM = @ma";
        return conn.ThucThi(sql,
            new SqlParameter("@ma", km.MaCTKM),
            new SqlParameter("@ten", km.TenCTKM),
            new SqlParameter("@bd", km.NgayBatDau ?? (object)DBNull.Value),
            new SqlParameter("@kt", km.NgayKetThuc ?? (object)DBNull.Value));
    }

    public int XoaCTKM(string maCTKM)
    {
        // Xóa cascade dữ liệu con trong các bảng liên quan trước khi xóa chương trình
        conn.ThucThi("DELETE FROM KM_SANPHAM WHERE MACTKM = @ma", new SqlParameter("@ma", maCTKM));
        conn.ThucThi("DELETE FROM KM_VOUCHER WHERE MACTKM = @ma", new SqlParameter("@ma", maCTKM));
        conn.ThucThi("DELETE FROM KM_HOADON WHERE MACTKM = @ma", new SqlParameter("@ma", maCTKM));
        return conn.ThucThi("DELETE FROM CTKHUYENMAI WHERE MACTKM = @ma", new SqlParameter("@ma", maCTKM));
    }

    // ================= KM_SANPHAM =================
    public DataTable LayTatCaKMSanPham()
    {
        return conn.LayBang("SELECT MACTKM, MATHUOC, PHANTRAMGIAM FROM KM_SANPHAM");
    }

    public int LuuKMSanPham(KMSanPham sp)
    {
        string sql = @"
            IF EXISTS (SELECT 1 FROM KM_SANPHAM WHERE MACTKM = @maKM AND MATHUOC = @maThuoc)
                UPDATE KM_SANPHAM SET PHANTRAMGIAM = @pt WHERE MACTKM = @maKM AND MATHUOC = @maThuoc
            ELSE
                INSERT INTO KM_SANPHAM (MACTKM, MATHUOC, PHANTRAMGIAM) VALUES (@maKM, @maThuoc, @pt)";
        return conn.ThucThi(sql,
            new SqlParameter("@maKM", sp.MaCTKM),
            new SqlParameter("@maThuoc", sp.MaThuoc),
            new SqlParameter("@pt", sp.PhanTramGiam));
    }

    public int XoaKMSanPham(string maKM, string maThuoc)
    {
        return conn.ThucThi("DELETE FROM KM_SANPHAM WHERE MACTKM = @maKM AND MATHUOC = @maThuoc",
            new SqlParameter("@maKM", maKM),
            new SqlParameter("@maThuoc", maThuoc));
    }

    // ================= KM_VOUCHER =================
    public DataTable LayTatCaVoucher()
    {
        return conn.LayBang("SELECT MAVOUCHER, MACTKM, MAKH, PHANTRAMGIAM FROM KM_VOUCHER");
    }

    public int ThemVoucher(KMVoucher vc)
    {
        string sql = "INSERT INTO KM_VOUCHER (MAVOUCHER, MACTKM, MAKH, PHANTRAMGIAM) VALUES (@maVC, @maKM, @maKH, @pt)";
        return conn.ThucThi(sql,
            new SqlParameter("@maVC", vc.MaVoucher),
            new SqlParameter("@maKM", vc.MaCTKM),
            new SqlParameter("@maKH", string.IsNullOrEmpty(vc.MaKH) ? DBNull.Value : vc.MaKH),
            new SqlParameter("@pt", vc.PhanTramGiam));
    }

    public int XoaVoucher(string maVoucher)
    {
        return conn.ThucThi("DELETE FROM KM_VOUCHER WHERE MAVOUCHER = @maVC",
            new SqlParameter("@maVC", maVoucher));
    }
  public int SuaVoucher(KMVoucher vc)
    {
        string sql = "UPDATE KM_VOUCHER SET MAKH = @maKH, PHANTRAMGIAM = @pt WHERE MAVOUCHER = @maVC";
        return conn.ThucThi(sql,
            new SqlParameter("@maVC", vc.MaVoucher),
            new SqlParameter("@maKH", string.IsNullOrEmpty(vc.MaKH) ? DBNull.Value : vc.MaKH),
            new SqlParameter("@pt", vc.PhanTramGiam));
    }

    // ================= KM_HOADON =================
    public DataTable LayTatCaKMHoaDon()
    {
        return conn.LayBang("SELECT MACTKM, MUCTIENHD, PHANTRAMGIAM FROM KM_HOADON");
    }

    public int ThemKMHoaDon(KMHoaDon hd)
    {
        string sql = @"
            IF EXISTS (SELECT 1 FROM KM_HOADON WHERE MACTKM = @maKM AND MUCTIENHD = @mucTien)
                UPDATE KM_HOADON SET PHANTRAMGIAM = @pt WHERE MACTKM = @maKM AND MUCTIENHD = @mucTien
            ELSE
                INSERT INTO KM_HOADON (MACTKM, MUCTIENHD, PHANTRAMGIAM) VALUES (@maKM, @mucTien, @pt)";
        return conn.ThucThi(sql,
            new SqlParameter("@maKM", hd.MaCTKM),
            new SqlParameter("@mucTien", hd.MucTienHD),
            new SqlParameter("@pt", hd.PhanTramGiam));
    }

    public int XoaKMHoaDon(string maKM, decimal mucTien)
    {
        return conn.ThucThi("DELETE FROM KM_HOADON WHERE MACTKM = @maKM AND MUCTIENHD = @mucTien",
            new SqlParameter("@maKM", maKM),
            new SqlParameter("@mucTien", mucTien));
    }
}
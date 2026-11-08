using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QLNT.DAO;

public class ThongKeDAO
{
    private readonly MyConnection conn = new MyConnection();

    public decimal TongThu(DateTime tuNgay, DateTime denNgay)
    {
        string sql = "SELECT ISNULL(SUM(TONGTIEN), 0) FROM HOADON WHERE NGAYLAP >= @tuNgay AND NGAYLAP <= @denNgay";
        DataTable dt = conn.LayBang(sql,
            new SqlParameter("@tuNgay", tuNgay.Date),
            new SqlParameter("@denNgay", denNgay.Date.AddDays(1).AddSeconds(-1)));
        
        if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
            return Convert.ToDecimal(dt.Rows[0][0]);
        return 0m;
    }

    public decimal TongChi(DateTime tuNgay, DateTime denNgay)
    {
        string sql = "SELECT ISNULL(SUM(TONGTIEN), 0) FROM PHIEUNHAP WHERE NGAYNHAP >= @tuNgay AND NGAYNHAP <= @denNgay";
        DataTable dt = conn.LayBang(sql,
            new SqlParameter("@tuNgay", tuNgay.Date),
            new SqlParameter("@denNgay", denNgay.Date.AddDays(1).AddSeconds(-1)));

        if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
            return Convert.ToDecimal(dt.Rows[0][0]);
        return 0m;
    }

    public DataTable LayThuTheoThang(int nam)
    {
        string sql = @"
            SELECT MONTH(NGAYLAP) AS Thang, ISNULL(SUM(TONGTIEN), 0) AS TongThu
            FROM HOADON
            WHERE YEAR(NGAYLAP) = @nam
            GROUP BY MONTH(NGAYLAP)";
        return conn.LayBang(sql, new SqlParameter("@nam", nam));
    }

    public DataTable LayChiTheoThang(int nam)
    {
        string sql = @"
            SELECT MONTH(NGAYNHAP) AS Thang, ISNULL(SUM(TONGTIEN), 0) AS TongChi
            FROM PHIEUNHAP
            WHERE YEAR(NGAYNHAP) = @nam
            GROUP BY MONTH(NGAYNHAP)";
        return conn.LayBang(sql, new SqlParameter("@nam", nam));
    }

    public DataTable LayThuTheoNam(int tuNam, int denNam)
    {
        string sql = @"
            SELECT YEAR(NGAYLAP) AS Nam, ISNULL(SUM(TONGTIEN), 0) AS TongThu
            FROM HOADON
            WHERE YEAR(NGAYLAP) >= @tuNam AND YEAR(NGAYLAP) <= @denNam
            GROUP BY YEAR(NGAYLAP)";
        return conn.LayBang(sql,
            new SqlParameter("@tuNam", tuNam),
            new SqlParameter("@denNam", denNam));
    }

    public DataTable LayChiTheoNam(int tuNam, int denNam)
    {
        string sql = @"
            SELECT YEAR(NGAYNHAP) AS Nam, ISNULL(SUM(TONGTIEN), 0) AS TongChi
            FROM PHIEUNHAP
            WHERE YEAR(NGAYNHAP) >= @tuNam AND YEAR(NGAYNHAP) <= @denNam
            GROUP BY YEAR(NGAYNHAP)";
        return conn.LayBang(sql,
            new SqlParameter("@tuNam", tuNam),
            new SqlParameter("@denNam", denNam));
    }

    public DataTable LayDoanhThuTheoNhanVienQuy(int nam)
    {
        string sql = @"
            SELECT 
                nv.MANV AS Ma,
                nv.HOTENNV AS Ten,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 1 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q1,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 2 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q2,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 3 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q3,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 4 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q4,
                ISNULL(SUM(hd.TONGTIEN), 0) AS TC
            FROM NHANVIEN nv
            LEFT JOIN HOADON hd ON nv.MANV = hd.MANV AND YEAR(hd.NGAYLAP) = @nam
            GROUP BY nv.MANV, nv.HOTENNV";
        return conn.LayBang(sql, new SqlParameter("@nam", nam));
    }

    public DataTable LayDoanhThuTheoKhachHangQuy(int nam)
    {
        string sql = @"
            SELECT 
                ISNULL(hd.MAKH, 'KLE') AS Ma,
                ISNULL(kh.HOTEN, N'Khách lẻ') AS Ten,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 1 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q1,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 2 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q2,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 3 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q3,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 4 THEN hd.TONGTIEN ELSE 0 END), 0) AS Q4,
                ISNULL(SUM(hd.TONGTIEN), 0) AS TC
            FROM HOADON hd
            LEFT JOIN KHACHHANG kh ON hd.MAKH = kh.MAKH
            WHERE YEAR(hd.NGAYLAP) = @nam
            GROUP BY hd.MAKH, kh.HOTEN";
        return conn.LayBang(sql, new SqlParameter("@nam", nam));
    }

    public DataTable LayDoanhThuTheoSanPhamQuy(int nam)
    {
        string sql = @"
            SELECT 
                t.MATHUOC AS Ma,
                t.TENTHUOC AS Ten,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 1 THEN ct.THANHTIEN ELSE 0 END), 0) AS Q1,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 2 THEN ct.THANHTIEN ELSE 0 END), 0) AS Q2,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 3 THEN ct.THANHTIEN ELSE 0 END), 0) AS Q3,
                ISNULL(SUM(CASE WHEN DATEPART(QUARTER, hd.NGAYLAP) = 4 THEN ct.THANHTIEN ELSE 0 END), 0) AS Q4,
                ISNULL(SUM(CASE WHEN hd.MAHD IS NOT NULL THEN ct.THANHTIEN ELSE 0 END), 0) AS TC
            FROM THUOC t
            LEFT JOIN CTHD ct ON t.MATHUOC = ct.MATHUOC
            LEFT JOIN HOADON hd ON ct.MAHD = hd.MAHD AND YEAR(hd.NGAYLAP) = @nam
            GROUP BY t.MATHUOC, t.TENTHUOC";
        return conn.LayBang(sql, new SqlParameter("@nam", nam));
    }
}

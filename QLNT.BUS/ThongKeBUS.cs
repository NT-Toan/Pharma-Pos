using System;
using System.Data;
using QLNT.DAO;

namespace QLNT.BUS;

public class ThongKeBUS
{
    private readonly ThongKeDAO dao = new();

    public decimal TongThu(DateTime tuNgay, DateTime denNgay) => dao.TongThu(tuNgay, denNgay);
    public decimal TongChi(DateTime tuNgay, DateTime denNgay) => dao.TongChi(tuNgay, denNgay);
    public decimal LoiNhuan(DateTime tuNgay, DateTime denNgay) => TongThu(tuNgay, denNgay) - TongChi(tuNgay, denNgay);

    public DataTable ThuChiTheoThang(int nam)
    {
        DataTable dt = TaoBangThuChi("Tháng");
        DataTable dtThu = dao.LayThuTheoThang(nam);
        DataTable dtChi = dao.LayChiTheoThang(nam);

        for (int i = 1; i <= 12; i++)
        {
            decimal thu = LayGiaTriTheoCot(dtThu, "Thang", i, "TongThu");
            decimal chi = LayGiaTriTheoCot(dtChi, "Thang", i, "TongChi");
            dt.Rows.Add($"T{i}", thu, chi, thu - chi);
        }
        return dt;
    }

    public DataTable ThuChiTheoQuy(int nam)
    {
        DataTable dtThang = ThuChiTheoThang(nam);
        DataTable dt = TaoBangThuChi("Quý");

        for (int q = 1; q <= 4; q++)
        {
            decimal tongThuQ = 0m, tongChiQ = 0m;
            int batDau = (q - 1) * 3;
            for (int m = 0; m < 3; m++)
            {
                tongThuQ += Convert.ToDecimal(dtThang.Rows[batDau + m]["Thu"]);
                tongChiQ += Convert.ToDecimal(dtThang.Rows[batDau + m]["Chi"]);
            }
            dt.Rows.Add($"Q{q}", tongThuQ, tongChiQ, tongThuQ - tongChiQ);
        }
        return dt;
    }

    public DataTable ThuChiTheoNam(int tuNam, int denNam)
    {
        DataTable dt = TaoBangThuChi("Năm");
        DataTable dtThu = dao.LayThuTheoNam(tuNam, denNam);
        DataTable dtChi = dao.LayChiTheoNam(tuNam, denNam);

        for (int y = tuNam; y <= denNam; y++)
        {
            decimal thu = LayGiaTriTheoCot(dtThu, "Nam", y, "TongThu");
            decimal chi = LayGiaTriTheoCot(dtChi, "Nam", y, "TongChi");
            dt.Rows.Add(y.ToString(), thu, chi, thu - chi);
        }
        return dt;
    }

    public DataTable TongTienTheoNhanVienQuy(int nam)
    {
        DataTable dt = dao.LayDoanhThuTheoNhanVienQuy(nam);
        ThemDongTongCong(dt);
        return dt;
    }

    public DataTable TongTienTheoKhachHangQuy(int nam)
    {
        DataTable dt = dao.LayDoanhThuTheoKhachHangQuy(nam);
        ThemDongTongCong(dt);
        return dt;
    }

    public DataTable TongTienTheoSanPhamQuy(int nam)
    {
        DataTable dt = dao.LayDoanhThuTheoSanPhamQuy(nam);
        ThemDongTongCong(dt);
        return dt;
    }

    private static DataTable TaoBangThuChi(string tenKy)
    {
        DataTable dt = new();
        dt.Columns.Add("Ky", typeof(string));
        dt.Columns.Add("Thu", typeof(decimal));
        dt.Columns.Add("Chi", typeof(decimal));
        dt.Columns.Add("LoiNhuan", typeof(decimal));
        return dt;
    }

    private static decimal LayGiaTriTheoCot(DataTable dt, string cotKhoa, int giaTriKhoa, string cotGiaTri)
    {
        foreach (DataRow row in dt.Rows)
        {
            if (Convert.ToInt32(row[cotKhoa]) == giaTriKhoa)
                return Convert.ToDecimal(row[cotGiaTri]);
        }
        return 0m;
    }

    private static void ThemDongTongCong(DataTable dt)
    {
        decimal tongQ1 = 0, tongQ2 = 0, tongQ3 = 0, tongQ4 = 0, tongTC = 0;
        foreach (DataRow row in dt.Rows)
        {
            tongQ1 += Convert.ToDecimal(row["Q1"]);
            tongQ2 += Convert.ToDecimal(row["Q2"]);
            tongQ3 += Convert.ToDecimal(row["Q3"]);
            tongQ4 += Convert.ToDecimal(row["Q4"]);
            tongTC += Convert.ToDecimal(row["TC"]);
        }

        DataRow dongTong = dt.NewRow();
        dongTong["Ma"] = "TC";
        dongTong["Ten"] = "TỔNG CỘNG";
        dongTong["Q1"] = tongQ1;
        dongTong["Q2"] = tongQ2;
        dongTong["Q3"] = tongQ3;
        dongTong["Q4"] = tongQ4;
        dongTong["TC"] = tongTC;
        dt.Rows.Add(dongTong);
    }
}
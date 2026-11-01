using System;
using System.Collections;
using System.Data;

public class CTKhuyenMaiBUS
{
    private static ArrayList? danhSachCTKM;

    private static ArrayList LayDanhSach()
    {
        if (danhSachCTKM != null) return danhSachCTKM;

        KhuyenMaiDAO kmDao = new KhuyenMaiDAO();
        DataTable dt = kmDao.LayTatCaCTKM();
        ArrayList ds = new ArrayList();

        foreach (DataRow row in dt.Rows)
        {
            DateTime? bd = row["NGAYBATDAU"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYBATDAU"]);
            DateTime? kt = row["NGAYKETTHUC"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYKETTHUC"]);
            ds.Add(new CTKhuyenMai(
                row["MACTKM"].ToString() ?? "",
                row["TENCTKM"].ToString() ?? "",
                bd, kt));
        }

        danhSachCTKM = ds;
        return ds;
    }

    public ArrayList LayTatCa() => new ArrayList(LayDanhSach());

    public static void TaiLai() => danhSachCTKM = null;

    public CTKhuyenMai? LayTheoMa(string maCTKM)
    {
        foreach (CTKhuyenMai km in LayDanhSach())
        {
            if (km.MaCTKM.Equals(maCTKM, StringComparison.OrdinalIgnoreCase))
                return km;
        }
        return null;
    }

    public bool KiemTraHieuLuc(string maCTKM, DateTime ngay)
    {
        CTKhuyenMai? km = LayTheoMa(maCTKM);
        if (km == null) return false;
        DateTime ngayXet = ngay.Date;
        if (km.NgayBatDau.HasValue && ngayXet < km.NgayBatDau.Value.Date) return false;
        if (km.NgayKetThuc.HasValue && ngayXet > km.NgayKetThuc.Value.Date) return false;
        return true;
    }
}
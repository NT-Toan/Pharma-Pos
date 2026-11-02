using System;
using System.Collections;
using System.Data;

public class KMSanPhamBUS
{
    private static ArrayList? danhSachKMSP;
    private readonly CTKhuyenMaiBUS ctkmBUS = new CTKhuyenMaiBUS();

    private static ArrayList LayDanhSach()
    {
        if (danhSachKMSP != null) return danhSachKMSP;

        KhuyenMaiDAO dao = new KhuyenMaiDAO();
        DataTable dt = dao.LayTatCaKMSanPham();
        ArrayList ds = new ArrayList();

        foreach (DataRow row in dt.Rows)
        {
            ds.Add(new KMSanPham(
                row["MACTKM"].ToString() ?? "",
                row["MATHUOC"].ToString() ?? "",
                Convert.ToDecimal(row["PHANTRAMGIAM"])));
        }

        danhSachKMSP = ds;
        return ds;
    }

    public static void TaiLai() => danhSachKMSP = null;

    // Lấy phần trăm giảm theo thuốc và ngày bán (nếu có nhiều CTKM thì lấy mức cao nhất)
    public decimal LayPhanTramGiam(string maThuoc, DateTime ngay)
    {
        decimal maxGiam = 0;
        foreach (KMSanPham sp in LayDanhSach())
        {
            if (sp.MaThuoc.Equals(maThuoc, StringComparison.OrdinalIgnoreCase))
            {
                if (ctkmBUS.KiemTraHieuLuc(sp.MaCTKM, ngay))
                {
                    if (sp.PhanTramGiam > maxGiam)
                        maxGiam = sp.PhanTramGiam;
                }
            }
        }
        return maxGiam;
    }
}
using System;
using System.Collections;
using System.Data;

public class KMSanPhamBUS
{
    private static ArrayList? danhSachKMSP;
    private readonly CTKhuyenMaiBUS ctkmBUS = new CTKhuyenMaiBUS();
    private readonly KhuyenMaiDAO dao = new KhuyenMaiDAO();

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

    public ArrayList LayTheoCTKM(string maCTKM)
    {
        ArrayList kq = new ArrayList();
        foreach (KMSanPham sp in LayDanhSach())
        {
            if (sp.MaCTKM.Equals(maCTKM, StringComparison.OrdinalIgnoreCase))
                kq.Add(sp);
        }
        return kq;
    }

    public static void TaiLai() => danhSachKMSP = null;

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

    public KetQua Luu(KMSanPham sp)
    {
        if (sp.PhanTramGiam <= 0 || sp.PhanTramGiam > 100)
            return KetQua.Loi("Phần trăm giảm phải từ 1 đến 100.");

        dao.LuuKMSanPham(sp);
        TaiLai();
        return KetQua.Ok("Lưu giảm giá sản phẩm thành công.");
    }

    public KetQua Xoa(string maKM, string maThuoc)
    {
        dao.XoaKMSanPham(maKM, maThuoc);
        TaiLai();
        return KetQua.Ok("Xóa giảm giá sản phẩm thành công.");
    }
}
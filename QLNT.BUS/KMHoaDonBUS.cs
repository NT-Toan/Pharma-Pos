using System;
using System.Collections;
using System.Data;

public class KMHoaDonBUS
{
    private static ArrayList? danhSachKMHD;
    private readonly CTKhuyenMaiBUS ctkmBUS = new CTKhuyenMaiBUS();
    private readonly KhuyenMaiDAO dao = new KhuyenMaiDAO();

    private static ArrayList LayDanhSach()
    {
        if (danhSachKMHD != null) return danhSachKMHD;

        KhuyenMaiDAO dao = new KhuyenMaiDAO();
        DataTable dt = dao.LayTatCaKMHoaDon();
        ArrayList ds = new ArrayList();

        foreach (DataRow row in dt.Rows)
        {
            ds.Add(new KMHoaDon(
                row["MACTKM"].ToString() ?? "",
                Convert.ToDecimal(row["MUCTIENHD"]),
                Convert.ToDecimal(row["PHANTRAMGIAM"])));
        }

        danhSachKMHD = ds;
        return ds;
    }

    public ArrayList LayTheoCTKM(string maCTKM)
    {
        ArrayList kq = new ArrayList();
        foreach (KMHoaDon km in LayDanhSach())
        {
            if (km.MaCTKM.Equals(maCTKM, StringComparison.OrdinalIgnoreCase))
                kq.Add(km);
        }
        return kq;
    }

    public static void TaiLai() => danhSachKMHD = null;

    public KMHoaDon? LayMocApDung(decimal tongTienHang, DateTime ngay)
    {
        KMHoaDon? mocThoa = null;

        foreach (KMHoaDon km in LayDanhSach())
        {
            if (tongTienHang >= km.MucTienHD)
            {
                if (ctkmBUS.KiemTraHieuLuc(km.MaCTKM, ngay))
                {
                    if (mocThoa == null || km.MucTienHD > mocThoa.MucTienHD)
                    {
                        mocThoa = km;
                    }
                }
            }
        }

        return mocThoa;
    }

    public KetQua Luu(KMHoaDon hd)
    {
        if (hd.MucTienHD < 0)
            return KetQua.Loi("Mức tiền hóa đơn không được âm.");

        if (hd.PhanTramGiam <= 0 || hd.PhanTramGiam > 100)
            return KetQua.Loi("Phần trăm giảm phải từ 1 đến 100.");

        dao.ThemKMHoaDon(hd);
        TaiLai();
        return KetQua.Ok("Lưu mốc giảm hóa đơn thành công.");
    }

    public KetQua Xoa(string maKM, decimal mucTien)
    {
        dao.XoaKMHoaDon(maKM, mucTien);
        TaiLai();
        return KetQua.Ok("Xóa mốc giảm hóa đơn thành công.");
    }
}
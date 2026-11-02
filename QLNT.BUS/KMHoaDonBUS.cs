using System;
using System.Collections;
using System.Data;

public class KMHoaDonBUS
{
    private static ArrayList? danhSachKMHD;
    private readonly CTKhuyenMaiBUS ctkmBUS = new CTKhuyenMaiBUS();

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

    public static void TaiLai() => danhSachKMHD = null;

    // Lấy mốc giảm giá hóa đơn cao nhất mà tổng tiền hàng đạt tới
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
}
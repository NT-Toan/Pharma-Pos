using System;
using System.Collections;
using System.Data;

public class KMVoucherBUS
{
    private static ArrayList? danhSachVoucher;
    private readonly CTKhuyenMaiBUS ctkmBUS = new CTKhuyenMaiBUS();
    private readonly KhuyenMaiDAO dao = new KhuyenMaiDAO();

    private static ArrayList LayDanhSach()
    {
        if (danhSachVoucher != null) return danhSachVoucher;

        KhuyenMaiDAO dao = new KhuyenMaiDAO();
        DataTable dt = dao.LayTatCaVoucher();
        ArrayList ds = new ArrayList();

        foreach (DataRow row in dt.Rows)
        {
            string? makh = row["MAKH"] == DBNull.Value ? null : row["MAKH"].ToString();
            ds.Add(new KMVoucher(
                row["MAVOUCHER"].ToString() ?? "",
                row["MACTKM"].ToString() ?? "",
                makh,
                Convert.ToDecimal(row["PHANTRAMGIAM"])));
        }

        danhSachVoucher = ds;
        return ds;
    }

    public ArrayList LayTatCa() => new ArrayList(LayDanhSach());

    public ArrayList LayTheoCTKM(string maCTKM)
    {
        ArrayList kq = new ArrayList();
        foreach (KMVoucher vc in LayDanhSach())
        {
            if (vc.MaCTKM.Equals(maCTKM, StringComparison.OrdinalIgnoreCase))
                kq.Add(vc);
        }
        return kq;
    }

    public static void TaiLai() => danhSachVoucher = null;

    public KMVoucher? LayTheoMa(string maVoucher)
    {
        foreach (KMVoucher vc in LayDanhSach())
        {
            if (vc.MaVoucher.Equals(maVoucher, StringComparison.OrdinalIgnoreCase))
                return vc;
        }
        return null;
    }

    public KetQua KiemTraHopLe(string maVoucher, string? maKH, DateTime ngay)
    {
        KMVoucher? vc = LayTheoMa(maVoucher);
        if (vc == null)
            return KetQua.Loi("Voucher không tồn tại.");

        if (!ctkmBUS.KiemTraHieuLuc(vc.MaCTKM, ngay))
            return KetQua.Loi("Chương trình khuyến mãi của voucher này chưa bắt đầu hoặc đã kết thúc.");

        if (!string.IsNullOrEmpty(vc.MaKH))
        {
            if (string.IsNullOrEmpty(maKH))
                return KetQua.Loi("Voucher này chỉ áp dụng cho khách hàng thành viên cụ thể (không áp dụng cho khách lẻ).");

            if (!vc.MaKH.Equals(maKH, StringComparison.OrdinalIgnoreCase))
                return KetQua.Loi("Voucher này không thuộc về khách hàng hiện tại.");
        }

        return KetQua.Ok();
    }

    public KetQua Them(KMVoucher vc)
    {
        if (string.IsNullOrWhiteSpace(vc.MaVoucher))
            return KetQua.Loi("Mã voucher không được để trống.");

        if (LayTheoMa(vc.MaVoucher) != null)
            return KetQua.Loi($"Mã voucher '{vc.MaVoucher}' đã tồn tại.");

        if (vc.PhanTramGiam <= 0 || vc.PhanTramGiam > 100)
            return KetQua.Loi("Phần trăm giảm phải từ 1 đến 100.");

        dao.ThemVoucher(vc);
        TaiLai();
        return KetQua.Ok("Thêm voucher thành công.");
    }
      public KetQua Sua(KMVoucher vc)
    {
        if (string.IsNullOrWhiteSpace(vc.MaVoucher))
            return KetQua.Loi("Mã voucher không được để trống.");

        if (LayTheoMa(vc.MaVoucher) == null)
            return KetQua.Loi($"Mã voucher '{vc.MaVoucher}' không tồn tại để sửa.");

        if (vc.PhanTramGiam <= 0 || vc.PhanTramGiam > 100)
            return KetQua.Loi("Phần trăm giảm phải từ 1 đến 100.");

        dao.SuaVoucher(vc);
        TaiLai();
        return KetQua.Ok("Cập nhật voucher thành công.");
    }

    public KetQua Xoa(string maVoucher)
    {
        dao.XoaVoucher(maVoucher);
        TaiLai();
        return KetQua.Ok("Xóa voucher thành công.");
    }
 
}
using System;
using System.Collections;
using System.Data;

public class KMVoucherBUS
{
    private static ArrayList? danhSachVoucher;
    private readonly CTKhuyenMaiBUS ctkmBUS = new CTKhuyenMaiBUS();

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

    // Kiểm tra hợp lệ: tồn tại, còn hạn, đúng khách hàng
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
}
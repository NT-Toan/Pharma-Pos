using System.Collections;
using System.Data;

public class KhachHangBUS
{
    public ArrayList LayTatCa()
    {
        DataTable bang = new KhachHangDAO().LayTatCa();
        var danhSach = new ArrayList();
        foreach (DataRow dong in bang.Rows)
        {
            danhSach.Add(new KhachHang
            {
                MaKH = dong["MAKH"].ToString() ?? string.Empty,
                HoTen = dong["HOTEN"].ToString() ?? string.Empty,
                SDT = dong.IsNull("SDT") ? null : dong["SDT"].ToString(),
                DiaChi = dong.IsNull("DIACHI") ? null : dong["DIACHI"].ToString()
            });
        }

        return danhSach;
    }

    public KetQuaLuuKhachHang Them(KhachHang khachHang)
    {
        string hoTen = khachHang.HoTen.Trim();
        if (hoTen.Length == 0)
            return KetQuaLuuKhachHang.Loi("Họ tên khách hàng không được để trống.");
        if (hoTen.Length > 100)
            return KetQuaLuuKhachHang.Loi("Họ tên không được vượt quá 100 ký tự.");
        if (khachHang.SDT?.Trim().Length > 15)
            return KetQuaLuuKhachHang.Loi("Số điện thoại không được vượt quá 15 ký tự.");
        if (!string.IsNullOrWhiteSpace(khachHang.SDT) &&
            !khachHang.SDT.Trim().All(char.IsDigit))
            return KetQuaLuuKhachHang.Loi("Số điện thoại chỉ được chứa chữ số.");
        if (khachHang.DiaChi?.Trim().Length > 200)
            return KetQuaLuuKhachHang.Loi("Địa chỉ không được vượt quá 200 ký tự.");

        try
        {
            khachHang.MaKH = new KhachHangDAO().Them(khachHang);
            return KetQuaLuuKhachHang.Ok(khachHang, "Đã thêm khách hàng.");
        }
        catch (Exception ex)
        {
            return KetQuaLuuKhachHang.Loi($"Không thêm được khách hàng: {ex.Message}");
        }
    }
}

public sealed class KetQuaLuuKhachHang
{
    public bool ThanhCong { get; private init; }
    public string ThongBao { get; private init; } = string.Empty;
    public KhachHang? KhachHang { get; private init; }

    public static KetQuaLuuKhachHang Loi(string thongBao) => new() { ThongBao = thongBao };
    public static KetQuaLuuKhachHang Ok(KhachHang khachHang, string thongBao) =>
        new() { ThanhCong = true, KhachHang = khachHang, ThongBao = thongBao };
}

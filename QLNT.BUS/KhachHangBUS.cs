using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;

public class KhachHangBUS
{
    private static ArrayList? danhSachKhachHang;

    public ArrayList LayTatCa()
    {
        if (danhSachKhachHang != null)
            return new ArrayList(danhSachKhachHang);

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

        danhSachKhachHang = danhSach;
        return new ArrayList(danhSachKhachHang);
    }

    public KhachHang? LayTheoMa(string maKH)
    {
        if (string.IsNullOrWhiteSpace(maKH))
            return null;

        foreach (KhachHang kh in LayTatCa())
        {
            if (kh.MaKH.Equals(maKH.Trim(), StringComparison.OrdinalIgnoreCase))
                return kh;
        }

        return null;
    }

    public ArrayList TimKiem(string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
            return LayTatCa();

        string canTim = tuKhoa.Trim();
        var ketQua = new ArrayList();

        foreach (KhachHang kh in LayTatCa())
        {
            if (kh.MaKH.Contains(canTim, StringComparison.OrdinalIgnoreCase) ||
                kh.HoTen.Contains(canTim, StringComparison.OrdinalIgnoreCase) ||
                (kh.SDT?.Contains(canTim, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (kh.DiaChi?.Contains(canTim, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                ketQua.Add(kh);
            }
        }

        return ketQua;
    }

    public ArrayList TimKiemNangCao(
        List<DieuKien> dieuKien,
        PhepNoi noi)
    {
        return BoLoc.Loc(LayTatCa(), dieuKien, noi);
    }

    public KetQuaLuuKhachHang Them(KhachHang khachHang)
    {
        KetQua kiemTra = KiemTraDuLieu(khachHang);
        if (!kiemTra.ThanhCong)
            return KetQuaLuuKhachHang.Loi(kiemTra.ThongBao);

        try
        {
            khachHang.MaKH = new KhachHangDAO().Them(khachHang);
            TaiLai();
            return KetQuaLuuKhachHang.Ok(
                khachHang, "Đã thêm khách hàng.");
        }
        catch (Exception ex)
        {
            return KetQuaLuuKhachHang.Loi(
                $"Không thêm được khách hàng: {ex.Message}");
        }
    }

    public KetQua Sua(KhachHang khachHang)
    {
        if (string.IsNullOrWhiteSpace(khachHang.MaKH) ||
            LayTheoMa(khachHang.MaKH) == null)
        {
            return KetQua.Loi("Không tìm thấy mã khách hàng để sửa.");
        }

        KetQua kiemTra = KiemTraDuLieu(khachHang);
        if (!kiemTra.ThanhCong)
            return kiemTra;

        try
        {
            int soDong = new KhachHangDAO().Sua(khachHang);
            if (soDong != 1)
                return KetQua.Loi("Không tìm thấy mã khách hàng để sửa.");

            TaiLai();
            return KetQua.Ok("Đã sửa khách hàng " + khachHang.MaKH);
        }
        catch (Exception ex)
        {
            return KetQua.Loi(
                "Không sửa được khách hàng: " + ex.Message);
        }
    }

    public KetQua Xoa(string maKH)
    {
        if (string.IsNullOrWhiteSpace(maKH) || LayTheoMa(maKH) == null)
            return KetQua.Loi("Không tìm thấy mã khách hàng để xóa.");

        string ma = maKH.Trim();

        try
        {
            int soDong = new KhachHangDAO().Xoa(ma);
            if (soDong != 1)
                return KetQua.Loi("Không tìm thấy mã khách hàng để xóa.");

            TaiLai();
            return KetQua.Ok("Đã xóa khách hàng " + ma);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return KetQua.Loi(
                "Không thể xóa khách hàng đã có hóa đơn.");
        }
        catch (Exception ex)
        {
            return KetQua.Loi(
                "Không xóa được khách hàng: " + ex.Message);
        }
    }

    private static KetQua KiemTraDuLieu(KhachHang khachHang)
    {
        if (string.IsNullOrWhiteSpace(khachHang.HoTen))
            return KetQua.Loi("Họ tên khách hàng không được để trống.");

        if (khachHang.HoTen.Trim().Length > 100)
            return KetQua.Loi("Họ tên không được vượt quá 100 ký tự.");

        if (khachHang.SDT?.Trim().Length > 15)
            return KetQua.Loi("Số điện thoại không được vượt quá 15 ký tự.");

        if (!string.IsNullOrWhiteSpace(khachHang.SDT) &&
            !khachHang.SDT.Trim().All(char.IsDigit))
        {
            return KetQua.Loi("Số điện thoại chỉ được chứa chữ số.");
        }

        if (khachHang.DiaChi?.Trim().Length > 200)
            return KetQua.Loi("Địa chỉ không được vượt quá 200 ký tự.");

        return KetQua.Ok();
    }

    public static void TaiLai()
    {
        danhSachKhachHang = null;
    }
}

public sealed class KetQuaLuuKhachHang
{
    public bool ThanhCong { get; private init; }
    public string ThongBao { get; private init; } = string.Empty;
    public KhachHang? KhachHang { get; private init; }

    public static KetQuaLuuKhachHang Loi(string thongBao) =>
        new() { ThongBao = thongBao };

    public static KetQuaLuuKhachHang Ok(
        KhachHang khachHang, string thongBao) =>
        new()
        {
            ThanhCong = true,
            KhachHang = khachHang,
            ThongBao = thongBao
        };
}
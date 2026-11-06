using System.Collections;
using System.Data;

public class ThamSoBUS
{
    private static ArrayList? danhSachThamSo;

    public ArrayList LayTatCa()
    {
        if (danhSachThamSo != null)
            return new ArrayList(danhSachThamSo);

        DataTable bang = new ThamSoDAO().LayTatCa();
        var danhSach = new ArrayList();

        foreach (DataRow dong in bang.Rows)
        {
            danhSach.Add(new ThamSo
            {
                MaThamSo = dong["MATHAMSO"].ToString() ?? string.Empty,
                GiaTri = dong["GIATRI"].ToString() ?? string.Empty
            });
        }

        danhSachThamSo = danhSach;
        return new ArrayList(danhSachThamSo);
    }

    public ThamSo? LayTheoMa(string maThamSo)
    {
        if (string.IsNullOrWhiteSpace(maThamSo))
            return null;

        foreach (ThamSo thamSo in LayTatCa())
        {
            if (thamSo.MaThamSo.Equals(
                    maThamSo.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return thamSo;
            }
        }

        return null;
    }

    public string LayGiaTri(string maThamSo, string macDinh = "")
    {
        return LayTheoMa(maThamSo)?.GiaTri ?? macDinh;
    }

    public int LayInt(string maThamSo, int macDinh = 0)
    {
        string giaTri = LayGiaTri(maThamSo);
        return int.TryParse(giaTri, out int so) ? so : macDinh;
    }

    public ArrayList TimKiem(string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
            return LayTatCa();

        string canTim = tuKhoa.Trim();
        var ketQua = new ArrayList();

        foreach (ThamSo thamSo in LayTatCa())
        {
            if (thamSo.MaThamSo.Contains(
                    canTim, StringComparison.OrdinalIgnoreCase) ||
                thamSo.GiaTri.Contains(
                    canTim, StringComparison.OrdinalIgnoreCase))
            {
                ketQua.Add(thamSo);
            }
        }

        return ketQua;
    }

    public KetQua Them(ThamSo thamSo)
    {
        KetQua kiemTra = KiemTraDuLieu(thamSo);
        if (!kiemTra.ThanhCong)
            return kiemTra;

        thamSo.MaThamSo = thamSo.MaThamSo.Trim();
        thamSo.GiaTri = thamSo.GiaTri.Trim();

        if (LayTheoMa(thamSo.MaThamSo) != null)
            return KetQua.Loi("Mã tham số đã tồn tại.");

        try
        {
            int soDong = new ThamSoDAO().Them(thamSo);
            if (soDong != 1)
                return KetQua.Loi("Không thêm được tham số.");

            TaiLai();
            return KetQua.Ok("Đã thêm tham số " + thamSo.MaThamSo);
        }
        catch (Exception ex)
        {
            return KetQua.Loi("Không thêm được tham số: " + ex.Message);
        }
    }

    public KetQua Sua(ThamSo thamSo)
    {
        KetQua kiemTra = KiemTraDuLieu(thamSo);
        if (!kiemTra.ThanhCong)
            return kiemTra;

        thamSo.MaThamSo = thamSo.MaThamSo.Trim();
        thamSo.GiaTri = thamSo.GiaTri.Trim();

        if (LayTheoMa(thamSo.MaThamSo) == null)
            return KetQua.Loi("Không tìm thấy mã tham số để sửa.");

        try
        {
            int soDong = new ThamSoDAO().Sua(thamSo);
            if (soDong != 1)
                return KetQua.Loi("Không tìm thấy mã tham số để sửa.");

            TaiLai();
            return KetQua.Ok("Đã sửa tham số " + thamSo.MaThamSo);
        }
        catch (Exception ex)
        {
            return KetQua.Loi("Không sửa được tham số: " + ex.Message);
        }
    }

    public KetQua Xoa(string maThamSo)
    {
        if (string.IsNullOrWhiteSpace(maThamSo) ||
            LayTheoMa(maThamSo) == null)
        {
            return KetQua.Loi("Không tìm thấy mã tham số để xóa.");
        }

        string ma = maThamSo.Trim();

        try
        {
            int soDong = new ThamSoDAO().Xoa(ma);
            if (soDong != 1)
                return KetQua.Loi("Không tìm thấy mã tham số để xóa.");

            TaiLai();
            return KetQua.Ok("Đã xóa tham số " + ma);
        }
        catch (Exception ex)
        {
            return KetQua.Loi("Không xóa được tham số: " + ex.Message);
        }
    }

    private static KetQua KiemTraDuLieu(ThamSo thamSo)
    {
        if (string.IsNullOrWhiteSpace(thamSo.MaThamSo))
            return KetQua.Loi("Mã tham số không được để trống.");

        if (thamSo.MaThamSo.Trim().Length > 50)
            return KetQua.Loi("Mã tham số không được quá 50 ký tự.");

        if (string.IsNullOrWhiteSpace(thamSo.GiaTri))
            return KetQua.Loi("Giá trị không được để trống.");

        if (thamSo.GiaTri.Trim().Length > 200)
            return KetQua.Loi("Giá trị không được quá 200 ký tự.");

        return KetQua.Ok();
    }

    public static void TaiLai()
    {
        danhSachThamSo = null;
    }
}
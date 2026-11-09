using System.Collections;
using System.Data;

public class NhanVienBUS
{
    private static ArrayList? danhSachNhanVien;

    public ArrayList LayTatCa()
    {
        if (danhSachNhanVien != null)
            return new ArrayList(danhSachNhanVien);

        DataTable bang = new NhanVienDAO().LayTatCa();
        var danhSach = new ArrayList();

        foreach (DataRow dong in bang.Rows)
        {
            danhSach.Add(new NhanVien
            {
                MaNV = dong["MANV"].ToString() ?? string.Empty,
                HoTenNV = dong["HOTENNV"].ToString() ?? string.Empty,
                ChucVu = dong["CHUCVU"].ToString() ?? string.Empty,
                Luong = dong.IsNull("LUONG")
                    ? null
                    : Convert.ToDecimal(dong["LUONG"])
            });
        }

        danhSachNhanVien = danhSach;
        return new ArrayList(danhSachNhanVien);
    }
    public NhanVien? LayTheoMa(string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV))
            return null;

        foreach (NhanVien nv in LayTatCa())
        {
            if (nv.MaNV.Equals(maNV.Trim(), StringComparison.OrdinalIgnoreCase))
                return nv;
        }

        return null;
    }
    public string TaoMaMoi()
    {
        var dsMa = new List<string>();

        foreach (NhanVien nv in LayTatCa())
            dsMa.Add(nv.MaNV);

        return MaTuDong.TaoMaMoi(dsMa, "NV");
    }
    public KetQua Them(NhanVien nv)
    {
        if (string.IsNullOrWhiteSpace(nv.HoTenNV))
            return KetQua.Loi("Họ tên nhân viên không được để trống.");

        if (nv.HoTenNV.Trim().Length > 100)
            return KetQua.Loi("Họ tên không được quá 100 ký tự.");

        if (nv.ChucVu?.Trim().Length > 50)
            return KetQua.Loi("Chức vụ không được quá 50 ký tự.");

        if (nv.Luong < 0)
            return KetQua.Loi("Lương không được âm.");

        nv.HoTenNV = nv.HoTenNV.Trim();
        nv.ChucVu = nv.ChucVu?.Trim() ?? string.Empty;

        try
        {
            nv.MaNV = TaoMaMoi();

            int soDong = new NhanVienDAO().Them(nv);
            if (soDong != 1)
                return KetQua.Loi("Không thêm được nhân viên.");

            TaiLai();
            return KetQua.Ok("Đã thêm nhân viên " + nv.MaNV);
        }
        catch (Exception ex)
        {
            return KetQua.Loi("Không thêm được nhân viên: " + ex.Message);
        }
    }
    public KetQua Sua(NhanVien nv)
{
    if (string.IsNullOrWhiteSpace(nv.MaNV) || LayTheoMa(nv.MaNV) == null)
        return KetQua.Loi("Không tìm thấy mã nhân viên để sửa.");

    if (string.IsNullOrWhiteSpace(nv.HoTenNV))
        return KetQua.Loi("Họ tên nhân viên không được để trống.");

    if (nv.HoTenNV.Trim().Length > 100)
        return KetQua.Loi("Họ tên không được quá 100 ký tự.");

    if (nv.ChucVu?.Trim().Length > 50)
        return KetQua.Loi("Chức vụ không được quá 50 ký tự.");

    if (nv.Luong < 0)
        return KetQua.Loi("Lương không được âm.");

    nv.HoTenNV = nv.HoTenNV.Trim();
    nv.ChucVu = nv.ChucVu?.Trim() ?? string.Empty;

    try
    {
        int soDong = new NhanVienDAO().Sua(nv);
        if (soDong != 1)
            return KetQua.Loi("Không tìm thấy mã nhân viên để sửa.");

        TaiLai();
        return KetQua.Ok("Đã sửa nhân viên " + nv.MaNV);
    }
    catch (Exception ex)
    {
        return KetQua.Loi("Không sửa được nhân viên: " + ex.Message);
    }
}
public KetQua Xoa(string maNV)
{
    if (string.IsNullOrWhiteSpace(maNV) || LayTheoMa(maNV) == null)
        return KetQua.Loi("Không tìm thấy mã nhân viên để xóa.");

    try
    {
        int soDong = new NhanVienDAO().Xoa(maNV.Trim());
        if (soDong != 1)
            return KetQua.Loi("Không tìm thấy mã nhân viên để xóa.");

        TaiLai();
        return KetQua.Ok("Đã xóa nhân viên " + maNV.Trim());
    }
    catch (Exception)
    {
        return KetQua.Loi(
            "Không xóa được nhân viên. Hãy kiểm tra nhân viên này đã có hóa đơn hoặc phiếu nhập chưa.");
    }
}
public ArrayList TimKiem(string tuKhoa)
{
    if (string.IsNullOrWhiteSpace(tuKhoa))
        return LayTatCa();

    string canTim = tuKhoa.Trim();
    var ketQua = new ArrayList();

    foreach (NhanVien nv in LayTatCa())
    {
        if (nv.MaNV.Contains(canTim, StringComparison.OrdinalIgnoreCase) ||
            nv.HoTenNV.Contains(canTim, StringComparison.OrdinalIgnoreCase) ||
            nv.ChucVu.Contains(canTim, StringComparison.OrdinalIgnoreCase))
        {
            ketQua.Add(nv);
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
    public static void TaiLai()
    {
        danhSachNhanVien = null;
    }
}
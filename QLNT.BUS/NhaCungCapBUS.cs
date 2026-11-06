using System.Collections;
using System.Data;

public class NhaCungCapBUS
{
    private static ArrayList? danhSachNhaCungCap;

    private static ArrayList LayDanhSach()
    {
        if (danhSachNhaCungCap != null)
            return danhSachNhaCungCap;

        DataTable bang = new NhaCungCapDAO().LayTatCa();
        var danhSach = new ArrayList();
        foreach (DataRow dong in bang.Rows)
        {
            danhSach.Add(new NhaCungCap
            {
                MaNCC = dong["MANCC"].ToString() ?? string.Empty,
                TenNCC = dong["TENNCC"].ToString() ?? string.Empty,
                DiaChi = dong.IsNull("DIACHI") ? null : dong["DIACHI"].ToString(),
                SDT = dong.IsNull("SDT") ? null : dong["SDT"].ToString()
            });
        }

        danhSachNhaCungCap = danhSach;
        return danhSach;
    }

    public ArrayList LayTatCa() => new(LayDanhSach());

    public static void TaiLai() => danhSachNhaCungCap = null;

    public NhaCungCap? LayTheoMa(string maNCC)
    {
        if (string.IsNullOrWhiteSpace(maNCC))
            return null;

        foreach (NhaCungCap ncc in LayDanhSach())
        {
            if (ncc.MaNCC.Equals(maNCC.Trim(), StringComparison.OrdinalIgnoreCase))
                return ncc;
        }

        return null;
    }

    public string TaoMaMoi()
    {
        var cacMa = new List<string>();
        foreach (NhaCungCap ncc in LayDanhSach())
            cacMa.Add(ncc.MaNCC);

        return MaTuDong.TaoMaMoi(cacMa, "NCC");
    }

    public ArrayList TimKiem(string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
            return LayTatCa();

        string noiDung = tuKhoa.Trim();
        var ketQua = new ArrayList();
        foreach (NhaCungCap ncc in LayDanhSach())
        {
            if (ncc.MaNCC.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                ncc.TenNCC.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                (ncc.DiaChi?.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (ncc.SDT?.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ?? false))
                ketQua.Add(ncc);
        }

        return ketQua;
    }

    private static string? KiemTraDuLieu(NhaCungCap? ncc)
    {
        if (ncc == null)
            return "Chưa có thông tin nhà cung cấp.";
        if (string.IsNullOrWhiteSpace(ncc.MaNCC) || ncc.MaNCC.Trim().Length > 10)
            return "Mã nhà cung cấp không hợp lệ.";
        if (string.IsNullOrWhiteSpace(ncc.TenNCC) || ncc.TenNCC.Trim().Length > 100)
            return "Tên nhà cung cấp phải có từ 1 đến 100 ký tự.";
        if (ncc.DiaChi?.Trim().Length > 200)
            return "Địa chỉ không được quá 200 ký tự.";
        if (ncc.SDT?.Trim().Length > 15)
            return "Số điện thoại không được quá 15 ký tự.";
        if (!string.IsNullOrWhiteSpace(ncc.SDT) &&
            !ncc.SDT.Trim().All(char.IsDigit))
            return "Số điện thoại chỉ được chứa chữ số.";

        return null;
    }

    public KetQua Them(NhaCungCap ncc)
    {
        try
        {
            string? loi = KiemTraDuLieu(ncc);
            if (loi != null)
                return KetQua.Loi(loi);
            if (LayTheoMa(ncc.MaNCC) != null)
                return KetQua.Loi("Mã nhà cung cấp đã tồn tại.");

            if (new NhaCungCapDAO().Them(ChuanHoa(ncc)) != 1)
                return KetQua.Loi("Không thêm được nhà cung cấp.");

            TaiLai();
            return KetQua.Ok("Đã thêm nhà cung cấp.");
        }
        catch (Exception)
        {
            return KetQua.Loi("Không thêm được nhà cung cấp. Hãy kiểm tra mã và kết nối database.");
        }
    }

    public KetQua Sua(NhaCungCap ncc)
    {
        try
        {
            string? loi = KiemTraDuLieu(ncc);
            if (loi != null)
                return KetQua.Loi(loi);

            if (new NhaCungCapDAO().Sua(ChuanHoa(ncc)) == 0)
                return KetQua.Loi("Không tìm thấy nhà cung cấp cần sửa.");

            TaiLai();
            return KetQua.Ok("Đã sửa nhà cung cấp.");
        }
        catch (Exception)
        {
            return KetQua.Loi("Không sửa được nhà cung cấp. Hãy kiểm tra kết nối database.");
        }
    }

    public KetQua Xoa(string maNCC)
    {
        if (string.IsNullOrWhiteSpace(maNCC))
            return KetQua.Loi("Chưa chọn nhà cung cấp cần xóa.");

        try
        {
            if (new NhaCungCapDAO().Xoa(maNCC.Trim()) == 0)
                return KetQua.Loi("Không tìm thấy nhà cung cấp cần xóa.");

            TaiLai();
            return KetQua.Ok("Đã xóa nhà cung cấp.");
        }
        catch (Exception)
        {
            return KetQua.Loi("Không xóa được nhà cung cấp. Nhà cung cấp có thể đang được phiếu nhập sử dụng.");
        }
    }

    private static NhaCungCap ChuanHoa(NhaCungCap ncc) => new()
    {
        MaNCC = ncc.MaNCC.Trim(),
        TenNCC = ncc.TenNCC.Trim(),
        DiaChi = string.IsNullOrWhiteSpace(ncc.DiaChi) ? null : ncc.DiaChi.Trim(),
        SDT = string.IsNullOrWhiteSpace(ncc.SDT) ? null : ncc.SDT.Trim()
    };
}

using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
public class LoaiThuocBUS
{
    private static ArrayList? danhSachLoaiThuoc;

    private static ArrayList LayDanhSach()
    {
        if (danhSachLoaiThuoc != null)
        {
            return danhSachLoaiThuoc;
        }

        LoaiThuocDAO dao = new LoaiThuocDAO();

        DataTable dt = dao.LayTatCa();
        ArrayList danhSach = new ArrayList();

        foreach (DataRow row in dt.Rows)
        {
            string ma = row["MALOAI"].ToString() ?? string.Empty;
            string ten = row["TENLOAI"].ToString() ?? string.Empty;

            LoaiThuoc loaiThuoc = new LoaiThuoc(ma, ten);

            danhSach.Add(loaiThuoc);
        }

        danhSachLoaiThuoc = danhSach;
        return danhSach;
    }

    public ArrayList LayTatCa()
    {
        return new ArrayList(LayDanhSach());
    }

    public static void TaiLai()
    {
        danhSachLoaiThuoc = null;
    }

    // Tìm theo mã
    public LoaiThuoc? TimTheoMa(string ma)
    {
        foreach (LoaiThuoc lt in LayDanhSach())
        {
            if (lt.MaLoai.Equals(ma, StringComparison.OrdinalIgnoreCase))
            {
                return lt;
            }
        }

        return null;
    }


    public ArrayList TimKiem(string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
        {
            return LayTatCa();
        }

        string noiDung = tuKhoa.Trim();
        ArrayList ketQua = new ArrayList();

        foreach (LoaiThuoc loaiThuoc in LayDanhSach())
        {
            if (loaiThuoc.MaLoai.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                loaiThuoc.TenLoai.Contains(noiDung, StringComparison.OrdinalIgnoreCase))
            {
                ketQua.Add(loaiThuoc);
            }
        }

        return ketQua;
    }

    public string TaoMaMoi()
{
    var cacMaHienCo = new List<string>();

    foreach (LoaiThuoc loaiThuoc in LayDanhSach())
    {
        cacMaHienCo.Add(loaiThuoc.MaLoai);
    }

    return MaTuDong.TaoMaMoi(cacMaHienCo, "LT");
}

private static string? KiemTraTenLoai(string? tenLoai)
{
    if (string.IsNullOrWhiteSpace(tenLoai))
        return "Tên loại thuốc không được để trống.";

    if (tenLoai.Trim().Length > 100)
        return "Tên loại thuốc không được dài quá 100 ký tự.";

    return null;
}

public KetQua Them(LoaiThuoc loaiThuoc)
{
    string? loi = KiemTraTenLoai(loaiThuoc.TenLoai);
    if (loi != null)
        return KetQua.Loi(loi);

    if (string.IsNullOrWhiteSpace(loaiThuoc.MaLoai))
        return KetQua.Loi("Mã loại thuốc không được để trống.");

    if (TimTheoMa(loaiThuoc.MaLoai) != null)
        return KetQua.Loi("Mã loại thuốc đã tồn tại.");

    try
    {
        var duLieu = new LoaiThuoc(
            loaiThuoc.MaLoai.Trim(),
            loaiThuoc.TenLoai.Trim());

        int soDong = new LoaiThuocDAO().Them(duLieu);
        if (soDong != 1)
            return KetQua.Loi("Không thêm được loại thuốc.");

        TaiLai();
        return KetQua.Ok("Đã thêm loại thuốc.");
    }
    catch (Exception)
    {
        return KetQua.Loi("Không thêm được loại thuốc. Hãy kiểm tra mã và kết nối database.");
    }
}

public KetQua Sua(LoaiThuoc loaiThuoc)
{
    if (string.IsNullOrWhiteSpace(loaiThuoc.MaLoai))
        return KetQua.Loi("Chưa chọn loại thuốc cần sửa.");

    string? loi = KiemTraTenLoai(loaiThuoc.TenLoai);
    if (loi != null)
        return KetQua.Loi(loi);

    try
    {
        var duLieu = new LoaiThuoc(
            loaiThuoc.MaLoai.Trim(),
            loaiThuoc.TenLoai.Trim());

        int soDong = new LoaiThuocDAO().Sua(duLieu);
        if (soDong == 0)
            return KetQua.Loi("Không tìm thấy loại thuốc cần sửa.");

        TaiLai();
        return KetQua.Ok("Đã sửa loại thuốc.");
    }
    catch (Exception)
    {
        return KetQua.Loi("Không sửa được loại thuốc. Hãy kiểm tra kết nối database.");
    }
}

public KetQua Xoa(string maLoai)
{
    if (string.IsNullOrWhiteSpace(maLoai))
        return KetQua.Loi("Chưa chọn loại thuốc cần xóa.");

    try
    {
        int soDong = new LoaiThuocDAO().Xoa(maLoai.Trim());
        if (soDong == 0)
            return KetQua.Loi("Không tìm thấy loại thuốc cần xóa.");

        TaiLai();
        return KetQua.Ok("Đã xóa loại thuốc.");
    }
    catch (Exception)
    {
        return KetQua.Loi(
            "Không xóa được loại thuốc. Loại này có thể đang được thuốc sử dụng, hoặc có lỗi kết nối database.");
    }
}
}

using System;
using System.Collections;
using System.Data;

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
}

using System.Collections;
using System.Data;

public class NhanVienBUS
{
    public ArrayList LayTatCa()
    {
        DataTable bang = new NhanVienDAO().LayTatCa();
        var danhSach = new ArrayList();
        foreach (DataRow dong in bang.Rows)
        {
            danhSach.Add(new NhanVien
            {
                MaNV = dong["MANV"].ToString() ?? string.Empty,
                HoTenNV = dong["HOTENNV"].ToString() ?? string.Empty,
                ChucVu = dong["CHUCVU"].ToString() ?? string.Empty
            });
        }

        return danhSach;
    }
}

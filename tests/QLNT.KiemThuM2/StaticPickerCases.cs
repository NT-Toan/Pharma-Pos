using System.Reflection;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraChonThuocVaNhaCungCapStatic()
    {
        KiemTraChonThuocStatic();
        KiemTraChonNhaCungCapStatic();
    }

    private static void KiemTraChonThuocStatic()
    {
        PropertyInfo? ketQua = typeof(frmChonThuoc).GetProperty(
            "ThuocDuocChon", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo? soLuong = typeof(frmChonThuoc).GetProperty(
            "SoLuongDuocChon", BindingFlags.Public | BindingFlags.Static);
        Check(ketQua != null && soLuong != null, "Picker thuoc tra thuoc va so luong qua static");

        var thuoc = new Thuoc { MaThuoc = "TH_TEST", TenThuoc = "Thuoc test", SoLuong = 5 };
        using (var form = new frmChonThuoc())
        {
            ListView danhSach = (ListView)typeof(frmChonThuoc).GetField(
                "lvThuoc", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            danhSach.Items.Add(new ListViewItem(thuoc.MaThuoc) { Tag = thuoc });
            _ = danhSach.Handle;
            danhSach.Items[0].Selected = true;
            typeof(frmChonThuoc).GetMethod("Chon", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, null);
            Check(ReferenceEquals(ketQua!.GetValue(null), thuoc) &&
                  Convert.ToInt32(soLuong!.GetValue(null)) == 1,
                "Chon thuoc ghi dung thuoc va so luong vao static");
        }

        using (var form = new frmChonThuoc())
        {
            Check(ketQua!.GetValue(null) == null && Convert.ToInt32(soLuong!.GetValue(null)) == 0,
                "Mo picker thuoc lan sau khong giu lua chon cu");
        }
    }

    private static void KiemTraChonNhaCungCapStatic()
    {
        PropertyInfo? ketQua = typeof(frmChonNhaCungCap).GetProperty(
            "NhaCungCapDuocChon", BindingFlags.Public | BindingFlags.Static);
        Check(ketQua != null, "Picker nha cung cap tra ket qua qua static");

        var ncc = new NhaCungCap { MaNCC = "NCC_TEST", TenNCC = "NCC test" };
        using (var form = new frmChonNhaCungCap())
        {
            ListView danhSach = (ListView)typeof(frmChonNhaCungCap).GetField(
                "lvNhaCungCap", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            danhSach.Items.Add(new ListViewItem(ncc.MaNCC) { Tag = ncc });
            _ = danhSach.Handle;
            danhSach.Items[0].Selected = true;
            typeof(frmChonNhaCungCap).GetMethod("Chon", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, null);
            Check(ReferenceEquals(ketQua!.GetValue(null), ncc),
                "Chon nha cung cap ghi dung ket qua vao static");
        }

        using (var form = new frmChonNhaCungCap())
        {
            Check(ketQua!.GetValue(null) == null,
                "Mo picker nha cung cap lan sau khong giu lua chon cu");
        }
    }
}

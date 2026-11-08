using System.Collections;
using System.Reflection;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraChonNhanVienStatic()
    {
        PropertyInfo? giaTriDuocChon = typeof(frmChonNhanVien).GetProperty(
            "NhanVienDuocChon", BindingFlags.Public | BindingFlags.Static);
        Check(giaTriDuocChon != null, "Form chon nhan vien co ket qua static");

        var nhanVien = new NhanVien { MaNV = "NV01", HoTenNV = "An" };
        using (var form = new frmChonNhanVien())
        {
            var danhSach = (ListView)typeof(frmChonNhanVien).GetField(
                "lvNhanVien", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            danhSach.Items.Add(new ListViewItem(nhanVien.MaNV) { Tag = nhanVien });
            _ = danhSach.Handle;
            danhSach.Items[0].Selected = true;
            typeof(frmChonNhanVien).GetMethod("Chon", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, null);
            Check(ReferenceEquals(giaTriDuocChon!.GetValue(null), nhanVien),
                "Chon nhan vien ghi ket qua static");
        }

        using (var form = new frmChonNhanVien())
        {
            Check(giaTriDuocChon!.GetValue(null) == null,
                "Mo lai form khong giu nhan vien cu");
            form.DialogResult = DialogResult.Cancel;
            form.Close();
            Check(giaTriDuocChon.GetValue(null) == null,
                "Huy chon khong tra nhan vien cu");
        }

        var cache = typeof(NhanVienBUS).GetField(
            "danhSachNhanVien", BindingFlags.Static | BindingFlags.NonPublic)!;
        try
        {
            cache.SetValue(null, new ArrayList
            {
                new NhanVien { MaNV = "NV01", HoTenNV = "An" },
                new NhanVien { MaNV = "NV02", HoTenNV = "Bình" }
            });
            using var form = new frmChonNhanVien();
            var tim = (TextBox)typeof(frmChonNhanVien).GetField(
                "txtTimKiem", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            var danhSach = (ListView)typeof(frmChonNhanVien).GetField(
                "lvNhanVien", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            tim.Text = "Bình";
            Check(danhSach.Items.Count == 1 && danhSach.Items[0].Text == "NV02",
                "Picker nhan vien tim tren ArrayList cua BUS");
        }
        finally
        {
            NhanVienBUS.TaiLai();
        }
    }
}

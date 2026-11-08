using System.Collections;
using System.Reflection;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraChonKhachHangStatic()
    {
        PropertyInfo? giaTriDuocChon = typeof(frmChonKhachHang).GetProperty(
            "KhachHangDuocChon", BindingFlags.Public | BindingFlags.Static);
        Check(giaTriDuocChon != null, "Form chon khach hang co ket qua static");

        var khach = new KhachHang { MaKH = "KH01", HoTen = "An" };
        using (var form = new frmChonKhachHang())
        {
            var danhSach = (ListView)typeof(frmChonKhachHang).GetField(
                "lvKhachHang", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            danhSach.Items.Add(new ListViewItem(khach.MaKH) { Tag = khach });
            _ = danhSach.Handle;
            danhSach.Items[0].Selected = true;
            typeof(frmChonKhachHang).GetMethod("Chon", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, null);
            Check(ReferenceEquals(giaTriDuocChon!.GetValue(null), khach),
                "Chon mot khach hang ghi ket qua static");
        }

        using (var form = new frmChonKhachHang())
        {
            Check(giaTriDuocChon!.GetValue(null) == null,
                "Mo lai form khong giu khach hang cu");
            form.DialogResult = DialogResult.Cancel;
            form.Close();
            Check(giaTriDuocChon.GetValue(null) == null,
                "Huy chon khong tra khach hang cu");
        }

        var cache = typeof(KhachHangBUS).GetField(
            "danhSachKhachHang", BindingFlags.Static | BindingFlags.NonPublic)!;
        try
        {
            cache.SetValue(null, new ArrayList
            {
                new KhachHang { MaKH = "KH01", HoTen = "An" },
                new KhachHang { MaKH = "KH02", HoTen = "Bình" }
            });
            using var form = new frmChonKhachHang();
            var tim = (TextBox)typeof(frmChonKhachHang).GetField(
                "txtTimKiem", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            var danhSach = (ListView)typeof(frmChonKhachHang).GetField(
                "lvKhachHang", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            tim.Text = "Bình";
            Check(danhSach.Items.Count == 1 && danhSach.Items[0].Text == "KH02",
                "Picker khach hang tim tren ArrayList cua BUS");
        }
        finally
        {
            KhachHangBUS.TaiLai();
        }
    }
}

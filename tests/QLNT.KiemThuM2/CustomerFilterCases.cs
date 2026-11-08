using System.Collections;
using System.Reflection;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraLocDiaChiKhachHang()
    {
        var truongCache = typeof(KhachHangBUS).GetField(
            "danhSachKhachHang", BindingFlags.Static | BindingFlags.NonPublic)!;
        try
        {
            truongCache.SetValue(null, new ArrayList
            {
                new KhachHang { MaKH = "KH01", HoTen = "An", DiaChi = "Quận 1, TP HCM" },
                new KhachHang { MaKH = "KH02", HoTen = "Bình", DiaChi = "Cầu Giấy, Hà Nội" },
                new KhachHang { MaKH = "KH03", HoTen = "Chi", DiaChi = "Đà Nẵng" }
            });

            using var form = new frmKhachHang();
            TextBox diaChi1 = (TextBox)form.Controls.Find("txtLocDiaChi1", true).Single();
            TextBox diaChi2 = (TextBox)form.Controls.Find("txtLocDiaChi2", true).Single();
            Button loc = (Button)form.Controls.Find("btnLocDiaChiOr", true).Single();
            ListView danhSach = (ListView)form.Controls.Find("lvKhachHang", true).Single();

            diaChi1.Text = "TP HCM";
            diaChi2.Text = "Hà Nội";
            typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(loc, [EventArgs.Empty]);

            Check(danhSach.Items.Count == 2 &&
                  danhSach.Items.Cast<ListViewItem>().Select(x => x.Text)
                      .OrderBy(x => x).SequenceEqual(new[] { "KH01", "KH02" }),
                "Form khach hang loc dia chi TP HCM OR Ha Noi tren ArrayList");
        }
        finally
        {
            KhachHangBUS.TaiLai();
        }
    }
}

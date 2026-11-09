using System.Reflection;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraBoCucTongTienHoaDon()
    {
        using var form = new frmTaoHoaDon
        {
            Size = new Size(900, 760),
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.Show();
        Application.DoEvents();

        Label LayNhan(string ten) => (Label)typeof(frmTaoHoaDon)
            .GetField(ten, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(form)!;

        LayNhan("lblLoaiKhuyenMaiHoaDon").Text =
            "KMHoaDon - KM0003 (3%; mốc 1.000.000,00 đ)";
        LayNhan("lblTongGoc").Text = "300.000,00";
        LayNhan("lblGiamSanPham").Text = "0,00";
        LayNhan("lblGiamHoaDon").Text = "9.000,00";
        LayNhan("lblTongTien").Text = "291.000,00";
        Application.DoEvents();

        var bangTong = (TableLayoutPanel)LayNhan("lblTongGoc").Parent!;
        var khung = (TableLayoutPanel)bangTong.Parent!;
        Check(bangTong.Top >= khung.GetControlFromPosition(0, 0)!.Bottom &&
              bangTong.Bottom <= khung.GetControlFromPosition(0, 3)!.Top,
            "Bang tong tien khong de len thong tin hoac nut luu");
        Check(khung.GetControlFromPosition(0, 1)!.Height >= 40,
            "Danh sach thuoc van con cho hien thi");

        DataGridView dgv = (DataGridView)form.Controls.Find("dgvChiTiet", true).Single();
        dgv.Rows.Add(6);
        Application.DoEvents();
        Check(dgv.Rows[5].Displayed,
            "Dong thuoc thu 6 hien thi ma khong can cuon");

        form.Size = new Size(900, 600);
        for (int i = 0; i < 30; i++)
            dgv.Rows.Add($"T{i}", $"Thuoc {i}", 1, "10.000", "0", "Khong", "10.000");
        Application.DoEvents();
        Check(dgv.ScrollBars.HasFlag(ScrollBars.Vertical) &&
              dgv.DisplayedRowCount(false) < dgv.Rows.Count,
            "Danh sach thuoc co the cuon doc khi form nho va co nhieu dong");

        foreach (string ten in new[] { "lblTongGoc", "lblGiamSanPham", "lblGiamHoaDon", "lblTongTien", "lblTienThua" })
        {
            Label nhan = LayNhan(ten);
            int canhPhai = form.PointToClient(nhan.PointToScreen(new Point(nhan.Width, 0))).X;
            Check(nhan.Visible && canhPhai <= form.ClientSize.Width,
                $"{ten} nam trong khung form hoa don (canh phai {canhPhai}, form {form.ClientSize.Width})");
        }
    }
}

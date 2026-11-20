using System.Data;
using QLNT.BUS;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraDuLieuBieuDo()
    {
        var bus = new ThongKeBUS();
        var theoQuy = new DataTable();
        theoQuy.Columns.Add("Ky", typeof(string));
        theoQuy.Columns.Add("Thu", typeof(decimal));
        theoQuy.Columns.Add("Chi", typeof(decimal));
        theoQuy.Columns.Add("LoiNhuan", typeof(decimal));
        theoQuy.Rows.Add("Q1", 200m, 100m, 100m);
        theoQuy.Rows.Add("Q2", 300m, 150m, 150m);
        DuLieuBieuDo duLieuQuy = bus.TaoDuLieuBieuDo(theoQuy, 1);
        Check(duLieuQuy.Cot.Count == 2 && duLieuQuy.Cot[0].GiaTriChinh == 200m &&
              duLieuQuy.Cot[0].GiaTriPhu == 100m && duLieuQuy.Tron.Sum(diem => diem.GiaTri) == 500m,
            "Bieu do cot Thu Chi va bieu do tron dung chung bang quy");

        var theoNhanVien = new DataTable();
        theoNhanVien.Columns.Add("Ma", typeof(string));
        theoNhanVien.Columns.Add("Ten", typeof(string));
        theoNhanVien.Columns.Add("TC", typeof(decimal));
        for (int i = 1; i <= 11; i++)
            theoNhanVien.Rows.Add($"NV{i}", $"Nhan vien {i}", i * 10m);
        theoNhanVien.Rows.Add("TC", "TỔNG CỘNG", 660m);
        DuLieuBieuDo duLieuNv = bus.TaoDuLieuBieuDo(theoNhanVien, 3);
        Check(duLieuNv.Cot.Count == 10 && duLieuNv.Tron.Count == 8 &&
              duLieuNv.Tron.Last().Nhan == "Khác" && duLieuNv.Tron.Sum(diem => diem.GiaTri) == 660m,
            "Bieu do doanh thu bo dong tong va gop nhom nho khong mat gia tri");

        theoQuy.Rows.Clear();
        theoQuy.Rows.Add("Q1", 0m, 50m, -50m);
        DuLieuBieuDo chi = bus.TaoDuLieuBieuDo(theoQuy, 1);
        Check(chi.Tron.Single().GiaTri == 50m && chi.TieuDeTron.Contains("chi"),
            "Bieu do tron dung chi khi khong co doanh thu");

        using var form = new frmThongKe();
        var khung = (TableLayoutPanel)form.Controls.Find("khungBaoCaoThongKe", true).Single();
        Check(form.Controls.Find("tabBaoCaoThongKe", true).Length == 0 &&
              khung.GetControlFromPosition(0, 0)?.Name == "grpBangThongKe" &&
              form.Controls.Find("grpCotThongKe", true).Length == 1 &&
              form.Controls.Find("grpTronThongKe", true).Length == 1,
            "Bang o tren va hai bieu do hien ngay ben duoi khong can chuyen tab");
        foreach (string ten in new[] { "bieuDoCotThongKe", "bieuDoTronThongKe" })
        {
            Control bieuDo = form.Controls.Find(ten, true).Single();
            bieuDo.GetType().GetProperty("DuLieu")!.SetValue(bieuDo, duLieuQuy);
            bieuDo.Dock = DockStyle.None;
            bieuDo.Size = new Size(700, 420);
            using var anh = new Bitmap(700, 420);
            bieuDo.DrawToBitmap(anh, new Rectangle(0, 0, anh.Width, anh.Height));
            Check(CoMauBieuDo(anh), $"{ten} ve duoc du lieu bang hien tai");
        }
    }

    private static bool CoMauBieuDo(Bitmap anh)
    {
        for (int y = 60; y < anh.Height - 60; y += 4)
            for (int x = 30; x < anh.Width - 30; x += 4)
            {
                Color mau = anh.GetPixel(x, y);
                if (mau.G > mau.R + 20 && mau.G > mau.B)
                    return true;
            }
        return false;
    }
}

using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace QLNT.GUI;

internal enum KieuBieuDo { Cot, Tron }

internal sealed class BieuDoThongKe : Control
{
    private static readonly Color[] Mau =
    [
        Color.FromArgb(19, 126, 97), Color.FromArgb(47, 111, 194),
        Color.FromArgb(238, 151, 64), Color.FromArgb(149, 91, 173),
        Color.FromArgb(80, 168, 186), Color.FromArgb(212, 91, 100),
        Color.FromArgb(122, 158, 76), Color.FromArgb(121, 131, 146)
    ];

    private DuLieuBieuDo? duLieu;

    public KieuBieuDo Kieu { get; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DuLieuBieuDo? DuLieu
    {
        get => duLieu;
        set { duLieu = value; Invalidate(); }
    }

    public BieuDoThongKe(KieuBieuDo kieu)
    {
        Kieu = kieu;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        if (Width < 220 || Height < 180)
        {
            VeThongBao(g, "Hãy mở rộng vùng biểu đồ.");
            return;
        }

        using var tieuDe = new Font("Segoe UI", 12, FontStyle.Bold);
        string ten = Kieu == KieuBieuDo.Cot ? duLieu?.TieuDe ?? "Biểu đồ cột"
            : duLieu?.TieuDeTron ?? "Biểu đồ tròn";
        g.DrawString(ten, tieuDe, Brushes.Black, 20, 16);
        if (duLieu == null || (Kieu == KieuBieuDo.Cot ? duLieu.Cot.Count == 0 : duLieu.Tron.Count == 0))
        {
            VeThongBao(g, "Không có dữ liệu để vẽ biểu đồ.");
            return;
        }

        if (Kieu == KieuBieuDo.Cot)
            VeCot(g, duLieu);
        else
            VeTron(g, duLieu);
    }

    private void VeCot(Graphics g, DuLieuBieuDo baoCao)
    {
        float trai = 85, tren = 65, phai = Width - 25, duoi = Height - 80;
        if (phai <= trai || duoi <= tren)
            return;
        float cao = duoi - tren;
        decimal lonNhat = baoCao.Cot.Max(diem => Math.Max(diem.GiaTriChinh, diem.GiaTriPhu ?? 0));
        if (lonNhat <= 0)
        {
            VeThongBao(g, "Các giá trị trong kỳ đều bằng 0.");
            return;
        }

        using var chuNho = new Font("Segoe UI", 8);
        using var canGiua = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
        using var canPhai = new StringFormat { Alignment = StringAlignment.Far };
        using var netLuoi = new Pen(Color.FromArgb(225, 230, 234));
        for (int moc = 0; moc <= 4; moc++)
        {
            float y = duoi - cao * moc / 4f;
            g.DrawLine(netLuoi, trai, y, phai, y);
            decimal giaTri = lonNhat * moc / 4m;
            g.DrawString(giaTri.ToString("N0"), chuNho, Brushes.DimGray,
                new RectangleF(2, y - 9, trai - 10, 20), canPhai);
        }

        float buoc = (phai - trai) / baoCao.Cot.Count;
        bool haiDay = baoCao.TenDayPhu != null;
        for (int i = 0; i < baoCao.Cot.Count; i++)
        {
            CotBieuDo diem = baoCao.Cot[i];
            float rongNhom = Math.Min(70, buoc * 0.72f);
            float rongCot = haiDay ? (rongNhom - 4) / 2 : rongNhom;
            float x = trai + i * buoc + (buoc - rongNhom) / 2;
            VeMotCot(g, x, rongCot, diem.GiaTriChinh, lonNhat, tren, duoi, Mau[0]);
            if (haiDay)
                VeMotCot(g, x + rongCot + 4, rongCot, diem.GiaTriPhu ?? 0, lonNhat, tren, duoi, Mau[1]);
            string nhan = diem.Nhan.Length > 13 ? diem.Nhan[..11] + "…" : diem.Nhan;
            g.DrawString(nhan, chuNho, Brushes.Black,
                new RectangleF(trai + i * buoc, duoi + 6, buoc, 25), canGiua);
        }

        using var butChinh = new SolidBrush(Mau[0]);
        g.FillRectangle(butChinh, trai, Height - 30, 13, 13);
        g.DrawString(baoCao.TenDayChinh, chuNho, Brushes.Black, trai + 19, Height - 33);
        if (haiDay)
        {
            using var butPhu = new SolidBrush(Mau[1]);
            float x = trai + 135;
            g.FillRectangle(butPhu, x, Height - 30, 13, 13);
            g.DrawString(baoCao.TenDayPhu!, chuNho, Brushes.Black, x + 19, Height - 33);
        }
    }

    private static void VeMotCot(Graphics g, float x, float rong, decimal giaTri,
        decimal lonNhat, float tren, float duoi, Color mau)
    {
        if (giaTri <= 0) return;
        float cao = (float)(giaTri / lonNhat) * (duoi - tren);
        using var but = new SolidBrush(mau);
        g.FillRectangle(but, x, duoi - cao, Math.Max(1, rong), cao);
    }

    private void VeTron(Graphics g, DuLieuBieuDo baoCao)
    {
        decimal tong = baoCao.Tron.Sum(diem => diem.GiaTri);
        if (tong <= 0)
        {
            VeThongBao(g, "Các giá trị trong kỳ đều bằng 0.");
            return;
        }

        float duongKinh = Math.Min(Math.Min(Height - 115, Width * 0.48f), 320);
        if (duongKinh < 100)
            return;
        float xTron = 25 + Math.Max(0, (Width * 0.55f - duongKinh) / 2);
        float yTron = 62 + Math.Max(0, (Height - 95 - duongKinh) / 2);
        float goc = -90;
        using var chu = new Font("Segoe UI", 9);
        for (int i = 0; i < baoCao.Tron.Count; i++)
        {
            LatBieuDo lat = baoCao.Tron[i];
            float doRong = i == baoCao.Tron.Count - 1
                ? 270 - goc
                : (float)(lat.GiaTri / tong * 360m);
            using var but = new SolidBrush(Mau[i % Mau.Length]);
            g.FillPie(but, xTron, yTron, duongKinh, duongKinh, goc, doRong);
            float xChu = Width * 0.57f;
            float yChu = 68 + i * 30;
            g.FillRectangle(but, xChu, yChu + 3, 13, 13);
            string nhan = lat.Nhan.Length > 20 ? lat.Nhan[..18] + "…" : lat.Nhan;
            g.DrawString($"{nhan}: {lat.GiaTri / tong:P1}", chu, Brushes.Black,
                new RectangleF(xChu + 20, yChu, Width - xChu - 30, 24));
            goc += doRong;
        }
    }

    private void VeThongBao(Graphics g, string thongBao)
    {
        using var chu = new Font("Segoe UI", 10);
        using var giua = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(thongBao, chu, Brushes.Gray, ClientRectangle, giua);
    }
}

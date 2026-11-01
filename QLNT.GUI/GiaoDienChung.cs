using System.Drawing;

namespace QLNT.GUI;

public static class GiaoDienChung
{
    public static readonly Color XanhChinh = Color.FromArgb(19, 125, 91);
    public static readonly Color Nen = Color.FromArgb(242, 245, 247);
    public static readonly Color MatTrang = Color.White;
    public static readonly Color Chu = Color.FromArgb(35, 48, 58);
    public static readonly Color ChuPhu = Color.FromArgb(98, 110, 118);
    public static readonly Color Vien = Color.FromArgb(216, 224, 228);
    public static readonly Color Menu = Color.FromArgb(31, 48, 59);
    public static readonly Color MenuDuocChon = Color.FromArgb(43, 71, 76);

    public static readonly Font PhongChu = new("Segoe UI", 10F);

    public static Button TaoNutPhu(string noiDung)
    {
        var nut = new Button
        {
            Text = noiDung,
            Height = 36,
            AutoSize = true,
            Padding = new Padding(12, 0, 12, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = MatTrang,
            ForeColor = Chu,
            Cursor = Cursors.Hand
        };
        nut.FlatAppearance.BorderColor = Vien;
        return nut;
    }
}

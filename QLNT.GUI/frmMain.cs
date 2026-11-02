using System.Drawing;

namespace QLNT.GUI;

public sealed class frmMain : Form
{
    private readonly Panel pnlNoiDung = new();
    private Button? nutDangChon;
    private Form? formDangMo;

    public frmMain()
    {
        Text = "QLNT - Quản lý nhà thuốc";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1200, 760);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        khung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(khung);

        var thanhMenu = new Panel { Dock = DockStyle.Fill, BackColor = GiaoDienChung.Menu };
        khung.Controls.Add(thanhMenu, 0, 0);
        khung.SetRowSpan(thanhMenu, 2);

        var dauTrang = new Panel { Dock = DockStyle.Fill, BackColor = GiaoDienChung.XanhChinh };
        var tenUngDung = new Label
        {
            Text = "Quản lý nhà thuốc",
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
        dauTrang.Controls.Add(tenUngDung);
        khung.Controls.Add(dauTrang, 1, 0);

        pnlNoiDung.Dock = DockStyle.Fill;
        pnlNoiDung.BackColor = GiaoDienChung.Nen;
        khung.Controls.Add(pnlNoiDung, 1, 1);

        var nhanMenu = new Label
        {
            Text = "QLNT",
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(20, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 17F, FontStyle.Bold)
        };
        var danhSachMenu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12, 8, 8, 12),
            BackColor = GiaoDienChung.Menu
        };
        thanhMenu.Controls.Add(danhSachMenu);
        thanhMenu.Controls.Add(nhanMenu);

        ThemNhom(danhSachMenu, "Điều hướng");
        Button nutTrangChinh = ThemNut(danhSachMenu, "Trang chính", MoTrangChinh);
        ThemNhom(danhSachMenu, "Danh mục");
        ThemNut(danhSachMenu, "Loại thuốc", () => MoFormCon(new frmLoaiThuoc()));
        ThemNut(danhSachMenu, "Thuốc", () => MoFormCon(new frmThuoc()));
        ThemNut(danhSachMenu, "Khách hàng", null);
        ThemNut(danhSachMenu, "Nhân viên", null);
        ThemNut(danhSachMenu, "Nhà cung cấp", () => MoFormCon(new frmNhaCungCap()));
        ThemNhom(danhSachMenu, "Giao dịch");
        ThemNut(danhSachMenu, "Bán hàng", () => MoFormCon(new frmHoaDon()));
        ThemNut(danhSachMenu, "Nhập hàng", null);
        ThemNut(danhSachMenu, "Khuyến mãi", () => MoFormCon(new frmKhuyenMai()));
        ThemNhom(danhSachMenu, "Báo cáo và hệ thống");
        ThemNut(danhSachMenu, "Thống kê", () => MoFormCon(new frmThongKe()));
        ThemNut(danhSachMenu, "Tham số", null);

        ChonNut(nutTrangChinh);
        MoTrangChinh();
    }

    private static void ThemNhom(FlowLayoutPanel menu, string tenNhom)
    {
        menu.Controls.Add(new Label
        {
            Text = tenNhom,
            Width = 190,
            Height = 32,
            Margin = new Padding(0, 12, 0, 0),
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = Color.FromArgb(172, 188, 194),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        });
    }

    private Button ThemNut(FlowLayoutPanel menu, string tenNut, Action? thaoTac)
    {
        var nut = new Button
        {
            Text = tenNut,
            Width = 190,
            Height = 40,
            Margin = new Padding(0, 2, 0, 2),
            Padding = new Padding(12, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat,
            BackColor = GiaoDienChung.Menu,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Enabled = thaoTac != null
        };
        nut.FlatAppearance.BorderSize = 0;
        nut.Click += (_, _) =>
        {
            ChonNut(nut);
            thaoTac?.Invoke();
        };
        menu.Controls.Add(nut);
        return nut;
    }

    private void ChonNut(Button nut)
    {
        if (nutDangChon != null)
        {
            nutDangChon.BackColor = GiaoDienChung.Menu;
        }
        nutDangChon = nut;
        nutDangChon.BackColor = GiaoDienChung.MenuDuocChon;
    }

    private void DonNoiDung()
    {
        while (pnlNoiDung.Controls.Count > 0)
        {
            Control control = pnlNoiDung.Controls[0];
            pnlNoiDung.Controls.RemoveAt(0);
            control.Dispose();
        }
        formDangMo = null;
    }

    public void MoFormCon(Form form)
    {
        DonNoiDung();
        formDangMo = form;
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        pnlNoiDung.Controls.Add(form);
        form.Show();
    }

    private void MoTrangChinh()
    {
        DonNoiDung();

        var trangChinh = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28),
            BackColor = GiaoDienChung.Nen
        };
        var tieuDe = new Label
        {
            Text = "Trang chính",
            Dock = DockStyle.Top,
            Height = 48,
            ForeColor = GiaoDienChung.Chu,
            Font = new Font("Segoe UI", 21F, FontStyle.Bold)
        };
        var huongDan = new Label
        {
            Text = "Chọn một chức năng từ menu bên trái để bắt đầu.",
            Dock = DockStyle.Top,
            Height = 42,
            ForeColor = GiaoDienChung.ChuPhu
        };
        var theChucNang = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(20),
            BackColor = GiaoDienChung.MatTrang
        };
        var tenChucNang = new Label
        {
            Text = "Danh mục loại thuốc",
            Dock = DockStyle.Top,
            Height = 30,
            ForeColor = GiaoDienChung.Chu,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };
        var moTa = new Label
        {
            Text = "Xem danh sách và tìm kiếm loại thuốc theo mã hoặc tên.",
            Dock = DockStyle.Fill,
            ForeColor = GiaoDienChung.ChuPhu
        };
        theChucNang.Controls.Add(moTa);
        theChucNang.Controls.Add(tenChucNang);
        trangChinh.Controls.Add(theChucNang);
        trangChinh.Controls.Add(huongDan);
        trangChinh.Controls.Add(tieuDe);
        pnlNoiDung.Controls.Add(trangChinh);
    }
}

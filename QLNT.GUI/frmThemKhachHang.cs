namespace QLNT.GUI;

public sealed class frmThemKhachHang : Form
{
    private readonly KhachHangBUS bus = new();
    private readonly TextBox txtHoTen = new() { Dock = DockStyle.Fill, MaxLength = 100 };
    private readonly TextBox txtSDT = new() { Dock = DockStyle.Fill, MaxLength = 15 };
    private readonly TextBox txtDiaChi = new() { Dock = DockStyle.Fill, MaxLength = 200 };

    public KhachHang? KhachHangMoi { get; private set; }

    public frmThemKhachHang()
    {
        Text = "Thêm khách hàng";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(480, 210);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 4,
            BackColor = GiaoDienChung.Nen
        };
        khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        khung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 3; i++)
            khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        ThemTruong(khung, "Họ tên:", txtHoTen, 0);
        ThemTruong(khung, "Số điện thoại:", txtSDT, 1);
        ThemTruong(khung, "Địa chỉ:", txtDiaChi, 2);

        var nut = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        var btnHuy = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
        var btnThem = new Button { Text = "Thêm khách hàng", AutoSize = true };
        btnThem.Click += (_, _) => Them();
        nut.Controls.AddRange([btnHuy, btnThem]);
        khung.Controls.Add(nut, 0, 3);
        khung.SetColumnSpan(nut, 2);
        Controls.Add(khung);
        AcceptButton = btnThem;
        CancelButton = btnHuy;
        Shown += (_, _) => txtHoTen.Focus();
    }

    private static void ThemTruong(TableLayoutPanel khung, string nhan, Control o, int hang)
    {
        khung.Controls.Add(new Label
        {
            Text = nhan,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, hang);
        khung.Controls.Add(o, 1, hang);
    }

    private void Them()
    {
        KetQuaLuuKhachHang ketQua = bus.Them(new KhachHang
        {
            HoTen = txtHoTen.Text,
            SDT = txtSDT.Text,
            DiaChi = txtDiaChi.Text
        });
        if (!ketQua.ThanhCong || ketQua.KhachHang == null)
        {
            MessageBox.Show(this, ketQua.ThongBao, "Không thể thêm khách hàng",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        KhachHangMoi = ketQua.KhachHang;
        DialogResult = DialogResult.OK;
        Close();
    }
}

namespace QLNT.GUI;

public sealed class frmChonKhachHang : Form
{
    private readonly KhachHangBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ListView lvKhachHang = new();
    public static KhachHang? KhachHangDuocChon { get; private set; }
    public bool ChonKhachLe { get; private set; }

    public frmChonKhachHang()
    {
        KhachHangDuocChon = null;
        Text = "Chọn khách hàng";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 360);
        Size = new Size(760, 470);

        var timKiem = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            Padding = new Padding(12),
            ColumnCount = 2
        };
        timKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        timKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        timKiem.Controls.Add(new Label { Text = "Tìm khách hàng:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        timKiem.Controls.Add(txtTimKiem, 1, 0);
        txtTimKiem.TextChanged += (_, _) => HienThiDanhSach();

        lvKhachHang.Dock = DockStyle.Fill;
        lvKhachHang.View = View.Details;
        lvKhachHang.FullRowSelect = true;
        lvKhachHang.MultiSelect = false;
        lvKhachHang.GridLines = true;
        lvKhachHang.Columns.Add("Mã khách hàng", 130);
        lvKhachHang.Columns.Add("Họ tên", 250);
        lvKhachHang.Columns.Add("Số điện thoại", 150);
        lvKhachHang.DoubleClick += (_, _) => Chon();

        var nut = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(12, 8, 0, 8),
            WrapContents = true
        };
        var btnChon = new Button { Text = "Chọn", AutoSize = true };
        var btnThem = new Button { Text = "Thêm khách hàng mới", AutoSize = true };
        var btnKhachLe = new Button { Text = "Khách lẻ", AutoSize = true };
        var btnHuy = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
        btnChon.Click += (_, _) => Chon();
        btnThem.Click += (_, _) => ThemKhachHang();
        btnKhachLe.Click += (_, _) =>
        {
            KhachHangDuocChon = null;
            ChonKhachLe = true;
            DialogResult = DialogResult.OK;
            Close();
        };
        nut.Controls.AddRange([btnChon, btnThem, btnKhachLe, btnHuy]);
        AcceptButton = btnChon;
        CancelButton = btnHuy;

        Controls.Add(lvKhachHang);
        Controls.Add(timKiem);
        Controls.Add(nut);
        Load += (_, _) => HienThiDanhSach();
        FormClosed += (_, _) =>
        {
            if (DialogResult != DialogResult.OK)
                KhachHangDuocChon = null;
        };
    }

    private void HienThiDanhSach()
    {
        try
        {
            lvKhachHang.BeginUpdate();
            try
            {
                lvKhachHang.Items.Clear();
                foreach (KhachHang khachHang in bus.TimKiem(txtTimKiem.Text))
                {
                    var dong = new ListViewItem(khachHang.MaKH) { Tag = khachHang };
                    dong.SubItems.Add(khachHang.HoTen);
                    dong.SubItems.Add(khachHang.SDT ?? string.Empty);
                    lvKhachHang.Items.Add(dong);
                }
            }
            finally
            {
                lvKhachHang.EndUpdate();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách khách hàng: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Chon()
    {
        if (lvKhachHang.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn một khách hàng hoặc chọn Khách lẻ.");
            return;
        }

        KhachHangDuocChon = (KhachHang)lvKhachHang.SelectedItems[0].Tag!;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ThemKhachHang()
    {
        using var form = new frmThemKhachHang();
        if (form.ShowDialog(this) != DialogResult.OK || form.KhachHangMoi == null)
            return;

        txtTimKiem.Clear();
        HienThiDanhSach();
        foreach (ListViewItem dong in lvKhachHang.Items)
        {
            if (dong.Tag is KhachHang khachHang &&
                khachHang.MaKH.Equals(form.KhachHangMoi.MaKH, StringComparison.OrdinalIgnoreCase))
            {
                dong.Selected = true;
                dong.Focused = true;
                dong.EnsureVisible();
                break;
            }
        }

        Chon();
    }
}

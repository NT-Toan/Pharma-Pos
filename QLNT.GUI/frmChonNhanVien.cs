namespace QLNT.GUI;

public sealed class frmChonNhanVien : Form
{
    private readonly NhanVienBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ListView lvNhanVien = new();
    public static NhanVien? NhanVienDuocChon { get; private set; }

    public frmChonNhanVien()
    {
        NhanVienDuocChon = null;
        Text = "Chọn nhân viên";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(520, 360);
        Size = new Size(700, 460);

        var timKiem = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            Padding = new Padding(12),
            ColumnCount = 2
        };
        timKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        timKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        timKiem.Controls.Add(new Label { Text = "Tìm nhân viên:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        timKiem.Controls.Add(txtTimKiem, 1, 0);
        txtTimKiem.TextChanged += (_, _) => HienThiDanhSach();

        lvNhanVien.Dock = DockStyle.Fill;
        lvNhanVien.View = View.Details;
        lvNhanVien.FullRowSelect = true;
        lvNhanVien.MultiSelect = false;
        lvNhanVien.GridLines = true;
        lvNhanVien.Columns.Add("Mã nhân viên", 130);
        lvNhanVien.Columns.Add("Họ tên", 240);
        lvNhanVien.Columns.Add("Chức vụ", 150);
        lvNhanVien.DoubleClick += (_, _) => Chon();

        var nut = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(12, 8, 0, 8) };
        var btnChon = new Button { Text = "Chọn", AutoSize = true };
        var btnHuy = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
        btnChon.Click += (_, _) => Chon();
        nut.Controls.AddRange([btnChon, btnHuy]);
        AcceptButton = btnChon;
        CancelButton = btnHuy;

        Controls.Add(lvNhanVien);
        Controls.Add(timKiem);
        Controls.Add(nut);
        Load += (_, _) => HienThiDanhSach();
        FormClosed += (_, _) =>
        {
            if (DialogResult != DialogResult.OK)
                NhanVienDuocChon = null;
        };
    }

    private void HienThiDanhSach()
    {
        try
        {
            lvNhanVien.BeginUpdate();
            try
            {
                lvNhanVien.Items.Clear();
                foreach (NhanVien nhanVien in bus.TimKiem(txtTimKiem.Text))
                {
                    var dong = new ListViewItem(nhanVien.MaNV) { Tag = nhanVien };
                    dong.SubItems.Add(nhanVien.HoTenNV);
                    dong.SubItems.Add(nhanVien.ChucVu);
                    lvNhanVien.Items.Add(dong);
                }
            }
            finally
            {
                lvNhanVien.EndUpdate();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách nhân viên: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Chon()
    {
        if (lvNhanVien.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn một nhân viên.");
            return;
        }

        NhanVienDuocChon = (NhanVien)lvNhanVien.SelectedItems[0].Tag!;
        DialogResult = DialogResult.OK;
        Close();
    }
}

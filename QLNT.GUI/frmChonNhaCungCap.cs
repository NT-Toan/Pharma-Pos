using System.Collections;

namespace QLNT.GUI;

public sealed class frmChonNhaCungCap : Form
{
    private readonly NhaCungCapBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ListView lvNhaCungCap = new();

    public NhaCungCap? NhaCungCapDuocChon { get; private set; }

    public frmChonNhaCungCap()
    {
        Text = "Chọn nhà cung cấp";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(600, 400);
        Size = new Size(800, 500);

        var thanhTimKiem = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            Padding = new Padding(12),
            ColumnCount = 2
        };
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        thanhTimKiem.Controls.Add(new Label
        {
            Text = "Tìm nhà cung cấp:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        lvNhaCungCap.Dock = DockStyle.Fill;
        lvNhaCungCap.View = View.Details;
        lvNhaCungCap.FullRowSelect = true;
        lvNhaCungCap.MultiSelect = false;
        lvNhaCungCap.GridLines = true;
        lvNhaCungCap.Columns.Add("Mã", 100);
        lvNhaCungCap.Columns.Add("Tên nhà cung cấp", 250);
        lvNhaCungCap.Columns.Add("Địa chỉ", 260);
        lvNhaCungCap.Columns.Add("SĐT", 130);
        lvNhaCungCap.DoubleClick += (_, _) => Chon();

        var thanhNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(12, 8, 0, 8)
        };
        var btnChon = new Button { Text = "Chọn", AutoSize = true };
        var btnHuy = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
        btnChon.Click += (_, _) => Chon();
        thanhNut.Controls.AddRange([btnChon, btnHuy]);
        AcceptButton = btnChon;
        CancelButton = btnHuy;

        Controls.Add(lvNhaCungCap);
        Controls.Add(thanhTimKiem);
        Controls.Add(thanhNut);
        Load += (_, _) =>
        {
            NhaCungCapBUS.TaiLai();
            TimKiem();
        };
    }

    private void TimKiem()
    {
        try
        {
            ArrayList danhSach = bus.TimKiem(txtTimKiem.Text);
            lvNhaCungCap.BeginUpdate();
            try
            {
                lvNhaCungCap.Items.Clear();
                foreach (NhaCungCap ncc in danhSach)
                {
                    var dong = new ListViewItem(ncc.MaNCC) { Tag = ncc };
                    dong.SubItems.Add(ncc.TenNCC);
                    dong.SubItems.Add(ncc.DiaChi ?? string.Empty);
                    dong.SubItems.Add(ncc.SDT ?? string.Empty);
                    lvNhaCungCap.Items.Add(dong);
                }
            }
            finally
            {
                lvNhaCungCap.EndUpdate();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách nhà cung cấp: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Chon()
    {
        if (lvNhaCungCap.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn một nhà cung cấp trong danh sách.");
            return;
        }

        NhaCungCapDuocChon = (NhaCungCap)lvNhaCungCap.SelectedItems[0].Tag!;
        DialogResult = DialogResult.OK;
        Close();
    }
}

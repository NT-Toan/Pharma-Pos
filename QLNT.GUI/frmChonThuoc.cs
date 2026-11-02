using System.Collections;

namespace QLNT.GUI;

public sealed class frmChonThuoc : Form
{
    private readonly ThuocBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ListView lvThuoc = new();

    public Thuoc? ThuocDuocChon { get; private set; }

    public frmChonThuoc()
    {
        Text = "Chọn thuốc";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(650, 400);
        Size = new Size(850, 520);

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
            Text = "Tìm thuốc:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        lvThuoc.Dock = DockStyle.Fill;
        lvThuoc.View = View.Details;
        lvThuoc.FullRowSelect = true;
        lvThuoc.MultiSelect = false;
        lvThuoc.GridLines = true;
        lvThuoc.Columns.Add("Mã", 95);
        lvThuoc.Columns.Add("Tên thuốc", 230);
        lvThuoc.Columns.Add("HSD", 100);
        lvThuoc.Columns.Add("Đơn giá", 125);
        lvThuoc.Columns.Add("Tồn kho", 85);
        lvThuoc.DoubleClick += (_, _) => Chon();

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

        Controls.Add(lvThuoc);
        Controls.Add(thanhTimKiem);
        Controls.Add(thanhNut);
        Load += (_, _) =>
        {
            ThuocBUS.TaiLai();
            TimKiem();
        };
    }

    private void TimKiem()
    {
        try
        {
            ArrayList danhSach = bus.TimKiem(txtTimKiem.Text);
            lvThuoc.BeginUpdate();
            try
            {
                lvThuoc.Items.Clear();
                foreach (Thuoc thuoc in danhSach)
                {
                    var dong = new ListViewItem(thuoc.MaThuoc) { Tag = thuoc };
                    dong.SubItems.Add(thuoc.TenThuoc);
                    dong.SubItems.Add(thuoc.HSD?.ToString("dd/MM/yyyy") ?? "");
                    dong.SubItems.Add(thuoc.DonGia.ToString("N2"));
                    dong.SubItems.Add(thuoc.SoLuong.ToString());
                    lvThuoc.Items.Add(dong);
                }
            }
            finally
            {
                lvThuoc.EndUpdate();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách thuốc: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Chon()
    {
        if (lvThuoc.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn một thuốc trong danh sách.");
            return;
        }

        ThuocDuocChon = (Thuoc)lvThuoc.SelectedItems[0].Tag!;
        DialogResult = DialogResult.OK;
        Close();
    }
}

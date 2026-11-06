using System.Collections;

namespace QLNT.GUI;

public sealed class frmChonThuoc : Form
{
    private readonly ThuocBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ListView lvThuoc = new();
    private readonly NumericUpDown nudSoLuong = new()
    {
        Minimum = 0,
        Maximum = int.MaxValue,
        Value = 0,
        Width = 90,
        ThousandsSeparator = true
    };
    private readonly Label lblTonKhaDung = new() { AutoSize = true };
    private readonly Button btnChon = new() { Text = "Chọn", AutoSize = true, Enabled = false };
    private readonly Func<Thuoc, int> laySoLuongToiDa;

    public Thuoc? ThuocDuocChon { get; private set; }
    public int SoLuongDuocChon { get; private set; }

    public frmChonThuoc(Func<Thuoc, int>? laySoLuongToiDa = null)
    {
        this.laySoLuongToiDa = laySoLuongToiDa ?? (thuoc => thuoc.SoLuong);
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
        lvThuoc.SelectedIndexChanged += (_, _) => CapNhatTonKhaDung();

        var thanhNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(12, 8, 0, 8)
        };
        var btnHuy = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
        btnChon.Click += (_, _) => Chon();
        thanhNut.Controls.AddRange([
            new Label { Text = "Số lượng:", AutoSize = true, Padding = new Padding(0, 7, 0, 0) },
            nudSoLuong,
            lblTonKhaDung,
            btnChon,
            btnHuy
        ]);
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
                lvThuoc.SelectedItems.Clear();
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
            if (lvThuoc.Items.Count > 0)
                lvThuoc.Items[0].Selected = true;
            CapNhatTonKhaDung();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách thuốc: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CapNhatTonKhaDung()
    {
        if (lvThuoc.SelectedItems.Count == 0 || lvThuoc.SelectedItems[0].Tag is not Thuoc thuoc)
        {
            // Khi đổi dòng, ListView tạm thời không có lựa chọn. Hạ Minimum trước khi gán 0.
            nudSoLuong.Minimum = 0;
            nudSoLuong.Value = 0;
            nudSoLuong.Enabled = false;
            btnChon.Enabled = false;
            lblTonKhaDung.Text = string.Empty;
            return;
        }

        int soLuongToiDa = Math.Clamp(laySoLuongToiDa(thuoc), 0, int.MaxValue);
        nudSoLuong.Enabled = soLuongToiDa > 0;
        nudSoLuong.Minimum = soLuongToiDa > 0 ? 1 : 0;
        nudSoLuong.Maximum = Math.Max(soLuongToiDa, 1);
        nudSoLuong.Value = soLuongToiDa > 0 ? 1 : 0;
        btnChon.Enabled = soLuongToiDa > 0;
        lblTonKhaDung.Text = soLuongToiDa > 0
            ? $"(Tối đa {soLuongToiDa:N0})"
            : "(Không đủ tồn kho)";
    }

    private void Chon()
    {
        if (lvThuoc.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn một thuốc trong danh sách.");
            return;
        }

        ThuocDuocChon = (Thuoc)lvThuoc.SelectedItems[0].Tag!;
        if (nudSoLuong.Value <= 0)
        {
            MessageBox.Show("Số lượng phải lớn hơn 0.", "Số lượng không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SoLuongDuocChon = decimal.ToInt32(nudSoLuong.Value);
        DialogResult = DialogResult.OK;
        Close();
    }
}

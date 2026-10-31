using System.Collections;

namespace QLNT.GUI;

public sealed class frmLoaiThuoc : Form
{
    private readonly LoaiThuocBUS bus = new();
    private readonly TextBox txtTimKiem = new();
    private readonly ListView lvLoaiThuoc = new();
    private readonly Label lblSoLuong = new();

    public frmLoaiThuoc()
    {
        Text = "Danh sách loại thuốc";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(560, 400);
        Size = new Size(760, 520);

        var thanhTimKiem = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(12),
            ColumnCount = 3,
            RowCount = 1
        };
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var nhanTimKiem = new Label
        {
            Text = "Tìm mã hoặc tên:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        txtTimKiem.Dock = DockStyle.Fill;
        txtTimKiem.PlaceholderText = "Nhập mã hoặc tên loại thuốc";
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        var btnTaiLai = new Button
        {
            Text = "Tải lại",
            AutoSize = true,
            Anchor = AnchorStyles.Right
        };
        btnTaiLai.Click += (_, _) => TaiLai();

        thanhTimKiem.Controls.Add(nhanTimKiem, 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        thanhTimKiem.Controls.Add(btnTaiLai, 2, 0);

        lvLoaiThuoc.Dock = DockStyle.Fill;
        lvLoaiThuoc.View = View.Details;
        lvLoaiThuoc.FullRowSelect = true;
        lvLoaiThuoc.GridLines = true;
        lvLoaiThuoc.HideSelection = false;
        lvLoaiThuoc.Columns.Add("Mã loại", 140);
        lvLoaiThuoc.Columns.Add("Tên loại thuốc", 540);

        lblSoLuong.Dock = DockStyle.Bottom;
        lblSoLuong.Height = 32;
        lblSoLuong.Padding = new Padding(12, 8, 0, 0);

        Controls.Add(lvLoaiThuoc);
        Controls.Add(thanhTimKiem);
        Controls.Add(lblSoLuong);
        Load += (_, _) => TaiLai();
    }

    private void TimKiem()
    {
        try
        {
            ArrayList danhSach = bus.TimKiem(txtTimKiem.Text);
            HienThi(danhSach);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tìm loại thuốc: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TaiLai()
    {
        try
        {
            LoaiThuocBUS.TaiLai();
            TimKiem();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách loại thuốc: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HienThi(ArrayList danhSach)
    {
        lvLoaiThuoc.BeginUpdate();
        try
        {
            lvLoaiThuoc.Items.Clear();
            foreach (LoaiThuoc loaiThuoc in danhSach)
            {
                lvLoaiThuoc.Items.Add(new ListViewItem(new[] { loaiThuoc.MaLoai, loaiThuoc.TenLoai }));
            }
        }
        finally
        {
            lvLoaiThuoc.EndUpdate();
        }

        lblSoLuong.Text = $"Số loại thuốc: {danhSach.Count}";
    }
}

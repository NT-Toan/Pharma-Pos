using System.Collections;

namespace QLNT.GUI;

public sealed class frmKhuyenMai : Form
{
    private readonly CTKhuyenMaiBUS bus = new();
    private readonly TextBox txtTimKiem = new();
    private readonly ListView lvKhuyenMai = new();
    private readonly Label lblSoLuong = new();

    public frmKhuyenMai()
    {
        Text = "Quản lý chương trình khuyến mãi";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(600, 420);
        Size = new Size(820, 540);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

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
            Text = "Tìm kiếm:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = GiaoDienChung.Chu
        };
        txtTimKiem.Dock = DockStyle.Fill;
        txtTimKiem.PlaceholderText = "Nhập mã hoặc tên chương trình khuyến mãi...";
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        var btnTaiLai = GiaoDienChung.TaoNutPhu("Tải lại");
        btnTaiLai.Anchor = AnchorStyles.Right;
        btnTaiLai.Click += (_, _) => TaiLai();

        thanhTimKiem.Controls.Add(nhanTimKiem, 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        thanhTimKiem.Controls.Add(btnTaiLai, 2, 0);

        lvKhuyenMai.Dock = DockStyle.Fill;
        lvKhuyenMai.View = View.Details;
        lvKhuyenMai.FullRowSelect = true;
        lvKhuyenMai.GridLines = true;
        lvKhuyenMai.HideSelection = false;
        lvKhuyenMai.Columns.Add("Mã CTKM", 120);
        lvKhuyenMai.Columns.Add("Tên chương trình khuyến mãi", 320);
        lvKhuyenMai.Columns.Add("Ngày bắt đầu", 160);
        lvKhuyenMai.Columns.Add("Ngày kết thúc", 160);

        lblSoLuong.Dock = DockStyle.Bottom;
        lblSoLuong.Height = 32;
        lblSoLuong.Padding = new Padding(12, 8, 0, 0);
        lblSoLuong.ForeColor = GiaoDienChung.ChuPhu;

        Controls.Add(lvKhuyenMai);
        Controls.Add(thanhTimKiem);
        Controls.Add(lblSoLuong);

        Load += (_, _) => TaiLai();
    }

    private void TimKiem()
    {
        try
        {
            string tuKhoa = txtTimKiem.Text.Trim();
            ArrayList tatCa = bus.LayTatCa();
            ArrayList ketQua = new ArrayList();

            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                ketQua = tatCa;
            }
            else
            {
                foreach (CTKhuyenMai km in tatCa)
                {
                    if (km.MaCTKM.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                        km.TenCTKM.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase))
                    {
                        ketQua.Add(km);
                    }
                }
            }

            HienThi(ketQua);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tìm khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TaiLai()
    {
        try
        {
            CTKhuyenMaiBUS.TaiLai();
            TimKiem();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HienThi(ArrayList danhSach)
    {
        lvKhuyenMai.BeginUpdate();
        try
        {
            lvKhuyenMai.Items.Clear();
            foreach (CTKhuyenMai km in danhSach)
            {
                string ngayBD = km.NgayBatDau.HasValue ? km.NgayBatDau.Value.ToString("dd/MM/yyyy") : "-";
                string ngayKT = km.NgayKetThuc.HasValue ? km.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "-";

                var dong = new ListViewItem(new[]
                {
                    km.MaCTKM,
                    km.TenCTKM,
                    ngayBD,
                    ngayKT
                });

                lvKhuyenMai.Items.Add(dong);
            }
        }
        finally
        {
            lvKhuyenMai.EndUpdate();
        }

        lblSoLuong.Text = $"Số chương trình khuyến mãi: {danhSach.Count}";
    }
}
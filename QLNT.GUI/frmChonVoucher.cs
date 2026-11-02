using System.Collections;

namespace QLNT.GUI;

public sealed class frmChonVoucher : Form
{
    // Biến static lưu voucher được chọn theo quy ước nhóm (Mục 8, trang 7-8)
    public static KMVoucher? DaChon;

    private readonly KMVoucherBUS bus = new();
    private readonly TextBox txtTimKiem = new();
    private readonly ListView lvVoucher = new();
    private readonly Label lblSoLuong = new();

    public frmChonVoucher()
    {
        Text = "Chọn Voucher khuyến mãi";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Size = new Size(700, 480);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        // Thanh tìm kiếm phía trên
        var thanhTimKiem = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(12, 6, 12, 6),
            ColumnCount = 2,
            RowCount = 1
        };
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var nhanTimKiem = new Label
        {
            Text = "Tìm voucher:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = GiaoDienChung.Chu
        };
        txtTimKiem.Dock = DockStyle.Fill;
        txtTimKiem.PlaceholderText = "Nhập mã voucher hoặc mã CTKM để tìm...";
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        thanhTimKiem.Controls.Add(nhanTimKiem, 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);

        // Danh sách voucher
        lvVoucher.Dock = DockStyle.Fill;
        lvVoucher.View = View.Details;
        lvVoucher.FullRowSelect = true;
        lvVoucher.GridLines = true;
        lvVoucher.HideSelection = false;
        lvVoucher.Columns.Add("Mã Voucher", 140);
        lvVoucher.Columns.Add("Mã CTKM", 120);
        lvVoucher.Columns.Add("Khách hàng áp dụng", 180);
        lvVoucher.Columns.Add("Giảm giá (%)", 120);

        // Double click vào dòng để chọn nhanh
        lvVoucher.DoubleClick += (_, _) => ChonVaDong();

        // Thanh dưới: số lượng và nút Chọn / Hủy
        var thanhDuoi = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(12, 8, 12, 8),
            BackColor = GiaoDienChung.Nen
        };

        lblSoLuong.Dock = DockStyle.Left;
        lblSoLuong.AutoSize = true;
        lblSoLuong.ForeColor = GiaoDienChung.ChuPhu;
        lblSoLuong.Padding = new Padding(0, 8, 0, 0);

        var pnlNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnChon = GiaoDienChung.TaoNutPhu("Chọn");
        btnChon.BackColor = GiaoDienChung.XanhChinh;
        btnChon.ForeColor = Color.White;
        btnChon.Click += (_, _) => ChonVaDong();

        var btnHuy = GiaoDienChung.TaoNutPhu("Hủy");
        btnHuy.Click += (_, _) =>
        {
            DaChon = null;
            Close();
        };

        pnlNut.Controls.Add(btnChon);
        pnlNut.Controls.Add(btnHuy);
        thanhDuoi.Controls.Add(lblSoLuong);
        thanhDuoi.Controls.Add(pnlNut);

        Controls.Add(lvVoucher);
        Controls.Add(thanhTimKiem);
        Controls.Add(thanhDuoi);

        Load += (_, _) => TaiDanhSach();
    }

    private void TaiDanhSach()
    {
        try
        {
            DaChon = null;
            TimKiem();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách voucher: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TimKiem()
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
            foreach (KMVoucher vc in tatCa)
            {
                if (vc.MaVoucher.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                    vc.MaCTKM.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase))
                {
                    ketQua.Add(vc);
                }
            }
        }

        HienThi(ketQua);
    }

    private void HienThi(ArrayList danhSach)
    {
        lvVoucher.BeginUpdate();
        try
        {
            lvVoucher.Items.Clear();
            foreach (KMVoucher vc in danhSach)
            {
                string kh = string.IsNullOrEmpty(vc.MaKH) ? "(Tất cả khách lẻ)" : vc.MaKH;
                var dong = new ListViewItem(new[]
                {
                    vc.MaVoucher,
                    vc.MaCTKM,
                    kh,
                    $"{vc.PhanTramGiam:0.##}%"
                })
                {
                    Tag = vc
                };

                lvVoucher.Items.Add(dong);
            }
        }
        finally
        {
            lvVoucher.EndUpdate();
        }

        lblSoLuong.Text = $"Số voucher hiện có: {danhSach.Count}";
    }

    private void ChonVaDong()
    {
        if (lvVoucher.SelectedItems.Count > 0)
        {
            DaChon = lvVoucher.SelectedItems[0].Tag as KMVoucher;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show("Vui lòng chọn một voucher từ danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
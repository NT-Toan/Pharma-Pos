using System.Collections;

namespace QLNT.GUI;

public sealed class frmKhuyenMai : Form
{
    private readonly CTKhuyenMaiBUS bus = new();
    private readonly TextBox txtMa = new();
    private readonly TextBox txtTen = new();
    private readonly DateTimePicker dtpBatDau = new();
    private readonly DateTimePicker dtpKetThuc = new();
    private readonly TextBox txtTimKiem = new();
    private readonly ListView lvKhuyenMai = new();
    private readonly Label lblSoLuong = new();

    public frmKhuyenMai()
    {
        Text = "Quản lý chương trình khuyến mãi";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(880, 580);
        Size = new Size(950, 640);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        // Vùng trên cùng chứa toàn bộ nhập liệu và nút bấm
        var pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(12)
        };

        var grpNhapLieu = new GroupBox
        {
            Text = "Thông tin chương trình khuyến mãi",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(16, 12, 16, 16),
            ForeColor = GiaoDienChung.Chu
        };

        // Bảng nhập thông tin: 2 dòng
        var tblInput = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 2,
            Padding = new Padding(0, 0, 0, 10)
        };
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        txtMa.ReadOnly = true;
        txtMa.PlaceholderText = "(Tự động sinh)";
        txtMa.Dock = DockStyle.Fill;

        txtTen.Dock = DockStyle.Fill;
        dtpBatDau.Format = DateTimePickerFormat.Short;
        dtpBatDau.Dock = DockStyle.Fill;
        dtpKetThuc.Format = DateTimePickerFormat.Short;
        dtpKetThuc.Dock = DockStyle.Fill;

        tblInput.Controls.Add(new Label { Text = "Mã CTKM:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 0, 0);
        tblInput.Controls.Add(txtMa, 1, 0);
        tblInput.Controls.Add(new Label { Text = "Tên CTKM:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 2, 0);
        tblInput.Controls.Add(txtTen, 3, 0);

        tblInput.Controls.Add(new Label { Text = "Ngày bắt đầu:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 0, 1);
        tblInput.Controls.Add(dtpBatDau, 1, 1);
        tblInput.Controls.Add(new Label { Text = "Ngày kết thúc:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 2, 1);
        tblInput.Controls.Add(dtpKetThuc, 3, 1);

        // Hàng nút thao tác tách riêng, căn phải
        var pnlNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 4, 0, 0)
        };

        var btnLamMoi = GiaoDienChung.TaoNutPhu("Làm trống");
        btnLamMoi.Click += (_, _) => LamTrong();

        var btnXoa = GiaoDienChung.TaoNutPhu("Xóa");
        btnXoa.Click += (_, _) => XuLyXoa();

        var btnSua = GiaoDienChung.TaoNutPhu("Cập nhật");
        btnSua.Click += (_, _) => XuLySua();

        var btnThem = GiaoDienChung.TaoNutPhu("Thêm mới");
        btnThem.BackColor = GiaoDienChung.XanhChinh;
        btnThem.ForeColor = Color.White;
        btnThem.Click += (_, _) => XuLyThem();

      var btnChiTiet = GiaoDienChung.TaoNutPhu("Chi tiết giảm giá...");
        btnChiTiet.Click += (_, _) => MoChiTietGiamGia();

        pnlNut.Controls.Add(btnChiTiet);
        pnlNut.Controls.Add(btnLamMoi);
        pnlNut.Controls.Add(btnXoa);
        pnlNut.Controls.Add(btnSua);
        pnlNut.Controls.Add(btnThem);

        grpNhapLieu.Controls.Add(pnlNut);
        grpNhapLieu.Controls.Add(tblInput);
        pnlTop.Controls.Add(grpNhapLieu);

        // Thanh tìm kiếm
        var thanhTimKiem = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(12, 4, 12, 4),
            ColumnCount = 3,
            RowCount = 1
        };
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        txtTimKiem.Dock = DockStyle.Fill;
        txtTimKiem.PlaceholderText = "Nhập mã hoặc tên chương trình khuyến mãi để tìm...";
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        var btnTaiLai = GiaoDienChung.TaoNutPhu("Tải lại");
        btnTaiLai.Click += (_, _) => TaiLai();

        thanhTimKiem.Controls.Add(new Label { Text = "Tìm kiếm:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        thanhTimKiem.Controls.Add(btnTaiLai, 2, 0);

        // Danh sách
        lvKhuyenMai.Dock = DockStyle.Fill;
        lvKhuyenMai.View = View.Details;
        lvKhuyenMai.FullRowSelect = true;
        lvKhuyenMai.GridLines = true;
        lvKhuyenMai.HideSelection = false;
        lvKhuyenMai.Columns.Add("Mã CTKM", 120);
        lvKhuyenMai.Columns.Add("Tên chương trình khuyến mãi", 320);
        lvKhuyenMai.Columns.Add("Ngày bắt đầu", 160);
        lvKhuyenMai.Columns.Add("Ngày kết thúc", 160);
        lvKhuyenMai.SelectedIndexChanged += (_, _) => ChonDong();

        lblSoLuong.Dock = DockStyle.Bottom;
        lblSoLuong.Height = 32;
        lblSoLuong.Padding = new Padding(12, 8, 0, 0);
        lblSoLuong.ForeColor = GiaoDienChung.ChuPhu;

        Controls.Add(lvKhuyenMai);
        Controls.Add(thanhTimKiem);
        Controls.Add(pnlTop);
        Controls.Add(lblSoLuong);

        Load += (_, _) => TaiLai();
    }

    private void ChonDong()
    {
        if (lvKhuyenMai.SelectedItems.Count == 0) return;
        var dong = lvKhuyenMai.SelectedItems[0];
        if (dong.Tag is CTKhuyenMai km)
        {
            txtMa.Text = km.MaCTKM;
            txtTen.Text = km.TenCTKM;
            if (km.NgayBatDau.HasValue) dtpBatDau.Value = km.NgayBatDau.Value;
            if (km.NgayKetThuc.HasValue) dtpKetThuc.Value = km.NgayKetThuc.Value;
        }
    }

    private void LamTrong()
    {
        txtMa.Text = string.Empty;
        txtTen.Text = string.Empty;
        dtpBatDau.Value = DateTime.Today;
        dtpKetThuc.Value = DateTime.Today.AddDays(30);
        lvKhuyenMai.SelectedIndices.Clear();
    }

    private void XuLyThem()
    {
        var km = new CTKhuyenMai
        {
            MaCTKM = string.Empty,
            TenCTKM = txtTen.Text.Trim(),
            NgayBatDau = dtpBatDau.Value.Date,
            NgayKetThuc = dtpKetThuc.Value.Date
        };
        var kq = bus.Them(km);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrong();
            TaiLai();
        }
    }

    private void XuLySua()
    {
        if (string.IsNullOrWhiteSpace(txtMa.Text))
        {
            MessageBox.Show("Vui lòng chọn một chương trình khuyến mãi từ danh sách để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var km = new CTKhuyenMai
        {
            MaCTKM = txtMa.Text.Trim(),
            TenCTKM = txtTen.Text.Trim(),
            NgayBatDau = dtpBatDau.Value.Date,
            NgayKetThuc = dtpKetThuc.Value.Date
        };
        var kq = bus.Sua(km);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            TaiLai();
        }
    }

    private void XuLyXoa()
    {
        if (string.IsNullOrWhiteSpace(txtMa.Text))
        {
            MessageBox.Show("Vui lòng chọn một chương trình khuyến mãi để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa chương trình '{txtTen.Text}' cùng các thiết lập giảm giá liên quan?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (xacNhan != DialogResult.Yes) return;

        var kq = bus.Xoa(txtMa.Text.Trim());
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrong();
            TaiLai();
        }
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
                })
                {
                    Tag = km
                };

                lvKhuyenMai.Items.Add(dong);
            }
        }
        finally
        {
            lvKhuyenMai.EndUpdate();
        }

        lblSoLuong.Text = $"Số chương trình khuyến mãi: {danhSach.Count}";
    }
    private void MoChiTietGiamGia()
    {
        if (lvKhuyenMai.SelectedItems.Count == 0 || lvKhuyenMai.SelectedItems[0].Tag is not CTKhuyenMai km)
        {
            MessageBox.Show("Vui lòng chọn một chương trình khuyến mãi từ danh sách để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var frm = new frmChiTietKhuyenMai(km);
        frm.ShowDialog(this);
    }
}
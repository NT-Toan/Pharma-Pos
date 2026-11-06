using System.Collections;

namespace QLNT.GUI;

public sealed class frmLoaiThuoc : Form
{
    private readonly LoaiThuocBUS bus = new();
    private readonly TextBox txtMaLoai = new();
    private readonly TextBox txtTenLoai = new();
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

        var thanhNhap = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            Padding = new Padding(12, 6, 12, 6),
            ColumnCount = 4,
            RowCount = 1
        };
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        txtMaLoai.ReadOnly = true;
        txtMaLoai.Dock = DockStyle.Fill;
        txtTenLoai.Dock = DockStyle.Fill;

        thanhNhap.Controls.Add(
            new Label { Text = "Mã loại:", AutoSize = true, Anchor = AnchorStyles.Left },
            0, 0);
        thanhNhap.Controls.Add(txtMaLoai, 1, 0);
        thanhNhap.Controls.Add(
            new Label { Text = "Tên loại:", AutoSize = true, Anchor = AnchorStyles.Left },
            2, 0);
        thanhNhap.Controls.Add(txtTenLoai, 3, 0);

        var thanhNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(12, 6, 0, 6),
            WrapContents = false
        };

        var btnMoi = new Button { Text = "Mới", AutoSize = true };
        var btnThem = new Button { Text = "Thêm", AutoSize = true };
        var btnSua = new Button { Text = "Sửa", AutoSize = true };
        var btnXoa = new Button { Text = "Xóa", AutoSize = true };

        btnMoi.Click += (_, _) => ChuanBiThem();
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();

        thanhNut.Controls.Add(btnMoi);
        thanhNut.Controls.Add(btnThem);
        thanhNut.Controls.Add(btnSua);
        thanhNut.Controls.Add(btnXoa);

        lvLoaiThuoc.Dock = DockStyle.Fill;
        lvLoaiThuoc.View = View.Details;
        lvLoaiThuoc.FullRowSelect = true;
        lvLoaiThuoc.GridLines = true;
        lvLoaiThuoc.HideSelection = false;
        lvLoaiThuoc.Columns.Add("Mã loại", 140);
        lvLoaiThuoc.Columns.Add("Tên loại thuốc", 540);

        lvLoaiThuoc.SelectedIndexChanged += (_, _) =>
        {
            if (lvLoaiThuoc.SelectedItems.Count == 0)
                return;

            ListViewItem dong = lvLoaiThuoc.SelectedItems[0];
            txtMaLoai.Text = dong.SubItems[0].Text;
            txtTenLoai.Text = dong.SubItems[1].Text;
        };

        lblSoLuong.Dock = DockStyle.Bottom;
        lblSoLuong.Height = 32;
        lblSoLuong.Padding = new Padding(12, 8, 0, 0);

        Controls.Add(lvLoaiThuoc);
        Controls.Add(thanhNut);
        Controls.Add(thanhNhap);
        Controls.Add(thanhTimKiem);
        Controls.Add(lblSoLuong);

        Load += (_, _) => TaiLai();
    }

    private void ChuanBiThem()
    {
        try
        {
            lvLoaiThuoc.SelectedItems.Clear();
            txtTenLoai.Clear();
            txtMaLoai.Text = bus.TaoMaMoi();
            txtTenLoai.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể tạo mã loại thuốc: {ex.Message}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void Them()
    {
        if (lvLoaiThuoc.SelectedItems.Count > 0)
        {
            MessageBox.Show("Hãy bấm Mới trước khi thêm loại thuốc.");
            return;
        }

        try
        {
            var loaiThuoc = new LoaiThuoc(txtMaLoai.Text, txtTenLoai.Text);
            KetQua ketQua = bus.Them(loaiThuoc);
            HienThiKetQua(ketQua);
        }
        catch (Exception ex)
        {
            HienThiLoi(ex);
        }
    }

    private void Sua()
    {
        if (lvLoaiThuoc.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn loại thuốc cần sửa trong danh sách.");
            return;
        }

        try
        {
            string maDangChon = lvLoaiThuoc.SelectedItems[0].SubItems[0].Text;
            var loaiThuoc = new LoaiThuoc(maDangChon, txtTenLoai.Text);

            KetQua ketQua = bus.Sua(loaiThuoc);
            HienThiKetQua(ketQua);
        }
        catch (Exception ex)
        {
            HienThiLoi(ex);
        }
    }

    private void Xoa()
    {
        if (lvLoaiThuoc.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn loại thuốc cần xóa trong danh sách.");
            return;
        }

        string maDangChon = lvLoaiThuoc.SelectedItems[0].SubItems[0].Text;
        DialogResult xacNhan = MessageBox.Show(
            $"Bạn có chắc muốn xóa loại thuốc {maDangChon}?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (xacNhan != DialogResult.Yes)
            return;

        try
        {
            KetQua ketQua = bus.Xoa(maDangChon);
            HienThiKetQua(ketQua);
        }
        catch (Exception ex)
        {
            HienThiLoi(ex);
        }
    }

    private void HienThiKetQua(KetQua ketQua)
    {
        MessageBox.Show(
            ketQua.ThongBao,
            ketQua.ThanhCong ? "Thành công" : "Không thành công",
            MessageBoxButtons.OK,
            ketQua.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

        if (ketQua.ThanhCong)
        {
            txtTimKiem.Clear();
            TaiLai();
        }
    }

    private static void HienThiLoi(Exception ex)
    {
        MessageBox.Show(
            $"Có lỗi khi thao tác với loại thuốc: {ex.Message}",
            "Lỗi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private void TimKiem()
    {
        try
        {
            ArrayList danhSach = bus.TimKiem(txtTimKiem.Text);
            HienThi(danhSach);

            txtTenLoai.Clear();
            txtMaLoai.Text = bus.TaoMaMoi();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể tìm loại thuốc: {ex.Message}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
            MessageBox.Show(
                $"Không thể tải danh sách loại thuốc: {ex.Message}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
                lvLoaiThuoc.Items.Add(
                    new ListViewItem(new[] { loaiThuoc.MaLoai, loaiThuoc.TenLoai }));
            }
        }
        finally
        {
            lvLoaiThuoc.EndUpdate();
        }

        lblSoLuong.Text = $"Số loại thuốc: {danhSach.Count}";
    }
}
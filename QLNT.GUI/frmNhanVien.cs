using System.Collections;

namespace QLNT.GUI;

public sealed class frmNhanVien : Form
{
    private readonly NhanVienBUS bus = new();

    private readonly TextBox txtMa = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox txtTen = new() { MaxLength = 100, Dock = DockStyle.Fill };
    private readonly TextBox txtChucVu = new() { MaxLength = 50, Dock = DockStyle.Fill };
    private readonly TextBox txtLuong = new() { Dock = DockStyle.Fill };
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly TextBox txtLuongTu = new() { Width = 120 };
private readonly TextBox txtLuongDen = new() { Width = 120 };
    private readonly ListView lvNhanVien = new();
    private readonly Label lblSoLuong = new();

    public frmNhanVien()
    {
        Text = "Quản lý nhân viên";
        MinimumSize = new Size(720, 480);
        Size = new Size(950, 600);
        BackColor = GiaoDienChung.Nen;
        Font = GiaoDienChung.PhongChu;

        var giaoDien = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 5
        };
        giaoDien.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        var thanhTim = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3
        };
        thanhTim.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTim.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        thanhTim.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTim.Controls.Add(TaoNhan("Tìm mã, tên hoặc chức vụ:"), 0, 0);
        thanhTim.Controls.Add(txtTimKiem, 1, 0);

        var btnTaiLai = GiaoDienChung.TaoNutPhu("Tải lại");
        btnTaiLai.Click += (_, _) => TaiLaiDanhSach();
        thanhTim.Controls.Add(btnTaiLai, 2, 0);
        txtTimKiem.TextChanged += (_, _) => TimKiem();
        var thanhLocLuong = new FlowLayoutPanel
{
    Dock = DockStyle.Fill,
    WrapContents = false
};

thanhLocLuong.Controls.Add(new Label
{
    Text = "Lương từ:",
    AutoSize = true,
    Padding = new Padding(0, 8, 0, 0)
});
thanhLocLuong.Controls.Add(txtLuongTu);

thanhLocLuong.Controls.Add(new Label
{
    Text = "Đến:",
    AutoSize = true,
    Padding = new Padding(12, 8, 0, 0)
});
thanhLocLuong.Controls.Add(txtLuongDen);

var btnLoc = GiaoDienChung.TaoNutPhu("Lọc");
btnLoc.Click += (_, _) => LocLuong();
thanhLocLuong.Controls.Add(btnLoc);

thanhTim.Controls.Add(thanhLocLuong, 0, 1);
thanhTim.SetColumnSpan(thanhLocLuong, 3);

        var bangNhap = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2
        };
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bangNhap.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        bangNhap.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        ThemO(bangNhap, "Mã NV:", txtMa, 0, 0);
        ThemO(bangNhap, "Họ tên:", txtTen, 2, 0);
        ThemO(bangNhap, "Chức vụ:", txtChucVu, 0, 1);
        ThemO(bangNhap, "Lương:", txtLuong, 2, 1);
        txtLuong.PlaceholderText = "Có thể để trống";

        var thanhNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnMoi = GiaoDienChung.TaoNutPhu("Mới");
        var btnThem = GiaoDienChung.TaoNutPhu("Thêm");
        var btnSua = GiaoDienChung.TaoNutPhu("Sửa");
        var btnXoa = GiaoDienChung.TaoNutPhu("Xóa");

        btnMoi.Click += (_, _) => ChuanBiThem();
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();

        thanhNut.Controls.AddRange([btnMoi, btnThem, btnSua, btnXoa]);

        lvNhanVien.Dock = DockStyle.Fill;
        lvNhanVien.View = View.Details;
        lvNhanVien.FullRowSelect = true;
        lvNhanVien.GridLines = true;
        lvNhanVien.MultiSelect = false;
        lvNhanVien.HideSelection = false;
        lvNhanVien.Columns.Add("Mã NV", 120);
        lvNhanVien.Columns.Add("Họ tên", 300);
        lvNhanVien.Columns.Add("Chức vụ", 220);
        lvNhanVien.Columns.Add("Lương", 160);
        lvNhanVien.SelectedIndexChanged += (_, _) => ChonDong();

        lblSoLuong.Dock = DockStyle.Fill;
        lblSoLuong.ForeColor = GiaoDienChung.ChuPhu;

        giaoDien.Controls.Add(thanhTim, 0, 0);
        giaoDien.Controls.Add(bangNhap, 0, 1);
        giaoDien.Controls.Add(thanhNut, 0, 2);
        giaoDien.Controls.Add(lvNhanVien, 0, 3);
        giaoDien.Controls.Add(lblSoLuong, 0, 4);
        Controls.Add(giaoDien);

        Load += (_, _) =>
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        };
    }

    private static Label TaoNhan(string noiDung) => new()
    {
        Text = noiDung,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        ForeColor = GiaoDienChung.Chu
    };

    private static void ThemO(
        TableLayoutPanel bang, string nhan, Control o, int cot, int hang)
    {
        bang.Controls.Add(TaoNhan(nhan), cot, hang);
        bang.Controls.Add(o, cot + 1, hang);
    }

    private void HienThi(ArrayList danhSach)
    {
        lvNhanVien.BeginUpdate();
        lvNhanVien.Items.Clear();

        foreach (NhanVien nv in danhSach)
        {
            var dong = new ListViewItem(nv.MaNV) { Tag = nv };
            dong.SubItems.Add(nv.HoTenNV);
            dong.SubItems.Add(nv.ChucVu);
            dong.SubItems.Add(nv.Luong?.ToString("N2") ?? "");
            lvNhanVien.Items.Add(dong);
        }

        lvNhanVien.EndUpdate();
        lblSoLuong.Text = $"Có {danhSach.Count} nhân viên";
    }

    private void TaiLaiDanhSach()
    {
        try
        {
            NhanVienBUS.TaiLai();
            HienThi(bus.TimKiem(txtTimKiem.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải nhân viên");
        }
    }
    private void LocLuong()
{
    var dieuKien = new List<DieuKien>();

    if (!string.IsNullOrWhiteSpace(txtLuongTu.Text))
    {
        if (!decimal.TryParse(txtLuongTu.Text, out decimal tu))
        {
            MessageBox.Show("Lương từ phải là số.");
            return;
        }

        dieuKien.Add(new DieuKien
        {
            TenTruong = "Luong",
            Phep = PhepSoSanh.LonHoacBang,
            GiaTri = tu
        });
    }

    if (!string.IsNullOrWhiteSpace(txtLuongDen.Text))
    {
        if (!decimal.TryParse(txtLuongDen.Text, out decimal den))
        {
            MessageBox.Show("Lương đến phải là số.");
            return;
        }

        dieuKien.Add(new DieuKien
        {
            TenTruong = "Luong",
            Phep = PhepSoSanh.NhoHoacBang,
            GiaTri = den
        });
    }

    try
    {
        HienThi(bus.TimKiemNangCao(dieuKien, PhepNoi.And));
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message, "Lỗi lọc nhân viên");
    }
}

    private void TimKiem()
    {
        try
        {
            HienThi(bus.TimKiem(txtTimKiem.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tìm kiếm");
        }
    }

    private void ChonDong()
    {
        if (lvNhanVien.SelectedItems.Count == 0)
            return;

        var nv = (NhanVien)lvNhanVien.SelectedItems[0].Tag!;
        txtMa.Text = nv.MaNV;
        txtTen.Text = nv.HoTenNV;
        txtChucVu.Text = nv.ChucVu;
        txtLuong.Text = nv.Luong?.ToString() ?? "";
    }

    private void ChuanBiThem()
    {
        try
        {
            lvNhanVien.SelectedItems.Clear();
            txtMa.Text = bus.TaoMaMoi();
            txtTen.Clear();
            txtChucVu.Clear();
            txtLuong.Clear();
            txtTen.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tạo mã nhân viên");
        }
    }

    private NhanVien? DocDuLieu()
    {
        decimal? luong = null;

        if (!string.IsNullOrWhiteSpace(txtLuong.Text))
        {
            if (!decimal.TryParse(txtLuong.Text, out decimal soLuong))
            {
                MessageBox.Show("Lương phải là một số hợp lệ.");
                txtLuong.Focus();
                return null;
            }

            luong = soLuong;
        }

        return new NhanVien
        {
            MaNV = txtMa.Text,
            HoTenNV = txtTen.Text,
            ChucVu = txtChucVu.Text,
            Luong = luong
        };
    }

    private void Them()
    {
        NhanVien? nv = DocDuLieu();
        if (nv == null) return;

        KetQua ketQua = bus.Them(nv);
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        }
    }

    private void Sua()
    {
        if (lvNhanVien.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn nhân viên cần sửa.");
            return;
        }

        NhanVien? nv = DocDuLieu();
        if (nv == null) return;

        KetQua ketQua = bus.Sua(nv);
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
            TaiLaiDanhSach();
    }

    private void Xoa()
    {
        if (lvNhanVien.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn nhân viên cần xóa.");
            return;
        }

        string maNV = txtMa.Text;
        if (MessageBox.Show(
                $"Xóa nhân viên {maNV}?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        KetQua ketQua = bus.Xoa(maNV);
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        }
    }
}
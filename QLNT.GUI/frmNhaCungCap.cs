using System.Collections;

namespace QLNT.GUI;

public sealed class frmNhaCungCap : Form
{
    private readonly NhaCungCapBUS bus = new();
    private readonly TextBox txtMa = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly TextBox txtTen = new() { Dock = DockStyle.Fill, MaxLength = 100 };
    private readonly TextBox txtDiaChi = new() { Dock = DockStyle.Fill, MaxLength = 200 };
    private readonly TextBox txtSDT = new() { Dock = DockStyle.Fill, MaxLength = 15 };
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ListView lvNhaCungCap = new();
    private readonly Label lblSoLuong = new();

    public frmNhaCungCap()
    {
        Text = "Danh sách nhà cung cấp";
        MinimumSize = new Size(700, 450);
        Size = new Size(950, 600);

        var thanhTimKiem = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            Padding = new Padding(12),
            ColumnCount = 3
        };
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.Controls.Add(Nhan("Tìm mã, tên, địa chỉ hoặc SĐT:"), 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        var btnTaiLai = new Button { Text = "Tải lại", AutoSize = true };
        btnTaiLai.Click += (_, _) => TaiLai();
        thanhTimKiem.Controls.Add(btnTaiLai, 2, 0);
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        var thanhNhap = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 92,
            Padding = new Padding(12, 6, 12, 6),
            ColumnCount = 4,
            RowCount = 2
        };
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        thanhNhap.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        thanhNhap.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        ThemO(thanhNhap, "Mã NCC:", txtMa, 0, 0);
        ThemO(thanhNhap, "Tên NCC:", txtTen, 2, 0);
        ThemO(thanhNhap, "Địa chỉ:", txtDiaChi, 0, 1);
        ThemO(thanhNhap, "SĐT:", txtSDT, 2, 1);

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
        btnMoi.Click += (_, _) => ChuanBiThem(true);
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();
        thanhNut.Controls.AddRange([btnMoi, btnThem, btnSua, btnXoa]);

        lvNhaCungCap.Dock = DockStyle.Fill;
        lvNhaCungCap.View = View.Details;
        lvNhaCungCap.FullRowSelect = true;
        lvNhaCungCap.GridLines = true;
        lvNhaCungCap.HideSelection = false;
        lvNhaCungCap.MultiSelect = false;
        lvNhaCungCap.Columns.Add("Mã NCC", 110);
        lvNhaCungCap.Columns.Add("Tên nhà cung cấp", 260);
        lvNhaCungCap.Columns.Add("Địa chỉ", 310);
        lvNhaCungCap.Columns.Add("SĐT", 130);
        lvNhaCungCap.SelectedIndexChanged += (_, _) => ChonNhaCungCap();

        lblSoLuong.Dock = DockStyle.Bottom;
        lblSoLuong.Height = 32;
        lblSoLuong.Padding = new Padding(12, 8, 0, 0);

        Controls.Add(lvNhaCungCap);
        Controls.Add(thanhNut);
        Controls.Add(thanhNhap);
        Controls.Add(thanhTimKiem);
        Controls.Add(lblSoLuong);
        Load += (_, _) => TaiLai();
    }

    private static Label Nhan(string noiDung) => new()
    {
        Text = noiDung,
        AutoSize = true,
        Anchor = AnchorStyles.Left
    };

    private static void ThemO(TableLayoutPanel bang, string nhan, Control o, int cot, int hang)
    {
        bang.Controls.Add(Nhan(nhan), cot, hang);
        bang.Controls.Add(o, cot + 1, hang);
    }

    private void ChonNhaCungCap()
    {
        if (lvNhaCungCap.SelectedItems.Count == 0)
            return;

        var ncc = (NhaCungCap)lvNhaCungCap.SelectedItems[0].Tag!;
        txtMa.Text = ncc.MaNCC;
        txtTen.Text = ncc.TenNCC;
        txtDiaChi.Text = ncc.DiaChi ?? string.Empty;
        txtSDT.Text = ncc.SDT ?? string.Empty;
    }

    private void ChuanBiThem(bool duaConTro)
    {
        try
        {
            lvNhaCungCap.SelectedItems.Clear();
            txtMa.Text = bus.TaoMaMoi();
            txtTen.Clear();
            txtDiaChi.Clear();
            txtSDT.Clear();
            if (duaConTro)
                txtTen.Focus();
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private NhaCungCap DocDuLieu(string maNCC) => new()
    {
        MaNCC = maNCC,
        TenNCC = txtTen.Text,
        DiaChi = txtDiaChi.Text,
        SDT = txtSDT.Text
    };

    private void Them()
    {
        if (lvNhaCungCap.SelectedItems.Count > 0)
        {
            MessageBox.Show("Hãy bấm Mới trước khi thêm nhà cung cấp.");
            return;
        }

        try
        {
            HienThiKetQua(bus.Them(DocDuLieu(txtMa.Text)));
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void Sua()
    {
        if (lvNhaCungCap.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn nhà cung cấp cần sửa trong danh sách.");
            return;
        }

        try
        {
            var ncc = (NhaCungCap)lvNhaCungCap.SelectedItems[0].Tag!;
            HienThiKetQua(bus.Sua(DocDuLieu(ncc.MaNCC)));
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void Xoa()
    {
        if (lvNhaCungCap.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn nhà cung cấp cần xóa trong danh sách.");
            return;
        }

        var ncc = (NhaCungCap)lvNhaCungCap.SelectedItems[0].Tag!;
        if (MessageBox.Show($"Xóa nhà cung cấp {ncc.MaNCC} - {ncc.TenNCC}?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            HienThiKetQua(bus.Xoa(ncc.MaNCC));
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void HienThiKetQua(KetQua ketQua)
    {
        MessageBox.Show(ketQua.ThongBao,
            ketQua.ThanhCong ? "Thành công" : "Không thành công",
            MessageBoxButtons.OK,
            ketQua.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (ketQua.ThanhCong)
        {
            txtTimKiem.Clear();
            TaiLai();
        }
    }

    private void TimKiem()
    {
        try
        {
            HienThi(bus.TimKiem(txtTimKiem.Text));
            ChuanBiThem(false);
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void TaiLai()
    {
        try
        {
            NhaCungCapBUS.TaiLai();
            HienThi(bus.TimKiem(txtTimKiem.Text));
            ChuanBiThem(false);
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void HienThi(ArrayList danhSach)
    {
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

        lblSoLuong.Text = $"Số nhà cung cấp: {danhSach.Count}";
    }

    private static void BaoLoi(Exception ex) => MessageBox.Show(
        $"Không thể thao tác với nhà cung cấp: {ex.Message}",
        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

using System.Collections;

namespace QLNT.GUI;

public sealed class frmThuoc : Form
{
    private readonly ThuocBUS bus = new();
    private readonly LoaiThuocBUS loaiBus = new();
    private readonly TextBox txtMa = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox txtTen = new() { Dock = DockStyle.Fill, MaxLength = 100 };
    private readonly TextBox txtTonKho = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ComboBox cboLoai = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker dtpHSD = new() { Dock = DockStyle.Fill, ShowCheckBox = true };
    private readonly NumericUpDown nudDonGia = new()
    {
        Dock = DockStyle.Fill,
        DecimalPlaces = 2,
        ThousandsSeparator = true,
        Maximum = 9999999999999999.99m
    };
    private readonly ListView lvThuoc = new();
    private readonly Label lblSoLuong = new();

    public frmThuoc()
    {
        Text = "Danh sách thuốc";
        MinimumSize = new Size(700, 500);
        Size = new Size(1050, 650);

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
        thanhTimKiem.Controls.Add(Nhan("Tìm mã, tên hoặc mã loại:"), 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        var btnTaiLai = new Button { Text = "Tải lại", AutoSize = true };
        btnTaiLai.Click += (_, _) => TaiLai();
        thanhTimKiem.Controls.Add(btnTaiLai, 2, 0);
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        var thanhNhap = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 126,
            Padding = new Padding(12, 6, 12, 6),
            ColumnCount = 4,
            RowCount = 3
        };
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        thanhNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 3; i++)
            thanhNhap.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3));

        cboLoai.DisplayMember = nameof(LoaiThuoc.TenLoai);
        cboLoai.ValueMember = nameof(LoaiThuoc.MaLoai);
        dtpHSD.Checked = false;
        txtTonKho.Text = "0";

        ThemO(thanhNhap, "Mã thuốc:", txtMa, 0, 0);
        ThemO(thanhNhap, "Tên thuốc:", txtTen, 2, 0);
        ThemO(thanhNhap, "Loại thuốc:", cboLoai, 0, 1);
        ThemO(thanhNhap, "Hạn dùng:", dtpHSD, 2, 1);
        ThemO(thanhNhap, "Đơn giá:", nudDonGia, 0, 2);
        ThemO(thanhNhap, "Tồn kho:", txtTonKho, 2, 2);

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

        lvThuoc.Dock = DockStyle.Fill;
        lvThuoc.View = View.Details;
        lvThuoc.FullRowSelect = true;
        lvThuoc.GridLines = true;
        lvThuoc.HideSelection = false;
        lvThuoc.MultiSelect = false;
        lvThuoc.Columns.Add("Mã thuốc", 105);
        lvThuoc.Columns.Add("Tên thuốc", 230);
        lvThuoc.Columns.Add("Loại thuốc", 150);
        lvThuoc.Columns.Add("Hạn dùng", 110);
        lvThuoc.Columns.Add("Đơn giá", 130);
        lvThuoc.Columns.Add("Tồn kho", 90);
        lvThuoc.SelectedIndexChanged += (_, _) => ChonThuoc();

        lblSoLuong.Dock = DockStyle.Bottom;
        lblSoLuong.Height = 32;
        lblSoLuong.Padding = new Padding(12, 8, 0, 0);

        Controls.Add(lvThuoc);
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

    private void NapLoaiThuoc()
    {
        cboLoai.Items.Clear();
        foreach (LoaiThuoc loai in loaiBus.LayTatCa())
            cboLoai.Items.Add(loai);
        cboLoai.SelectedIndex = cboLoai.Items.Count > 0 ? 0 : -1;
    }

    private void ChonThuoc()
    {
        if (lvThuoc.SelectedItems.Count == 0)
            return;

        var thuoc = (Thuoc)lvThuoc.SelectedItems[0].Tag!;
        txtMa.Text = thuoc.MaThuoc;
        txtTen.Text = thuoc.TenThuoc;
        txtTonKho.Text = thuoc.SoLuong.ToString();
        nudDonGia.Value = thuoc.DonGia;
        dtpHSD.Checked = thuoc.HSD.HasValue;
        if (thuoc.HSD.HasValue)
            dtpHSD.Value = thuoc.HSD.Value;

        cboLoai.SelectedIndex = -1;
        for (int i = 0; i < cboLoai.Items.Count; i++)
        {
            if (((LoaiThuoc)cboLoai.Items[i]!).MaLoai == thuoc.MaLoai)
            {
                cboLoai.SelectedIndex = i;
                break;
            }
        }
    }

    private void ChuanBiThem(bool duaConTro)
    {
        try
        {
            lvThuoc.SelectedItems.Clear();
            txtMa.Text = bus.TaoMaMoi();
            txtTen.Clear();
            txtTonKho.Text = "0";
            nudDonGia.Value = 0;
            dtpHSD.Checked = false;
            cboLoai.SelectedIndex = cboLoai.Items.Count > 0 ? 0 : -1;
            if (duaConTro)
                txtTen.Focus();
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private Thuoc DocDuLieu(string maThuoc) => new()
    {
        MaThuoc = maThuoc,
        TenThuoc = txtTen.Text,
        MaLoai = (cboLoai.SelectedItem as LoaiThuoc)?.MaLoai ?? string.Empty,
        HSD = dtpHSD.Checked ? dtpHSD.Value.Date : null,
        DonGia = nudDonGia.Value
    };

    private void Them()
    {
        if (lvThuoc.SelectedItems.Count > 0)
        {
            MessageBox.Show("Hãy bấm Mới trước khi thêm thuốc.");
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
        if (lvThuoc.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn thuốc cần sửa trong danh sách.");
            return;
        }

        try
        {
            var thuoc = (Thuoc)lvThuoc.SelectedItems[0].Tag!;
            HienThiKetQua(bus.Sua(DocDuLieu(thuoc.MaThuoc)));
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void Xoa()
    {
        if (lvThuoc.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn thuốc cần xóa trong danh sách.");
            return;
        }

        var thuoc = (Thuoc)lvThuoc.SelectedItems[0].Tag!;
        if (MessageBox.Show($"Xóa thuốc {thuoc.MaThuoc} - {thuoc.TenThuoc}?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            HienThiKetQua(bus.Xoa(thuoc.MaThuoc));
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
            ThuocBUS.TaiLai();
            LoaiThuocBUS.TaiLai();
            NapLoaiThuoc();
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
        lvThuoc.BeginUpdate();
        try
        {
            lvThuoc.Items.Clear();
            foreach (Thuoc thuoc in danhSach)
            {
                var dong = new ListViewItem(thuoc.MaThuoc) { Tag = thuoc };
                dong.SubItems.Add(thuoc.TenThuoc);
                dong.SubItems.Add(thuoc.MaLoai);
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

        lblSoLuong.Text = $"Số thuốc: {danhSach.Count}";
    }

    private static void BaoLoi(Exception ex) => MessageBox.Show(
        $"Không thể thao tác với thuốc: {ex.Message}",
        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

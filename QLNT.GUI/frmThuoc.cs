using System.Collections;

namespace QLNT.GUI;

public sealed class frmThuoc : Form
{
    private readonly ThuocBUS bus = new();
    private readonly LoaiThuocBUS loaiBus = new();
    private readonly TextBox txtMa = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox txtTen = new() { Dock = DockStyle.Fill, MaxLength = 100 };
    private readonly TextBox txtTonKho = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox txtLocMa = new() { Name = "txtLocMaThuoc", Dock = DockStyle.Fill, MaxLength = 10 };
    private readonly TextBox txtLocTen = new() { Name = "txtLocTenThuoc", Dock = DockStyle.Fill, MaxLength = 100 };
    private readonly ComboBox cboLocLoai = new() { Name = "cboLocLoaiThuoc", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cboPhepNoi = new() { Name = "cboLocPhepNoi", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox chkGiaTu = new() { Name = "chkLocGiaTu", Text = "Giá từ:", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly CheckBox chkGiaDen = new() { Name = "chkLocGiaDen", Text = "Giá đến:", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly NumericUpDown nudGiaTu = OGiaLoc("nudLocGiaTu");
    private readonly NumericUpDown nudGiaDen = OGiaLoc("nudLocGiaDen");
    private readonly Label lblLoiLoc = new() { Name = "lblLoiLocThuoc", AutoSize = true, ForeColor = Color.DarkRed, Padding = new Padding(8, 7, 0, 0) };
    private readonly ComboBox cboLoai = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker dtpHSD = new() { Dock = DockStyle.Fill, ShowCheckBox = true };
    private readonly NumericUpDown nudDonGia = new()
    {
        Dock = DockStyle.Fill,
        DecimalPlaces = 2,
        ThousandsSeparator = true,
        Maximum = 9999999999999999.99m
    };
    private readonly ListView lvThuoc = new() { Name = "lvThuoc" };
    private readonly Label lblSoLuong = new() { Name = "lblSoLuongThuoc" };

    public frmThuoc()
    {
        Text = "Danh sách thuốc";
        MinimumSize = new Size(850, 600);
        Size = new Size(1100, 730);

        GroupBox thanhTimKiem = TaoVungLoc();

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
        var btnNhapExcel = new Button { Name = "btnNhapExcelThuoc", Text = "Nhập Excel", AutoSize = true };
        var btnMauExcel = new Button { Name = "btnMauExcelThuoc", Text = "Tạo mẫu Excel", AutoSize = true };
        btnMoi.Click += (_, _) => ChuanBiThem(true);
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();
        btnNhapExcel.Click += (_, _) => NhapExcel();
        btnMauExcel.Click += (_, _) => TaoMauExcel();
        thanhNut.Controls.AddRange([btnMoi, btnThem, btnSua, btnXoa, btnNhapExcel, btnMauExcel]);

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

    private static NumericUpDown OGiaLoc(string ten) => new()
    {
        Name = ten, Dock = DockStyle.Fill, DecimalPlaces = 2,
        Maximum = 9999999999999999.99m, ThousandsSeparator = true, Enabled = false
    };

    private GroupBox TaoVungLoc()
    {
        var nhom = new GroupBox
        {
            Name = "grpLocThuoc", Text = "Tìm kiếm nâng cao (giá bán)",
            Dock = DockStyle.Top, Height = 145, Padding = new Padding(12, 8, 12, 6)
        };
        var bang = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 3 };
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        bang.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        bang.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        bang.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        ThemO(bang, "Mã thuốc:", txtLocMa, 0, 0);
        ThemO(bang, "Tên thuốc:", txtLocTen, 2, 0);
        ThemO(bang, "Loại thuốc:", cboLocLoai, 4, 0);
        cboLocLoai.DisplayMember = nameof(LoaiThuoc.TenLoai);
        cboLocLoai.ValueMember = nameof(LoaiThuoc.MaLoai);
        bang.Controls.Add(chkGiaTu, 0, 1);
        bang.Controls.Add(nudGiaTu, 1, 1);
        bang.Controls.Add(chkGiaDen, 2, 1);
        bang.Controls.Add(nudGiaDen, 3, 1);
        ThemO(bang, "Kết hợp:", cboPhepNoi, 4, 1);
        cboPhepNoi.Items.AddRange(["AND", "OR"]);
        cboPhepNoi.SelectedIndex = 0;
        chkGiaTu.CheckedChanged += (_, _) => nudGiaTu.Enabled = chkGiaTu.Checked;
        chkGiaDen.CheckedChanged += (_, _) => nudGiaDen.Enabled = chkGiaDen.Checked;

        var thanhNut = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
        var btnTim = new Button { Name = "btnTimKiemThuoc", Text = "Tìm", AutoSize = true };
        var btnXoaLoc = new Button { Name = "btnXoaBoLocThuoc", Text = "Xóa bộ lọc", AutoSize = true };
        var btnTaiLai = new Button { Name = "btnTaiLaiThuoc", Text = "Tải lại", AutoSize = true };
        btnTim.Click += (_, _) => TimKiem();
        btnXoaLoc.Click += (_, _) => { DatLaiBoLoc(); TimKiem(); };
        btnTaiLai.Click += (_, _) => TaiLai();
        thanhNut.Controls.AddRange([btnTim, btnXoaLoc, btnTaiLai, lblLoiLoc]);
        bang.Controls.Add(thanhNut, 0, 2);
        bang.SetColumnSpan(thanhNut, 6);
        nhom.Controls.Add(bang);

        foreach (Control o in new Control[] { txtLocMa, txtLocTen, nudGiaTu, nudGiaDen, cboLocLoai, cboPhepNoi })
            o.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                TimKiem();
            };
        return nhom;
    }

    private void DatLaiBoLoc()
    {
        txtLocMa.Clear();
        txtLocTen.Clear();
        cboLocLoai.SelectedIndex = cboLocLoai.Items.Count > 0 ? 0 : -1;
        chkGiaTu.Checked = false;
        chkGiaDen.Checked = false;
        nudGiaTu.Value = 0;
        nudGiaDen.Value = 0;
        cboPhepNoi.SelectedIndex = 0;
        lblLoiLoc.Text = string.Empty;
    }

    private static void ThemO(TableLayoutPanel bang, string nhan, Control o, int cot, int hang)
    {
        bang.Controls.Add(Nhan(nhan), cot, hang);
        bang.Controls.Add(o, cot + 1, hang);
    }

    private void NapLoaiThuoc()
    {
        string? maLoaiLoc = (cboLocLoai.SelectedItem as LoaiThuoc)?.MaLoai;
        cboLoai.Items.Clear();
        cboLocLoai.Items.Clear();
        cboLocLoai.Items.Add(new LoaiThuoc(string.Empty, "Tất cả loại"));
        foreach (LoaiThuoc loai in loaiBus.LayTatCa())
        {
            cboLoai.Items.Add(loai);
            cboLocLoai.Items.Add(loai);
        }
        cboLoai.SelectedIndex = cboLoai.Items.Count > 0 ? 0 : -1;
        cboLocLoai.SelectedIndex = 0;
        for (int i = 1; i < cboLocLoai.Items.Count; i++)
            if (((LoaiThuoc)cboLocLoai.Items[i]!).MaLoai.Equals(maLoaiLoc, StringComparison.OrdinalIgnoreCase))
            {
                cboLocLoai.SelectedIndex = i;
                break;
            }
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

    private void NhapExcel()
    {
        using var chonFile = new OpenFileDialog
        {
            Title = "Nhập danh mục thuốc từ Excel (tồn ban đầu 0)",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            CheckFileExists = true
        };
        if (chonFile.ShowDialog(this) != DialogResult.OK) return;
        if (MessageBox.Show("Thêm các thuốc mới từ file đã chọn? Mã được tự tạo, tồn ban đầu là 0. " +
            "Nếu có dòng sai hoặc trùng, toàn bộ file sẽ không được lưu.", "Nhập thuốc từ Excel",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        UseWaitCursor = true;
        try { HienThiKetQua(bus.NhapTuExcel(chonFile.FileName)); }
        finally { UseWaitCursor = false; }
    }

    private void TaoMauExcel()
    {
        using var chonFile = new SaveFileDialog
        {
            Title = "Tạo file mẫu nhập thuốc", FileName = "MauNhapThuoc.xlsx",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx", DefaultExt = "xlsx", AddExtension = true
        };
        if (chonFile.ShowDialog(this) != DialogResult.OK) return;
        KetQua ketQua = bus.TaoMauExcel(chonFile.FileName);
        MessageBox.Show(ketQua.ThongBao, "Mẫu nhập thuốc", MessageBoxButtons.OK,
            ketQua.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void HienThiKetQua(KetQua ketQua)
    {
        MessageBox.Show(ketQua.ThongBao,
            ketQua.ThanhCong ? "Thành công" : "Không thành công",
            MessageBoxButtons.OK,
            ketQua.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (ketQua.ThanhCong)
        {
            DatLaiBoLoc();
            TaiLai();
        }
    }

    private void TimKiem()
    {
        try
        {
            ArrayList ketQua = bus.TimKiemNangCao(
                maThuoc: txtLocMa.Text,
                tenThuoc: txtLocTen.Text,
                maLoai: (cboLocLoai.SelectedItem as LoaiThuoc)?.MaLoai,
                giaTu: chkGiaTu.Checked ? nudGiaTu.Value : null,
                giaDen: chkGiaDen.Checked ? nudGiaDen.Value : null,
                noi: cboPhepNoi.SelectedIndex == 1 ? PhepNoi.Or : PhepNoi.And);
            HienThi(ketQua);
            ChuanBiThem(false);
            lblLoiLoc.Text = string.Empty;
        }
        catch (ArgumentException ex)
        {
            lblLoiLoc.Text = ex.Message;
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
            TimKiem();
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
            lvThuoc.SelectedItems.Clear();
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

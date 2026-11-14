using System.Collections;

using System.Globalization;

namespace QLNT.GUI;

public sealed class frmHoaDon : Form
{
    private const int SoDongChiTietHienThi = 30;
    private readonly HoaDonBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Width = 260 };
    private readonly TextBox txtLocMa = new() { Name = "txtLocMaHoaDon", Dock = DockStyle.Fill };
    private readonly TextBox txtLocNhanVien = new() { Name = "txtLocNhanVienHoaDon", Dock = DockStyle.Fill };
    private readonly TextBox txtLocKhachHang = new() { Name = "txtLocKhachHangHoaDon", Dock = DockStyle.Fill };
    private readonly DateTimePicker dtpLocTuNgay = new() { Name = "dtpLocTuNgayHoaDon", Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly DateTimePicker dtpLocDenNgay = new() { Name = "dtpLocDenNgayHoaDon", Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly CheckBox chkTienTu = new() { Text = "Tiền từ:", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly CheckBox chkTienDen = new() { Text = "Tiền đến:", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly NumericUpDown nudTienTu = TaoOChiPhi("nudLocTienTuHoaDon");
    private readonly NumericUpDown nudTienDen = TaoOChiPhi("nudLocTienDenHoaDon");
    private readonly ComboBox cboPhepNoi = new() { Name = "cboLocPhepNoiHoaDon", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cboTruongThem = new() { Name = "cboLocTruongThemHoaDon", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cboToanTuThem = new() { Name = "cboLocToanTuThemHoaDon", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtGiaTriThem = new() { Name = "txtLocGiaTriThemHoaDon", Dock = DockStyle.Fill };
    private readonly CheckBox chkNot = new() { Name = "chkLocNotHoaDon", Text = "NOT", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label lblLoiLoc = new() { Name = "lblLoiLocHoaDon", AutoSize = true, ForeColor = Color.DarkRed, Padding = new Padding(6, 7, 0, 0) };
    private readonly ListView lvHoaDon = new() { Name = "lvHoaDon" };
    private readonly ListView lvChiTiet = new();
    private readonly Label lblChiTiet = new() { AutoSize = true, Text = "Chi tiết hóa đơn" };
    private readonly Label lblTienHang = new() { Name = "lblTienHang" };
    private readonly Label lblGiamHoaDon = new() { Name = "lblGiamHoaDon" };
    private readonly Label lblLoaiKhuyenMai = new() { Name = "lblLoaiKhuyenMai" };
    private readonly Label lblTongSauGiam = new() { Name = "lblTongSauGiam" };
    private readonly Label lblTienKhachDua = new() { Name = "lblTienKhachDua" };
    private readonly Label lblTienThoi = new() { Name = "lblTienThoi" };
    private readonly Label lblTrangThaiThanhToan = new() { Name = "lblTrangThaiThanhToan" };
    private bool dangNap;
    private bool daTaiDanhSach;
    private bool dangXoaBoLoc;

    public frmHoaDon()
    {
        Text = "Hóa đơn bán hàng";
        MinimumSize = new Size(850, 560);
        Size = new Size(1100, 720);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = GiaoDienChung.Nen
        };
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 216));
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 380));
        int chieuCaoDong = Math.Max(24, lvChiTiet.Font.Height + 8);
        int chieuCaoChiTiet = SoDongChiTietHienThi * chieuCaoDong + lvChiTiet.Font.Height + 260;
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, chieuCaoChiTiet));
        Controls.Add(khung);

        khung.Controls.Add(TaoVungLocHoaDon(), 0, 0);
        khung.Controls.Add(TaoThanhCongCu(), 0, 1);
        CauHinhDanhSachHoaDon();
        CauHinhDanhSachChiTiet();
        khung.Controls.Add(TaoKhungDanhSach("Danh sách hóa đơn", lvHoaDon), 0, 2);
        khung.Controls.Add(TaoKhungChiTiet(), 0, 3);

        Load += (_, _) => TaiDanhSach();
        txtTimKiem.TextChanged += (_, _) => TimKiemNangCao();
    }

    private static NumericUpDown TaoOChiPhi(string ten) => new()
    {
        Name = ten, Dock = DockStyle.Fill, DecimalPlaces = 2,
        ThousandsSeparator = true, Maximum = 9999999999999999.99m, Enabled = false
    };

    private Control TaoVungLocHoaDon()
    {
        var nhom = new GroupBox
        {
            Name = "grpLocHoaDon", Text = "Tìm kiếm nâng cao hóa đơn",
            Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 6)
        };
        var bang = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 5 };
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        for (int i = 0; i < 4; i++)
            bang.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        bang.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        static Label Nhan(string chu) => new() { Text = chu, AutoSize = true, Anchor = AnchorStyles.Left };
        void ThemO(string chu, Control o, int cot, int hang)
        {
            bang.Controls.Add(Nhan(chu), cot, hang);
            bang.Controls.Add(o, cot + 1, hang);
        }

        ThemO("Mã HĐ:", txtLocMa, 0, 0);
        ThemO("Nhân viên:", txtLocNhanVien, 2, 0);
        ThemO("Khách hàng:", txtLocKhachHang, 4, 0);
        ThemO("Từ ngày:", dtpLocTuNgay, 0, 1);
        ThemO("Đến ngày:", dtpLocDenNgay, 2, 1);
        ThemO("Kết hợp:", cboPhepNoi, 4, 1);
        dtpLocTuNgay.Checked = false;
        dtpLocDenNgay.Checked = false;
        cboPhepNoi.Items.AddRange(["AND", "OR"]);
        cboPhepNoi.SelectedIndex = 0;

        bang.Controls.Add(chkTienTu, 0, 2);
        bang.Controls.Add(nudTienTu, 1, 2);
        bang.Controls.Add(chkTienDen, 2, 2);
        bang.Controls.Add(nudTienDen, 3, 2);
        chkTienTu.CheckedChanged += (_, _) => nudTienTu.Enabled = chkTienTu.Checked;
        chkTienDen.CheckedChanged += (_, _) => nudTienDen.Enabled = chkTienDen.Checked;

        bang.Controls.Add(Nhan("Thêm lọc:"), 0, 3);
        bang.Controls.Add(cboTruongThem, 1, 3);
        bang.Controls.Add(cboToanTuThem, 2, 3);
        bang.Controls.Add(txtGiaTriThem, 3, 3);
        bang.Controls.Add(chkNot, 4, 3);
        cboTruongThem.Items.AddRange(["Không dùng", "Mã hóa đơn", "Mã NV", "Tên NV", "Mã KH",
            "Tên khách", "Ngày lập", "Tổng tiền", "Tiền giảm", "Tiền khách đưa", "Voucher", "Mã KM hóa đơn"]);
        cboTruongThem.SelectedIndexChanged += (_, _) => CapNhatToanTuThem();
        cboTruongThem.SelectedIndex = 0;

        var nut = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
        var btnTim = new Button { Name = "btnTimHoaDon", Text = "Tìm", AutoSize = true };
        var btnXoa = new Button { Name = "btnXoaBoLocHoaDon", Text = "Xóa bộ lọc", AutoSize = true };
        btnTim.Click += (_, _) => TimKiemNangCao();
        btnXoa.Click += (_, _) => { DatLaiBoLoc(); TimKiemNangCao(); };
        nut.Controls.AddRange([btnTim, btnXoa, lblLoiLoc]);
        bang.Controls.Add(nut, 0, 4);
        bang.SetColumnSpan(nut, 6);
        nhom.Controls.Add(bang);

        foreach (Control o in new Control[] { txtLocMa, txtLocNhanVien, txtLocKhachHang,
            nudTienTu, nudTienDen, cboPhepNoi, cboTruongThem, cboToanTuThem, txtGiaTriThem })
            o.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                TimKiemNangCao();
            };
        return nhom;
    }

    private void CapNhatToanTuThem()
    {
        string? cu = cboToanTuThem.SelectedItem?.ToString();
        cboToanTuThem.Items.Clear();
        if (cboTruongThem.SelectedIndex is >= 1 and <= 5 or 10 or 11)
            cboToanTuThem.Items.Add("Chứa");
        cboToanTuThem.Items.AddRange(["=", "<>", ">", ">=", "<", "<="]);
        int viTri = cu == null ? -1 : cboToanTuThem.Items.IndexOf(cu);
        cboToanTuThem.SelectedIndex = viTri >= 0 ? viTri : 0;
    }

    private DieuKien? TaoDieuKienThem()
    {
        if (cboTruongThem.SelectedIndex <= 0)
            return null;
        string chuoi = txtGiaTriThem.Text.Trim();
        if (chuoi.Length == 0)
            throw new ArgumentException("Hãy nhập giá trị cho điều kiện thêm.");
        string truong = cboTruongThem.SelectedIndex switch
        {
            1 => nameof(HoaDon.MaHD), 2 => nameof(HoaDon.MaNV),
            3 => nameof(HoaDon.TenNhanVien), 4 => nameof(HoaDon.MaKH),
            5 => nameof(HoaDon.TenKhachHang), 6 => nameof(HoaDon.NgayLap),
            7 => nameof(HoaDon.TongTien), 8 => nameof(HoaDon.TienGiam),
            9 => nameof(HoaDon.TienKhachDua), 10 => nameof(HoaDon.MaVoucher),
            11 => nameof(HoaDon.MaCTKMHD),
            _ => throw new ArgumentException("Trường lọc không hợp lệ.")
        };
        PhepSoSanh phep = cboToanTuThem.SelectedItem?.ToString() switch
        {
            "Chứa" => PhepSoSanh.Chua, "=" => PhepSoSanh.Bang,
            "<>" => PhepSoSanh.Khac, ">" => PhepSoSanh.Lon,
            ">=" => PhepSoSanh.LonHoacBang, "<" => PhepSoSanh.Nho,
            "<=" => PhepSoSanh.NhoHoacBang,
            _ => throw new ArgumentException("Toán tử lọc không hợp lệ.")
        };
        object giaTri = chuoi;
        if (cboTruongThem.SelectedIndex is >= 7 and <= 9)
        {
            if (!decimal.TryParse(chuoi, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal so))
                throw new ArgumentException("Giá trị tiền phải là số hợp lệ.");
            giaTri = so;
        }
        else if (cboTruongThem.SelectedIndex == 6)
        {
            if (!DateTime.TryParseExact(chuoi, "dd/MM/yyyy", CultureInfo.CurrentCulture,
                DateTimeStyles.None, out DateTime ngay))
                throw new ArgumentException("Ngày lập cần đúng dạng dd/MM/yyyy.");
            giaTri = ngay.Date;
        }
        return new DieuKien { TenTruong = truong, Phep = phep, GiaTri = giaTri, Not = chkNot.Checked };
    }

    private void DatLaiBoLoc()
    {
        dangXoaBoLoc = true;
        try
        {
            txtTimKiem.Clear();
            txtLocMa.Clear();
            txtLocNhanVien.Clear();
            txtLocKhachHang.Clear();
            dtpLocTuNgay.Checked = false;
            dtpLocDenNgay.Checked = false;
            chkTienTu.Checked = false;
            chkTienDen.Checked = false;
            nudTienTu.Value = 0;
            nudTienDen.Value = 0;
            cboPhepNoi.SelectedIndex = 0;
            cboTruongThem.SelectedIndex = 0;
            txtGiaTriThem.Clear();
            chkNot.Checked = false;
            lblLoiLoc.Text = string.Empty;
        }
        finally { dangXoaBoLoc = false; }
    }

    private Control TaoThanhCongCu()
    {
        var congCu = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(0, 0, 0, 12),
            BackColor = GiaoDienChung.Nen
        };
        var nhanTim = new Label { Text = "Tìm hóa đơn:", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 0, 0) };
        var btnTaiLai = GiaoDienChung.TaoNutPhu("Tải lại");
        var btnLap = GiaoDienChung.TaoNutPhu("Lập hóa đơn");
        var btnDonWeb = GiaoDienChung.TaoNutPhu("Đơn đặt từ web");
        var btnSua = GiaoDienChung.TaoNutPhu("Sửa hóa đơn");
        var btnHuy = GiaoDienChung.TaoNutPhu("Hủy hóa đơn / hoàn tồn");
        var btnIn = GiaoDienChung.TaoNutPhu("In hóa đơn");
        var btnPdf = GiaoDienChung.TaoNutPhu("Xuất PDF");
        btnTaiLai.Click += (_, _) => TaiDanhSach();
        btnLap.Click += (_, _) => LapHoaDonNhap();
        btnDonWeb.Click += (_, _) => { using var form = new frmDonDatThuoc(); form.ShowDialog(this); TaiDanhSach(); };
        btnSua.Click += (_, _) => SuaHoaDon();
        btnHuy.Click += (_, _) => HuyHoaDon();
        btnIn.Click += (_, _) => InHoaDon();
        btnPdf.Click += (_, _) => XuatPdfHoaDon();
        congCu.Controls.AddRange([nhanTim, txtTimKiem, btnTaiLai, btnDonWeb, btnLap, btnSua, btnHuy, btnIn, btnPdf]);
        return congCu;
    }

    private void CauHinhDanhSachHoaDon()
    {
        lvHoaDon.Dock = DockStyle.Fill;
        lvHoaDon.View = View.Details;
        lvHoaDon.FullRowSelect = true;
        lvHoaDon.MultiSelect = false;
        lvHoaDon.GridLines = true;
        lvHoaDon.HideSelection = false;
        lvHoaDon.Columns.Add("Mã HĐ", 90);
        lvHoaDon.Columns.Add("Ngày lập", 100);
        lvHoaDon.Columns.Add("Nhân viên", 190);
        lvHoaDon.Columns.Add("Khách hàng", 190);
        lvHoaDon.Columns.Add("Tổng tiền", 140);
        lvHoaDon.SelectedIndexChanged += (_, _) => HienThiChiTietDaChon();
    }

    private void CauHinhDanhSachChiTiet()
    {
        lvChiTiet.Dock = DockStyle.Fill;
        lvChiTiet.View = View.Details;
        lvChiTiet.FullRowSelect = true;
        lvChiTiet.MultiSelect = false;
        lvChiTiet.GridLines = true;
        lvChiTiet.Columns.Add("Mã thuốc", 100);
        lvChiTiet.Columns.Add("Tên thuốc", 270);
        lvChiTiet.Columns.Add("Số lượng", 90);
        lvChiTiet.Columns.Add("Đơn giá", 130);
        lvChiTiet.Columns.Add("Thành tiền", 150);
    }

    private Control TaoKhungDanhSach(string tieuDe, Control noiDung)
    {
        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12),
            BackColor = GiaoDienChung.MatTrang
        };
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        khung.Controls.Add(new Label
        {
            Text = tieuDe,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = GiaoDienChung.Chu
        }, 0, 0);
        khung.Controls.Add(noiDung, 0, 1);
        return khung;
    }

    private Control TaoKhungChiTiet()
    {
        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12),
            BackColor = GiaoDienChung.MatTrang
        };
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lblChiTiet.Padding = new Padding(0, 0, 0, 8);
        lblChiTiet.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblChiTiet.ForeColor = GiaoDienChung.Chu;
        khung.Controls.Add(lblChiTiet, 0, 0);
        khung.Controls.Add(TaoTongKetChiTiet(), 0, 1);
        khung.Controls.Add(lvChiTiet, 0, 2);
        return khung;
    }

    private Control TaoTongKetChiTiet()
    {
        var tongKet = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            BackColor = GiaoDienChung.MatTrang
        };
        tongKet.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        tongKet.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 7; i++)
            tongKet.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        ThemDongTongKet(tongKet, 0, "Tiền hàng (đã giảm SP):", lblTienHang);
        ThemDongTongKet(tongKet, 1, "Giảm giá hóa đơn:", lblGiamHoaDon);
        ThemDongTongKet(tongKet, 2, "Loại khuyến mãi áp dụng:", lblLoaiKhuyenMai);
        ThemDongTongKet(tongKet, 3, "TỔNG SAU GIẢM:", lblTongSauGiam, true);
        ThemDongTongKet(tongKet, 4, "Tiền khách đưa:", lblTienKhachDua);
        ThemDongTongKet(tongKet, 5, "Tiền thối cho khách:", lblTienThoi);
        ThemDongTongKet(tongKet, 6, "Thanh toán:", lblTrangThaiThanhToan);
        XoaTongKetChiTiet();
        return tongKet;
    }

    private static void ThemDongTongKet(TableLayoutPanel bang, int hang, string ten, Label giaTri, bool noiBat = false)
    {
        bang.Controls.Add(new Label
        {
            Text = ten,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = noiBat ? new Font("Segoe UI", 10F, FontStyle.Bold) : GiaoDienChung.PhongChu
        }, 0, hang);
        giaTri.Dock = DockStyle.Fill;
        giaTri.AutoEllipsis = true;
        giaTri.TextAlign = ContentAlignment.MiddleLeft;
        giaTri.Font = noiBat ? new Font("Segoe UI", 10F, FontStyle.Bold) : GiaoDienChung.PhongChu;
        giaTri.ForeColor = noiBat ? GiaoDienChung.XanhChinh : GiaoDienChung.Chu;
        bang.Controls.Add(giaTri, 1, hang);
    }

    private void XoaTongKetChiTiet()
    {
        lblTienHang.Text = "—";
        lblGiamHoaDon.Text = "—";
        lblLoaiKhuyenMai.Text = "—";
        lblTongSauGiam.Text = "—";
        lblTienKhachDua.Text = "—";
        lblTienThoi.Text = "—";
        lblTrangThaiThanhToan.Text = "—";
    }

    private void TaiDanhSach()
    {
        try
        {
            bus.TaiLai();
            bus.LayTatCa();
            daTaiDanhSach = true;
            TimKiemNangCao();
        }
        catch (Exception ex)
        {
            daTaiDanhSach = false;
            MessageBox.Show($"Không thể tải danh sách hóa đơn: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TimKiemNangCao()
    {
        if (!daTaiDanhSach || dangXoaBoLoc)
            return;
        try
        {
            var loc = new BoLocHoaDon
            {
                TuKhoa = txtTimKiem.Text,
                MaHD = txtLocMa.Text,
                NhanVien = txtLocNhanVien.Text,
                KhachHang = txtLocKhachHang.Text,
                TuNgay = dtpLocTuNgay.Checked ? dtpLocTuNgay.Value.Date : null,
                DenNgay = dtpLocDenNgay.Checked ? dtpLocDenNgay.Value.Date : null,
                TienTu = chkTienTu.Checked ? nudTienTu.Value : null,
                TienDen = chkTienDen.Checked ? nudTienDen.Value : null,
                Noi = cboPhepNoi.SelectedIndex == 1 ? PhepNoi.Or : PhepNoi.And,
                DieuKienThem = TaoDieuKienThem()
            };
            ArrayList ketQua = bus.TimKiemNangCao(loc);
            HienThiDanhSachHoaDon(ketQua);
            lblLoiLoc.Text = string.Empty;
        }
        catch (Exception ex)
        {
            lblLoiLoc.Text = ex.Message;
        }
    }

    private void HienThiDanhSachHoaDon(ArrayList danhSach)
    {
        dangNap = true;
        try
        {
            lvHoaDon.BeginUpdate();
            try
            {
                lvHoaDon.Items.Clear();
                lvChiTiet.Items.Clear();
                lblChiTiet.Text = "Chi tiết hóa đơn";
                XoaTongKetChiTiet();
                foreach (HoaDon hoaDon in danhSach)
                {
                    var dong = new ListViewItem(hoaDon.MaHD) { Tag = hoaDon };
                    dong.SubItems.Add(hoaDon.NgayLap.ToString("dd/MM/yyyy"));
                    dong.SubItems.Add($"{hoaDon.MaNV} - {hoaDon.TenNhanVien}".Trim(' ', '-'));
                    dong.SubItems.Add(hoaDon.MaKH == null
                        ? "Khách lẻ"
                        : $"{hoaDon.MaKH} - {hoaDon.TenKhachHang}".Trim(' ', '-'));
                    dong.SubItems.Add(hoaDon.TongTien.ToString("N2"));
                    lvHoaDon.Items.Add(dong);
                }
            }
            finally
            {
                lvHoaDon.EndUpdate();
            }
        }
        finally
        {
            dangNap = false;
        }

        if (lvHoaDon.Items.Count > 0)
            lvHoaDon.Items[0].Selected = true;
    }

    private void HienThiChiTietDaChon()
    {
        if (dangNap || lvHoaDon.SelectedItems.Count == 0)
            return;

        if (lvHoaDon.SelectedItems[0].Tag is not HoaDon hoaDon)
        {
            MessageBox.Show("Dữ liệu hóa đơn được chọn không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        try
        {
            ArrayList chiTiet = bus.LayChiTiet(hoaDon.MaHD);
            lvChiTiet.BeginUpdate();
            try
            {
                lvChiTiet.Items.Clear();
                foreach (CTHoaDon dongChiTiet in chiTiet)
                {
                    var dong = new ListViewItem(dongChiTiet.MaThuoc);
                    dong.SubItems.Add(dongChiTiet.TenThuoc);
                    dong.SubItems.Add(dongChiTiet.SoLuong.ToString());
                    dong.SubItems.Add(dongChiTiet.DonGia.ToString("N2"));
                    dong.SubItems.Add(dongChiTiet.ThanhTien.ToString("N2"));
                    lvChiTiet.Items.Add(dong);
                }
            }
            finally
            {
                lvChiTiet.EndUpdate();
            }

            lblChiTiet.Text = $"Chi tiết hóa đơn {hoaDon.MaHD} - {hoaDon.TenKhachHang} - Tổng tiền: {hoaDon.TongTien:N2}";
            decimal tienHang = chiTiet.Cast<CTHoaDon>().Sum(dong => dong.ThanhTien);
            lblTienHang.Text = $"{tienHang:N2} đ";
            lblGiamHoaDon.Text = $"{hoaDon.TienGiam:N2} đ";
            lblLoaiKhuyenMai.Text = bus.MoTaKhuyenMaiDaApDung(hoaDon, tienHang);
            lblTongSauGiam.Text = $"{hoaDon.TongTien:N2} đ";
            lblTienKhachDua.Text = hoaDon.TienKhachDua is decimal tienDua
                ? $"{tienDua:N2} đ"
                : "Chưa ghi nhận (hóa đơn cũ)";
            lblTienThoi.Text = hoaDon.TienKhachDua is decimal tienKhachDua
                ? $"{Math.Max(0m, tienKhachDua - hoaDon.TongTien):N2} đ"
                : "Chưa ghi nhận (hóa đơn cũ)";
            lblTrangThaiThanhToan.Text = hoaDon.PhuongThucThanhToan switch
            {
                PhuongThucThanhToanCode.Online => "Chuyển khoản online · Đã đối chiếu",
                PhuongThucThanhToanCode.COD when hoaDon.TrangThaiThanhToan == "ChoThuTien" => "COD · Chờ thu tiền",
                PhuongThucThanhToanCode.COD => "COD · Đã thu tiền",
                _ => "Tại cửa hàng · Đã thanh toán"
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải chi tiết hóa đơn {hoaDon.MaHD}: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LapHoaDonNhap()
    {
        using var form = new frmTaoHoaDon();
        if (form.ShowDialog(this) == DialogResult.OK)
            TaiDanhSach();
    }

    private void SuaHoaDon()
    {
        if (lvHoaDon.SelectedItems.Count == 0 || lvHoaDon.SelectedItems[0].Tag is not HoaDon hoaDon)
        {
            MessageBox.Show("Hãy chọn hóa đơn cần sửa.", "Chưa chọn hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var form = new frmTaoHoaDon(hoaDon);
        form.ShowDialog(this);
        TaiDanhSach();
    }

    private void HuyHoaDon()
    {
        if (lvHoaDon.SelectedItems.Count == 0 || lvHoaDon.SelectedItems[0].Tag is not HoaDon hoaDon)
        {
            MessageBox.Show("Hãy chọn hóa đơn cần hủy.", "Chưa chọn hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult xacNhan = MessageBox.Show(
            $"Hủy hóa đơn {hoaDon.MaHD}? Chi tiết sẽ bị xóa và các thuốc trong hóa đơn sẽ được hoàn vào kho.",
            "Xác nhận hủy hóa đơn",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (xacNhan != DialogResult.Yes)
            return;

        KetQuaLuuHoaDon ketQua = bus.HuyHoaDon(hoaDon.MaHD);
        if (!ketQua.ThanhCong)
        {
            MessageBox.Show(ketQua.ThongBao, "Không thể hủy hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show(ketQua.ThongBao, "Đã hủy hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        TaiDanhSach();
    }

    private void InHoaDon()
    {
        if (lvHoaDon.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn hóa đơn cần in.");
            return;
        }

        HoaDon hoaDon = (HoaDon)lvHoaDon.SelectedItems[0].Tag!;
        ArrayList chiTiet;
        try
        {
            chiTiet = bus.LayChiTiet(hoaDon.MaHD);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải dữ liệu để in hóa đơn: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var cacDong = new List<CTHoaDon>();
        foreach (object? dong in chiTiet)
        {
            if (dong is not CTHoaDon chiTietHoaDon)
            {
                MessageBox.Show("Dữ liệu chi tiết hóa đơn không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            cacDong.Add(chiTietHoaDon);
        }

        HoaDonIn.In(this, hoaDon, cacDong);
    }
    private void XuatPdfHoaDon()
    {
        if (lvHoaDon.SelectedItems.Count == 0 || lvHoaDon.SelectedItems[0].Tag is not HoaDon hoaDon)
        {
            MessageBox.Show("Hãy chọn hóa đơn cần xuất PDF.");
            return;
        }

        try
        {
            List<CTHoaDon> cacDong = bus.LayChiTiet(hoaDon.MaHD).Cast<CTHoaDon>().ToList();
            HoaDonIn.XuatPdf(this, hoaDon, cacDong);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải dữ liệu để xuất PDF: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

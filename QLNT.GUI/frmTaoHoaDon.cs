using System.ComponentModel;

namespace QLNT.GUI;

public sealed class frmTaoHoaDon : Form
{
    private readonly HoaDonBUS hoaDonBUS = new();
    private readonly ThuocBUS thuocBUS = new();
    private readonly BindingList<DongHoaDonDuThao> chiTiet = [];
    private readonly DataGridView dgvChiTiet = new() { Name = "dgvChiTiet" };
    private readonly TextBox txtNhanVien = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox txtKhachHang = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox txtVoucher = new() { MaxLength = 20, Width = 140 };
    private readonly DateTimePicker dtpNgayLap = new() { Format = DateTimePickerFormat.Short, Width = 140 };
    private readonly NumericUpDown nudTienKhachDua = new()
    {
        DecimalPlaces = 2,
        Maximum = 9999999999999999.99m,
        ThousandsSeparator = true,
        Width = 180
    };
    private readonly Label lblTongGoc = TaoNhanTien();
    private readonly Label lblGiamSanPham = TaoNhanTien();
    private readonly Label lblGiamHoaDon = TaoNhanTien();
    private readonly Label lblLoaiKhuyenMaiHoaDon = new()
    {
        Name = "lblLoaiKhuyenMaiHoaDon",
        AutoSize = true,
        Text = "Không áp dụng",
        ForeColor = GiaoDienChung.ChuPhu
    };
    private readonly Label lblTongTien = TaoNhanTien(true);
    private readonly Label lblTienThua = TaoNhanTien();
    private readonly Label lblTrangThai = new() { AutoSize = true, ForeColor = GiaoDienChung.ChuPhu };
    private readonly Button btnLuu = new() { Text = "Lưu hóa đơn", AutoSize = true };
    private readonly HoaDon? hoaDonSua;
    private readonly Dictionary<string, int> tonKhoCongLai = new(StringComparer.OrdinalIgnoreCase);
    private NhanVien? nhanVien;
    private KhachHang? khachHang;
    private bool khachLe = true;
    private bool dangNapDuLieu;

    public frmTaoHoaDon(HoaDon? hoaDonCanSua = null)
    {
        hoaDonSua = hoaDonCanSua;
        Text = hoaDonSua == null ? "Lập hóa đơn bán hàng" : $"Sửa hóa đơn {hoaDonSua.MaHD}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 600);
        Size = new Size(1100, 760);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = GiaoDienChung.Nen
        };
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(khung);

        var thongTin = TaoKhungThongTin();
        khung.Controls.Add(thongTin, 0, 0);
        khung.Controls.Add(TaoBangChiTiet(), 0, 1);
        khung.Controls.Add(TaoTongTien(), 0, 2);
        khung.Controls.Add(TaoThanhNut(), 0, 3);

        dtpNgayLap.ValueChanged += (_, _) => TinhVaHienThi();
        txtVoucher.TextChanged += (_, _) => TinhVaHienThi();
        nudTienKhachDua.ValueChanged += (_, _) => HienThiTienThua();
        chiTiet.ListChanged += (_, _) => TinhVaHienThi();
        if (hoaDonSua != null)
            NapHoaDonCanSua(hoaDonSua);
        TinhVaHienThi();
    }

    private Control TaoKhungThongTin()
    {
        var thongTin = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(12),
            BackColor = GiaoDienChung.MatTrang
        };
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var btnNhanVien = new Button { Text = "…", Width = 38, Height = 28 };
        btnNhanVien.Click += (_, _) => ChonNhanVien();
        var btnKhachHang = new Button { Text = "…", Width = 38, Height = 28 };
        btnKhachHang.Click += (_, _) => ChonKhachHang();

        thongTin.Controls.Add(TaoNhanTruong("Nhân viên:"), 0, 0);
        thongTin.Controls.Add(txtNhanVien, 1, 0);
        thongTin.Controls.Add(btnNhanVien, 2, 0);
        thongTin.Controls.Add(TaoNhanTruong("Ngày lập:"), 0, 1);
        thongTin.Controls.Add(dtpNgayLap, 1, 1);
        thongTin.Controls.Add(TaoNhanTruong("Khách hàng:"), 2, 1);
        thongTin.Controls.Add(txtKhachHang, 3, 1);
        thongTin.Controls.Add(btnKhachHang, 3, 2);
        thongTin.Controls.Add(TaoNhanTruong("Voucher:"), 0, 2);
        thongTin.Controls.Add(txtVoucher, 1, 2);
        thongTin.SetColumnSpan(txtVoucher, 2);
        thongTin.Controls.Add(lblTrangThai, 0, 3);
        thongTin.SetColumnSpan(lblTrangThai, 4);
        return thongTin;
    }

    private Control TaoBangChiTiet()
    {
        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 12, 0, 12),
            ColumnCount = 1,
            RowCount = 2,
            BackColor = GiaoDienChung.Nen
        };
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var thanhNut = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        var btnThem = GiaoDienChung.TaoNutPhu("Thêm thuốc");
        var btnXoa = GiaoDienChung.TaoNutPhu("Xóa dòng");
        btnThem.Click += (_, _) => ThemThuoc();
        btnXoa.Click += (_, _) => XoaDong();
        thanhNut.Controls.AddRange([btnThem, btnXoa]);

        dgvChiTiet.Dock = DockStyle.Fill;
        dgvChiTiet.AllowUserToAddRows = false;
        dgvChiTiet.AllowUserToDeleteRows = false;
        dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvChiTiet.BackgroundColor = GiaoDienChung.MatTrang;
        dgvChiTiet.RowHeadersVisible = false;
        dgvChiTiet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvChiTiet.MultiSelect = false;
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaThuoc", HeaderText = "Mã thuốc", ReadOnly = true, FillWeight = 75 });
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Tên thuốc", ReadOnly = true, FillWeight = 150 });
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "Số lượng", FillWeight = 65 });
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonGia", HeaderText = "Đơn giá", ReadOnly = true, FillWeight = 90 });
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = "GiamGia", HeaderText = "Giảm SP (%)", ReadOnly = true, FillWeight = 75 });
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = "LoaiCTKM", HeaderText = "Loại CTKM", ReadOnly = true, FillWeight = 90 });
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = "ThanhTien", HeaderText = "Thành tiền", ReadOnly = true, FillWeight = 100 });
        dgvChiTiet.CellValidating += KiemTraSoLuong;
        dgvChiTiet.CellEndEdit += (_, e) =>
        {
            if (dgvChiTiet.Columns["SoLuong"]?.Index == e.ColumnIndex)
                CapNhatSoLuong(e.RowIndex);
        };

        khung.Controls.Add(thanhNut, 0, 0);
        khung.Controls.Add(dgvChiTiet, 0, 1);
        return khung;
    }

    private Control TaoTongTien()
    {
        var tongTien = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(14),
            BackColor = GiaoDienChung.MatTrang
        };
        for (int i = 0; i < 8; i++)
            tongTien.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tongTien.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        tongTien.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        ThemDongTong(tongTien, 0, "Tổng tiền hàng trước giảm:", lblTongGoc);
        ThemDongTong(tongTien, 1, "Giảm giá sản phẩm:", lblGiamSanPham);
        ThemDongTong(tongTien, 2, "Giảm giá hóa đơn:", lblGiamHoaDon);
        ThemDongTong(tongTien, 3, "CTKM hóa đơn / mức giảm:", lblLoaiKhuyenMaiHoaDon);
        ThemDongTong(tongTien, 4, "TỔNG THANH TOÁN:", lblTongTien);
        ThemDongTong(tongTien, 5, "Tiền khách đưa:", nudTienKhachDua);
        ThemDongTong(tongTien, 6, "Tiền thừa:", lblTienThua);
        tongTien.Controls.Add(new Label
        {
            Text = "Tiền khách đưa chỉ dùng để đối chiếu; schema hiện tại không lưu khoản thanh toán.",
            AutoSize = true,
            ForeColor = GiaoDienChung.ChuPhu,
            Padding = new Padding(0, 5, 0, 0)
        }, 0, 7);
        tongTien.SetColumnSpan(tongTien.GetControlFromPosition(0, 7)!, 2);
        return tongTien;
    }

    private Control TaoThanhNut()
    {
        var thanhNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(0, 10, 0, 0),
            FlowDirection = FlowDirection.RightToLeft
        };
        var btnDong = new Button { Text = "Đóng", AutoSize = true, DialogResult = DialogResult.Cancel };
        var btnIn = new Button { Text = "In nháp", AutoSize = true };
        var btnPdf = new Button { Text = "Xuất PDF", AutoSize = true };
        btnLuu.Text = hoaDonSua == null ? "Lưu hóa đơn" : "Cập nhật hóa đơn";
        btnLuu.Click += (_, _) => LuuHoaDon();
        btnIn.Click += (_, _) => InHoaDonNhap();
        btnPdf.Click += (_, _) => XuatPdfHoaDonNhap();
        thanhNut.Controls.AddRange([btnDong, btnPdf, btnIn, btnLuu]);
        CancelButton = btnDong;
        return thanhNut;
    }

    private void ChonNhanVien()
    {
        using var form = new frmChonNhanVien();
        if (form.ShowDialog(this) != DialogResult.OK || form.NhanVienDuocChon == null)
            return;

        nhanVien = form.NhanVienDuocChon;
        txtNhanVien.Text = $"{nhanVien.MaNV} - {nhanVien.HoTenNV}";
        TinhVaHienThi();
    }

    private void ChonKhachHang()
    {
        using var form = new frmChonKhachHang();
        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        khachHang = form.KhachHangDuocChon;
        khachLe = form.ChonKhachLe;
        txtKhachHang.Text = khachLe ? "Khách lẻ" : $"{khachHang!.MaKH} - {khachHang.HoTen}";
        TinhVaHienThi();
    }

    private void ThemThuoc()
    {
        ThuocBUS.TaiLai();
        using var form = new frmChonThuoc(thuoc =>
        {
            tonKhoCongLai.TryGetValue(thuoc.MaThuoc, out int hoanLai);
            DongHoaDonDuThao? dongHienTai = chiTiet.FirstOrDefault(d =>
                d.Thuoc.MaThuoc.Equals(thuoc.MaThuoc, StringComparison.OrdinalIgnoreCase));
            long conCoTheThem = (long)thuoc.SoLuong + hoanLai - (dongHienTai?.SoLuong ?? 0);
            return (int)Math.Clamp(conCoTheThem, 0, int.MaxValue);
        });
        if (form.ShowDialog(this) != DialogResult.OK || form.ThuocDuocChon == null)
            return;

        Thuoc thuoc = form.ThuocDuocChon;
        DongHoaDonDuThao? dongCu = chiTiet.FirstOrDefault(d =>
            d.Thuoc.MaThuoc.Equals(thuoc.MaThuoc, StringComparison.OrdinalIgnoreCase));
        int soLuongMoi = checked((dongCu?.SoLuong ?? 0) + form.SoLuongDuocChon);
        tonKhoCongLai.TryGetValue(thuoc.MaThuoc, out int hoanLai);
        if ((thuoc.HSD.HasValue && thuoc.HSD.Value.Date < dtpNgayLap.Value.Date) ||
            soLuongMoi > (long)thuoc.SoLuong + hoanLai)
        {
            MessageBox.Show("Thuốc đã hết hạn hoặc không đủ tồn kho.", "Không thể thêm thuốc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (dongCu == null)
            chiTiet.Add(new DongHoaDonDuThao(thuoc) { SoLuong = soLuongMoi });
        else
            dongCu.SoLuong = soLuongMoi;
        HienThiChiTiet();
        TinhVaHienThi();
    }

    private void XoaDong()
    {
        if (dgvChiTiet.SelectedRows.Count == 0)
            return;

        DongHoaDonDuThao dong = (DongHoaDonDuThao)dgvChiTiet.SelectedRows[0].Tag!;
        chiTiet.Remove(dong);
        HienThiChiTiet();
        TinhVaHienThi();
    }

    private void KiemTraSoLuong(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (dgvChiTiet.Columns[e.ColumnIndex].Name != "SoLuong")
            return;

        if (!int.TryParse(Convert.ToString(e.FormattedValue), out int soLuong) || soLuong <= 0)
        {
            e.Cancel = true;
            dgvChiTiet.Rows[e.RowIndex].ErrorText = "Số lượng phải là số nguyên lớn hơn 0.";
        }
        else
        {
            dgvChiTiet.Rows[e.RowIndex].ErrorText = string.Empty;
        }
    }

    private void CapNhatSoLuong(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= dgvChiTiet.Rows.Count ||
            !int.TryParse(Convert.ToString(dgvChiTiet.Rows[rowIndex].Cells["SoLuong"].Value), out int soLuong))
            return;

        DongHoaDonDuThao dong = (DongHoaDonDuThao)dgvChiTiet.Rows[rowIndex].Tag!;
        tonKhoCongLai.TryGetValue(dong.Thuoc.MaThuoc, out int hoanLai);
        if (soLuong > (long)dong.Thuoc.SoLuong + hoanLai)
        {
            MessageBox.Show(
                $"Số lượng không được vượt quá tồn khả dụng ({(long)dong.Thuoc.SoLuong + hoanLai:N0}).",
                "Vượt tồn kho",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            dgvChiTiet.Rows[rowIndex].Cells["SoLuong"].Value = dong.SoLuong;
            return;
        }

        dong.SoLuong = soLuong;
        TinhVaHienThi();
    }

    private void HienThiChiTiet()
    {
        dgvChiTiet.Rows.Clear();
        foreach (DongHoaDonDuThao dong in chiTiet)
        {
            int rowIndex = dgvChiTiet.Rows.Add(
                dong.Thuoc.MaThuoc,
                dong.Thuoc.TenThuoc,
                dong.SoLuong,
                dong.Thuoc.DonGia.ToString("N2"),
                dong.PhanTramGiam.ToString("N2"),
                dong.PhanTramGiam > 0 ? $"KMSanPham ({dong.PhanTramGiam:0.##}%)" : "Không",
                dong.ThanhTien.ToString("N2"));
            dgvChiTiet.Rows[rowIndex].Tag = dong;
        }
    }

    private void TinhVaHienThi()
    {
        if (dangNapDuLieu)
            return;

        KetQuaTinhHoaDon ketQua;
        try
        {
            ketQua = hoaDonBUS.TinhTienDuThao(
                chiTiet,
                dtpNgayLap.Value.Date,
                khachLe ? null : khachHang?.MaKH,
                txtVoucher.Text,
                tonKhoCongLai);
        }
        catch (Exception ex)
        {
            HienThiTongTienRong();
            lblTrangThai.Text = $"Không thể tính tiền do lỗi dữ liệu hoặc kết nối: {ex.Message}";
            return;
        }

        if (!ketQua.ThanhCong)
        {
            HienThiTongTienRong();
            lblTrangThai.Text = chiTiet.Count == 0 ? "Thêm thuốc để bắt đầu lập hóa đơn nháp." : ketQua.ThongBao;
            return;
        }

        lblTongGoc.Text = ketQua.TongGoc.ToString("N2");
        lblGiamSanPham.Text = ketQua.TienGiamSanPham.ToString("N2");
        lblGiamHoaDon.Text = ketQua.TienGiamHoaDon.ToString("N2");
        lblLoaiKhuyenMaiHoaDon.Text = ketQua.MaVoucher != null
            ? $"KMVoucher - {ketQua.MaVoucher} ({ketQua.PhanTramGiamHoaDon:0.##}%)"
            : ketQua.MaCTKMHD != null
                ? $"KMHoaDon - {ketQua.MaCTKMHD} ({ketQua.PhanTramGiamHoaDon:0.##}%; mốc {ketQua.MucTienHD:N2} đ)"
                : "Không áp dụng";
        lblTongTien.Text = ketQua.TongTien.ToString("N2");
        foreach (DataGridViewRow row in dgvChiTiet.Rows)
        {
            if (row.Tag is DongHoaDonDuThao dong)
            {
                row.Cells["GiamGia"].Value = dong.PhanTramGiam.ToString("N2");
                row.Cells["LoaiCTKM"].Value = dong.PhanTramGiam > 0
                    ? $"KMSanPham ({dong.PhanTramGiam:0.##}%)"
                    : "Không";
                row.Cells["ThanhTien"].Value = dong.ThanhTien.ToString("N2");
            }
        }

        lblTrangThai.Text = hoaDonSua == null
            ? "Hóa đơn chưa được lưu; tồn kho chỉ thay đổi sau khi lưu thành công."
            : $"Đang sửa hóa đơn {hoaDonSua.MaHD}; tồn kho sẽ cập nhật cùng giao dịch lưu.";
        HienThiTienThua();
    }

    private void LuuHoaDon()
    {
        if (nhanVien == null)
        {
            MessageBox.Show("Hãy chọn nhân viên lập hóa đơn.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        KetQuaTinhHoaDon ketQua;
        try
        {
            ketQua = hoaDonBUS.TinhTienDuThao(
                chiTiet,
                dtpNgayLap.Value.Date,
                khachLe ? null : khachHang?.MaKH,
                txtVoucher.Text,
                tonKhoCongLai);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể kiểm tra hóa đơn do lỗi dữ liệu hoặc kết nối: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!ketQua.ThanhCong)
        {
            MessageBox.Show(ketQua.ThongBao, "Hóa đơn chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (nudTienKhachDua.Value < ketQua.TongTien)
        {
            MessageBox.Show("Tiền khách đưa không được nhỏ hơn tổng thanh toán.", "Chưa đủ tiền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        KetQuaLuuHoaDon ketQuaLuu = hoaDonSua == null
            ? hoaDonBUS.TaoHoaDon(nhanVien, khachLe ? null : khachHang, dtpNgayLap.Value.Date, txtVoucher.Text, chiTiet)
            : hoaDonBUS.SuaHoaDon(hoaDonSua, nhanVien, khachLe ? null : khachHang, dtpNgayLap.Value.Date, txtVoucher.Text, chiTiet);
        if (!ketQuaLuu.ThanhCong)
        {
            MessageBox.Show(ketQuaLuu.ThongBao, "Không thể lưu hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show(
            $"{ketQuaLuu.ThongBao}\nMã hóa đơn: {ketQuaLuu.MaHD}\nTiền khách đưa: {nudTienKhachDua.Value:N2} đ\nTiền thừa: {nudTienKhachDua.Value - ketQua.TongTien:N2} đ",
            "Hoàn tất bán hàng", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void HienThiTongTienRong()
    {
        lblTongGoc.Text = "0,00";
        lblGiamSanPham.Text = "0,00";
        lblGiamHoaDon.Text = "0,00";
        lblLoaiKhuyenMaiHoaDon.Text = "Không áp dụng";
        lblTongTien.Text = "0,00";
        lblTienThua.Text = "0,00";
    }

    private static Label TaoNhanTien(bool noiBat = false) => new()
    {
        AutoSize = true,
        TextAlign = ContentAlignment.MiddleRight,
        Dock = DockStyle.Fill,
        Font = noiBat ? new Font("Segoe UI", 13F, FontStyle.Bold) : GiaoDienChung.PhongChu,
        ForeColor = noiBat ? GiaoDienChung.XanhChinh : GiaoDienChung.Chu
    };

    private static Label TaoNhanTruong(string noiDung) => new()
    {
        Text = noiDung,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Padding = new Padding(0, 5, 8, 5)
    };

    private static void ThemDongTong(TableLayoutPanel bang, int dong, string tieuDe, Control giaTri)
    {
        var nhan = TaoNhanTruong(tieuDe);
        nhan.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        bang.Controls.Add(nhan, 0, dong);
        bang.Controls.Add(giaTri, 1, dong);
    }

    private void NapHoaDonCanSua(HoaDon hoaDon)
    {
        dangNapDuLieu = true;
        try
        {
            nhanVien = new NhanVien { MaNV = hoaDon.MaNV, HoTenNV = hoaDon.TenNhanVien };
            txtNhanVien.Text = $"{nhanVien.MaNV} - {nhanVien.HoTenNV}";
            khachLe = string.IsNullOrWhiteSpace(hoaDon.MaKH);
            khachHang = khachLe ? null : new KhachHang { MaKH = hoaDon.MaKH!, HoTen = hoaDon.TenKhachHang };
            txtKhachHang.Text = khachLe ? "Khách lẻ" : $"{khachHang!.MaKH} - {khachHang.HoTen}";
            dtpNgayLap.Value = hoaDon.NgayLap;
            txtVoucher.Text = hoaDon.MaVoucher ?? string.Empty;

            ThuocBUS.TaiLai();
            foreach (object? dong in hoaDonBUS.LayChiTiet(hoaDon.MaHD))
            {
                if (dong is not CTHoaDon chiTietCu)
                    throw new InvalidOperationException("Dữ liệu chi tiết hóa đơn không hợp lệ.");
                Thuoc thuoc = thuocBUS.LayTheoMa(chiTietCu.MaThuoc)
                    ?? throw new InvalidOperationException($"Không tìm thấy thuốc {chiTietCu.MaThuoc} để sửa hóa đơn.");
                chiTiet.Add(new DongHoaDonDuThao(thuoc) { SoLuong = chiTietCu.SoLuong });
                tonKhoCongLai.TryGetValue(chiTietCu.MaThuoc, out int soLuong);
                tonKhoCongLai[chiTietCu.MaThuoc] = checked(soLuong + chiTietCu.SoLuong);
            }
        }
        catch (Exception ex)
        {
            btnLuu.Enabled = false;
            lblTrangThai.Text = $"Không tải được dữ liệu hóa đơn để sửa: {ex.Message}";
        }
        finally
        {
            dangNapDuLieu = false;
        }
        HienThiChiTiet();
    }

    private void HienThiTienThua()
    {
        if (decimal.TryParse(lblTongTien.Text, out decimal tongTien) && nudTienKhachDua.Value >= tongTien)
            lblTienThua.Text = (nudTienKhachDua.Value - tongTien).ToString("N2");
        else
            lblTienThua.Text = "0,00";
    }

    private void InHoaDonNhap()
    {
        if (TaoHoaDonIn(out HoaDon hoaDon, out List<CTHoaDon> chiTietIn))
            HoaDonIn.In(this, hoaDon, chiTietIn);
    }

    private void XuatPdfHoaDonNhap()
    {
        if (TaoHoaDonIn(out HoaDon hoaDon, out List<CTHoaDon> chiTietIn))
            HoaDonIn.XuatPdf(this, hoaDon, chiTietIn);
    }

    private bool TaoHoaDonIn(out HoaDon hoaDon, out List<CTHoaDon> chiTietIn)
    {
        hoaDon = new HoaDon();
        chiTietIn = [];
        KetQuaTinhHoaDon tinhTien;
        try
        {
            tinhTien = hoaDonBUS.TinhTienDuThao(
                chiTiet,
                dtpNgayLap.Value.Date,
                khachLe ? null : khachHang?.MaKH,
                txtVoucher.Text,
                tonKhoCongLai);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể chuẩn bị hóa đơn để in hoặc xuất PDF: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        if (!tinhTien.ThanhCong)
        {
            MessageBox.Show(tinhTien.ThongBao, "Hóa đơn chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var hoaDonMoi = new HoaDon
        {
            MaHD = hoaDonSua?.MaHD ?? "NHÁP",
            MaNV = nhanVien?.MaNV ?? string.Empty,
            TenNhanVien = nhanVien?.HoTenNV ?? string.Empty,
            NgayLap = dtpNgayLap.Value.Date,
            MaKH = khachLe ? null : khachHang?.MaKH,
            TenKhachHang = khachLe ? "Khách lẻ" : khachHang?.HoTen ?? "Khách hàng",
            TongTien = tinhTien.TongTien,
            TienGiam = tinhTien.TienGiamHoaDon
        };
        hoaDon = hoaDonMoi;
        chiTietIn = chiTiet.Select(dong => new CTHoaDon
        {
            MaHD = hoaDonMoi.MaHD,
            MaThuoc = dong.Thuoc.MaThuoc,
            TenThuoc = dong.Thuoc.TenThuoc,
            SoLuong = dong.SoLuong,
            DonGia = dong.Thuoc.DonGia,
            ThanhTien = dong.ThanhTien
        }).ToList();
        return true;
    }
}

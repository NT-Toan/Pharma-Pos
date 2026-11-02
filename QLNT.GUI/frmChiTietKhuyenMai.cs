using System.Collections;
using QLNT.BUS;
using QLNT.DAO;

namespace QLNT.GUI;

public sealed class frmChiTietKhuyenMai : Form
{
    private readonly CTKhuyenMai ctkm;
    private readonly KMSanPhamBUS busSP = new();
    private readonly KMVoucherBUS busVC = new();
    private readonly KMHoaDonBUS busHD = new();

    // Tab 1: Sản phẩm
    private readonly TextBox txtMaThuoc = new();
    private readonly NumericUpDown nudPtGiamSP = new();
    private readonly ListView lvSanPham = new();

    // Tab 2: Voucher
    private readonly TextBox txtMaVoucher = new();
    private readonly TextBox txtMaKH = new();
    private readonly NumericUpDown nudPtGiamVC = new();
    private readonly ListView lvVoucher = new();

    // Tab 3: Mốc hóa đơn
    private readonly NumericUpDown nudMucTien = new();
    private readonly NumericUpDown nudPtGiamHD = new();
    private readonly ListView lvHoaDon = new();

    public frmChiTietKhuyenMai(CTKhuyenMai ctkm)
    {
        this.ctkm = ctkm;

        Text = $"Cấu hình chi tiết giảm giá — [{ctkm.MaCTKM}] {ctkm.TenCTKM}";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1000, 640);
        MinimumSize = new Size(950, 580);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(16, 8)
        };

        tabControl.TabPages.Add(TaoTabSanPham());
        tabControl.TabPages.Add(TaoTabVoucher());
        tabControl.TabPages.Add(TaoTabHoaDon());

        Controls.Add(tabControl);

        Load += (_, _) =>
        {
            TaiDSSanPham();
            TaiDSVoucher();
            TaiDSHoaDon();
        };
    }

    // ==========================================
    // TAB 1: GIẢM THEO THUỐC
    // ==========================================
    private TabPage TaoTabSanPham()
    {
        var tab = new TabPage("1. Giảm theo thuốc (KM_SANPHAM)");
        tab.BackColor = GiaoDienChung.Nen;

        var grp = new GroupBox
        {
            Text = "Thông tin giảm giá theo thuốc",
            Dock = DockStyle.Top,
            Height = 175,
            Padding = new Padding(16, 12, 16, 12),
            ForeColor = GiaoDienChung.Chu
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        txtMaThuoc.Dock = DockStyle.Fill;
        txtMaThuoc.PlaceholderText = "Nhập mã thuốc (VD: TH0001)...";

        nudPtGiamSP.Minimum = 1;
        nudPtGiamSP.Maximum = 100;
        nudPtGiamSP.Value = 10;
        nudPtGiamSP.Dock = DockStyle.Fill;

        tbl.Controls.Add(new Label { Text = "Mã thuốc:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        tbl.Controls.Add(txtMaThuoc, 1, 0);
        tbl.Controls.Add(new Label { Text = "Mức giảm (%):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        tbl.Controls.Add(nudPtGiamSP, 1, 1);

        var pnlNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0)
        };

        var btnLamTrong = GiaoDienChung.TaoNutPhu("Làm trống");
        btnLamTrong.Click += (_, _) => LamTrongSP();

        var btnXoa = GiaoDienChung.TaoNutPhu("Xóa");
        btnXoa.Click += (_, _) => XoaSP();

        var btnSua = GiaoDienChung.TaoNutPhu("Cập nhật");
        btnSua.Click += (_, _) => SuaSP();

        var btnThem = GiaoDienChung.TaoNutPhu("Thêm mới");
        btnThem.BackColor = GiaoDienChung.XanhChinh;
        btnThem.ForeColor = Color.White;
        btnThem.Click += (_, _) => ThemSP();

        pnlNut.Controls.Add(btnLamTrong);
        pnlNut.Controls.Add(btnXoa);
        pnlNut.Controls.Add(btnSua);
        pnlNut.Controls.Add(btnThem);

        tbl.Controls.Add(pnlNut, 0, 2);
        tbl.SetColumnSpan(pnlNut, 4);

        grp.Controls.Add(tbl);

        lvSanPham.Dock = DockStyle.Fill;
        lvSanPham.View = View.Details;
        lvSanPham.FullRowSelect = true;
        lvSanPham.GridLines = true;
        lvSanPham.Columns.Add("Mã CTKM", 140);
        lvSanPham.Columns.Add("Mã thuốc áp dụng", 240);
        lvSanPham.Columns.Add("Phần trăm giảm (%)", 180);
        lvSanPham.SelectedIndexChanged += (_, _) =>
        {
            if (lvSanPham.SelectedItems.Count > 0 && lvSanPham.SelectedItems[0].Tag is KMSanPham sp)
            {
                txtMaThuoc.Text = sp.MaThuoc;
                txtMaThuoc.ReadOnly = true;
                nudPtGiamSP.Value = Math.Clamp(sp.PhanTramGiam, 1, 100);
            }
        };

        tab.Controls.Add(lvSanPham);
        tab.Controls.Add(grp);
        return tab;
    }

    private void LamTrongSP()
    {
        txtMaThuoc.Text = string.Empty;
        txtMaThuoc.ReadOnly = false;
        nudPtGiamSP.Value = 10;
        lvSanPham.SelectedIndices.Clear();
    }

    private void TaiDSSanPham()
    {
        lvSanPham.Items.Clear();
        ArrayList ds = busSP.LayTheoCTKM(ctkm.MaCTKM);
        foreach (KMSanPham sp in ds)
        {
            var item = new ListViewItem(new[] { sp.MaCTKM, sp.MaThuoc, $"{sp.PhanTramGiam:0.##}%" }) { Tag = sp };
            lvSanPham.Items.Add(item);
        }
    }

    private void ThemSP()
    {
        string maThuoc = txtMaThuoc.Text.Trim();
        if (string.IsNullOrWhiteSpace(maThuoc))
        {
            MessageBox.Show("Vui lòng nhập mã thuốc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var sp = new KMSanPham(ctkm.MaCTKM, maThuoc, nudPtGiamSP.Value);
        var kq = busSP.Luu(sp);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrongSP();
            TaiDSSanPham();
        }
    }

    private void SuaSP()
    {
        string maThuoc = txtMaThuoc.Text.Trim();
        if (string.IsNullOrWhiteSpace(maThuoc))
        {
            MessageBox.Show("Vui lòng chọn thuốc trong danh sách để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var sp = new KMSanPham(ctkm.MaCTKM, maThuoc, nudPtGiamSP.Value);
        var kq = busSP.Luu(sp);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Cập nhật thành công." : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            TaiDSSanPham();
        }
    }

    private void XoaSP()
    {
        string maThuoc = txtMaThuoc.Text.Trim();
        if (string.IsNullOrWhiteSpace(maThuoc))
        {
            MessageBox.Show("Vui lòng chọn thuốc trong danh sách để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var kq = busSP.Xoa(ctkm.MaCTKM, maThuoc);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrongSP();
            TaiDSSanPham();
        }
    }

    // ==========================================
    // TAB 2: VOUCHER
    // ==========================================
    private TabPage TaoTabVoucher()
    {
        var tab = new TabPage("2. Mã Voucher (KM_VOUCHER)");
        tab.BackColor = GiaoDienChung.Nen;

        var grp = new GroupBox
        {
            Text = "Thông tin Voucher",
            Dock = DockStyle.Top,
            Height = 175,
            Padding = new Padding(16, 12, 16, 12),
            ForeColor = GiaoDienChung.Chu
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        txtMaVoucher.Dock = DockStyle.Fill;
        txtMaVoucher.PlaceholderText = "VD: VC2026_01...";

        txtMaKH.Dock = DockStyle.Fill;
        txtMaKH.PlaceholderText = "Để trống = Áp dụng cho tất cả khách";

        nudPtGiamVC.Minimum = 1;
        nudPtGiamVC.Maximum = 100;
        nudPtGiamVC.Value = 10;
        nudPtGiamVC.Dock = DockStyle.Fill;

        tbl.Controls.Add(new Label { Text = "Mã Voucher:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        tbl.Controls.Add(txtMaVoucher, 1, 0);
        tbl.Controls.Add(new Label { Text = "Mã KH áp dụng:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        tbl.Controls.Add(txtMaKH, 3, 0);

        tbl.Controls.Add(new Label { Text = "Mức giảm (%):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        tbl.Controls.Add(nudPtGiamVC, 1, 1);

        var pnlNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0)
        };

        var btnLamTrong = GiaoDienChung.TaoNutPhu("Làm trống");
        btnLamTrong.Click += (_, _) => LamTrongVoucher();

        var btnXoa = GiaoDienChung.TaoNutPhu("Xóa");
        btnXoa.Click += (_, _) => XoaVoucher();

        var btnSua = GiaoDienChung.TaoNutPhu("Cập nhật");
        btnSua.Click += (_, _) => SuaVoucher();

        var btnThem = GiaoDienChung.TaoNutPhu("Thêm mới");
        btnThem.BackColor = GiaoDienChung.XanhChinh;
        btnThem.ForeColor = Color.White;
        btnThem.Click += (_, _) => ThemVoucher();

        pnlNut.Controls.Add(btnLamTrong);
        pnlNut.Controls.Add(btnXoa);
        pnlNut.Controls.Add(btnSua);
        pnlNut.Controls.Add(btnThem);

        tbl.Controls.Add(pnlNut, 0, 2);
        tbl.SetColumnSpan(pnlNut, 4);

        grp.Controls.Add(tbl);

        lvVoucher.Dock = DockStyle.Fill;
        lvVoucher.View = View.Details;
        lvVoucher.FullRowSelect = true;
        lvVoucher.GridLines = true;
        lvVoucher.Columns.Add("Mã Voucher", 160);
        lvVoucher.Columns.Add("Khách hàng định danh", 240);
        lvVoucher.Columns.Add("Phần trăm giảm (%)", 180);
        lvVoucher.SelectedIndexChanged += (_, _) =>
        {
            if (lvVoucher.SelectedItems.Count > 0 && lvVoucher.SelectedItems[0].Tag is KMVoucher vc)
            {
                txtMaVoucher.Text = vc.MaVoucher;
                txtMaVoucher.ReadOnly = true;
                txtMaKH.Text = vc.MaKH ?? string.Empty;
                nudPtGiamVC.Value = Math.Clamp(vc.PhanTramGiam, 1, 100);
            }
        };

        tab.Controls.Add(lvVoucher);
        tab.Controls.Add(grp);
        return tab;
    }

    private void LamTrongVoucher()
    {
        txtMaVoucher.Text = string.Empty;
        txtMaVoucher.ReadOnly = false;
        txtMaKH.Text = string.Empty;
        nudPtGiamVC.Value = 10;
        lvVoucher.SelectedIndices.Clear();
    }

    private void TaiDSVoucher()
    {
        lvVoucher.Items.Clear();
        ArrayList ds = busVC.LayTheoCTKM(ctkm.MaCTKM);
        foreach (KMVoucher vc in ds)
        {
            string kh = string.IsNullOrEmpty(vc.MaKH) ? "(Tất cả khách)" : vc.MaKH;
            var item = new ListViewItem(new[] { vc.MaVoucher, kh, $"{vc.PhanTramGiam:0.##}%" }) { Tag = vc };
            lvVoucher.Items.Add(item);
        }
    }

    private void ThemVoucher()
    {
        string maVC = txtMaVoucher.Text.Trim();
        if (string.IsNullOrWhiteSpace(maVC))
        {
            MessageBox.Show("Vui lòng nhập mã voucher.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string? maKH = string.IsNullOrWhiteSpace(txtMaKH.Text) ? null : txtMaKH.Text.Trim();
        var vc = new KMVoucher(maVC, ctkm.MaCTKM, maKH, nudPtGiamVC.Value);
        var kq = busVC.Them(vc);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrongVoucher();
            TaiDSVoucher();
        }
    }

    private void SuaVoucher()
    {
        string maVC = txtMaVoucher.Text.Trim();
        if (string.IsNullOrWhiteSpace(maVC))
        {
            MessageBox.Show("Vui lòng chọn voucher từ danh sách để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string? maKH = string.IsNullOrWhiteSpace(txtMaKH.Text) ? null : txtMaKH.Text.Trim();
        var vc = new KMVoucher(maVC, ctkm.MaCTKM, maKH, nudPtGiamVC.Value);
        var kq = busVC.Sua(vc);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            TaiDSVoucher();
        }
    }

    private void XoaVoucher()
    {
        string maVC = txtMaVoucher.Text.Trim();
        if (string.IsNullOrWhiteSpace(maVC))
        {
            MessageBox.Show("Vui lòng chọn voucher để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var kq = busVC.Xoa(maVC);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrongVoucher();
            TaiDSVoucher();
        }
    }

    // ==========================================
    // TAB 3: MỐC HÓA ĐƠN
    // ==========================================
    private TabPage TaoTabHoaDon()
    {
        var tab = new TabPage("3. Mốc tiền hóa đơn (KM_HOADON)");
        tab.BackColor = GiaoDienChung.Nen;

        var grp = new GroupBox
        {
            Text = "Thông tin mốc giảm giá theo hóa đơn",
            Dock = DockStyle.Top,
            Height = 175,
            Padding = new Padding(16, 12, 16, 12),
            ForeColor = GiaoDienChung.Chu
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        nudMucTien.Minimum = 0;
        nudMucTien.Maximum = 100000000;
        nudMucTien.Increment = 50000;
        nudMucTien.ThousandsSeparator = true;
        nudMucTien.Value = 200000;
        nudMucTien.Dock = DockStyle.Fill;

        nudPtGiamHD.Minimum = 1;
        nudPtGiamHD.Maximum = 100;
        nudPtGiamHD.Value = 5;
        nudPtGiamHD.Dock = DockStyle.Fill;

        tbl.Controls.Add(new Label { Text = "Mức tiền (VNĐ):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        tbl.Controls.Add(nudMucTien, 1, 0);
        tbl.Controls.Add(new Label { Text = "Mức giảm (%):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        tbl.Controls.Add(nudPtGiamHD, 1, 1);

        var pnlNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0)
        };

        var btnLamTrong = GiaoDienChung.TaoNutPhu("Làm trống");
        btnLamTrong.Click += (_, _) => LamTrongHD();

        var btnXoa = GiaoDienChung.TaoNutPhu("Xóa");
        btnXoa.Click += (_, _) => XoaHD();

        var btnSua = GiaoDienChung.TaoNutPhu("Cập nhật");
        btnSua.Click += (_, _) => SuaHD();

        var btnThem = GiaoDienChung.TaoNutPhu("Thêm mới");
        btnThem.BackColor = GiaoDienChung.XanhChinh;
        btnThem.ForeColor = Color.White;
        btnThem.Click += (_, _) => ThemHD();

        pnlNut.Controls.Add(btnLamTrong);
        pnlNut.Controls.Add(btnXoa);
        pnlNut.Controls.Add(btnSua);
        pnlNut.Controls.Add(btnThem);

        tbl.Controls.Add(pnlNut, 0, 2);
        tbl.SetColumnSpan(pnlNut, 4);

        grp.Controls.Add(tbl);

        lvHoaDon.Dock = DockStyle.Fill;
        lvHoaDon.View = View.Details;
        lvHoaDon.FullRowSelect = true;
        lvHoaDon.GridLines = true;
        lvHoaDon.Columns.Add("Mã CTKM", 140);
        lvHoaDon.Columns.Add("Mức tiền hóa đơn tối thiểu", 250);
        lvHoaDon.Columns.Add("Phần trăm giảm (%)", 180);
        lvHoaDon.SelectedIndexChanged += (_, _) =>
        {
            if (lvHoaDon.SelectedItems.Count > 0 && lvHoaDon.SelectedItems[0].Tag is KMHoaDon hd)
            {
                nudMucTien.Value = Math.Clamp(hd.MucTienHD, 0, 100000000);
                nudPtGiamHD.Value = Math.Clamp(hd.PhanTramGiam, 1, 100);
            }
        };

        tab.Controls.Add(lvHoaDon);
        tab.Controls.Add(grp);
        return tab;
    }

    private void LamTrongHD()
    {
        nudMucTien.Value = 200000;
        nudPtGiamHD.Value = 5;
        lvHoaDon.SelectedIndices.Clear();
    }

    private void TaiDSHoaDon()
    {
        lvHoaDon.Items.Clear();
        ArrayList ds = busHD.LayTheoCTKM(ctkm.MaCTKM);
        foreach (KMHoaDon hd in ds)
        {
            var item = new ListViewItem(new[] { hd.MaCTKM, $"{hd.MucTienHD:N0} đ", $"{hd.PhanTramGiam:0.##}%" }) { Tag = hd };
            lvHoaDon.Items.Add(item);
        }
    }

    private void ThemHD()
    {
        var hd = new KMHoaDon(ctkm.MaCTKM, nudMucTien.Value, nudPtGiamHD.Value);
        var kq = busHD.Luu(hd);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thêm mới thành công." : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrongHD();
            TaiDSHoaDon();
        }
    }

    private void SuaHD()
    {
        if (lvHoaDon.SelectedItems.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn mốc tiền trong danh sách để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var hd = new KMHoaDon(ctkm.MaCTKM, nudMucTien.Value, nudPtGiamHD.Value);
        var kq = busHD.Luu(hd);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Cập nhật thành công." : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            TaiDSHoaDon();
        }
    }

    private void XoaHD()
    {
        if (lvHoaDon.SelectedItems.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn mốc tiền trong danh sách để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var kq = busHD.Xoa(ctkm.MaCTKM, nudMucTien.Value);
        MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Lỗi",
            MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (kq.ThanhCong)
        {
            LamTrongHD();
            TaiDSHoaDon();
        }
    }
}
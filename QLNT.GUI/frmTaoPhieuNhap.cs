using System.Collections;

namespace QLNT.GUI;

public sealed class frmTaoPhieuNhap : Form
{
    private readonly PhieuNhapBUS bus = new();
    private ArrayList chiTiet = new();
    private NhanVien? nhanVien;
    private NhaCungCap? nhaCungCap;
    private Thuoc? thuocDangChon;
    private bool dangLuu;
    private DuLieuPhieuNhap? banGoc;

    private readonly TextBox txtMa = OChiDoc("txtMaPN");
    private readonly TextBox txtNhanVien = OChiDoc("txtNhanVien");
    private readonly TextBox txtNCC = OChiDoc("txtNCC");
    private readonly TextBox txtThuoc = OChiDoc("txtThuoc");
    private readonly TextBox txtTongTien = OChiDoc("txtTongTien");
    private readonly DateTimePicker dtpNgayNhap = new()
    {
        Dock = DockStyle.Fill,
        Format = DateTimePickerFormat.Short,
        Value = DateTime.Today,
        Enabled = false
    };
    private readonly NumericUpDown nudSoLuong = new()
    {
        Name = "nudSoLuong",
        Dock = DockStyle.Fill,
        Minimum = 1,
        Maximum = int.MaxValue,
        Value = 1,
        ThousandsSeparator = true
    };
    private readonly NumericUpDown nudDonGia = new()
    {
        Name = "nudDonGiaNhap",
        Dock = DockStyle.Fill,
        DecimalPlaces = 2,
        Maximum = 9999999999999999.99m,
        ThousandsSeparator = true
    };
    private readonly ListView lvChiTiet = new() { Name = "lvChiTietDuThao" };
    private readonly Button btnThemThuoc = new() { Name = "btnThemThuocVaoPhieu", Text = "Thêm thuốc", AutoSize = true, Enabled = false };
    private readonly Button btnCapNhat = new() { Name = "btnCapNhatThuocTrongPhieu", Text = "Cập nhật", AutoSize = true, Enabled = false };
    private readonly Button btnBoThuoc = new() { Name = "btnBoThuocKhoiPhieu", Text = "Bỏ khỏi phiếu", AutoSize = true, Enabled = false };
    private readonly Button btnNhapMoi = new() { Name = "btnNhapThuocMoi", Text = "Nhập mới", AutoSize = true };
    private readonly Button btnLuu = new() { Name = "btnLuuPhieu", Text = "Lưu phiếu", AutoSize = true };

    public string? MaPhieuDaLuu { get; private set; }

    public frmTaoPhieuNhap(string? maPN = null)
    {
        Text = maPN == null ? "Thêm phiếu nhập" : "Sửa phiếu nhập";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(950, 680);
        MinimumSize = new Size(800, 600);

        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 5
        };
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        var thongTin = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        thongTin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 3; i++)
            thongTin.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3));
        thongTin.Controls.Add(Nhan("Mã phiếu:"), 0, 0);
        thongTin.Controls.Add(txtMa, 1, 0);
        thongTin.Controls.Add(Nhan("Ngày nhập:"), 2, 0);
        thongTin.Controls.Add(dtpNgayNhap, 3, 0);
        thongTin.Controls.Add(Nhan("Nhân viên:"), 0, 1);
        TableLayoutPanel oNhanVien = OChon(txtNhanVien, ChonNhanVien);
        thongTin.Controls.Add(oNhanVien, 1, 1);
        thongTin.SetColumnSpan(oNhanVien, 3);
        thongTin.Controls.Add(Nhan("Nhà cung cấp:"), 0, 2);
        TableLayoutPanel oNCC = OChon(txtNCC, ChonNhaCungCap);
        thongTin.Controls.Add(oNCC, 1, 2);
        thongTin.SetColumnSpan(oNCC, 3);

        var nhomThuoc = new GroupBox { Text = "Thuốc nhập", Dock = DockStyle.Fill };
        var nhapThuoc = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
        nhapThuoc.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        nhapThuoc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        nhapThuoc.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        nhapThuoc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        nhapThuoc.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        nhapThuoc.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        nhapThuoc.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        nhapThuoc.Controls.Add(Nhan("Thuốc:"), 0, 0);
        TableLayoutPanel oThuoc = OChon(txtThuoc, ChonThuoc);
        nhapThuoc.Controls.Add(oThuoc, 1, 0);
        nhapThuoc.SetColumnSpan(oThuoc, 3);
        nhapThuoc.Controls.Add(Nhan("Số lượng:"), 0, 1);
        nhapThuoc.Controls.Add(nudSoLuong, 1, 1);
        nhapThuoc.Controls.Add(Nhan("Đơn giá nhập:"), 2, 1);
        nhapThuoc.Controls.Add(nudDonGia, 3, 1);

        var thanhDong = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        btnThemThuoc.Click += (_, _) => GhiDong(false);
        btnCapNhat.Click += (_, _) => GhiDong(true);
        btnBoThuoc.Click += (_, _) => BoDong();
        btnNhapMoi.Click += (_, _) => ChuanBiDong();
        thanhDong.Controls.AddRange([btnThemThuoc, btnCapNhat, btnBoThuoc, btnNhapMoi]);
        nhapThuoc.Controls.Add(thanhDong, 0, 2);
        nhapThuoc.SetColumnSpan(thanhDong, 4);
        nhomThuoc.Controls.Add(nhapThuoc);

        lvChiTiet.Dock = DockStyle.Fill;
        lvChiTiet.View = View.Details;
        lvChiTiet.FullRowSelect = true;
        lvChiTiet.MultiSelect = false;
        lvChiTiet.GridLines = true;
        lvChiTiet.HideSelection = false;
        lvChiTiet.Columns.Add("Mã thuốc", 100);
        lvChiTiet.Columns.Add("Tên thuốc", 280);
        lvChiTiet.Columns.Add("Số lượng", 100);
        lvChiTiet.Columns.Add("Đơn giá nhập", 150);
        lvChiTiet.Columns.Add("Thành tiền", 160);
        lvChiTiet.SelectedIndexChanged += (_, _) => ChonDong();

        var tong = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        tong.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tong.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        tong.Controls.Add(Nhan("Tổng tiền phiếu nhập:"), 0, 0);
        txtTongTien.Text = "0.00";
        txtTongTien.TextAlign = HorizontalAlignment.Right;
        tong.Controls.Add(txtTongTien, 1, 0);

        var thanhNut = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var btnHuy = new Button { Text = "Đóng", AutoSize = true, DialogResult = DialogResult.Cancel };
        btnLuu.Click += (_, _) => LuuPhieu();
        thanhNut.Controls.AddRange([btnHuy, btnLuu]);
        CancelButton = btnHuy;

        khung.Controls.Add(thongTin, 0, 0);
        khung.Controls.Add(nhomThuoc, 0, 1);
        khung.Controls.Add(lvChiTiet, 0, 2);
        khung.Controls.Add(tong, 0, 3);
        khung.Controls.Add(thanhNut, 0, 4);
        Controls.Add(khung);
        txtThuoc.TextChanged += (_, _) => CapNhatNutChiTiet();
        nudSoLuong.ValueChanged += (_, _) => CapNhatNutChiTiet();
        nudDonGia.ValueChanged += (_, _) => CapNhatNutChiTiet();
        CapNhatNutChiTiet();
        Load += (_, _) =>
        {
            try
            {
                PhieuNhapBUS.TaiLai();
                if (maPN == null)
                    txtMa.Text = bus.TaoMaMoi();
                else
                    NapPhieuSua(maPN);
            }
            catch (Exception ex)
            {
                btnLuu.Enabled = false;
                BaoLoi(ex.Message);
            }
        };
    }

    private void NapPhieuSua(string maPN)
    {
        banGoc = bus.LayDeSua(maPN);
        PhieuNhap phieu = banGoc.Phieu;
        txtMa.Text = phieu.MaPN;
        dtpNgayNhap.Value = phieu.NgayNhap;
        nhanVien = new NhanVien { MaNV = phieu.MaNV, HoTenNV = phieu.TenNhanVien };
        nhaCungCap = new NhaCungCap { MaNCC = phieu.MaNCC, TenNCC = phieu.TenNCC };
        txtNhanVien.Text = $"{phieu.MaNV} - {phieu.TenNhanVien}";
        txtNCC.Text = $"{phieu.MaNCC} - {phieu.TenNCC}";
        foreach (CTPhieuNhap dong in banGoc.ChiTiet)
            chiTiet.Add(new CTPhieuNhap
            {
                MaPN = dong.MaPN, MaThuoc = dong.MaThuoc, TenThuoc = dong.TenThuoc,
                SoLuong = dong.SoLuong, DonGia = dong.DonGia
            });
        HienThiChiTiet(bus.TinhTongTien(chiTiet));
    }

    private static TextBox OChiDoc(string ten) => new() { Name = ten, Dock = DockStyle.Fill, ReadOnly = true };
    private static Label Nhan(string text) => new() { Text = text, AutoSize = true, Anchor = AnchorStyles.Left };

    private static TableLayoutPanel OChon(TextBox o, Action chon)
    {
        var khung = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = Padding.Empty };
        khung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
        var btn = new Button { Text = "…", Dock = DockStyle.Fill, Margin = new Padding(3, 0, 0, 0) };
        btn.Click += (_, _) => chon();
        khung.Controls.Add(o, 0, 0);
        khung.Controls.Add(btn, 1, 0);
        return khung;
    }

    private void ChonNhanVien()
    {
        using var form = new frmChonNhanVien();
        if (form.ShowDialog(this) == DialogResult.OK && frmChonNhanVien.NhanVienDuocChon is NhanVien nv)
        {
            nhanVien = nv;
            txtNhanVien.Text = $"{nv.MaNV} - {nv.HoTenNV}";
        }
    }

    private void ChonNhaCungCap()
    {
        using var form = new frmChonNhaCungCap();
        if (form.ShowDialog(this) == DialogResult.OK && form.NhaCungCapDuocChon is NhaCungCap ncc)
        {
            nhaCungCap = ncc;
            txtNCC.Text = $"{ncc.MaNCC} - {ncc.TenNCC}";
        }
    }

    private void ChonThuoc()
    {
        // Nhập hàng được chọn cả thuốc tồn 0; giới hạn theo sức chứa kiểu int.
        using var form = new frmChonThuoc(t => (int)Math.Min(int.MaxValue,
            (long)int.MaxValue - t.SoLuong + (banGoc?.ChiTiet.Cast<CTPhieuNhap>()
                .FirstOrDefault(d => d.MaThuoc.Equals(t.MaThuoc, StringComparison.OrdinalIgnoreCase))?.SoLuong ?? 0)));
        if (form.ShowDialog(this) == DialogResult.OK && form.ThuocDuocChon is Thuoc thuoc)
        {
            thuocDangChon = thuoc;
            txtThuoc.Text = $"{thuoc.MaThuoc} - {thuoc.TenThuoc}";
            nudSoLuong.Value = form.SoLuongDuocChon;
            nudDonGia.Value = 0;
            CapNhatNutChiTiet();
        }
    }

    private void ChonDong()
    {
        if (lvChiTiet.SelectedItems.Count == 0)
        {
            CapNhatNutChiTiet();
            return;
        }

        var dong = (CTPhieuNhap)lvChiTiet.SelectedItems[0].Tag!;
        thuocDangChon = new Thuoc { MaThuoc = dong.MaThuoc, TenThuoc = dong.TenThuoc };
        txtThuoc.Text = $"{dong.MaThuoc} - {dong.TenThuoc}";
        nudSoLuong.Value = dong.SoLuong;
        nudDonGia.Value = dong.DonGia;
        CapNhatNutChiTiet();
    }

    private void ChuanBiDong()
    {
        lvChiTiet.SelectedItems.Clear();
        thuocDangChon = null;
        txtThuoc.Clear();
        nudSoLuong.Value = 1;
        nudDonGia.Value = 0;
        CapNhatNutChiTiet();
    }

    private CTPhieuNhap DocThuocDangNhap() => new()
    {
        MaThuoc = thuocDangChon!.MaThuoc,
        TenThuoc = thuocDangChon.TenThuoc,
        SoLuong = decimal.ToInt32(nudSoLuong.Value),
        DonGia = nudDonGia.Value
    };

    private void CapNhatNutChiTiet()
    {
        bool dangSuaThuoc = lvChiTiet.SelectedItems.Count > 0;
        btnThemThuoc.Enabled = false;
        btnCapNhat.Enabled = false;
        btnBoThuoc.Enabled = dangSuaThuoc && !dangLuu;
        btnNhapMoi.Enabled = !dangLuu;
        if (thuocDangChon == null || dangLuu) return;

        // BUS kiểm tra cùng dữ liệu dự kiến sẽ thêm/cập nhật; GUI chỉ phản ánh bằng trạng thái nút.
        var duThao = new ArrayList(chiTiet);
        if (dangSuaThuoc)
        {
            int viTri = chiTiet.IndexOf(lvChiTiet.SelectedItems[0].Tag!);
            if (viTri < 0) return;
            duThao[viTri] = DocThuocDangNhap();
        }
        else
            duThao.Add(DocThuocDangNhap());
        try
        {
            bus.TinhTongTien(duThao);
            btnThemThuoc.Enabled = !dangSuaThuoc;
            btnCapNhat.Enabled = dangSuaThuoc;
        }
        catch (ArgumentException)
        {
            // Chưa đủ điều kiện thêm/cập nhật. BUS vẫn kiểm tra lại khi người dùng lưu phiếu.
        }
    }

    private void GhiDong(bool capNhat)
    {
        if (thuocDangChon == null)
        {
            BaoLoi("Hãy chọn thuốc bằng nút ….");
            return;
        }
        if (capNhat && lvChiTiet.SelectedItems.Count == 0)
        {
            BaoLoi("Hãy chọn thuốc cần cập nhật trong bảng.");
            return;
        }

        CTPhieuNhap dong = DocThuocDangNhap();
        var danhSachMoi = new ArrayList(chiTiet);
        if (capNhat)
            danhSachMoi[chiTiet.IndexOf(lvChiTiet.SelectedItems[0].Tag!)] = dong;
        else
            danhSachMoi.Add(dong);

        try
        {
            decimal tongTien = bus.TinhTongTien(danhSachMoi);
            chiTiet = danhSachMoi;
            HienThiChiTiet(tongTien);
            ChuanBiDong();
        }
        catch (Exception ex)
        {
            BaoLoi(ex.Message);
        }
    }

    private void BoDong()
    {
        if (lvChiTiet.SelectedItems.Count == 0)
        {
            BaoLoi("Hãy chọn thuốc cần bỏ khỏi phiếu trong bảng.");
            return;
        }

        chiTiet.Remove(lvChiTiet.SelectedItems[0].Tag);
        HienThiChiTiet(bus.TinhTongTien(chiTiet));
        ChuanBiDong();
    }

    private void HienThiChiTiet(decimal tongTien)
    {
        lvChiTiet.BeginUpdate();
        try
        {
            lvChiTiet.SelectedItems.Clear();
            lvChiTiet.Items.Clear();
            foreach (CTPhieuNhap ct in chiTiet)
            {
                var dong = new ListViewItem(ct.MaThuoc) { Tag = ct };
                dong.SubItems.Add(ct.TenThuoc);
                dong.SubItems.Add(ct.SoLuong.ToString("N0"));
                dong.SubItems.Add(ct.DonGia.ToString("N2"));
                dong.SubItems.Add(ct.ThanhTien.ToString("N2"));
                lvChiTiet.Items.Add(dong);
            }
        }
        finally
        {
            lvChiTiet.EndUpdate();
        }
        txtTongTien.Text = tongTien.ToString("N2");
        CapNhatNutChiTiet();
    }

    private void LuuPhieu()
    {
        if (dangLuu)
            return;

        dangLuu = true;
        btnLuu.Enabled = false;
        CapNhatNutChiTiet();
        try
        {
            var phieu = new PhieuNhap
            {
                MaPN = txtMa.Text,
                MaNV = nhanVien?.MaNV ?? string.Empty,
                MaNCC = nhaCungCap?.MaNCC ?? string.Empty,
                NgayNhap = dtpNgayNhap.Value.Date
            };
            KetQua ketQua = banGoc == null ? bus.Them(phieu, chiTiet) : bus.Sua(phieu, chiTiet, banGoc);
            if (!ketQua.ThanhCong)
            {
                BaoLoi(ketQua.ThongBao);
                return;
            }

            MaPhieuDaLuu = phieu.MaPN;
            MessageBox.Show(ketQua.ThongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            BaoLoi(ex.Message);
        }
        finally
        {
            dangLuu = false;
            if (!IsDisposed)
            {
                btnLuu.Enabled = true;
                CapNhatNutChiTiet();
            }
        }
    }

    private static void BaoLoi(string thongBao) => MessageBox.Show(
        thongBao, "Phiếu nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

using System.Collections;

namespace QLNT.GUI;

public sealed class frmPhieuNhap : Form
{
    private readonly PhieuNhapBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Name = "txtTimKiem", Dock = DockStyle.Fill };
    private readonly ListView lvPhieuNhap = new() { Name = "lvPhieuNhap" };
    private readonly ListView lvChiTiet = new() { Name = "lvChiTiet" };
    private readonly Label lblThongTin = new() { Dock = DockStyle.Fill, AutoSize = true };

    public frmPhieuNhap()
    {
        Text = "Phiếu nhập hàng";
        Size = new Size(1050, 650);
        MinimumSize = new Size(650, 500);

        var khung = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 3
        };
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        var thanhTimKiem = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6 };
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTimKiem.Controls.Add(new Label
        {
            Text = "Tìm phiếu, NCC hoặc nhân viên:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);
        thanhTimKiem.Controls.Add(txtTimKiem, 1, 0);
        var btnTaiLai = new Button { Text = "Tải lại", AutoSize = true };
        var btnThem = new Button { Name = "btnThemPhieu", Text = "Thêm phiếu", AutoSize = true };
        var btnSua = new Button { Name = "btnSuaPhieu", Text = "Sửa", AutoSize = true };
        var btnXoa = new Button { Name = "btnXoaPhieu", Text = "Xóa", AutoSize = true };
        btnTaiLai.Click += (_, _) => TaiLai();
        btnThem.Click += (_, _) => ThemPhieu();
        btnSua.Click += (_, _) => SuaPhieu();
        btnXoa.Click += (_, _) => XoaPhieu();
        txtTimKiem.TextChanged += (_, _) => TimKiem();
        thanhTimKiem.Controls.Add(btnTaiLai, 2, 0);
        thanhTimKiem.Controls.Add(btnThem, 3, 0);
        thanhTimKiem.Controls.Add(btnSua, 4, 0);
        thanhTimKiem.Controls.Add(btnXoa, 5, 0);

        CauHinhDanhSach(lvPhieuNhap);
        lvPhieuNhap.Columns.Add("Mã phiếu", 100);
        lvPhieuNhap.Columns.Add("Ngày nhập", 110);
        lvPhieuNhap.Columns.Add("Nhà cung cấp", 280);
        lvPhieuNhap.Columns.Add("Nhân viên", 210);
        lvPhieuNhap.Columns.Add("Tổng tiền", 150);
        lvPhieuNhap.SelectedIndexChanged += (_, _) => ChonPhieu();

        CauHinhDanhSach(lvChiTiet);
        lvChiTiet.Columns.Add("Mã thuốc", 100);
        lvChiTiet.Columns.Add("Tên thuốc", 280);
        lvChiTiet.Columns.Add("Số lượng", 100);
        lvChiTiet.Columns.Add("Đơn giá nhập", 150);
        lvChiTiet.Columns.Add("Thành tiền", 160);

        var chia = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            Size = new Size(900, 500),
            SplitterDistance = 235
        };
        var nhomPhieu = new GroupBox { Text = "Danh sách phiếu nhập", Dock = DockStyle.Fill };
        var nhomChiTiet = new GroupBox { Text = "Chi tiết phiếu đang chọn", Dock = DockStyle.Fill };
        nhomPhieu.Controls.Add(lvPhieuNhap);
        nhomChiTiet.Controls.Add(lvChiTiet);
        chia.Panel1.Controls.Add(nhomPhieu);
        chia.Panel2.Controls.Add(nhomChiTiet);

        khung.Controls.Add(thanhTimKiem, 0, 0);
        khung.Controls.Add(chia, 0, 1);
        khung.Controls.Add(lblThongTin, 0, 2);
        Controls.Add(khung);
        Load += (_, _) => TaiLai();
    }

    private static void CauHinhDanhSach(ListView danhSach)
    {
        danhSach.Dock = DockStyle.Fill;
        danhSach.View = View.Details;
        danhSach.FullRowSelect = true;
        danhSach.MultiSelect = false;
        danhSach.GridLines = true;
        danhSach.HideSelection = false;
    }

    private void TaiLai(string? maCanChon = null)
    {
        PhieuNhapBUS.TaiLai();
        TimKiem(maCanChon);
    }

    private void TimKiem(string? maCanChon = null)
    {
        try
        {
            ArrayList danhSach = bus.TimKiem(txtTimKiem.Text);
            lvPhieuNhap.BeginUpdate();
            try
            {
                lvPhieuNhap.Items.Clear();
                lvChiTiet.Items.Clear();
                foreach (PhieuNhap phieu in danhSach)
                {
                    var dong = new ListViewItem(phieu.MaPN) { Tag = phieu };
                    dong.SubItems.Add(phieu.NgayNhap.ToString("dd/MM/yyyy"));
                    dong.SubItems.Add(phieu.TenNCC);
                    dong.SubItems.Add(phieu.TenNhanVien);
                    dong.SubItems.Add(phieu.TongTien.ToString("N2"));
                    lvPhieuNhap.Items.Add(dong);
                }
            }
            finally
            {
                lvPhieuNhap.EndUpdate();
            }

            lblThongTin.Text = $"Số phiếu: {danhSach.Count}. Chọn một phiếu để xem chi tiết.";
            ListViewItem? dongChon = lvPhieuNhap.Items.Cast<ListViewItem>()
                .FirstOrDefault(d => d.Text == maCanChon);
            dongChon ??= lvPhieuNhap.Items.Count > 0 ? lvPhieuNhap.Items[0] : null;
            if (dongChon != null)
            {
                dongChon.Selected = true;
                dongChon.EnsureVisible();
            }
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void ChonPhieu()
    {
        lvChiTiet.Items.Clear();
        if (lvPhieuNhap.SelectedItems.Count == 0)
            return;

        try
        {
            var phieu = (PhieuNhap)lvPhieuNhap.SelectedItems[0].Tag!;
            ArrayList chiTiet = bus.LayChiTiet(phieu.MaPN);
            lvChiTiet.BeginUpdate();
            try
            {
                foreach (CTPhieuNhap ct in chiTiet)
                {
                    var dong = new ListViewItem(ct.MaThuoc);
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

            lblThongTin.Text = $"{phieu.MaPN}: {chiTiet.Count} dòng thuốc | " +
                $"Tổng tiền phiếu: {phieu.TongTien:N2} | Tổng chi tiết: {bus.TinhTongTien(chiTiet):N2}";
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private void ThemPhieu()
    {
        using var form = new frmTaoPhieuNhap();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            txtTimKiem.Clear();
            TaiLai(form.MaPhieuDaLuu);
        }
    }

    private void SuaPhieu()
    {
        if (lvPhieuNhap.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn phiếu nhập cần sửa.");
            return;
        }
        using var form = new frmTaoPhieuNhap(lvPhieuNhap.SelectedItems[0].Text);
        if (form.ShowDialog(this) == DialogResult.OK)
            TaiLai(form.MaPhieuDaLuu);
    }

    private void XoaPhieu()
    {
        if (lvPhieuNhap.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn phiếu nhập cần xóa.");
            return;
        }
        try
        {
            DuLieuPhieuNhap banGoc = bus.LayDeSua(lvPhieuNhap.SelectedItems[0].Text);
            if (MessageBox.Show($"Xóa phiếu {banGoc.Phieu.MaPN} ({banGoc.Phieu.TongTien:N2}) và hoàn tồn kho?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            KetQua ketQua = bus.Xoa(banGoc.Phieu.MaPN, banGoc);
            MessageBox.Show(ketQua.ThongBao, "Phiếu nhập", MessageBoxButtons.OK,
                ketQua.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ketQua.ThanhCong) TaiLai();
        }
        catch (Exception ex)
        {
            BaoLoi(ex);
        }
    }

    private static void BaoLoi(Exception ex) => MessageBox.Show(
        "Không thao tác được với phiếu nhập: " + ex.Message,
        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

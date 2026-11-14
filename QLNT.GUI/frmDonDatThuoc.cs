using System.Collections;

namespace QLNT.GUI;

public sealed class frmDonDatThuoc : Form
{
    private readonly DatThuocBUS bus = new();
    private readonly TextBox txtTim = new() { Width = 230, PlaceholderText = "Mã đơn, tên, SĐT, địa chỉ" };
    private readonly CheckBox chkDonMoi = new() { Text = "Chỉ đơn mới", Checked = true, AutoSize = true, Padding = new Padding(4, 7, 0, 0) };
    private readonly TextBox txtNhanVien = new() { Width = 165, ReadOnly = true };
    private readonly Button btnChonNhanVien = new() { Text = "...", Width = 36 };
    private readonly NumericUpDown nudKhachDua = new() { Width = 125, DecimalPlaces = 2,
        ThousandsSeparator = true, Maximum = 9999999999999999.99m };
    private readonly CheckBox chkDoiChieuOnline = new() { Text = "Đã đối chiếu chuyển khoản", AutoSize = true };
    private readonly ListView lvDon = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, GridLines = true };
    private readonly ListView lvThuoc = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
    private readonly Label lblThongTin = new() { Dock = DockStyle.Fill, AutoSize = false };
    private readonly Button btnXacNhan = new() { Text = "Tạo hóa đơn", AutoSize = true };
    private readonly Button btnHuy = new() { Text = "Hủy đơn / hoàn tồn", AutoSize = true };
    private readonly Button btnThuCOD = new() { Text = "Xác nhận thu COD", AutoSize = true };
    private Dictionary<int, List<DongDatThuoc>> chiTiet = [];
    private NhanVien? nhanVien;

    public frmDonDatThuoc()
    {
        Text = "Đơn đặt thuốc từ web";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1120, 760);
        MinimumSize = new Size(820, 540);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var khung = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 4 };
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 54));
        khung.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
        Controls.Add(khung);

        var congCu = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
        var btnTaiLai = new Button { Text = "Tải đơn mới", AutoSize = true };
        congCu.Controls.AddRange([new Label { Text = "Tìm đơn:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            txtTim, chkDonMoi, btnTaiLai,
            new Label { Text = "Nhân viên:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) }, txtNhanVien, btnChonNhanVien,
            new Label { Text = "Khách đưa:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) }, nudKhachDua,
            chkDoiChieuOnline, btnXacNhan, btnHuy, btnThuCOD]);
        khung.Controls.Add(congCu, 0, 0);
        khung.RowStyles[0].Height = 110;

        lvDon.Columns.Add("Đơn", 75);
        lvDon.Columns.Add("Ngày đặt", 135);
        lvDon.Columns.Add("Người đặt", 170);
        lvDon.Columns.Add("Số điện thoại", 120);
        lvDon.Columns.Add("Địa chỉ", 240);
        lvDon.Columns.Add("Tổng sau voucher", 140);
        lvDon.Columns.Add("Trạng thái", 115);
        lvDon.Columns.Add("Thanh toán", 135);
        khung.Controls.Add(lvDon, 0, 1);

        lblThongTin.Padding = new Padding(6);
        lblThongTin.BackColor = GiaoDienChung.MatTrang;
        khung.Controls.Add(lblThongTin, 0, 2);

        lvThuoc.Columns.Add("Mã thuốc", 110);
        lvThuoc.Columns.Add("Tên thuốc", 300);
        lvThuoc.Columns.Add("Số lượng", 100);
        lvThuoc.Columns.Add("Giá lúc đặt", 140);
        lvThuoc.Columns.Add("Thành tiền", 150);
        khung.Controls.Add(lvThuoc, 0, 3);

        Load += (_, _) => TaiLai();
        btnChonNhanVien.Click += (_, _) => ChonNhanVien();
        txtTim.TextChanged += (_, _) => LocDon();
        chkDonMoi.CheckedChanged += (_, _) => LocDon();
        btnTaiLai.Click += (_, _) => TaiLai();
        lvDon.SelectedIndexChanged += (_, _) => HienThiChiTiet();
        btnXacNhan.Click += (_, _) => DoiTrangThai(false);
        btnHuy.Click += (_, _) => DoiTrangThai(true);
        btnThuCOD.Click += (_, _) => ThuCOD();
        chkDoiChieuOnline.CheckedChanged += (_, _) => CapNhatNut();
        CapNhatNut();
    }

    private DonDatThuoc? DonDangChon => lvDon.SelectedItems.Count == 0
        ? null : lvDon.SelectedItems[0].Tag as DonDatThuoc;

    private void ChonNhanVien()
    {
        using var form = new frmChonNhanVien();
        if (form.ShowDialog(this) != DialogResult.OK || frmChonNhanVien.NhanVienDuocChon is not NhanVien nv)
            return;
        nhanVien = nv;
        txtNhanVien.Text = $"{nv.MaNV} - {nv.HoTenNV}";
        CapNhatNut();
    }

    private void TaiLai()
    {
        try
        {
            bus.TaiLaiDon();
            bus.LayDon();
            chiTiet = bus.LayChiTietDon();
            LocDon();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được đơn web: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LocDon()
    {
        int? maCu = DonDangChon?.MaDon;
        lvDon.BeginUpdate();
        try
        {
            lvDon.Items.Clear();
            foreach (DonDatThuoc don in bus.TimDon(txtTim.Text, chkDonMoi.Checked))
            {
                var dong = new ListViewItem($"#{don.MaDon}") { Tag = don };
                dong.SubItems.Add(don.NgayDat.ToString("dd/MM/yyyy HH:mm"));
                dong.SubItems.Add(don.HoTen);
                dong.SubItems.Add(don.SDT);
                dong.SubItems.Add(don.DiaChi);
                dong.SubItems.Add(don.TongTamTinh.ToString("N0") + " đ");
                dong.SubItems.Add(don.TrangThai switch { "Moi" => "Mới", "DaXacNhan" => "Đã xác nhận", "DaHuy" => "Đã hủy", _ => don.TrangThai });
                dong.SubItems.Add(don.TrangThaiThanhToan switch { "DaThanhToan" => "Đã thanh toán", "ChoThuTien" => "Chờ thu COD", _ => "Chưa xử lý" });
                lvDon.Items.Add(dong);
                if (don.MaDon == maCu) dong.Selected = true;
            }
        }
        finally { lvDon.EndUpdate(); }
        HienThiChiTiet();
    }

    private void HienThiChiTiet()
    {
        lvThuoc.Items.Clear();
        DonDatThuoc? don = DonDangChon;
        if (don == null)
        {
            lblThongTin.Text = "Chọn một đơn để xem thuốc, voucher và thông tin nhận hàng.";
            CapNhatNut();
            return;
        }
        lblThongTin.Text = $"Đơn #{don.MaDon} · {don.HoTen} · {don.SDT}\n" +
            $"Địa chỉ: {don.DiaChi}    Ghi chú: {don.GhiChu ?? "—"}\n" +
            $"Giảm SP: {don.GiamSanPham:N0} đ    " +
            $"KM hóa đơn: {don.MaVoucher ?? don.MaCTKMHD ?? "Không"}    Giảm: {don.TienGiam:N0} đ    " +
            $"Tổng sau giảm: {don.TongTamTinh:N0} đ    " +
            (don.DaTruTon ? "Kho: đã giữ thuốc" : "Đơn cũ: chưa trừ tồn") +
            (don.MaHD == null ? "" : $"    Hóa đơn: {don.MaHD}") + "\n" +
            $"Thanh toán: {TenPhuongThuc(don.PhuongThucThanhToan)}" +
            (don.TrangThaiThanhToan == "ChoThuTien" ? " — Chờ xác nhận đã thu khi giao" : "");
        if (don.TrangThai == "Moi")
        {
            bool taiQuay = don.PhuongThucThanhToan == PhuongThucThanhToanCode.TaiCuaHang;
            nudKhachDua.Enabled = taiQuay;
            nudKhachDua.Value = taiQuay ? Math.Min(nudKhachDua.Maximum, don.TongTamTinh) : 0;
            chkDoiChieuOnline.Checked = false;
            chkDoiChieuOnline.Visible = don.PhuongThucThanhToan == PhuongThucThanhToanCode.Online;
        }
        else
        {
            nudKhachDua.Enabled = false;
            nudKhachDua.Value = 0;
            chkDoiChieuOnline.Visible = false;
        }
        if (chiTiet.TryGetValue(don.MaDon, out List<DongDatThuoc>? cacDong))
            foreach (DongDatThuoc thuoc in cacDong)
            {
                var dong = new ListViewItem(thuoc.MaThuoc);
                dong.SubItems.Add(thuoc.TenThuoc);
                dong.SubItems.Add(thuoc.SoLuong.ToString());
                dong.SubItems.Add(thuoc.DonGiaDuKien.ToString("N0") + " đ");
                dong.SubItems.Add(thuoc.ThanhTien.ToString("N0") + " đ");
                lvThuoc.Items.Add(dong);
            }
        CapNhatNut();
    }

    private void CapNhatNut()
    {
        bool donMoi = DonDangChon?.TrangThai == "Moi";
        btnXacNhan.Enabled = donMoi && nhanVien != null &&
            (DonDangChon?.PhuongThucThanhToan != PhuongThucThanhToanCode.Online || chkDoiChieuOnline.Checked);
        btnHuy.Enabled = donMoi;
        btnThuCOD.Enabled = DonDangChon is { PhuongThucThanhToan: PhuongThucThanhToanCode.COD,
            TrangThaiThanhToan: "ChoThuTien", MaHD: not null };
    }

    private void DoiTrangThai(bool huy)
    {
        DonDatThuoc? don = DonDangChon;
        if (don == null) return;
        string canhBao = huy
            ? don.DaTruTon ? "Hủy đơn và hoàn lại số thuốc đã giữ?" : "Hủy đơn cũ này? Đơn chưa trừ tồn nên không hoàn kho."
            : $"Tạo hóa đơn từ đơn web #{don.MaDon}? {TenPhuongThuc(don.PhuongThucThanhToan)}. " +
              $"Tổng {don.TongTamTinh:N0} đ, tiền khách đưa tại quầy {nudKhachDua.Value:N0} đ. Tồn kho sẽ không bị trừ lần hai.";
        if (MessageBox.Show(canhBao, $"Đơn #{don.MaDon}", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
        KetQua ketQua = huy ? bus.Huy(don.MaDon)
            : bus.XacNhan(don.MaDon, nhanVien?.MaNV ?? string.Empty, nudKhachDua.Value, chkDoiChieuOnline.Checked);
        MessageBox.Show(ketQua.ThongBao, ketQua.ThanhCong ? "Thành công" : "Không thành công",
            MessageBoxButtons.OK, ketQua.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (ketQua.ThanhCong) TaiLai();
    }

    private void ThuCOD()
    {
        DonDatThuoc? don = DonDangChon;
        if (don?.MaHD == null || don.TrangThaiThanhToan != "ChoThuTien") return;
        using var hopThoai = new Form { Text = $"Xác nhận thu COD #{don.MaDon}",
            StartPosition = FormStartPosition.CenterParent, Size = new Size(390, 180),
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
        var soTien = new NumericUpDown { Left = 18, Top = 52, Width = 330,
            DecimalPlaces = 2, ThousandsSeparator = true, Maximum = 9999999999999999.99m,
            Value = Math.Min(9999999999999999.99m, don.TongTamTinh) };
        var nhan = new Label { Left = 18, Top = 17, Width = 340,
            Text = $"Số tiền thực nhận (cần ít nhất {don.TongTamTinh:N0} đ):" };
        var dongY = new Button { Text = "Đã thu tiền", Left = 210, Top = 95,
            Width = 140, DialogResult = DialogResult.OK };
        hopThoai.Controls.AddRange([nhan, soTien, dongY]);
        hopThoai.AcceptButton = dongY;
        if (hopThoai.ShowDialog(this) != DialogResult.OK) return;
        KetQua ketQua = bus.XacNhanThuCOD(don.MaDon, soTien.Value);
        MessageBox.Show(ketQua.ThongBao, ketQua.ThanhCong ? "Thành công" : "Không thành công",
            MessageBoxButtons.OK, ketQua.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (ketQua.ThanhCong) TaiLai();
    }

    private static string TenPhuongThuc(string ma) => ma switch
    {
        PhuongThucThanhToanCode.Online => "Chuyển khoản online",
        PhuongThucThanhToanCode.COD => "COD – Thu khi giao hàng",
        _ => "Thanh toán tại cửa hàng"
    };
}

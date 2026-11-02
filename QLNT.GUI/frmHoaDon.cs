using System.Collections;

namespace QLNT.GUI;

public sealed class frmHoaDon : Form
{
    private readonly HoaDonBUS bus = new();
    private readonly TextBox txtTimKiem = new() { Width = 260 };
    private readonly ListView lvHoaDon = new();
    private readonly ListView lvChiTiet = new();
    private readonly Label lblChiTiet = new() { AutoSize = true, Text = "Chi tiết hóa đơn" };
    private bool dangNap;
    private bool daTaiDanhSach;

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
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = GiaoDienChung.Nen
        };
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        Controls.Add(khung);

        khung.Controls.Add(TaoThanhCongCu(), 0, 0);
        CauHinhDanhSachHoaDon();
        CauHinhDanhSachChiTiet();
        khung.Controls.Add(TaoKhungDanhSach("Danh sách hóa đơn", lvHoaDon), 0, 1);
        khung.Controls.Add(TaoKhungChiTiet(), 0, 2);

        Load += (_, _) => TaiDanhSach();
        txtTimKiem.TextChanged += (_, _) => HienThiDanhSachHoaDon();
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
        var btnSua = GiaoDienChung.TaoNutPhu("Sửa hóa đơn");
        var btnHuy = GiaoDienChung.TaoNutPhu("Hủy hóa đơn / hoàn tồn");
        var btnIn = GiaoDienChung.TaoNutPhu("In / xuất PDF");
        btnTaiLai.Click += (_, _) => TaiDanhSach();
        btnLap.Click += (_, _) => LapHoaDonNhap();
        btnSua.Click += (_, _) => SuaHoaDon();
        btnHuy.Click += (_, _) => HuyHoaDon();
        btnIn.Click += (_, _) => InHoaDon();
        congCu.Controls.AddRange([nhanTim, txtTimKiem, btnTaiLai, btnLap, btnSua, btnHuy, btnIn]);
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
            RowCount = 2,
            Padding = new Padding(12),
            BackColor = GiaoDienChung.MatTrang
        };
        khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        khung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lblChiTiet.Padding = new Padding(0, 0, 0, 8);
        lblChiTiet.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblChiTiet.ForeColor = GiaoDienChung.Chu;
        khung.Controls.Add(lblChiTiet, 0, 0);
        khung.Controls.Add(lvChiTiet, 0, 1);
        return khung;
    }

    private void TaiDanhSach()
    {
        try
        {
            bus.TaiLai();
            ArrayList danhSach = bus.LayTatCa();
            daTaiDanhSach = true;
            HienThiDanhSachHoaDon(danhSach);
        }
        catch (Exception ex)
        {
            daTaiDanhSach = false;
            MessageBox.Show($"Không thể tải danh sách hóa đơn: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HienThiDanhSachHoaDon()
    {
        if (daTaiDanhSach)
            HienThiDanhSachHoaDon(bus.TimKiem(txtTimKiem.Text));
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
        if (form.ShowDialog(this) == DialogResult.OK)
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
            MessageBox.Show("Hãy chọn hóa đơn cần in hoặc xuất PDF.");
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
}

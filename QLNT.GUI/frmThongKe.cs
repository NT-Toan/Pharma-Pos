using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLNT.BUS;

namespace QLNT.GUI;

public sealed class frmThongKe : Form
{
    private readonly ThongKeBUS bus = new();
    private readonly ComboBox cboLoaiThongKe = new();
    private readonly NumericUpDown nudNam = new();
    private readonly DateTimePicker dtpTuNgay = new();
    private readonly DateTimePicker dtpDenNgay = new();
    private readonly Label lblTongThuKy = new();
    private readonly Label lblTongChiKy = new();
    private readonly Label lblLoiNhuanKy = new();
    private readonly Label lblTongThuTuyChon = new();
    private readonly Label lblTongChiTuyChon = new();
    private readonly Label lblLoiNhuanTuyChon = new();
    private readonly Label lblTrangThai = new();
    private readonly DataGridView dgvDuLieu = new();
    private DataTable? bangHienTai;

    public frmThongKe()
    {
        Text = "Thống kê doanh thu và báo cáo";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(880, 580);
        Size = new Size(1000, 680);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(12)
        };

        var grpBoLoc = new GroupBox
        {
            Text = "Điều kiện lọc & Báo cáo tổng hợp",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(16, 12, 16, 14),
            ForeColor = GiaoDienChung.Chu
        };

        var tblInput = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 2,
            Padding = new Padding(0, 0, 0, 8)
        };
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

        cboLoaiThongKe.DropDownStyle = ComboBoxStyle.DropDownList;
        cboLoaiThongKe.Dock = DockStyle.Fill;
        cboLoaiThongKe.DropDownWidth = 320;
        cboLoaiThongKe.Items.AddRange(new object[]
        {
            "Thu - Chi theo Tháng",
            "Thu - Chi theo Quý",
            "Thu - Chi theo Năm",
            "Doanh thu theo Nhân viên (theo Quý)",
            "Doanh thu theo Khách hàng (theo Quý)",
            "Doanh thu theo Sản phẩm (theo Quý)"
        });
        cboLoaiThongKe.SelectedIndex = 1;
        cboLoaiThongKe.SelectedIndexChanged += (_, _) => TaiBaoCaoBang();

        nudNam.Minimum = 2020;
        nudNam.Maximum = 2035;
        nudNam.Value = 2026;
        nudNam.Dock = DockStyle.Fill;
        nudNam.ValueChanged += (_, _) => TaiBaoCaoBang();

        tblInput.Controls.Add(new Label { Text = "Loại thống kê:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 0, 0);
        tblInput.Controls.Add(cboLoaiThongKe, 1, 0);
        tblInput.Controls.Add(new Label { Text = "Năm:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 2, 0);
        tblInput.Controls.Add(nudNam, 3, 0);

        dtpTuNgay.Format = DateTimePickerFormat.Short;
        dtpTuNgay.Dock = DockStyle.Fill;
        dtpTuNgay.Value = new DateTime(2026, 1, 1);

        dtpDenNgay.Format = DateTimePickerFormat.Short;
        dtpDenNgay.Dock = DockStyle.Fill;
        dtpDenNgay.Value = DateTime.Today;

        tblInput.Controls.Add(new Label { Text = "Từ ngày:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 0, 1);
        tblInput.Controls.Add(dtpTuNgay, 1, 1);
        tblInput.Controls.Add(new Label { Text = "Đến ngày:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 2, 1);
        tblInput.Controls.Add(dtpDenNgay, 3, 1);

        var pnlNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 4, 0, 0)
        };

        var btnXuatExcel = GiaoDienChung.TaoNutPhu("Xuất Excel");
        btnXuatExcel.Click += (_, _) => XuatExcel();

        var btnXemNgay = GiaoDienChung.TaoNutPhu("Tính theo ngày");
        btnXemNgay.Click += (_, _) => TinhTongHopTheoKhoangNgay();

        var btnXemBang = GiaoDienChung.TaoNutPhu("Cập nhật bảng");
        btnXemBang.BackColor = GiaoDienChung.XanhChinh;
        btnXemBang.ForeColor = Color.White;
        btnXemBang.Click += (_, _) => TaiBaoCaoBang();

        pnlNut.Controls.Add(btnXuatExcel);
        pnlNut.Controls.Add(btnXemNgay);
        pnlNut.Controls.Add(btnXemBang);

        var pnlTongHop = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(0, 8, 0, 0)
        };
        pnlTongHop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        pnlTongHop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        pnlTongHop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

        DinhDangTheTongHop(lblTongThuKy, Color.DarkGreen);
        DinhDangTheTongHop(lblTongChiKy, Color.DarkRed);
        DinhDangTheTongHop(lblLoiNhuanKy, Color.DarkBlue);
        pnlTongHop.Controls.Add(lblTongThuKy, 0, 0);
        pnlTongHop.Controls.Add(lblTongChiKy, 1, 0);
        pnlTongHop.Controls.Add(lblLoiNhuanKy, 2, 0);

        DinhDangTheTongHop(lblTongThuTuyChon, Color.DarkGreen);
        DinhDangTheTongHop(lblTongChiTuyChon, Color.DarkRed);
        DinhDangTheTongHop(lblLoiNhuanTuyChon, Color.DarkBlue);
        pnlTongHop.Controls.Add(lblTongThuTuyChon, 0, 1);
        pnlTongHop.Controls.Add(lblTongChiTuyChon, 1, 1);
        pnlTongHop.Controls.Add(lblLoiNhuanTuyChon, 2, 1);

        grpBoLoc.Controls.Add(pnlTongHop);
        grpBoLoc.Controls.Add(pnlNut);
        grpBoLoc.Controls.Add(tblInput);
        pnlTop.Controls.Add(grpBoLoc);

        dgvDuLieu.Dock = DockStyle.Fill;
        dgvDuLieu.ReadOnly = true;
        dgvDuLieu.AllowUserToAddRows = false;
        dgvDuLieu.BackgroundColor = Color.White;
        dgvDuLieu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDuLieu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        lblTrangThai.Dock = DockStyle.Bottom;
        lblTrangThai.Height = 32;
        lblTrangThai.Padding = new Padding(12, 8, 0, 0);
        lblTrangThai.ForeColor = GiaoDienChung.ChuPhu;

        Controls.Add(dgvDuLieu);
        Controls.Add(pnlTop);
        Controls.Add(lblTrangThai);

        Load += (_, _) =>
        {
            TaiBaoCaoBang();
            TinhTongHopTheoKhoangNgay();
        };
    }

    private static void DinhDangTheTongHop(Label lbl, Color mau)
    {
        lbl.AutoSize = true;
        lbl.Font = new Font(GiaoDienChung.PhongChu, FontStyle.Bold);
        lbl.ForeColor = mau;
        lbl.Padding = new Padding(0, 3, 8, 3);
    }

    private void TaiBaoCaoBang()
    {
        try
        {
            int nam = (int)nudNam.Value;
            switch (cboLoaiThongKe.SelectedIndex)
            {
                case 0:
                    bangHienTai = bus.ThuChiTheoThang(nam);
                    break;
                case 1:
                    bangHienTai = bus.ThuChiTheoQuy(nam);
                    break;
                case 2:
                    bangHienTai = bus.ThuChiTheoNam(nam - 2, nam);
                    break;
                case 3:
                    bangHienTai = bus.TongTienTheoNhanVienQuy(nam);
                    break;
                case 4:
                    bangHienTai = bus.TongTienTheoKhachHangQuy(nam);
                    break;
                case 5:
                    bangHienTai = bus.TongTienTheoSanPhamQuy(nam);
                    break;
            }

            dgvDuLieu.DataSource = bangHienTai;
            DinhDangLuoi();

            DateTime tuDauNam = new(nam, 1, 1);
            DateTime denCuoiNam = new(nam, 12, 31);
            decimal thu = bus.TongThu(tuDauNam, denCuoiNam);
            decimal chi = bus.TongChi(tuDauNam, denCuoiNam);
            decimal loiNhuan = bus.LoiNhuan(tuDauNam, denCuoiNam);

            lblTongThuKy.Text = $"Thu năm {nam}: {thu:N0} đ";
            lblTongChiKy.Text = $"Chi: {chi:N0} đ";
            lblLoiNhuanKy.Text = $"Lợi nhuận: {loiNhuan:N0} đ";

            lblTrangThai.Text = $"Số dòng dữ liệu: {bangHienTai?.Rows.Count ?? 0}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải báo cáo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TinhTongHopTheoKhoangNgay()
    {
        try
        {
            DateTime tuNgay = dtpTuNgay.Value.Date;
            DateTime denNgay = dtpDenNgay.Value.Date;

            if (denNgay < tuNgay)
            {
                MessageBox.Show("Đến ngày phải lớn hơn hoặc bằng Từ ngày.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal thu = bus.TongThu(tuNgay, denNgay);
            decimal chi = bus.TongChi(tuNgay, denNgay);
            decimal loiNhuan = bus.LoiNhuan(tuNgay, denNgay);

            lblTongThuTuyChon.Text = $"Thu ({tuNgay:dd/MM} - {denNgay:dd/MM}): {thu:N0} đ";
            lblTongChiTuyChon.Text = $"Chi: {chi:N0} đ";
            lblLoiNhuanTuyChon.Text = $"Lợi nhuận: {loiNhuan:N0} đ";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tính tổng thu chi theo ngày: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DinhDangLuoi()
    {
        if (dgvDuLieu.Columns.Count == 0) return;

        foreach (DataGridViewColumn cot in dgvDuLieu.Columns)
        {
            if (cot.ValueType == typeof(decimal))
            {
                cot.DefaultCellStyle.Format = "N0";
                cot.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }

    private void XuatExcel()
    {
        if (bangHienTai == null || bangHienTai.Rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất file Excel.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using SaveFileDialog sfd = new()
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"ThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                ExcelHelper.XuatBang(bangHienTai, sfd.FileName);
                MessageBox.Show("Xuất file Excel (.xlsx) thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
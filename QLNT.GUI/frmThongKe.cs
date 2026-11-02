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
    private readonly DataGridView dgvDuLieu = new();
    private readonly Label lblTongThu = new();
    private readonly Label lblTongChi = new();
    private readonly Label lblLoiNhuan = new();
    private DataTable? bangHienTai;

    public frmThongKe()
    {
        Text = "Thống kê doanh thu và báo cáo";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(950, 600);
        Size = new Size(1000, 650);
        Font = GiaoDienChung.PhongChu;
        BackColor = GiaoDienChung.Nen;

        var pnlDieuKien = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(12)
        };

        var tblFilter = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            ColumnCount = 6
        };
        // Tăng chiều rộng nhãn lên 135px để không bị rớt chữ "kê"
        tblFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
        // Đặt cố định ô chọn loại thống kê 280px để thấy trọn vẹn chữ
        tblFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
        tblFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
        tblFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
        tblFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tblFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

        cboLoaiThongKe.DropDownStyle = ComboBoxStyle.DropDownList;
        // Đặt độ rộng menu thả xuống rộng rãi để không bao giờ bị cắt chữ
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
        cboLoaiThongKe.SelectedIndexChanged += (_, _) => TaiBaoCao();

        nudNam.Minimum = 2020;
        nudNam.Maximum = 2035;
        nudNam.Value = 2026;
        nudNam.ValueChanged += (_, _) => TaiBaoCao();

        var btnXem = GiaoDienChung.TaoNutPhu("Cập Nhật");
        btnXem.BackColor = GiaoDienChung.XanhChinh;
        btnXem.ForeColor = Color.White;
        btnXem.Click += (_, _) => TaiBaoCao();

        var btnXuatExcel = GiaoDienChung.TaoNutPhu("Xuất Excel");
        btnXuatExcel.Click += (_, _) => XuatExcel();

        tblFilter.Controls.Add(new Label { Text = "Loại thống kê:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 0, 0);
        tblFilter.Controls.Add(cboLoaiThongKe, 1, 0);
        tblFilter.Controls.Add(new Label { Text = "Năm:", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = GiaoDienChung.Chu }, 2, 0);
        tblFilter.Controls.Add(nudNam, 3, 0);
        tblFilter.Controls.Add(btnXem, 4, 0);
        tblFilter.Controls.Add(btnXuatExcel, 5, 0);

        var pnlTheTongHop = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight
        };

        lblTongThu.AutoSize = true;
        lblTongThu.Font = new Font(Font, FontStyle.Bold);
        lblTongThu.ForeColor = Color.DarkGreen;
        lblTongThu.Padding = new Padding(0, 10, 24, 0);

        lblTongChi.AutoSize = true;
        lblTongChi.Font = new Font(Font, FontStyle.Bold);
        lblTongChi.ForeColor = Color.DarkRed;
        lblTongChi.Padding = new Padding(0, 10, 24, 0);

        lblLoiNhuan.AutoSize = true;
        lblLoiNhuan.Font = new Font(Font, FontStyle.Bold);
        lblLoiNhuan.ForeColor = Color.DarkBlue;
        lblLoiNhuan.Padding = new Padding(0, 10, 0, 0);

        pnlTheTongHop.Controls.Add(lblTongThu);
        pnlTheTongHop.Controls.Add(lblTongChi);
        pnlTheTongHop.Controls.Add(lblLoiNhuan);

        pnlDieuKien.Controls.Add(tblFilter);
        pnlDieuKien.Controls.Add(pnlTheTongHop);

        dgvDuLieu.Dock = DockStyle.Fill;
        dgvDuLieu.ReadOnly = true;
        dgvDuLieu.AllowUserToAddRows = false;
        dgvDuLieu.BackgroundColor = Color.White;
        dgvDuLieu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDuLieu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        Controls.Add(dgvDuLieu);
        Controls.Add(pnlDieuKien);

        Load += (_, _) => TaiBaoCao();
    }

    private void TaiBaoCao()
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

            lblTongThu.Text = $"Tổng thu năm {nam}: {thu:N0} đ";
            lblTongChi.Text = $"Tổng chi năm {nam}: {chi:N0} đ";
            lblLoiNhuan.Text = $"Lợi nhuận năm {nam}: {loiNhuan:N0} đ";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải báo cáo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Filter = "Excel CSV (*.csv)|*.csv",
            FileName = $"ThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                ExcelHelper.XuatBang(bangHienTai, sfd.FileName);
                MessageBox.Show("Xuất file thành công! Bạn có thể mở trực tiếp bằng Excel.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
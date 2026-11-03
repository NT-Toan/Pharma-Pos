using System.Collections;

namespace QLNT.GUI;

public sealed class frmThamSo : Form
{
    private readonly ThamSoBUS bus = new();

    private readonly TextBox txtMa = new() { Dock = DockStyle.Fill, MaxLength = 50 };
    private readonly TextBox txtGiaTri = new() { Dock = DockStyle.Fill, MaxLength = 200 };
    private readonly TextBox txtTimKiem = new() { Dock = DockStyle.Fill };
    private readonly ListView lvThamSo = new();
    private readonly Label lblSoLuong = new();

    public frmThamSo()
    {
        Text = "Quản lý tham số";
        MinimumSize = new Size(650, 450);
        Size = new Size(850, 550);
        BackColor = GiaoDienChung.Nen;
        Font = GiaoDienChung.PhongChu;

        var giaoDien = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 5
        };
        giaoDien.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        var thanhTim = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3
        };
        thanhTim.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTim.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        thanhTim.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        thanhTim.Controls.Add(TaoNhan("Tìm mã hoặc giá trị:"), 0, 0);
        thanhTim.Controls.Add(txtTimKiem, 1, 0);

        var btnTaiLai = GiaoDienChung.TaoNutPhu("Tải lại");
        btnTaiLai.Click += (_, _) => TaiLaiDanhSach();
        thanhTim.Controls.Add(btnTaiLai, 2, 0);
        txtTimKiem.TextChanged += (_, _) => TimKiem();

        var bangNhap = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1
        };
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        bangNhap.Controls.Add(TaoNhan("Mã tham số:"), 0, 0);
        bangNhap.Controls.Add(txtMa, 1, 0);
        bangNhap.Controls.Add(TaoNhan("Giá trị:"), 2, 0);
        bangNhap.Controls.Add(txtGiaTri, 3, 0);

        var thanhNut = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false
        };

        var btnMoi = GiaoDienChung.TaoNutPhu("Mới");
        var btnThem = GiaoDienChung.TaoNutPhu("Thêm");
        var btnSua = GiaoDienChung.TaoNutPhu("Sửa");
        var btnXoa = GiaoDienChung.TaoNutPhu("Xóa");

        btnMoi.Click += (_, _) => ChuanBiThem();
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();

        thanhNut.Controls.AddRange([btnMoi, btnThem, btnSua, btnXoa]);

        lvThamSo.Dock = DockStyle.Fill;
        lvThamSo.View = View.Details;
        lvThamSo.FullRowSelect = true;
        lvThamSo.GridLines = true;
        lvThamSo.MultiSelect = false;
        lvThamSo.HideSelection = false;
        lvThamSo.Columns.Add("Mã tham số", 250);
        lvThamSo.Columns.Add("Giá trị", 500);
        lvThamSo.SelectedIndexChanged += (_, _) => ChonDong();

        lblSoLuong.Dock = DockStyle.Fill;
        lblSoLuong.ForeColor = GiaoDienChung.ChuPhu;

        giaoDien.Controls.Add(thanhTim, 0, 0);
        giaoDien.Controls.Add(bangNhap, 0, 1);
        giaoDien.Controls.Add(thanhNut, 0, 2);
        giaoDien.Controls.Add(lvThamSo, 0, 3);
        giaoDien.Controls.Add(lblSoLuong, 0, 4);
        Controls.Add(giaoDien);

        Load += (_, _) =>
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        };
    }

    private static Label TaoNhan(string noiDung) => new()
    {
        Text = noiDung,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        ForeColor = GiaoDienChung.Chu
    };

    private void HienThi(ArrayList danhSach)
    {
        lvThamSo.BeginUpdate();
        lvThamSo.Items.Clear();

        foreach (ThamSo thamSo in danhSach)
        {
            var dong = new ListViewItem(thamSo.MaThamSo) { Tag = thamSo };
            dong.SubItems.Add(thamSo.GiaTri);
            lvThamSo.Items.Add(dong);
        }

        lvThamSo.EndUpdate();
        lblSoLuong.Text = $"Có {danhSach.Count} tham số";
    }

    private void TaiLaiDanhSach()
    {
        try
        {
            ThamSoBUS.TaiLai();
            HienThi(bus.TimKiem(txtTimKiem.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải tham số");
        }
    }

    private void TimKiem()
    {
        try
        {
            HienThi(bus.TimKiem(txtTimKiem.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tìm kiếm");
        }
    }

    private void ChonDong()
    {
        if (lvThamSo.SelectedItems.Count == 0)
            return;

        var thamSo = (ThamSo)lvThamSo.SelectedItems[0].Tag!;
        txtMa.Text = thamSo.MaThamSo;
        txtMa.ReadOnly = true;
        txtGiaTri.Text = thamSo.GiaTri;
    }

    private void ChuanBiThem()
    {
        lvThamSo.SelectedItems.Clear();
        txtMa.ReadOnly = false;
        txtMa.Clear();
        txtGiaTri.Clear();
        txtMa.Focus();
    }

    private ThamSo DocDuLieu() => new()
    {
        MaThamSo = txtMa.Text,
        GiaTri = txtGiaTri.Text
    };

    private void Them()
    {
        if (txtMa.ReadOnly)
        {
            MessageBox.Show("Bấm Mới trước khi thêm tham số.");
            return;
        }

        KetQua ketQua = bus.Them(DocDuLieu());
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        }
    }

    private void Sua()
    {
        if (lvThamSo.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn tham số cần sửa.");
            return;
        }

        KetQua ketQua = bus.Sua(DocDuLieu());
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        }
    }

    private void Xoa()
    {
        if (lvThamSo.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn tham số cần xóa.");
            return;
        }

        string ma = txtMa.Text;
        if (MessageBox.Show(
                $"Xóa tham số {ma}?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        KetQua ketQua = bus.Xoa(ma);
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        }
    }
}
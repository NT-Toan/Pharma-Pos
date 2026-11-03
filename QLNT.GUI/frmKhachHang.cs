using System.Collections;

namespace QLNT.GUI;

public sealed class frmKhachHang : Form
{
    private readonly KhachHangBUS bus = new();

    private readonly TextBox txtMa = new()
    {
        ReadOnly = true,
        Dock = DockStyle.Fill
    };

    private readonly TextBox txtTen = new()
    {
        MaxLength = 100,
        Dock = DockStyle.Fill
    };

    private readonly TextBox txtSDT = new()
    {
        MaxLength = 15,
        Dock = DockStyle.Fill
    };

    private readonly TextBox txtDiaChi = new()
    {
        MaxLength = 200,
        Dock = DockStyle.Fill
    };

    private readonly TextBox txtTimKiem = new()
    {
        Dock = DockStyle.Fill
    };

    private readonly ListView lvKhachHang = new();
    private readonly Label lblSoLuong = new();

    public frmKhachHang()
    {
        Text = "Quản lý khách hàng";
        MinimumSize = new Size(720, 480);
        Size = new Size(950, 600);
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
        giaoDien.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
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

        thanhTim.Controls.Add(
            TaoNhan("Tìm mã, tên, SĐT hoặc địa chỉ:"), 0, 0);
        thanhTim.Controls.Add(txtTimKiem, 1, 0);

        var btnTaiLai = GiaoDienChung.TaoNutPhu("Tải lại");
        btnTaiLai.Click += (_, _) => TaiLaiDanhSach();
        thanhTim.Controls.Add(btnTaiLai, 2, 0);

        txtTimKiem.TextChanged += (_, _) => TimKiem();

        var bangNhap = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2
        };
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
        bangNhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bangNhap.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        bangNhap.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        ThemO(bangNhap, "Mã KH:", txtMa, 0, 0);
        ThemO(bangNhap, "Họ tên:", txtTen, 2, 0);
        ThemO(bangNhap, "SĐT:", txtSDT, 0, 1);
        ThemO(bangNhap, "Địa chỉ:", txtDiaChi, 2, 1);

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

        lvKhachHang.Dock = DockStyle.Fill;
        lvKhachHang.View = View.Details;
        lvKhachHang.FullRowSelect = true;
        lvKhachHang.GridLines = true;
        lvKhachHang.MultiSelect = false;
        lvKhachHang.HideSelection = false;
        lvKhachHang.Columns.Add("Mã KH", 110);
        lvKhachHang.Columns.Add("Họ tên", 250);
        lvKhachHang.Columns.Add("SĐT", 150);
        lvKhachHang.Columns.Add("Địa chỉ", 350);
        lvKhachHang.SelectedIndexChanged += (_, _) => ChonDong();

        lblSoLuong.Dock = DockStyle.Fill;
        lblSoLuong.ForeColor = GiaoDienChung.ChuPhu;

        giaoDien.Controls.Add(thanhTim, 0, 0);
        giaoDien.Controls.Add(bangNhap, 0, 1);
        giaoDien.Controls.Add(thanhNut, 0, 2);
        giaoDien.Controls.Add(lvKhachHang, 0, 3);
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

    private static void ThemO(
        TableLayoutPanel bang, string nhan, Control o, int cot, int hang)
    {
        bang.Controls.Add(TaoNhan(nhan), cot, hang);
        bang.Controls.Add(o, cot + 1, hang);
    }

    private void HienThi(ArrayList danhSach)
    {
        lvKhachHang.BeginUpdate();
        lvKhachHang.Items.Clear();

        foreach (KhachHang kh in danhSach)
        {
            var dong = new ListViewItem(kh.MaKH) { Tag = kh };
            dong.SubItems.Add(kh.HoTen);
            dong.SubItems.Add(kh.SDT ?? "");
            dong.SubItems.Add(kh.DiaChi ?? "");
            lvKhachHang.Items.Add(dong);
        }

        lvKhachHang.EndUpdate();
        lblSoLuong.Text = $"Có {danhSach.Count} khách hàng";
    }

    private void TaiLaiDanhSach()
    {
        try
        {
            KhachHangBUS.TaiLai();
            HienThi(bus.TimKiem(txtTimKiem.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải khách hàng");
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
        if (lvKhachHang.SelectedItems.Count == 0)
            return;

        var kh = (KhachHang)lvKhachHang.SelectedItems[0].Tag!;
        txtMa.Text = kh.MaKH;
        txtTen.Text = kh.HoTen;
        txtSDT.Text = kh.SDT ?? "";
        txtDiaChi.Text = kh.DiaChi ?? "";
    }

    private void ChuanBiThem()
    {
        lvKhachHang.SelectedItems.Clear();
        txtMa.Text = "(Tự tạo khi thêm)";
        txtTen.Clear();
        txtSDT.Clear();
        txtDiaChi.Clear();
        txtTen.Focus();
    }

    private KhachHang DocDuLieu() => new()
    {
        MaKH = lvKhachHang.SelectedItems.Count > 0 ? txtMa.Text : "",
        HoTen = txtTen.Text,
        SDT = txtSDT.Text,
        DiaChi = txtDiaChi.Text
    };

    private void Them()
    {
        if (lvKhachHang.SelectedItems.Count > 0)
        {
            MessageBox.Show("Bấm Mới trước khi thêm khách hàng.");
            return;
        }

        KetQuaLuuKhachHang ketQua = bus.Them(DocDuLieu());
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        }
    }

    private void Sua()
    {
        if (lvKhachHang.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn khách hàng cần sửa.");
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
        if (lvKhachHang.SelectedItems.Count == 0)
        {
            MessageBox.Show("Hãy chọn khách hàng cần xóa.");
            return;
        }

        string maKH = txtMa.Text;
        if (MessageBox.Show(
                $"Xóa khách hàng {maKH}?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        KetQua ketQua = bus.Xoa(maKH);
        MessageBox.Show(ketQua.ThongBao);

        if (ketQua.ThanhCong)
        {
            TaiLaiDanhSach();
            ChuanBiThem();
        }
    }
}
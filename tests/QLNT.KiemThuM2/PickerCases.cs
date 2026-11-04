using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraChuyenThuoc()
    {
        KiemTraChuyenThuoc(false);
        KiemTraChuyenThuoc(true);
    }

    private static void KiemTraChuyenThuoc(bool nhapHang)
    {
        string cheDo = nhapHang ? "Nhap hang" : "Ban hang";
        int GioiHan(Thuoc thuoc) => nhapHang ? int.MaxValue - thuoc.SoLuong : thuoc.SoLuong;
        using var picker = new frmChonThuoc(GioiHan);
        // Đưa chính các điều khiển thật sang cửa sổ ẩn để tạo sự kiện WinForms thật.
        // Không mở picker gốc: Load của picker không chạy, nên không đọc SQL/cache.
        bool daLoadPicker = false;
        picker.Load += (_, _) => daLoadPicker = true;
        using var host = new Form { Opacity = 0, ShowInTaskbar = false, Size = new Size(850, 520) };
        foreach (Control control in picker.Controls.Cast<Control>().ToArray()) host.Controls.Add(control);
        var danhSach = host.Controls.OfType<ListView>().Single();
        var thanhNut = host.Controls.OfType<FlowLayoutPanel>().Single();
        var soLuong = thanhNut.Controls.OfType<NumericUpDown>().Single();
        var nutChon = thanhNut.Controls.OfType<Button>().Single(b => b.Text == "Chọn");
        Thuoc[] cacThuoc =
        [
            new() { MaThuoc = "TESTA", SoLuong = 5 }, new() { MaThuoc = "TESTB", SoLuong = 2 },
            new() { MaThuoc = "TESTC", SoLuong = 0 }, new() { MaThuoc = "TESTD", SoLuong = int.MaxValue }
        ];
        foreach (Thuoc thuoc in cacThuoc) danhSach.Items.Add(new ListViewItem(thuoc.MaThuoc) { Tag = thuoc });
        host.Show();
        Application.DoEvents();
        danhSach.Focus();

        void Chon(int index)
        {
            danhSach.SelectedItems.Clear();
            Application.DoEvents();
            danhSach.Items[index].Selected = true;
            Application.DoEvents();
        }
        bool DungGioiHan(int index)
        {
            int toiDa = GioiHan(cacThuoc[index]);
            return soLuong.Minimum == (toiDa > 0 ? 1 : 0) && soLuong.Maximum == Math.Max(toiDa, 1) &&
                soLuong.Value == (toiDa > 0 ? 1 : 0) && nutChon.Enabled == (toiDa > 0) && soLuong.Enabled == (toiDa > 0);
        }

        Chon(0);
        Check(DungGioiHan(0), cheDo + " chon thuoc co so luong hop le");
        if (soLuong.Enabled) soLuong.Value = soLuong.Maximum;
        Chon(1);
        Check(DungGioiHan(1), cheDo + " doi dong khi so luong cu dang o muc toi da");
        danhSach.SelectedItems.Clear();
        Application.DoEvents();
        Check(soLuong.Minimum == 0 && soLuong.Value == 0 && !soLuong.Enabled && !nutChon.Enabled,
            cheDo + " bo chon khong gan 0 truoc khi ha Minimum");
        Chon(2);
        Check(DungGioiHan(2), cheDo + " chon thuoc ton 0 dung gioi han");
        if (soLuong.Enabled) soLuong.Value = soLuong.Maximum;
        Chon(3);
        Check(DungGioiHan(3), cheDo + " chuyen sang thuoc ton int MaxValue");
        Chon(0);
        Check(DungGioiHan(0), cheDo + " chon lai thuoc binh thuong sau cac gioi han");
        danhSach.SelectedItems.Clear();
        danhSach.Items.Clear();
        Application.DoEvents();
        Check(soLuong.Minimum == 0 && soLuong.Value == 0 && !nutChon.Enabled,
            cheDo + " danh sach bi xoa khong loi so luong");
        Check(!daLoadPicker, cheDo + " kiem thu dieu khien khong chay Load ket noi SQL");
        host.Close();
    }
}

using System.Reflection;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraSoanPhieu()
    {
        using var form = new frmTaoPhieuNhap();
        using var host = new Form { Opacity = 0, ShowInTaskbar = false, Size = form.Size };
        // Dùng điều khiển và sự kiện thật; không chạy Load hoặc Lưu phiếu nên không kết nối SQL.
        foreach (Control control in form.Controls.Cast<Control>().ToArray()) host.Controls.Add(control);
        host.Show();
        Application.DoEvents();
        T O<T>(string ten) where T : Control => (T)host.Controls.Find(ten, true).Single();
        var them = O<Button>("btnThemThuocVaoPhieu");
        var sua = O<Button>("btnCapNhatThuocTrongPhieu");
        var bo = O<Button>("btnBoThuocKhoiPhieu");
        var moi = O<Button>("btnNhapThuocMoi");
        var thuoc = O<TextBox>("txtThuoc");
        var soLuong = O<NumericUpDown>("nudSoLuong");
        var gia = O<NumericUpDown>("nudDonGiaNhap");
        var tong = O<TextBox>("txtTongTien");
        var danhSach = O<ListView>("lvChiTietDuThao");
        var ma = O<TextBox>("txtMaPN");
        ma.Text = "PNTEST";

        void ChonThuoc(string code)
        {
            // Thay kết quả hộp thoại chọn thuốc bằng dữ liệu giả để kiểm tra bản nháp độc lập SQL.
            typeof(frmTaoPhieuNhap).GetField("thuocDangChon", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(form, new Thuoc { MaThuoc = code, TenThuoc = "Thuoc " + code });
            thuoc.Text = code;
            Application.DoEvents();
        }
        void ChonDong(int index)
        {
            danhSach.Focus();
            danhSach.SelectedItems.Clear();
            danhSach.Items[index].Selected = true;
            Application.DoEvents();
        }
        bool VungNhapMoi() => thuoc.Text.Length == 0 && soLuong.Value == 1 && gia.Value == 0 &&
            !them.Enabled && !sua.Enabled && !bo.Enabled && moi.Enabled;

        Check(VungNhapMoi(), "Soan phieu ban dau chi cho nhap moi chua bat nut ghi thuoc");
        ChonThuoc("TESTA"); soLuong.Value = 8; gia.Value = 5000;
        Check(them.Enabled && !sua.Enabled && !bo.Enabled, "Chon thuoc moi hop le chi bat Them thuoc");
        them.PerformClick(); Application.DoEvents();
        Check(danhSach.Items.Count == 1 && tong.Text == 40000m.ToString("N2") && VungNhapMoi(),
            "Them thuoc cap nhat bang tong tien va tu lam moi o nhap");
        ChonDong(0);
        Check(!them.Enabled && sua.Enabled && bo.Enabled && soLuong.Value == 8 && gia.Value == 5000,
            "Chon dong nap du lieu va chuyen sang che do cap nhat");
        soLuong.Value = 10; gia.Value = 6000;
        sua.PerformClick(); Application.DoEvents();
        Check(danhSach.Items.Count == 1 && ((CTPhieuNhap)danhSach.Items[0].Tag!).SoLuong == 10 &&
            tong.Text == 60000m.ToString("N2") && VungNhapMoi(), "Cap nhat sua dung dong khong them trung va tu lam moi");
        ChonDong(0); soLuong.Value = 20;
        moi.PerformClick(); Application.DoEvents();
        Check(danhSach.Items.Count == 1 && tong.Text == 60000m.ToString("N2") && VungNhapMoi() && ma.Text == "PNTEST",
            "Nhap moi huy du lieu dang sua nhung giu bang tong tien va dau phieu");
        ChonThuoc("TESTA");
        Check(!them.Enabled, "Them thuoc da co trong phieu bi vo hieu hoa");
        moi.PerformClick(); ChonThuoc("TESTB"); soLuong.Value = 2; gia.Value = 5000;
        them.PerformClick(); Application.DoEvents();
        Check(danhSach.Items.Count == 2 && tong.Text == 70000m.ToString("N2"), "Them thuoc thu hai giu thuoc dau va cong tong dung");
        ChonDong(1); gia.Value = gia.Maximum;
        Check(!sua.Enabled && bo.Enabled, "Tong tien vuot gioi han khoa Cap nhat van cho Bo khoi phieu");
        gia.Value = 5000;
        Check(sua.Enabled, "Sua du lieu hop le bat lai Cap nhat");
        bo.PerformClick(); Application.DoEvents();
        Check(danhSach.Items.Count == 1 && tong.Text == 60000m.ToString("N2") && VungNhapMoi(),
            "Bo thuoc chi xoa dong dang chon va tinh lai tong");
        ChonDong(0); bo.PerformClick(); Application.DoEvents();
        Check(danhSach.Items.Count == 0 && tong.Text == 0m.ToString("N2") && VungNhapMoi(),
            "Bo thuoc cuoi ve ban nhap rong va khoa nut sua xoa");
        ChonThuoc("TESTC");
        Check(them.Enabled && gia.Value == 0, "Gia nhap 0 van hop le theo rang buoc hien co");
        moi.PerformClick();
        Check(VungNhapMoi() && danhSach.Items.Count == 0, "Nhap moi xoa du lieu dang them do");
        host.Close();
    }
}

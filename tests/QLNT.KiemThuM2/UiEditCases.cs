using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using QLNT.GUI;

internal static partial class Program
{
    private delegate bool EnumWindowsCallback(IntPtr cuaSo, IntPtr duLieu);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr duLieu);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr cuaSo, StringBuilder tenLop, int doDai);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr cuaSo, StringBuilder tieuDe, int doDai);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr cuaSo, out uint maTienTrinh);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr cuaSo, uint thongDiep, IntPtr wParam, IntPtr lParam);

    private static Task<string> DongThongBaoCuaTienTrinh()
    {
        return Task.Run(async () =>
        {
            for (int lan = 0; lan < 400; lan++)
            {
                string? tieuDeDaDong = null;
                EnumWindows((cuaSo, _) =>
                {
                    GetWindowThreadProcessId(cuaSo, out uint maTienTrinh);
                    if (maTienTrinh != Environment.ProcessId) return true;
                    var tenLop = new StringBuilder(64);
                    GetClassName(cuaSo, tenLop, tenLop.Capacity);
                    if (tenLop.ToString() != "#32770") return true;

                    var tieuDe = new StringBuilder(128);
                    GetWindowText(cuaSo, tieuDe, tieuDe.Capacity);
                    tieuDeDaDong = tieuDe.ToString();
                    PostMessage(cuaSo, 0x0010, IntPtr.Zero, IntPtr.Zero);
                    return false;
                }, IntPtr.Zero);
                if (tieuDeDaDong != null) return tieuDeDaDong;
                await Task.Delay(50);
            }
            return "KHONG_CO_THONG_BAO";
        });
    }

    private static T LayTruong<T>(object doiTuong, string ten) where T : class =>
        (T)(doiTuong.GetType().GetField(ten, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(doiTuong) ?? throw new InvalidOperationException("Không thấy trường " + ten));

    private static void KiemTraSuaLuuTrenForm()
    {
        new MyConnection().ThucThi("INSERT INTO THUOC VALUES ('TH0900', N'Thuoc thu form', '2030-12-31', 100, 0, 'LT0001')");
        ThuocBUS.TaiLai();
        var phieuBUS = new PhieuNhapBUS();
        PhieuNhap phieu = Phieu();
        Check(phieuBUS.Them(phieu, new ArrayList { Dong("TH0900", 10, 5) }).ThanhCong && Ton("TH0900") == 10,
            "Tao phieu rieng de thu form sua");

        using (var form = new frmTaoPhieuNhap(phieu.MaPN) { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show();
            Application.DoEvents();
            var danhSach = (ListView)form.Controls.Find("lvChiTietDuThao", true).Single();
            var soLuong = (NumericUpDown)form.Controls.Find("nudSoLuong", true).Single();
            var capNhat = (Button)form.Controls.Find("btnCapNhatThuocTrongPhieu", true).Single();
            var luu = (Button)form.Controls.Find("btnLuuPhieu", true).Single();
            danhSach.Items[0].Selected = true;
            Application.DoEvents();
            soLuong.Value = 12;
            Check(capNhat.Enabled, "Form sua phieu cho cap nhat so luong dong");
            capNhat.PerformClick();
            Check(((CTPhieuNhap)danhSach.Items[0].Tag!).SoLuong == 12,
                "Form sua phieu cap nhat ban nhap truoc khi luu");

            Task<string> thongBao = DongThongBaoCuaTienTrinh();
            luu.PerformClick();
            Check(thongBao.GetAwaiter().GetResult() == "Thành công" &&
                  form.MaPhieuDaLuu == phieu.MaPN && Ton("TH0900") == 12 &&
                  phieuBUS.LayDeSua(phieu.MaPN).Phieu.TongTien == 60m,
                "Bam luu tren form sua phieu ghi SQL va tang ton theo chenh lech");
        }

        ThuocBUS.TaiLai();
        Thuoc thuoc = new ThuocBUS().LayTheoMa("TH0900")!;
        var hoaDonBUS = new HoaDonBUS();
        KetQuaLuuHoaDon ketQuaTao = hoaDonBUS.TaoHoaDon(
            new NhanVien { MaNV = "NV0001", HoTenNV = "Nhan vien thu" }, null,
            DateTime.Today, null, new List<DongHoaDonDuThao> { new(thuoc) { SoLuong = 2 } });
        Check(ketQuaTao.ThanhCong && Ton("TH0900") == 10, "Tao hoa don rieng de thu form sua");
        HoaDon hoaDon = hoaDonBUS.LayTatCa().Cast<HoaDon>().Single(h => h.MaHD == ketQuaTao.MaHD);

        using (var form = new frmTaoHoaDon(hoaDon) { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show();
            Application.DoEvents();
            DataGridView danhSach = LayTruong<DataGridView>(form, "dgvChiTiet");
            danhSach.Rows[0].Cells["SoLuong"].Value = "3";
            form.GetType().GetMethod("CapNhatSoLuong", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, [0]);
            NumericUpDown tienDuaThem = LayTruong<NumericUpDown>(form, "nudTienKhachDua");
            tienDuaThem.Value = 99;
            Check(LayTruong<Label>(form, "lblTongTien").Text == 300m.ToString("N2"),
                "Form sua hoa don tinh lai tien sau khi doi so luong");

            Task<string> thongBaoChuaDu = DongThongBaoCuaTienTrinh();
            LayTruong<Button>(form, "btnLuu").PerformClick();
            Check(thongBaoChuaDu.GetAwaiter().GetResult() == "Chưa đủ tiền" &&
                  hoaDonBUS.LayChiTiet(hoaDon.MaHD).Cast<CTHoaDon>().Single().SoLuong == 2,
                "Form sua hoa don chi yeu cau thu them phan chenh lech va khong luu neu thieu");

            tienDuaThem.Value = 100;
            Check(LayTruong<Label>(form, "lblTongTienDaLuu").Text ==
                  $"{200m:N2} / +{100m:N2}",
                "Form sua hoa don hien tong cu va tien can thu them");
            Task<string> thongBao = DongThongBaoCuaTienTrinh();
            LayTruong<Button>(form, "btnLuu").PerformClick();
            string tieuDe = thongBao.GetAwaiter().GetResult();
            int tonSauSua = Ton("TH0900");
            int luongSauSua = hoaDonBUS.LayChiTiet(hoaDon.MaHD).Cast<CTHoaDon>().Single().SoLuong;
            hoaDonBUS.TaiLai();
            decimal tienSauSua = hoaDonBUS.LayTatCa().Cast<HoaDon>().Single(h => h.MaHD == hoaDon.MaHD).TongTien;
            Check(tieuDe == "Hoàn tất bán hàng" && tonSauSua == 9 &&
                  luongSauSua == 3 && tienSauSua == 300m,
                "Bam luu tren form sua hoa don ghi SQL va tru them mot vien");
        }
    }
}

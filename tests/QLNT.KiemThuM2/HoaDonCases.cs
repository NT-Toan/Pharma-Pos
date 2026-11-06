using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraHoaDon()
    {
        var bus = new HoaDonBUS();
        ThuocBUS.TaiLai();
        var thuocBUS = new ThuocBUS();
        var khachHang = new KhachHang { MaKH = "KH0001", HoTen = "Khach hang test" };
        var nhanVien = new NhanVien { MaNV = "NV0001", HoTenNV = "Nhan vien test" };
        var chiTiet = new List<DongHoaDonDuThao>
        {
            new(thuocBUS.LayTheoMa("TH0001")!) { SoLuong = 2 },
            new(thuocBUS.LayTheoMa("TH0002")!) { SoLuong = 2 }
        };

        KetQuaLuuHoaDon tao = bus.TaoHoaDon(nhanVien, khachHang, DateTime.Today, "VC0001", chiTiet);
        Check(tao.ThanhCong && tao.MaHD == "HD0001", "Tao hoa don co ma va luu thanh cong");
        if (!tao.ThanhCong)
            throw new InvalidOperationException(tao.ThongBao);

        DataTable header = LayHeaderHoaDon(tao.MaHD);
        List<CTHoaDon> dongDaLuu = bus.LayChiTiet(tao.MaHD).Cast<CTHoaDon>().ToList();
        decimal tongDong = dongDaLuu.Sum(dong => dong.ThanhTien);
        decimal tongTruocGiamSanPham = dongDaLuu.Sum(dong => dong.DonGia * dong.SoLuong);
        decimal tienGiamHoaDon = Convert.ToDecimal(header.Rows[0]["TIENGIAM"]);
        decimal tongThanhToan = Convert.ToDecimal(header.Rows[0]["TONGTIEN"]);
        Check(header.Rows.Count == 1 && header.Rows[0]["MAVOUCHER"].ToString() == "VC0001" &&
            header.Rows[0].IsNull("MACTKM_HD") && tienGiamHoaDon == 87 && tongThanhToan == 493,
            "Luu voucher cao hon moc va dung tong giam hoa don");
        Check(dongDaLuu.Count == 2 && dongDaLuu.Single(dong => dong.MaThuoc == "TH0001").ThanhTien == 180 &&
            dongDaLuu.Single(dong => dong.MaThuoc == "TH0002").ThanhTien == 400 &&
            tongTruocGiamSanPham - tongDong == 20 && tongDong - tienGiamHoaDon == tongThanhToan,
            "CTHD doi soat dung giam san pham va HOADON.TIENGIAM");
        Check(Ton("TH0001") == 8 && Ton("TH0002") == 18,
            "Tao hoa don tru dung ton kho tung thuoc");

        HoaDon hoaDonIn = bus.LayTatCa().Cast<HoaDon>().Single(hoaDon => hoaDon.MaHD == tao.MaHD);
        string duongDanPdf = Path.Combine(thuMucExcel, "hoa-don-test.pdf");
        HoaDonIn.XuatPdf(duongDanPdf, hoaDonIn, dongDaLuu);
        using (var tepPdf = File.OpenRead(duongDanPdf))
        {
            Span<byte> dauTep = stackalloc byte[5];
            Check(tepPdf.Read(dauTep) == dauTep.Length && Encoding.ASCII.GetString(dauTep) == "%PDF-",
                "Xuat truc tiep PDF tu hoa don da luu");
        }

        ThuocBUS.TaiLai();
        thuocBUS = new ThuocBUS();
        var chiTietSua = new List<DongHoaDonDuThao>
        {
            new(thuocBUS.LayTheoMa("TH0001")!) { SoLuong = 3 },
            new(thuocBUS.LayTheoMa("TH0002")!) { SoLuong = 1 }
        };
        KetQuaLuuHoaDon sua = bus.SuaHoaDon(hoaDonIn, nhanVien, khachHang, DateTime.Today, null, chiTietSua);
        Check(sua.ThanhCong && sua.MaHD == tao.MaHD, "Sua hoa don hien co thanh cong");
        if (!sua.ThanhCong)
            throw new InvalidOperationException(sua.ThongBao);

        header = LayHeaderHoaDon(tao.MaHD);
        decimal tongSauSua = bus.LayChiTiet(tao.MaHD).Cast<CTHoaDon>().Sum(dong => dong.ThanhTien);
        Check(tongSauSua == 470 && Convert.ToDecimal(header.Rows[0]["TONGTIEN"]) == 470 &&
            Convert.ToDecimal(header.Rows[0]["TIENGIAM"]) == 0 &&
            header.Rows[0].IsNull("MAVOUCHER") && header.Rows[0].IsNull("MACTKM_HD") &&
            header.Rows[0].IsNull("MUCTIENHD"),
            "Sua hoa don tinh lai CTHD va xoa thong tin giam cu");
        Check(Ton("TH0001") == 7 && Ton("TH0002") == 19,
            "Sua hoa don hoan ton cu va tru ton moi dung mot lan");

        KetQuaLuuHoaDon huy = bus.HuyHoaDon(tao.MaHD);
        Check(huy.ThanhCong && Convert.ToInt32(new MyConnection().LayGiaTri("SELECT COUNT(*) FROM HOADON")) == 0 &&
            Convert.ToInt32(new MyConnection().LayGiaTri("SELECT COUNT(*) FROM CTHD")) == 0,
            "Huy hoa don xoa dau va chi tiet");
        Check(Ton("TH0001") == 10 && Ton("TH0002") == 20,
            "Huy hoa don hoan lai toan bo ton kho");

        using (var form = new frmTaoHoaDon { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show();
            Application.DoEvents();
            typeof(frmTaoHoaDon).GetField("khachLe", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(form, false);
            typeof(frmTaoHoaDon).GetField("khachHang", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(form, khachHang);
            var cacDongDuThao = (BindingList<DongHoaDonDuThao>)typeof(frmTaoHoaDon)
                .GetField("chiTiet", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            ThuocBUS.TaiLai();
            var thuocHienTai = new ThuocBUS();
            cacDongDuThao.Add(new DongHoaDonDuThao(thuocHienTai.LayTheoMa("TH0001")!) { SoLuong = 2 });
            cacDongDuThao.Add(new DongHoaDonDuThao(thuocHienTai.LayTheoMa("TH0002")!) { SoLuong = 2 });

            var txtVoucher = (TextBox)typeof(frmTaoHoaDon)
                .GetField("txtVoucher", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            var lblNguon = (Label)form.Controls.Find("lblLoaiKhuyenMaiHoaDon", true).Single();
            var dgv = (DataGridView)form.Controls.Find("dgvChiTiet", true).Single();
            typeof(frmTaoHoaDon).GetMethod("HienThiChiTiet", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, null);
            Check(lblNguon.Text.Contains("KMHoaDon - KM0001 (5%;", StringComparison.Ordinal) &&
                dgv.Rows[0].Cells["LoaiCTKM"].Value?.ToString() == "KMSanPham (10%)" &&
                dgv.Rows[1].Cells["LoaiCTKM"].Value?.ToString() == "Không",
                "Form nhap hien ro loai va phan tram KM tung dong va KMHoaDon");

            txtVoucher.Text = "VC0001";
            Application.DoEvents();
            Check(lblNguon.Text == "KMVoucher - VC0001 (15%)",
                "Form nhap hien KMVoucher va dung phan tram khi voucher duoc ap dung");
            form.Close();
        }

        new MyConnection().ThucThi("UPDATE THUOC SET SOLUONG = 0 WHERE MATHUOC = 'TH0002'");
        HoaDonBUS.TaiLaiDuLieuBanHang();
        ThuocBUS.TaiLai();
        var hetTon = new DongHoaDonDuThao(new ThuocBUS().LayTheoMa("TH0001")!) { SoLuong = 11 };
        KetQuaLuuHoaDon tuChoi = bus.TaoHoaDon(nhanVien, khachHang, DateTime.Today, null, [hetTon]);
        Check(!tuChoi.ThanhCong && Convert.ToInt32(new MyConnection().LayGiaTri("SELECT COUNT(*) FROM HOADON")) == 0 &&
            Ton("TH0001") == 10,
            "Chan tao hoa don vuot ton ma khong ghi du lieu");

        using (var form = new frmHoaDon { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show();
            Application.DoEvents();
            string[] nutHoaDon = CacDieuKhien(form).OfType<Button>().Select(nut => nut.Text).ToArray();
            Check(nutHoaDon.Contains("In hóa đơn") && nutHoaDon.Contains("Xuất PDF"),
                "Man hinh danh sach tach rieng nut in va xuat PDF");
            form.Close();
        }
        using (var form = new frmKhuyenMai { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show();
            Application.DoEvents();
            var danhSach = (ListView)CacDieuKhien(form).OfType<ListView>()
                .Single(danhSach => danhSach.Columns.Cast<ColumnHeader>()
                    .Any(cot => cot.Text == "Thể Loại Khuyến Mãi"));
            string TheLoai(string ma) => danhSach.Items.Cast<ListViewItem>()
                .Single(dong => dong.SubItems[0].Text == ma).SubItems[2].Text;

            Check(TheLoai("KM0001") == "KMHoaDon, KMSanPham, KMVoucher" &&
                TheLoai("KM0002") == "KMSanPham" &&
                TheLoai("KM0003") == "KMVoucher" &&
                TheLoai("KM0004") == "KMHoaDon",
                "Trang khuyen mai liet ke dung mot hoac nhieu the loai theo cau hinh");

            var timKiem = CacDieuKhien(form).OfType<TextBox>()
                .Single(o => o.PlaceholderText.Contains("thể loại", StringComparison.OrdinalIgnoreCase));
            timKiem.Text = "KMVoucher";
            Application.DoEvents();
            Check(danhSach.Items.Count == 2 &&
                danhSach.Items.Cast<ListViewItem>().All(dong => dong.SubItems[2].Text.Contains("KMVoucher")),
                "Tim kiem trang khuyen mai loc duoc theo the loai");
            form.Close();
        }
        using (var form = new frmTaoHoaDon { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show();
            Application.DoEvents();
            string[] nutLap = CacDieuKhien(form).OfType<Button>().Select(nut => nut.Text).ToArray();
            Check(nutLap.Contains("In nháp") && nutLap.Contains("Xuất PDF"),
                "Man hinh lap nhap tach rieng nut in va xuat PDF");
            form.Close();
        }
    }

    private static DataTable LayHeaderHoaDon(string maHD) => new MyConnection().LayBang(
        "SELECT MAHD, MAVOUCHER, MACTKM_HD, MUCTIENHD, TIENGIAM, TONGTIEN FROM HOADON WHERE MAHD = @maHD",
        new SqlParameter("@maHD", SqlDbType.VarChar, 10) { Value = maHD });

    private static IEnumerable<Control> CacDieuKhien(Control cha)
    {
        foreach (Control dieuKhien in cha.Controls)
        {
            yield return dieuKhien;
            foreach (Control con in CacDieuKhien(dieuKhien))
                yield return con;
        }
    }
}

using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using QLNT.GUI;

internal static partial class Program
{
    private static int soCa;
    private static string thuMucExcel = "";

    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        if (args.Contains("--picker-only") || args.Contains("--draft-only") ||
            args.Contains("--customer-filter-only") || args.Contains("--customer-picker-only") ||
            args.Contains("--invoice-layout-only") || args.Contains("--employee-picker-only") ||
            args.Contains("--thuoc-filter-only") || args.Contains("--thuoc-filter-ui-only"))
        {
            try
            {
                if (!args.Contains("--thuoc-filter-only")) Application.EnableVisualStyles();
                if (args.Contains("--thuoc-filter-only")) KiemTraLocThuocNangCao();
                else if (args.Contains("--thuoc-filter-ui-only")) KiemTraGiaoDienLocThuoc();
                else if (args.Contains("--employee-picker-only")) KiemTraChonNhanVienStatic();
                else if (args.Contains("--invoice-layout-only")) KiemTraBoCucTongTienHoaDon();
                else if (args.Contains("--customer-picker-only")) KiemTraChonKhachHangStatic();
                else if (args.Contains("--customer-filter-only")) KiemTraLocDiaChiKhachHang();
                else if (args.Contains("--draft-only")) KiemTraSoanPhieu();
                else KiemTraChuyenThuoc();
                string phanKiemTra = args.Contains("--thuoc-filter-only") ? "BUS" : "UI";
                Console.WriteLine($"PASS {phanKiemTra}: {soCa} kiem tra; khong ket noi database.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
        // Luôn ghi đè cấu hình database bằng tên ngẫu nhiên: không thể chạy vào DB ứng dụng.
        string database = "QLNT_Test_PN_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable("QLNT_DATABASE", database);
        Console.WriteLine("Database kiem thu rieng: " + database);
        thuMucExcel = Path.Combine(Path.GetTempPath(), "QLNT_Test_Files_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(thuMucExcel);

        var adminBuilder = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("QLNT_SQL_SERVER") ?? "localhost",
            InitialCatalog = "master",
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };
        bool daTao = false;
        try
        {
            using (var admin = new SqlConnection(adminBuilder.ConnectionString))
            {
                admin.Open();
                using var lenh = new SqlCommand($"CREATE DATABASE [{database}]", admin);
                lenh.ExecuteNonQuery();
                daTao = true;
            }

            if (Convert.ToString(new MyConnection().LayGiaTri("SELECT DB_NAME()")) != database)
                throw new InvalidOperationException("Ket noi khong tro dung database kiem thu; dung truoc moi lenh ghi.");
            TaoDuLieuKiemThu();
            if (args.Contains("--ui-edit-only"))
            {
                KiemTraSuaLuuTrenForm();
                Console.WriteLine($"PASS UI EDIT: {soCa} ca; database ung dung khong bi ghi du lieu.");
                return 0;
            }
            KiemTraHoaDon();
            KiemTraBUSVaGiaoDich();
            KiemTraSuaXoa();
            KiemTraExcel();
            KiemTraForm();
            KiemTraChuyenThuoc();
            KiemTraSoanPhieu();
            KiemTraSuaLuuTrenForm();
            KiemTraLocDiaChiKhachHang();
            KiemTraChonKhachHangStatic();
            KiemTraChonNhanVienStatic();
            KiemTraBoCucTongTienHoaDon();
            KiemTraLocThuocNangCao();
            KiemTraGiaoDienLocThuoc();
            Console.WriteLine($"PASS ALL: {soCa} ca; database ung dung khong bi ghi du lieu.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
        finally
        {
            try
            {
                if (daTao)
                {
                    SqlConnection.ClearAllPools();
                    using var admin = new SqlConnection(adminBuilder.ConnectionString);
                    admin.Open();
                    if (!Regex.IsMatch(database, "^QLNT_Test_PN_[a-f0-9]{32}$"))
                        throw new InvalidOperationException("Ten database kiem thu khong an toan.");
                    using var lenh = new SqlCommand($"DROP DATABASE [{database}]", admin);
                    lenh.ExecuteNonQuery();
                    Console.WriteLine("Da xoa database kiem thu rieng: " + database);
                }
            }
            finally
            {
                foreach (string file in Directory.GetFiles(thuMucExcel)) File.Delete(file);
                Directory.Delete(thuMucExcel);
            }
        }
    }

    private static void TaoDuLieuKiemThu()
    {
        new MyConnection().ThucThi("""
            CREATE TABLE LOAITHUOC (MALOAI VARCHAR(10) PRIMARY KEY, TENLOAI NVARCHAR(100) NOT NULL);
            CREATE TABLE NHANVIEN (MANV VARCHAR(10) PRIMARY KEY, HOTENNV NVARCHAR(100) NOT NULL, CHUCVU NVARCHAR(50), LUONG DECIMAL(18,2));
            CREATE TABLE NHACUNGCAP (MANCC VARCHAR(10) PRIMARY KEY, TENNCC NVARCHAR(100) NOT NULL, DIACHI NVARCHAR(200), SDT VARCHAR(15));
            CREATE TABLE THUOC (
                MATHUOC VARCHAR(10) PRIMARY KEY, TENTHUOC NVARCHAR(100) NOT NULL, HSD DATE,
                DONGIA DECIMAL(18,2) NOT NULL CHECK (DONGIA > 0),
                SOLUONG INT NOT NULL DEFAULT 0 CHECK (SOLUONG >= 0),
                MALOAI VARCHAR(10) NOT NULL REFERENCES LOAITHUOC(MALOAI));
            CREATE TABLE KHACHHANG (
                MAKH VARCHAR(10) PRIMARY KEY, HOTEN NVARCHAR(100) NOT NULL,
                SDT VARCHAR(15), DIACHI NVARCHAR(200));
            CREATE TABLE CTKHUYENMAI (
                MACTKM VARCHAR(10) PRIMARY KEY, TENCTKM NVARCHAR(100) NOT NULL,
                NGAYBATDAU DATE, NGAYKETTHUC DATE);
            CREATE TABLE KM_SANPHAM (
                MACTKM VARCHAR(10) NOT NULL REFERENCES CTKHUYENMAI(MACTKM),
                MATHUOC VARCHAR(10) NOT NULL REFERENCES THUOC(MATHUOC),
                PHANTRAMGIAM DECIMAL(5,2) NOT NULL,
                PRIMARY KEY (MACTKM, MATHUOC));
            CREATE TABLE KM_VOUCHER (
                MAVOUCHER VARCHAR(20) PRIMARY KEY,
                MACTKM VARCHAR(10) NOT NULL REFERENCES CTKHUYENMAI(MACTKM),
                MAKH VARCHAR(10) REFERENCES KHACHHANG(MAKH),
                PHANTRAMGIAM DECIMAL(5,2) NOT NULL);
            CREATE TABLE KM_HOADON (
                MACTKM VARCHAR(10) NOT NULL REFERENCES CTKHUYENMAI(MACTKM),
                MUCTIENHD DECIMAL(18,2) NOT NULL,
                PHANTRAMGIAM DECIMAL(5,2) NOT NULL,
                PRIMARY KEY (MACTKM, MUCTIENHD));
            CREATE TABLE HOADON (
                MAHD VARCHAR(10) PRIMARY KEY,
                MANV VARCHAR(10) NOT NULL REFERENCES NHANVIEN(MANV),
                NGAYLAP DATE NOT NULL,
                TONGTIEN DECIMAL(18,2) NOT NULL CHECK (TONGTIEN >= 0),
                MAKH VARCHAR(10) REFERENCES KHACHHANG(MAKH),
                MAVOUCHER VARCHAR(20) REFERENCES KM_VOUCHER(MAVOUCHER),
                MACTKM_HD VARCHAR(10) REFERENCES CTKHUYENMAI(MACTKM),
                MUCTIENHD DECIMAL(18,2),
                TIENGIAM DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (TIENGIAM >= 0));
            CREATE TABLE CTHD (
                MAHD VARCHAR(10) NOT NULL REFERENCES HOADON(MAHD),
                MATHUOC VARCHAR(10) NOT NULL REFERENCES THUOC(MATHUOC),
                SOLUONG INT NOT NULL CHECK (SOLUONG > 0),
                DONGIA DECIMAL(18,2) NOT NULL CHECK (DONGIA >= 0),
                THANHTIEN DECIMAL(18,2) NOT NULL CHECK (THANHTIEN >= 0),
                PRIMARY KEY (MAHD, MATHUOC));
            CREATE TABLE PHIEUNHAP (
                MAPN VARCHAR(10) PRIMARY KEY,
                MANCC VARCHAR(10) NOT NULL REFERENCES NHACUNGCAP(MANCC),
                MANV VARCHAR(10) NOT NULL REFERENCES NHANVIEN(MANV),
                NGAYNHAP DATE NOT NULL,
                TONGTIEN DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (TONGTIEN >= 0));
            CREATE TABLE CTPN (
                MATHUOC VARCHAR(10) REFERENCES THUOC(MATHUOC),
                MAPN VARCHAR(10) REFERENCES PHIEUNHAP(MAPN),
                SOLUONG INT NOT NULL CHECK (SOLUONG > 0),
                DONGIA DECIMAL(18,2) NOT NULL CHECK (DONGIA >= 0),
                PRIMARY KEY (MATHUOC, MAPN));
            INSERT INTO LOAITHUOC VALUES ('LT0001', N'Loai test');
            INSERT INTO NHANVIEN VALUES ('NV0001', N'Nhan vien test', NULL, NULL);
            INSERT INTO NHACUNGCAP VALUES ('NCC0001', N'NCC test', NULL, NULL);
            INSERT INTO THUOC VALUES
                ('TH0001', N'A thuoc test', '2030-12-31', 100, 10, 'LT0001'),
                ('TH0002', N'B thuoc test', NULL, 200, 0, 'LT0001'),
                ('TH0003', N'C thuoc test', NULL, 300, 2147483647, 'LT0001'),
                ('TH0004', N'D thuoc test', NULL, 400, 0, 'LT0001');
            INSERT INTO KHACHHANG VALUES ('KH0001', N'Khach hang test', NULL, NULL);
            INSERT INTO CTKHUYENMAI VALUES
                ('KM0001', N'Khuyen mai test', '2000-01-01', '2099-12-31'),
                ('KM0002', N'Chi giam san pham', '2000-01-01', '2099-12-31'),
                ('KM0003', N'Chi voucher', '2000-01-01', '2099-12-31'),
                ('KM0004', N'Chi giam hoa don', '2099-01-01', '2099-12-31');
            INSERT INTO KM_SANPHAM VALUES ('KM0001', 'TH0001', 10);
            INSERT INTO KM_SANPHAM VALUES ('KM0002', 'TH0004', 10);
            INSERT INTO KM_HOADON VALUES ('KM0001', 500, 5);
            INSERT INTO KM_HOADON VALUES ('KM0004', 100, 5);
            INSERT INTO KM_VOUCHER VALUES ('VC0001', 'KM0001', 'KH0001', 15);
            INSERT INTO KM_VOUCHER VALUES ('VC0002', 'KM0003', 'KH0001', 10);
            UPDATE THUOC SET SOLUONG = 20 WHERE MATHUOC = 'TH0002';
            """);
    }

    private static void Check(bool dung, string ten)
    {
        if (!dung) throw new InvalidOperationException("FAIL " + ten);
        Console.WriteLine("PASS " + ten);
        soCa++;
    }

    private static void LoiDuLieu(Action thaoTac, string ten)
    {
        try { thaoTac(); }
        catch (ArgumentException) { Check(true, ten); return; }
        throw new InvalidOperationException("FAIL Khong tu choi: " + ten);
    }

    private static PhieuNhap Phieu() => new()
    {
        MaNV = "NV0001", MaNCC = "NCC0001", NgayNhap = DateTime.Today, TongTien = 999
    };

    private static CTPhieuNhap Dong(string ma, int soLuong, decimal gia) => new()
    {
        MaThuoc = ma, TenThuoc = ma, SoLuong = soLuong, DonGia = gia
    };

    private static int Dem(string bang) => Convert.ToInt32(new MyConnection().LayGiaTri("SELECT COUNT(*) FROM " + bang));
    private static int Ton(string ma) => Convert.ToInt32(new MyConnection().LayGiaTri(
        "SELECT SOLUONG FROM THUOC WHERE MATHUOC = @ma",
        new SqlParameter("@ma", SqlDbType.VarChar, 10) { Value = ma }));

    private static void KiemTraBUSVaGiaoDich()
    {
        var bus = new PhieuNhapBUS();
        Check(bus.TinhTongTien(new ArrayList()) == 0, "Du thao rong co tong 0");
        Check(bus.TinhTongTien(new ArrayList { Dong("TH0001", 3, 2.50m), Dong("TH0002", 2, 0) }) == 7.50m,
            "Tinh tong thap phan va don gia nhap 0");
        LoiDuLieu(() => bus.TinhTongTien(new ArrayList { Dong("TH0001", 0, 1) }), "Chan so luong 0");
        LoiDuLieu(() => bus.TinhTongTien(new ArrayList { Dong("TH0001", -1, 1) }), "Chan so luong am");
        LoiDuLieu(() => bus.TinhTongTien(new ArrayList { Dong("TH0001", 1, -1) }), "Chan gia am");
        LoiDuLieu(() => bus.TinhTongTien(new ArrayList { Dong("TH0001", 1, 0.001m) }), "Chan gia qua 2 so le");
        LoiDuLieu(() => bus.TinhTongTien(new ArrayList { Dong("TH0001", 1, 1), Dong(" th0001 ", 1, 1) }), "Chan trung ma thuoc");
        LoiDuLieu(() => bus.TinhTongTien(new ArrayList { Dong("TH0001", 2, 9999999999999999.99m) }), "Chan tran thanh tien");
        LoiDuLieu(() => bus.TinhTongTien(new ArrayList { Dong("TH0001", 1, 9999999999999999.99m), Dong("TH0002", 1, 1) }), "Chan tran tong tien");
        Check(!bus.KiemTraDuLieu(Phieu(), new ArrayList()).ThanhCong, "Chan phieu khong co thuoc");
        var saiNV = Phieu(); saiNV.MaNV = "NV9999";
        Check(!bus.Them(saiNV, new ArrayList { Dong("TH0001", 1, 1) }).ThanhCong && Dem("PHIEUNHAP") == 0,
            "Nhan vien khong ton tai khong ghi phieu");
        var saiNCC = Phieu(); saiNCC.MaNCC = "NCC9999";
        Check(!bus.Them(saiNCC, new ArrayList { Dong("TH0001", 1, 1) }).ThanhCong && Dem("PHIEUNHAP") == 0,
            "NCC khong ton tai khong ghi phieu");
        Check(!bus.Them(Phieu(), new ArrayList { Dong("TH9999", 1, 1) }).ThanhCong && Dem("PHIEUNHAP") == 0,
            "Thuoc khong ton tai khong ghi phieu");

        Check(bus.LayTatCa().Count == 0, "Cache phieu truoc khi them");
        var thuocBUS = new ThuocBUS();
        Check(thuocBUS.LayTheoMa("TH0001")!.SoLuong == 10, "Nap cache ton cu");
        var phieu = Phieu();
        KetQua ketQua = bus.Them(phieu, new ArrayList { Dong("TH0001", 3, 2.50m), Dong("TH0002", 2, 0) });
        Check(ketQua.ThanhCong && phieu.MaPN == "PN0001" && phieu.TongTien == 7.50m, "Commit phieu va tinh lai tong trong BUS");
        Check(Dem("PHIEUNHAP") == 1 && Dem("CTPN") == 2 && Ton("TH0001") == 13 && Ton("TH0002") == 2,
            "Commit dau phieu chi tiet va tang kho");
        Check(bus.LayTatCa().Count == 1 && thuocBUS.LayTheoMa("TH0001")!.SoLuong == 13, "Cache phieu va thuoc duoc lam moi");
        Check(Convert.ToDecimal(new MyConnection().LayGiaTri("SELECT DONGIA FROM THUOC WHERE MATHUOC = 'TH0001'")) == 100,
            "Gia nhap khong ghi de gia ban");

        var dao = new PhieuNhapDAO();
        bool daRollback = false;
        try { dao.Them(Phieu(), new ArrayList { Dong("TH0001", 2, 1), Dong("TH9999", 1, 1) }); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("TH9999")) { daRollback = true; }
        Check(daRollback && Dem("PHIEUNHAP") == 1 && Dem("CTPN") == 2 && Ton("TH0001") == 13,
            "Lenh bat buoc 0 dong rollback ca cac lenh da thanh cong");

        daRollback = false;
        try { dao.Them(Phieu(), new ArrayList { Dong("TH0001", 2, 1), Dong("TH0003", 1, 1) }); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("TH0003")) { daRollback = true; }
        Check(daRollback && Dem("PHIEUNHAP") == 1 && Dem("CTPN") == 2 && Ton("TH0001") == 13 && Ton("TH0003") == int.MaxValue,
            "Tran ton kho rollback toan bo");

        daRollback = false;
        try { dao.Them(Phieu(), new ArrayList { Dong("TH0001", 2, 1), Dong("TH0001", 1, 1) }); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627) { daRollback = true; }
        Check(daRollback && Dem("PHIEUNHAP") == 1 && Dem("CTPN") == 2 && Ton("TH0001") == 13,
            "Loi SQL o chi tiet rollback phieu va tang kho truoc do");

        var phieuA = Phieu(); var phieuB = Phieu();
        Task<KetQua> a = Task.Run(() => new PhieuNhapBUS().Them(phieuA, new ArrayList { Dong("TH0001", 1, 1) }));
        Task<KetQua> b = Task.Run(() => new PhieuNhapBUS().Them(phieuB, new ArrayList { Dong("TH0001", 1, 1) }));
        Task.WaitAll(a, b);
        Check(a.Result.ThanhCong && b.Result.ThanhCong && phieuA.MaPN != phieuB.MaPN && Dem("PHIEUNHAP") == 3 && Ton("TH0001") == 15,
            "Hai nguoi them dong thoi co ma rieng va khong mat tang kho");
    }

    private static void KiemTraForm()
    {
        Application.EnableVisualStyles();
        using (var form = new frmPhieuNhap { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show(); Application.DoEvents();
            var phieu = (ListView)form.Controls.Find("lvPhieuNhap", true).Single();
            var chiTiet = (ListView)form.Controls.Find("lvChiTiet", true).Single();
            Check(phieu.Items.Count == Dem("PHIEUNHAP"), "Form doc du danh sach phieu");
            Check(form.Controls.Find("btnSuaPhieu", true).Length == 1 && form.Controls.Find("btnXoaPhieu", true).Length == 1,
                "Form phieu co nut sua va xoa");
            phieu.SelectedItems.Clear();
            phieu.Items.Cast<ListViewItem>().Single(d => d.Text == "PN0001").Selected = true;
            Application.DoEvents();
            Check(chiTiet.Items.Count == 2, "Chon phieu hien dung hai dong chi tiet");
            form.Close();
        }
        using (var form = new frmTaoPhieuNhap { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show(); Application.DoEvents();
            var ma = (TextBox)form.Controls.Find("txtMaPN", true).Single();
            var tong = (TextBox)form.Controls.Find("txtTongTien", true).Single();
            Check(ma.ReadOnly && tong.ReadOnly && ma.Text == new PhieuNhapBUS().TaoMaMoi(), "Form them co ma du kien va o tu tinh chi doc");
            form.Close();
        }
        using (var form = new frmTaoPhieuNhap("PN0001") { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show(); Application.DoEvents();
            var ma = (TextBox)form.Controls.Find("txtMaPN", true).Single();
            var chiTiet = (ListView)form.Controls.Find("lvChiTietDuThao", true).Single();
            var tong = (TextBox)form.Controls.Find("txtTongTien", true).Single();
            DuLieuPhieuNhap duLieu = new PhieuNhapBUS().LayDeSua("PN0001");
            Check(ma.ReadOnly && ma.Text == "PN0001" && chiTiet.Items.Count == duLieu.ChiTiet.Count &&
                tong.Text == new PhieuNhapBUS().TinhTongTien(duLieu.ChiTiet).ToString("N2"), "Form sua nap ma cu chi tiet va tong tien");
            form.Close();
        }
        using (var form = new frmThuoc { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show(); Application.DoEvents();
            Check(form.Controls.Find("btnNhapExcelThuoc", true).Length == 1 && form.Controls.Find("btnMauExcelThuoc", true).Length == 1,
                "Form Thuoc co nut nhap Excel va tao mau");
            form.Close();
        }
        using (var form = new frmChonThuoc(t => int.MaxValue - t.SoLuong) { Opacity = 0, ShowInTaskbar = false })
        {
            form.Show(); Application.DoEvents();
            ListView danhSach = form.Controls.OfType<ListView>().Single();
            var thanhNut = form.Controls.OfType<FlowLayoutPanel>().Single();
            Button nutChon = thanhNut.Controls.OfType<Button>().Single(n => n.Text == "Chọn");
            var soLuong = thanhNut.Controls.OfType<NumericUpDown>().Single();
            var timKiem = form.Controls.OfType<TableLayoutPanel>().Single().Controls.OfType<TextBox>().Single();
            timKiem.Text = "KhongCoThuoc_" + Guid.NewGuid().ToString("N");
            Application.DoEvents();
            Check(danhSach.Items.Count == 0 && soLuong.Minimum == 0 && soLuong.Value == 0 && !nutChon.Enabled,
                "Picker tim khong co ket qua khoa nut chon va ha so luong ve 0");
            timKiem.Clear();
            Application.DoEvents();
            Check(danhSach.Items.Count > 0 && nutChon.Enabled && soLuong.Minimum == 1 && soLuong.Value == 1,
                "Picker xoa tu khoa nap lai danh sach va gioi han hop le");
            danhSach.SelectedItems.Clear();
            danhSach.Items.Cast<ListViewItem>().Single(d => d.Text == "TH0004").Selected = true;
            Application.DoEvents();
            Check(nutChon.Enabled, "Nhap hang chon duoc thuoc ton 0");
            nutChon.PerformClick(); Application.DoEvents();
            Check(form.ThuocDuocChon?.MaThuoc == "TH0004" && form.SoLuongDuocChon == 1, "Picker tra thuoc va so luong nhap");
            form.Close();
        }
    }
}

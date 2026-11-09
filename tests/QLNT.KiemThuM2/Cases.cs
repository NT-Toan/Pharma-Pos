using System.Collections;
using System.Data;
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using QLNT.BUS;

internal static partial class Program
{
    private static string AnhDuLieu()
    {
        var ketQua = new System.Text.StringBuilder();
        foreach (string truyVan in new[]
        {
            "SELECT * FROM PHIEUNHAP ORDER BY MAPN", "SELECT * FROM CTPN ORDER BY MAPN, MATHUOC",
            "SELECT * FROM THUOC ORDER BY MATHUOC"
        })
            foreach (DataRow dong in new MyConnection().LayBang(truyVan).Rows)
                ketQua.AppendLine(string.Join("|", dong.ItemArray.Select(v => Convert.ToString(v, CultureInfo.InvariantCulture))));
        return ketQua.ToString();
    }

    private static void DaoPhaiLoi(Action thaoTac, string ten)
    {
        string truoc = AnhDuLieu();
        bool loi = false;
        try { thaoTac(); }
        catch (Exception ex) when (ex is InvalidOperationException or SqlException) { loi = true; }
        Check(loi && AnhDuLieu() == truoc, ten);
    }

    private static void BusPhaiLoi(Func<KetQua> thaoTac, string ten)
    {
        string truoc = AnhDuLieu();
        KetQua kq = thaoTac();
        Check(!kq.ThanhCong && AnhDuLieu() == truoc, ten);
    }

    private static void KiemTraSuaXoa()
    {
        var bus = new PhieuNhapBUS();
        var dao = new PhieuNhapDAO();
        var thongKe = new ThongKeBUS();
        decimal tongChiCu = thongKe.TongChi(DateTime.Today, DateTime.Today);
        var phieu = Phieu();
        Check(bus.Them(phieu, new ArrayList { Dong("TH0001", 10, 3), Dong("TH0002", 5, 4) }).ThanhCong, "Tao phieu de thu sua xoa");
        Check(thongKe.TongChi(DateTime.Today, DateTime.Today) == tongChiCu + 50,
            "BUS thong ke M4 doc dung tong chi sau them phieu");
        DuLieuPhieuNhap banGoc = bus.LayDeSua(phieu.MaPN);
        int ton1 = Ton("TH0001"), ton2 = Ton("TH0002");
        var moi = new ArrayList { Dong("TH0001", 14, 2), Dong("TH0004", 3, 5) };
        Check(bus.Sua(phieu, moi, banGoc).ThanhCong && phieu.TongTien == 43, "Sua tinh lai tong tien");
        Check(thongKe.TongChi(DateTime.Today, DateTime.Today) == tongChiCu + 43,
            "BUS thong ke M4 doc dung tong chi sau sua phieu");
        Check(Ton("TH0001") == ton1 + 4 && Ton("TH0002") == ton2 - 5 && Ton("TH0004") == 3,
            "Sua tang luong doi thuoc va bo dong dieu chinh ton theo chenh lech");
        Check(bus.LayChiTiet(phieu.MaPN).Count == 2 && bus.LayDeSua(phieu.MaPN).Phieu.TongTien == 43 &&
            new ThuocBUS().LayTheoMa("TH0001")!.SoLuong == ton1 + 4, "Sua luu chi tiet tong tien va lam moi cache");
        BusPhaiLoi(() => bus.Sua(phieu, moi, banGoc), "Chan sua ban cu khi nguoi khac da doi phieu");
        BusPhaiLoi(() => bus.Xoa(phieu.MaPN, banGoc), "Chan xoa bang ban chup cu");

        new MyConnection().ThucThi("UPDATE THUOC SET SOLUONG = 1 WHERE MATHUOC IN ('TH0001', 'TH0004')");
        banGoc = bus.LayDeSua(phieu.MaPN);
        Check(bus.Sua(phieu, new ArrayList { Dong("TH0001", 14, 3), Dong("TH0004", 3, 5) }, banGoc).ThanhCong &&
            Ton("TH0001") == 1 && Ton("TH0004") == 1 && phieu.TongTien == 57,
            "Sua gia khi da ban bot thuoc khong tru roi cong lai toan bo so cu");
        banGoc = bus.LayDeSua(phieu.MaPN);
        BusPhaiLoi(() => bus.Sua(phieu, new ArrayList { Dong("TH0001", 15, 3), Dong("TH0004", 1, 5) }, banGoc),
            "Sua lenh thu hai 0 dong do ton am rollback ca lenh tang thuoc dau");
        BusPhaiLoi(() => bus.Sua(phieu, new ArrayList { Dong("TH0001", 14, 3) }, banGoc), "Chan bo thuoc khi khong du ton hoan");
        Check(bus.Sua(phieu, new ArrayList { Dong("TH0001", 15, 3), Dong("TH0004", 3, 5) }, banGoc).ThanhCong && Ton("TH0001") == 2,
            "Sua tang them van duoc khi ton hien tai nho hon so luong phieu cu");

        new MyConnection().ThucThi("UPDATE THUOC SET SOLUONG = 20 WHERE MATHUOC = 'TH0001'");
        banGoc = bus.LayDeSua(phieu.MaPN);
        BusPhaiLoi(() => bus.Xoa(phieu.MaPN, banGoc), "Xoa thieu ton thuoc thu hai rollback ca hoan thuoc dau");
        new MyConnection().ThucThi("UPDATE THUOC SET SOLUONG = 3 WHERE MATHUOC = 'TH0004'");
        Check(bus.Xoa(phieu.MaPN, banGoc).ThanhCong && Ton("TH0001") == 5 && Ton("TH0004") == 0 &&
            bus.LayChiTiet(phieu.MaPN).Count == 0 && !bus.LayTatCa().Cast<PhieuNhap>().Any(p => p.MaPN == phieu.MaPN),
            "Xoa du ton xoa dau phieu chi tiet hoan kho va lam moi cache");
        Check(thongKe.TongChi(DateTime.Today, DateTime.Today) == tongChiCu,
            "BUS thong ke M4 khong tinh lai phieu da xoa");
        BusPhaiLoi(() => bus.Xoa(phieu.MaPN), "Xoa phieu khong con ton tai khong ghi gi");
        BusPhaiLoi(() => bus.Sua(phieu, moi), "Sua phieu khong con ton tai khong ghi gi");

        DuLieuPhieuNhap goc = bus.LayDeSua("PN0001");
        var saiFK = new PhieuNhap { MaPN = "PN0001", MaNV = "NV9999", MaNCC = "NCC0001", NgayNhap = DateTime.Today, TongTien = 10 };
        DaoPhaiLoi(() => dao.Sua(saiFK, new ArrayList { Dong("TH0001", 4, 2.5m), Dong("TH0002", 2, 0) }, goc),
            "Sua loi FK cuoi giao dich rollback kho va thay chi tiet truoc do");
        DaoPhaiLoi(() => dao.Sua(goc.Phieu, new ArrayList { Dong("TH0001", 4, 1), Dong("TH9999", 1, 1) }, goc),
            "Sua thuoc mat lenh 0 dong rollback toan bo");
        DaoPhaiLoi(() => dao.Sua(goc.Phieu, new ArrayList { Dong("TH0001", 4, 1), Dong("TH0003", 1, 1) }, goc),
            "Sua tran int ton kho rollback toan bo");
        BusPhaiLoi(() => bus.Sua(goc.Phieu, new ArrayList(), goc), "Sua khong cho phieu rong");
        BusPhaiLoi(() => bus.Sua(goc.Phieu, new ArrayList { Dong("TH0001", 0, 1) }, goc), "Sua chan so luong 0");

        int truoc = Ton("TH0001");
        var phieuA = new PhieuNhap { MaPN = "PN0001", MaNV = "NV0001", MaNCC = "NCC0001", NgayNhap = goc.Phieu.NgayNhap };
        var phieuB = new PhieuNhap { MaPN = "PN0001", MaNV = "NV0001", MaNCC = "NCC0001", NgayNhap = goc.Phieu.NgayNhap };
        Task<KetQua> a = Task.Run(() => new PhieuNhapBUS().Sua(phieuA, new ArrayList { Dong("TH0001", 4, 2.5m), Dong("TH0002", 2, 0) }, goc));
        Task<KetQua> b = Task.Run(() => new PhieuNhapBUS().Sua(phieuB, new ArrayList { Dong("TH0001", 5, 2.5m), Dong("TH0002", 2, 0) }, goc));
        Task.WaitAll(a, b);
        Check(a.Result.ThanhCong != b.Result.ThanhCong && Ton("TH0001") == truoc + (a.Result.ThanhCong ? 1 : 2),
            "Hai nguoi sua cung ban chup chi mot nguoi thanh cong ton khong bi cong hai lan");

        new MyConnection().ThucThi("INSERT INTO THUOC VALUES ('TH0005', N'Thuoc tranh chap', NULL, 100, 0, 'LT0001')");
        ThuocBUS.TaiLai();
        var tranhChap = Phieu();
        Check(bus.Them(tranhChap, new ArrayList { Dong("TH0005", 10, 1) }).ThanhCong, "Tao du lieu tranh chap nhap ban");
        Task<KetQua> xoa = Task.Run(() => new PhieuNhapBUS().Xoa(tranhChap.MaPN));
        Task<bool> ban = Task.Run(() =>
        {
            try
            {
                return new MyConnection().ChayGiaoDich((ketNoi, giaoDich) =>
                {
                    using var lenh = new SqlCommand("UPDATE THUOC WITH (UPDLOCK, ROWLOCK) SET SOLUONG = SOLUONG - 7 " +
                        "WHERE MATHUOC = 'TH0005' AND SOLUONG >= 7", ketNoi, giaoDich);
                    if (lenh.ExecuteNonQuery() != 1) throw new InvalidOperationException("Khong du ton ban");
                    return true;
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException or SqlException) { return false; }
        });
        Task.WaitAll(xoa, ban);
        Check(xoa.Result.ThanhCong != ban.Result && Ton("TH0005") == (xoa.Result.ThanhCong ? 0 : 3),
            "Xoa phieu dong thoi lenh tru kho ban hang khong tao ton am");
    }

    private static string VietExcel(string ten, object?[][] cacDong, string[]? tieuDe = null)
    {
        string path = Path.Combine(thuMucExcel, ten + ".xlsx");
        using var book = new XLWorkbook();
        var sheet = book.Worksheets.Add("THUOC");
        tieuDe ??= ["TENTHUOC", "MALOAI", "HSD", "DONGIA"];
        for (int i = 0; i < tieuDe.Length; i++) sheet.Cell(1, i + 1).Value = tieuDe[i];
        for (int hang = 0; hang < cacDong.Length; hang++)
            for (int cot = 0; cot < cacDong[hang].Length; cot++)
            {
                var cell = sheet.Cell(hang + 2, cot + 1);
                object? value = cacDong[hang][cot];
                if (value is DateTime date) { cell.Value = date; cell.Style.DateFormat.Format = "dd/MM/yyyy"; }
                else if (value is decimal gia) cell.Value = (double)gia;
                else if (value is int so) cell.Value = so;
                else if (value != null) cell.Value = value.ToString()!;
            }
        book.SaveAs(path);
        return path;
    }

    private static void KiemTraExcel()
    {
        var bus = new ThuocBUS();
        string mau = Path.Combine(thuMucExcel, "MauNhapThuoc.xlsx");
        Check(bus.TaoMauExcel(mau).ThanhCong && File.Exists(mau), "Tao mau xlsx that");
        using (var book = new XLWorkbook(mau))
            Check(book.Worksheet("THUOC").Cell(1, 4).GetString() == "DONGIA" &&
                book.Worksheet("LOAITHUOC").Cell(2, 1).GetString() == "LT0001", "Mau co tieu de va ma loai hien tai");
        BusPhaiLoi(() => bus.NhapTuExcel(mau), "Mau trong khong them thuoc");
        string hopLe = VietExcel("HopLe", [
            ["Thuốc Excel có dấu", "LT0001", new DateTime(2031, 12, 31), 1234.50m],
            [null, null, null, null], ["Thuoc khong HSD", "LT0001", null, "25.75"],
            ["Thuoc ngay chu", "LT0001", "30/11/2032", 10], ["Thuoc ngay ISO", "LT0001", "2033-01-20", 20]
        ]);
        int soCu = Dem("THUOC");
        Check(bus.NhapTuExcel(hopLe).ThanhCong && Dem("THUOC") == soCu + 4, "Nhap nhieu dong bo dong trang ngay Excel ngay chu va gia thap phan");
        ArrayList thuoc = bus.LayTatCa();
        var daNhap = thuoc.Cast<Thuoc>().Where(t => t.TenThuoc.StartsWith("Thuốc Excel") || t.TenThuoc.StartsWith("Thuoc khong") || t.TenThuoc.StartsWith("Thuoc ngay")).ToArray();
        Check(daNhap.Length == 4 && daNhap.All(t => t.SoLuong == 0 && t.MaThuoc.StartsWith("TH") && t.MaThuoc.Length == 6) &&
            daNhap.Single(t => t.TenThuoc == "Thuốc Excel có dấu").DonGia == 1234.50m, "Nhap tu tao ma giu Unicode gia ban va ton 0");
        BusPhaiLoi(() => bus.NhapTuExcel(hopLe), "Nhap lai file bi chan trung thuoc khong luu them");
        string doiCot = VietExcel("DoiCot", [[12.25m, "LT0001", "Thuoc doi cot", "2031-02-01"]], ["DONGIA", "MALOAI", "TENTHUOC", "HSD"]);
        Check(bus.NhapTuExcel(doiCot).ThanhCong, "Nhan cot theo ten thay vi vi tri co dinh");

        (string Ten, object?[] Dong)[] cacLoi =
        [
            ("TenRong", ["", "LT0001", null, 10]),
            ("TenDai", [new string('A', 101), "LT0001", null, 10]),
            ("LoaiKhongCo", ["Thuoc loai sai", "LT9999", null, 10]),
            ("Gia0", ["Thuoc gia 0", "LT0001", null, 0]),
            ("GiaAm", ["Thuoc gia am", "LT0001", null, -1]),
            ("GiaLe", ["Thuoc gia le", "LT0001", null, 1.234m]),
            ("GiaChu", ["Thuoc gia chu", "LT0001", null, "abc"]),
            ("GiaPhanCach", ["Thuoc gia ngan", "LT0001", null, "1,234"]),
            ("NgaySai", ["Thuoc ngay sai", "LT0001", "31/02/2030", 10]),
            ("NgayMoHo", ["Thuoc ngay mo ho", "LT0001", "11/04/30", 10]),
            ("NgayQuaSom", ["Thuoc ngay qua som", "LT0001", "01/01/1600", 10]),
            ("NgayQuaLon", ["Thuoc ngay qua lon", "LT0001", "31/12/9999", 10])
        ];
        foreach (var ca in cacLoi)
        {
            string file = VietExcel(ca.Ten, [["Dong dung chua duoc luu " + ca.Ten, "LT0001", null, 10], ca.Dong]);
            BusPhaiLoi(() => bus.NhapTuExcel(file), "Excel " + ca.Ten + " o dong sau khong luu dong dau");
            Check(bus.NhapTuExcel(file).ThongBao.Contains("Dòng 3"), "Excel bao so dong sai " + ca.Ten);
        }
        string trung = VietExcel("Trung", [["Thuoc trung", "LT0001", null, 10], [" thuoc trung ", "lt0001", null, 20]]);
        BusPhaiLoi(() => bus.NhapTuExcel(trung), "Excel trung ten loai HSD trong file bi tu choi toan bo");
        string thieuCot = VietExcel("ThieuCot", [], ["TENTHUOC", "MALOAI", "DONGIA"]);
        BusPhaiLoi(() => bus.NhapTuExcel(thieuCot), "Excel thieu cot bi tu choi");
        string trungCot = VietExcel("TrungCot", [], ["TENTHUOC", "MALOAI", "HSD", "HSD"]);
        BusPhaiLoi(() => bus.NhapTuExcel(trungCot), "Excel trung tieu de bi tu choi");
        string themCot = VietExcel("ThemCot", [], ["TENTHUOC", "MALOAI", "HSD", "DONGIA", "SOLUONG"]);
        BusPhaiLoi(() => bus.NhapTuExcel(themCot), "Excel cot SOLUONG khong duoc am tham bo qua");
        string congThuc = VietExcel("CongThuc", [["Thuoc cong thuc", "LT0001", null, 10]]);
        using (var book = new XLWorkbook(congThuc)) { book.Worksheet(1).Cell(2, 4).FormulaA1 = "1+1"; book.Save(); }
        BusPhaiLoi(() => bus.NhapTuExcel(congThuc), "Excel cong thuc bi tu choi");
        BusPhaiLoi(() => bus.NhapTuExcel(Path.Combine(thuMucExcel, "KhongCo.xlsx")), "Bao file Excel khong ton tai");
        BusPhaiLoi(() => bus.NhapTuExcel(Path.Combine(thuMucExcel, "Cu.xls")), "Bao ro khong ho tro xls cu");
        string hong = Path.Combine(thuMucExcel, "Hong.xlsx"); File.WriteAllText(hong, "Khong phai Excel");
        BusPhaiLoi(() => bus.NhapTuExcel(hong), "File hong khong ghi du lieu");
        using (var stream = new FileStream(hong, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Check(stream.Length > 0, "File Excel hong van duoc dong ngay sau khi bao loi");

        var dao = new ThuocDAO();
        Thuoc Moi(string ten, string loai = "LT0001") => new() { TenThuoc = ten, MaLoai = loai, DonGia = 1 };
        DaoPhaiLoi(() => dao.ThemHangLoat(new ArrayList { Moi("Lo Excel rollback FK"), Moi("Lo Excel loi", "LT9999") }),
            "Loi FK giua lo Excel rollback ca thuoc dau");
        DaoPhaiLoi(() => dao.ThemHangLoat(new ArrayList { Moi("Lo Excel trung"), Moi("Lo Excel trung") }),
            "Loi trung trong giao dich rollback ca thuoc dau");
        string aFile = VietExcel("SongSongA", [["Nhap dong thoi A", "LT0001", null, 1]]);
        string bFile = VietExcel("SongSongB", [["Nhap dong thoi B", "LT0001", null, 1]]);
        soCu = Dem("THUOC");
        Task<KetQua> a = Task.Run(() => new ThuocBUS().NhapTuExcel(aFile));
        Task<KetQua> b = Task.Run(() => new ThuocBUS().NhapTuExcel(bFile));
        Task.WaitAll(a, b);
        Check(a.Result.ThanhCong && b.Result.ThanhCong && Dem("THUOC") == soCu + 2,
            "Hai file nhap dong thoi duoc cap ma rieng va luu du dong");
        new MyConnection().ThucThi("INSERT INTO THUOC VALUES ('TH9999', N'Thuoc het ma', NULL, 1, 0, 'LT0001')");
        DaoPhaiLoi(() => dao.ThemHangLoat(new ArrayList { Moi("Lo Excel het ma") }), "Het ma TH9999 khong ghi lo nhap");
        new MyConnection().ThucThi("DELETE FROM THUOC WHERE MATHUOC = 'TH9999'");
        ThuocBUS.TaiLai();
    }
}

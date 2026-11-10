using System.Collections;
using System.Reflection;

internal static partial class Program
{
    private static void KiemTraLocThuocNangCao()
    {
        var truongCache = typeof(ThuocBUS).GetField(
            "danhSachThuoc", BindingFlags.Static | BindingFlags.NonPublic)!;
        object? cacheGoc = truongCache.GetValue(null);
        try
        {
            var danhSach = new ArrayList
            {
                new Thuoc { MaThuoc = "TH0001", TenThuoc = "Paracetamol 500mg", MaLoai = "LT0001", DonGia = 5000 },
                new Thuoc { MaThuoc = "TH0002", TenThuoc = "Paracetamol 650mg", MaLoai = "LT0001", DonGia = 20000 },
                new Thuoc { MaThuoc = "TH0003", TenThuoc = "Para Extra", MaLoai = "LT0002", DonGia = 25000 },
                new Thuoc { MaThuoc = "TH0004", TenThuoc = "Ibuprofen", MaLoai = "LT0002", DonGia = 10000 },
                new Thuoc { MaThuoc = "TH0005", TenThuoc = "Vitamin C", MaLoai = "LT0003", DonGia = 30000 },
                new Thuoc { MaThuoc = "TH0006", TenThuoc = "Amoxicillin", MaLoai = "LT0002", DonGia = 1000 },
                new Thuoc { MaThuoc = "TH0007", TenThuoc = "Thuốc bổ gan", MaLoai = "LT0003", DonGia = 12999.50m },
                new Thuoc { MaThuoc = "TH0008", TenThuoc = "Paracetamol giá thấp", MaLoai = "LT0001", DonGia = 4999.99m }
            };
            truongCache.SetValue(null, danhSach);
            var bus = new ThuocBUS();
            string[] tatCa = danhSach.Cast<Thuoc>().Select(t => t.MaThuoc).ToArray();

            (string Ten, Func<ArrayList> Loc, string[] Ma)[] cacCa =
            [
                ("Bo trong tat ca tra toan bo", () => bus.TimKiemNangCao(), tatCa),
                ("Bo qua chuoi rong va khoang trang", () => bus.TimKiemNangCao(" ", "", "\t"), tatCa),
                ("Ma thuoc tim theo chuoi chua", () => bus.TimKiemNangCao(maThuoc: "0004"), ["TH0004"]),
                ("Trim va khong phan biet hoa thuong ma", () => bus.TimKiemNangCao(maThuoc: " th0001 "), ["TH0001"]),
                ("Ten tim chuoi chua khong phan biet hoa thuong", () => bus.TimKiemNangCao(tenThuoc: " PARA "), ["TH0001", "TH0002", "TH0003", "TH0008"]),
                ("Ten tieng Viet co dau", () => bus.TimKiemNangCao(tenThuoc: "THUỐC BỔ"), ["TH0007"]),
                ("Ma loai khop dung va trim", () => bus.TimKiemNangCao(maLoai: " lt0001 "), ["TH0001", "TH0002", "TH0008"]),
                ("Ma loai khong tim theo tien to", () => bus.TimKiemNangCao(maLoai: "LT00"), []),
                ("AND ten va loai", () => bus.TimKiemNangCao(tenThuoc: "Para", maLoai: "LT0002"), ["TH0003"]),
                ("OR ten va loai", () => bus.TimKiemNangCao(tenThuoc: "Para", maLoai: "LT0003", noi: PhepNoi.Or), ["TH0001", "TH0002", "TH0003", "TH0005", "TH0007", "TH0008"]),
                ("AND ma va ten mau thuan", () => bus.TimKiemNangCao(maThuoc: "TH0001", tenThuoc: "Ibu"), []),
                ("OR ma va ten", () => bus.TimKiemNangCao(maThuoc: "TH0001", tenThuoc: "Ibu", noi: PhepNoi.Or), ["TH0001", "TH0004"]),
                ("Khoang gia tinh ca hai dau", () => bus.TimKiemNangCao(giaTu: 5000, giaDen: 20000), ["TH0001", "TH0002", "TH0004", "TH0007"]),
                ("Chi khoang gia voi OR van dung ca hai can", () => bus.TimKiemNangCao(giaTu: 5000, giaDen: 20000, noi: PhepNoi.Or), ["TH0001", "TH0002", "TH0004", "TH0007"]),
                ("Chi co gia tu", () => bus.TimKiemNangCao(giaTu: 10000), ["TH0002", "TH0003", "TH0004", "TH0005", "TH0007"]),
                ("Chi co gia den", () => bus.TimKiemNangCao(giaDen: 10000), ["TH0001", "TH0004", "TH0006", "TH0008"]),
                ("Hai can gia bang nhau", () => bus.TimKiemNangCao(giaTu: 10000, giaDen: 10000), ["TH0004"]),
                ("Gia decimal dung do chinh xac", () => bus.TimKiemNangCao(giaTu: 4999.99m, giaDen: 5000), ["TH0001", "TH0008"]),
                ("Gia tu 0 hop le", () => bus.TimKiemNangCao(giaTu: 0), tatCa),
                ("Gia den 0 khong bi coi la bo trong", () => bus.TimKiemNangCao(giaDen: 0), []),
                ("AND ten Para va khoang gia", () => bus.TimKiemNangCao(tenThuoc: "Para", giaTu: 5000, giaDen: 20000), ["TH0001", "TH0002"]),
                ("OR ten va nhom khoang gia khong lay tat ca", () => bus.TimKiemNangCao(tenThuoc: "Para", giaTu: 5000, giaDen: 20000, noi: PhepNoi.Or), ["TH0001", "TH0002", "TH0003", "TH0004", "TH0007", "TH0008"]),
                ("OR giu thu tu goc khi gop ket qua gia", () => bus.TimKiemNangCao(maThuoc: "TH0005", giaTu: 5000, giaDen: 20000, noi: PhepNoi.Or), ["TH0001", "TH0002", "TH0004", "TH0005", "TH0007"]),
                ("AND nhieu truong va khoang gia", () => bus.TimKiemNangCao("TH000", "Para", "LT0001", 5000, 20000), ["TH0001", "TH0002"]),
                ("OR ket qua trung chi xuat hien mot lan", () => bus.TimKiemNangCao(maThuoc: "TH0001", tenThuoc: "Para", noi: PhepNoi.Or), ["TH0001", "TH0002", "TH0003", "TH0008"]),
                ("OR bo qua truong rong", () => bus.TimKiemNangCao(maThuoc: " ", tenThuoc: "Para", noi: PhepNoi.Or), ["TH0001", "TH0002", "TH0003", "TH0008"]),
                ("Khong tim thay tra danh sach rong", () => bus.TimKiemNangCao(maThuoc: "KHONGCO"), []),
                ("OR loai va nhom gia", () => bus.TimKiemNangCao(maLoai: "LT0002", giaTu: 5000, giaDen: 20000, noi: PhepNoi.Or), ["TH0001", "TH0002", "TH0003", "TH0004", "TH0006", "TH0007"])
            ];
            foreach (var ca in cacCa)
                Check(ca.Loc().Cast<Thuoc>().Select(t => t.MaThuoc).SequenceEqual(ca.Ma), "Loc Thuoc: " + ca.Ten);

            ArrayList banSao = bus.TimKiemNangCao();
            banSao.Clear();
            Check(bus.LayTatCa().Count == tatCa.Length, "Loc Thuoc: thay doi danh sach ket qua khong xoa cache");
            Check(ReferenceEquals(truongCache.GetValue(null), danhSach) &&
                danhSach.Cast<Thuoc>().Select(t => t.MaThuoc).SequenceEqual(tatCa), "Loc Thuoc: khong doi cache hoac thu tu goc");
            LoiDuLieu(() => bus.TimKiemNangCao(giaTu: -1), "Loc Thuoc: chan gia tu am");
            LoiDuLieu(() => bus.TimKiemNangCao(giaDen: -1), "Loc Thuoc: chan gia den am");
            LoiDuLieu(() => bus.TimKiemNangCao(giaTu: 20000, giaDen: 5000), "Loc Thuoc: chan khoang gia dao nguoc");
            LoiDuLieu(() => bus.TimKiemNangCao(noi: (PhepNoi)99), "Loc Thuoc: chan phep noi khong hop le");
            truongCache.SetValue(null, new ArrayList());
            Check(bus.TimKiemNangCao().Count == 0, "Loc Thuoc: cache rong va khong co dieu kien");
            Check(bus.TimKiemNangCao(tenThuoc: "Para", giaTu: 1, giaDen: 20000, noi: PhepNoi.Or).Count == 0,
                "Loc Thuoc: cache rong voi nhieu dieu kien OR");
        }
        finally
        {
            truongCache.SetValue(null, cacheGoc);
        }
    }
}

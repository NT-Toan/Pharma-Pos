using System.Collections;
using System.Reflection;

internal static partial class Program
{
    private static void KiemTraLocHoaDonNangCao()
    {
        var bus = new HoaDonBUS();
        var duLieu = new ArrayList
        {
            new HoaDon { MaHD = "HD0001", MaNV = "NV001", TenNhanVien = "Lan", MaKH = "KH001", TenKhachHang = "Hoa", NgayLap = new DateTime(2026, 1, 10), TongTien = 100m, TienGiam = 10m },
            new HoaDon { MaHD = "HD0002", MaNV = "NV002", TenNhanVien = "Binh", MaKH = "KH002", TenKhachHang = "Minh", NgayLap = new DateTime(2026, 2, 10), TongTien = 250m, TienGiam = 0m },
            new HoaDon { MaHD = "HD0003", MaNV = "NV001", TenNhanVien = "Lan", MaKH = null, TenKhachHang = "Khách lẻ", NgayLap = new DateTime(2026, 3, 10), TongTien = 500m, TienGiam = 50m }
        };
        typeof(HoaDonBUS).GetField("danhSachHoaDon", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(bus, duLieu);

        string[] Ma(BoLocHoaDon loc) => bus.TimKiemNangCao(loc).Cast<HoaDon>().Select(hd => hd.MaHD).ToArray();
        Check(Ma(new BoLocHoaDon { MaHD = "hd0002" }).SequenceEqual(["HD0002"]),
            "Tim hoa don theo ma trong ArrayList");
        Check(Ma(new BoLocHoaDon { NhanVien = "Lan", KhachHang = "Hoa", Noi = PhepNoi.And })
            .SequenceEqual(["HD0001"]), "Loc nhan vien va khach hang AND");
        Check(Ma(new BoLocHoaDon { NhanVien = "Binh", KhachHang = "Hoa", Noi = PhepNoi.Or })
            .SequenceEqual(["HD0001", "HD0002"]), "Loc nhan vien hoac khach hang OR");
        Check(Ma(new BoLocHoaDon { TuNgay = new DateTime(2026, 2, 1), DenNgay = new DateTime(2026, 3, 1), TienTu = 200, TienDen = 300 })
            .SequenceEqual(["HD0002"]), "Loc khoang ngay va tong tien AND");
        Check(Ma(new BoLocHoaDon { MaHD = "HD0001", TienTu = 200, TienDen = 300, Noi = PhepNoi.Or })
            .SequenceEqual(["HD0001", "HD0002"]), "OR giu khoang tien thanh mot nhom AND");
        Check(Ma(new BoLocHoaDon { DieuKienThem = new DieuKien
        {
            TenTruong = nameof(HoaDon.TienGiam), Phep = PhepSoSanh.Lon,
            GiaTri = 0m, Not = true
        } }).SequenceEqual(["HD0002"]), "Loc hoa don co NOT tren tien giam");
        try
        {
            Ma(new BoLocHoaDon { TuNgay = new DateTime(2026, 3, 1), DenNgay = new DateTime(2026, 2, 1) });
            Check(false, "Chan khoang ngay hoa don dao nguoc");
        }
        catch (ArgumentException)
        {
            Check(true, "Chan khoang ngay hoa don dao nguoc");
        }
    }
}

using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QLNT.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly DatThuocBUS bus = new();
    public List<Thuoc> Thuoc { get; private set; } = [];
    public Dictionary<string, decimal> GiamTheoThuoc { get; private set; } =
        new(StringComparer.OrdinalIgnoreCase);
    public string? Loi { get; private set; }
    public string? ThanhCong { get; private set; }

    [BindProperty] public string HoTen { get; set; } = string.Empty;
    [BindProperty] public string SDT { get; set; } = string.Empty;
    [BindProperty] public string DiaChi { get; set; } = string.Empty;
    [BindProperty] public string? GhiChu { get; set; }
    [BindProperty] public string? MaVoucher { get; set; }
    [BindProperty] public string PhuongThucThanhToan { get; set; } = PhuongThucThanhToanCode.TaiCuaHang;
    public bool LaPost => HttpContext.Request.Method == "POST";

    public string SoLuongDaNhap(string maThuoc) =>
        HttpContext.Request.Method == "POST"
            ? HttpContext.Request.Form[$"soluong-{maThuoc}"].ToString()
            : "0";

    public void OnGet(int? don)
    {
        if (don is > 0)
            ThanhCong = $"Đã đặt đơn #{don}. Thuốc đã được giữ trong kho; nhà thuốc sẽ liên hệ với bạn.";
        NapThuoc();
    }

    public IActionResult OnGetVoucher(string? ma)
    {
        var ketQua = bus.XemVoucher(ma);
        return new JsonResult(new { hopLe = ketQua.HopLe, phanTram = ketQua.PhanTram, thongBao = ketQua.ThongBao });
    }

    public IActionResult OnGetGia(string? gio, string? maVoucher)
    {
        var dong = new List<DongDatThuoc>();
        foreach (string muc in (gio ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] phan = muc.Split(':');
            if (phan.Length != 2 || !int.TryParse(phan[1], out int soLuong) || soLuong is < 1 or > 999)
                return new JsonResult(new { hopLe = false, thongBao = "Giỏ hàng không hợp lệ." });
            dong.Add(new DongDatThuoc { MaThuoc = phan[0], SoLuong = soLuong });
        }
        (GiaDonDatThuoc? gia, string thongBao) = bus.TinhGia(dong, maVoucher);
        if (gia == null) return new JsonResult(new { hopLe = false, thongBao });
        return new JsonResult(new
        {
            hopLe = true, gia.TongGoc, gia.GiamSanPham, gia.GiamHoaDon,
            gia.TongThanhToan, gia.PhanTramGiamHoaDon, gia.MaVoucher, gia.MaCTKMHD, gia.MucTienHD,
            cacDong = gia.ThanhTienTheoThuoc.Select(x => new { ma = x.Key, thanhTien = x.Value,
                phanTramGiam = gia.PhanTramGiamTheoThuoc[x.Key] })
        });
    }

    public IActionResult OnPost()
    {
        NapThuoc();
        if (Loi != null)
            return Page();
        var cacDong = new List<DongDatThuoc>();
        foreach (Thuoc thuoc in Thuoc)
        {
            string? soLuongNhap = Request.Form[$"soluong-{thuoc.MaThuoc}"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(soLuongNhap) || soLuongNhap == "0")
                continue;
            if (!int.TryParse(soLuongNhap, out int soLuong) || soLuong < 0)
            {
                Loi = $"Số lượng của {thuoc.TenThuoc} không hợp lệ.";
                return Page();
            }
            if (soLuong > 0)
                cacDong.Add(new DongDatThuoc { MaThuoc = thuoc.MaThuoc, SoLuong = soLuong });
        }

        KetQuaDatThuoc ketQua = bus.Dat(new DonDatThuoc
        {
            HoTen = HoTen, SDT = SDT, DiaChi = DiaChi, GhiChu = GhiChu,
            MaVoucher = MaVoucher, PhuongThucThanhToan = PhuongThucThanhToan
        }, cacDong);
        if (!ketQua.ThanhCong)
        {
            Loi = ketQua.ThongBao;
            return Page();
        }
        return RedirectToPage("/Index", new { don = ketQua.MaDon });
    }

    private void NapThuoc()
    {
        try
        {
            ThuocBUS.TaiLai();
            ArrayList danhSach = bus.TimThuoc(string.Empty);
            Thuoc = danhSach.Cast<Thuoc>().OrderBy(t => t.TenThuoc).ToList();
            KMSanPhamBUS.TaiLai();
            CTKhuyenMaiBUS.TaiLai();
            var khuyenMai = new KMSanPhamBUS();
            GiamTheoThuoc = Thuoc.ToDictionary(t => t.MaThuoc,
                t => khuyenMai.LayPhanTramGiam(t.MaThuoc, DateTime.Today),
                StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            Loi = "Không tải được danh sách thuốc. Kiểm tra SQL Server và thử lại.";
        }
    }
}

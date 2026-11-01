using System.Collections;
using System.Data;

public class ThuocBUS
{
    private static ArrayList? danhSachThuoc;

    private static ArrayList LayDanhSach()
    {
        if (danhSachThuoc != null)
            return danhSachThuoc;

        DataTable bang = new ThuocDAO().LayTatCa();
        var danhSach = new ArrayList();

        foreach (DataRow dong in bang.Rows)
        {
            danhSach.Add(new Thuoc
            {
                MaThuoc = dong["MATHUOC"].ToString() ?? string.Empty,
                TenThuoc = dong["TENTHUOC"].ToString() ?? string.Empty,
                HSD = dong.IsNull("HSD") ? null : Convert.ToDateTime(dong["HSD"]),
                DonGia = Convert.ToDecimal(dong["DONGIA"]),
                SoLuong = Convert.ToInt32(dong["SOLUONG"]),
                MaLoai = dong["MALOAI"].ToString() ?? string.Empty
            });
        }

        danhSachThuoc = danhSach;
        return danhSach;
    }

    public ArrayList LayTatCa() => new(LayDanhSach());

    public static void TaiLai() => danhSachThuoc = null;

    public Thuoc? LayTheoMa(string maThuoc)
    {
        if (string.IsNullOrWhiteSpace(maThuoc))
            return null;

        foreach (Thuoc thuoc in LayDanhSach())
        {
            if (thuoc.MaThuoc.Equals(maThuoc.Trim(), StringComparison.OrdinalIgnoreCase))
                return thuoc;
        }

        return null;
    }

    public string TaoMaMoi()
    {
        var cacMa = new List<string>();
        foreach (Thuoc thuoc in LayDanhSach())
            cacMa.Add(thuoc.MaThuoc);

        return MaTuDong.TaoMaMoi(cacMa, "TH");
    }

    public ArrayList TimKiem(string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
            return LayTatCa();

        string noiDung = tuKhoa.Trim();
        var ketQua = new ArrayList();
        foreach (Thuoc thuoc in LayDanhSach())
        {
            if (thuoc.MaThuoc.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                thuoc.TenThuoc.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                thuoc.MaLoai.Contains(noiDung, StringComparison.OrdinalIgnoreCase))
                ketQua.Add(thuoc);
        }

        return ketQua;
    }

    public KetQua KiemTraBan(string maThuoc, int soLuong, DateTime ngayBan)
    {
        if (soLuong <= 0)
            return KetQua.Loi("Số lượng bán phải lớn hơn 0.");

        Thuoc? thuoc = LayTheoMa(maThuoc);
        if (thuoc == null)
            return KetQua.Loi("Không tìm thấy thuốc.");
        if (thuoc.HSD.HasValue && thuoc.HSD.Value.Date < ngayBan.Date)
            return KetQua.Loi("Thuốc đã hết hạn.");
        if (thuoc.SoLuong < soLuong)
            return KetQua.Loi("Thuốc không đủ tồn kho.");

        return KetQua.Ok();
    }

    public ArrayList LayThuocSapHetHan(int soNgay)
    {
        var ketQua = new ArrayList();
        if (soNgay < 0)
            return ketQua;

        DateTime homNay = DateTime.Today;
        DateTime moc = homNay.AddDays(soNgay);
        foreach (Thuoc thuoc in LayDanhSach())
        {
            if (thuoc.HSD.HasValue && thuoc.HSD.Value.Date >= homNay &&
                thuoc.HSD.Value.Date <= moc)
                ketQua.Add(thuoc);
        }

        return ketQua;
    }

    public ArrayList LayThuocTonThap(int nguong)
    {
        var ketQua = new ArrayList();
        foreach (Thuoc thuoc in LayDanhSach())
        {
            if (thuoc.SoLuong <= nguong)
                ketQua.Add(thuoc);
        }

        return ketQua;
    }

    private static string? KiemTraDuLieu(Thuoc? thuoc)
    {
        if (thuoc == null)
            return "Chưa có thông tin thuốc.";
        if (string.IsNullOrWhiteSpace(thuoc.MaThuoc) || thuoc.MaThuoc.Trim().Length > 10)
            return "Mã thuốc không hợp lệ.";
        if (string.IsNullOrWhiteSpace(thuoc.TenThuoc) || thuoc.TenThuoc.Trim().Length > 100)
            return "Tên thuốc phải có từ 1 đến 100 ký tự.";
        if (thuoc.DonGia <= 0 || thuoc.DonGia > 9999999999999999.99m ||
            decimal.Round(thuoc.DonGia, 2) != thuoc.DonGia)
            return "Đơn giá phải lớn hơn 0 và có tối đa 2 chữ số thập phân.";
        if (string.IsNullOrWhiteSpace(thuoc.MaLoai) ||
            new LoaiThuocBUS().TimTheoMa(thuoc.MaLoai.Trim()) == null)
            return "Hãy chọn loại thuốc đang tồn tại.";

        return null;
    }

    public KetQua Them(Thuoc thuoc)
    {
        try
        {
            string? loi = KiemTraDuLieu(thuoc);
            if (loi != null)
                return KetQua.Loi(loi);
            if (LayTheoMa(thuoc.MaThuoc) != null)
                return KetQua.Loi("Mã thuốc đã tồn tại.");

            var duLieu = new Thuoc
            {
                MaThuoc = thuoc.MaThuoc.Trim(),
                TenThuoc = thuoc.TenThuoc.Trim(),
                HSD = thuoc.HSD?.Date,
                DonGia = thuoc.DonGia,
                SoLuong = 0,
                MaLoai = thuoc.MaLoai.Trim()
            };

            if (new ThuocDAO().Them(duLieu) != 1)
                return KetQua.Loi("Không thêm được thuốc.");

            TaiLai();
            return KetQua.Ok("Đã thêm thuốc. Tồn kho ban đầu là 0.");
        }
        catch (Exception)
        {
            return KetQua.Loi("Không thêm được thuốc. Hãy kiểm tra mã, loại thuốc và kết nối database.");
        }
    }

    public KetQua Sua(Thuoc thuoc)
    {
        try
        {
            string? loi = KiemTraDuLieu(thuoc);
            if (loi != null)
                return KetQua.Loi(loi);

            var duLieu = new Thuoc
            {
                MaThuoc = thuoc.MaThuoc.Trim(),
                TenThuoc = thuoc.TenThuoc.Trim(),
                HSD = thuoc.HSD?.Date,
                DonGia = thuoc.DonGia,
                MaLoai = thuoc.MaLoai.Trim()
            };

            if (new ThuocDAO().Sua(duLieu) == 0)
                return KetQua.Loi("Không tìm thấy thuốc cần sửa.");

            TaiLai();
            return KetQua.Ok("Đã sửa thuốc. Tồn kho không thay đổi.");
        }
        catch (Exception)
        {
            return KetQua.Loi("Không sửa được thuốc. Hãy kiểm tra loại thuốc và kết nối database.");
        }
    }

    public KetQua Xoa(string maThuoc)
    {
        if (string.IsNullOrWhiteSpace(maThuoc))
            return KetQua.Loi("Chưa chọn thuốc cần xóa.");

        try
        {
            if (new ThuocDAO().Xoa(maThuoc.Trim()) == 0)
                return KetQua.Loi("Không tìm thấy thuốc cần xóa.");

            TaiLai();
            return KetQua.Ok("Đã xóa thuốc.");
        }
        catch (Exception)
        {
            return KetQua.Loi("Không xóa được thuốc. Thuốc có thể đang được hóa đơn, phiếu nhập hoặc khuyến mãi sử dụng.");
        }
    }
}

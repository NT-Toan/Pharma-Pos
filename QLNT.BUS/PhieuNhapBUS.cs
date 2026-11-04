using System.Collections;
using System.Data;

public class PhieuNhapBUS
{
    private const decimal TienToiDa = 9999999999999999.99m;
    private static ArrayList? danhSachPhieuNhap;
    private readonly PhieuNhapDAO dao = new();

    public ArrayList LayTatCa()
    {
        if (danhSachPhieuNhap != null)
            return new ArrayList(danhSachPhieuNhap);

        var danhSach = new ArrayList();
        foreach (DataRow dong in dao.LayTatCa().Rows)
        {
            danhSach.Add(new PhieuNhap
            {
                MaPN = dong["MAPN"].ToString() ?? string.Empty,
                MaNCC = dong["MANCC"].ToString() ?? string.Empty,
                TenNCC = dong["TENNCC"].ToString() ?? string.Empty,
                MaNV = dong["MANV"].ToString() ?? string.Empty,
                TenNhanVien = dong["HOTENNV"].ToString() ?? string.Empty,
                NgayNhap = Convert.ToDateTime(dong["NGAYNHAP"]),
                TongTien = Convert.ToDecimal(dong["TONGTIEN"])
            });
        }

        danhSachPhieuNhap = danhSach;
        return new ArrayList(danhSach);
    }

    public static void TaiLai() => danhSachPhieuNhap = null;

    public ArrayList TimKiem(string tuKhoa)
    {
        string noiDung = tuKhoa?.Trim() ?? string.Empty;
        var ketQua = new ArrayList();
        foreach (PhieuNhap phieu in LayTatCa())
        {
            if (noiDung.Length == 0 ||
                phieu.MaPN.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                phieu.MaNCC.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                phieu.TenNCC.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                phieu.MaNV.Contains(noiDung, StringComparison.OrdinalIgnoreCase) ||
                phieu.TenNhanVien.Contains(noiDung, StringComparison.OrdinalIgnoreCase))
                ketQua.Add(phieu);
        }

        return ketQua;
    }

    public ArrayList LayChiTiet(string maPN)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(maPN);
        var danhSach = new ArrayList();
        foreach (DataRow dong in dao.LayChiTiet(maPN.Trim()).Rows)
        {
            danhSach.Add(new CTPhieuNhap
            {
                MaPN = dong["MAPN"].ToString() ?? string.Empty,
                MaThuoc = dong["MATHUOC"].ToString() ?? string.Empty,
                TenThuoc = dong["TENTHUOC"].ToString() ?? string.Empty,
                SoLuong = Convert.ToInt32(dong["SOLUONG"]),
                DonGia = Convert.ToDecimal(dong["DONGIA"])
            });
        }

        return danhSach;
    }

    // Mã dự kiến để hiển thị; DAO cấp mã chính thức trong giao dịch lúc lưu.
    public string TaoMaMoi()
    {
        var cacMa = new ArrayList();
        foreach (PhieuNhap phieu in LayTatCa())
            cacMa.Add(phieu.MaPN);
        return MaTuDong.TaoMaMoi(cacMa.Cast<string>(), "PN");
    }

    public decimal TinhTongTien(ArrayList chiTiet)
    {
        ArgumentNullException.ThrowIfNull(chiTiet);
        decimal tong = 0;
        var cacMa = new ArrayList();
        foreach (object? phanTu in chiTiet)
        {
            if (phanTu is not CTPhieuNhap dong)
                throw new ArgumentException("Chi tiết phiếu nhập không hợp lệ.");
            string ma = dong.MaThuoc?.Trim() ?? string.Empty;
            if (ma.Length == 0 || ma.Length > 10)
                throw new ArgumentException("Mã thuốc không hợp lệ.");
            foreach (string maDaCo in cacMa)
            {
                if (maDaCo.Equals(ma, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Thuốc {ma} đã có trong phiếu. Hãy cập nhật dòng hiện có.");
            }
            cacMa.Add(ma);

            if (dong.SoLuong <= 0)
                throw new ArgumentException($"Số lượng nhập của thuốc {ma} phải lớn hơn 0.");
            if (dong.DonGia < 0 || dong.DonGia > TienToiDa || decimal.Round(dong.DonGia, 2) != dong.DonGia)
                throw new ArgumentException($"Đơn giá nhập của thuốc {ma} phải không âm và có tối đa 2 chữ số thập phân.");

            decimal thanhTien = dong.ThanhTien;
            if (thanhTien > TienToiDa || tong > TienToiDa - thanhTien)
                throw new ArgumentException("Tổng tiền phiếu nhập vượt giới hạn lưu trữ.");
            tong += thanhTien;
        }

        return tong;
    }

    public KetQua KiemTraDuLieu(PhieuNhap? phieu, ArrayList? chiTiet)
    {
        if (phieu == null)
            return KetQua.Loi("Chưa có thông tin phiếu nhập.");
        if (string.IsNullOrWhiteSpace(phieu.MaNV) || phieu.MaNV.Trim().Length > 10)
            return KetQua.Loi("Hãy chọn nhân viên nhập hàng.");
        if (string.IsNullOrWhiteSpace(phieu.MaNCC) || phieu.MaNCC.Trim().Length > 10)
            return KetQua.Loi("Hãy chọn nhà cung cấp.");
        if (phieu.NgayNhap == default)
            return KetQua.Loi("Ngày nhập không hợp lệ.");
        if (chiTiet == null || chiTiet.Count == 0)
            return KetQua.Loi("Phiếu nhập cần có ít nhất một thuốc.");

        try
        {
            TinhTongTien(chiTiet);
            if (new NhanVienBUS().LayTheoMa(phieu.MaNV) == null)
                return KetQua.Loi("Nhân viên đã chọn không còn tồn tại.");
            if (new NhaCungCapBUS().LayTheoMa(phieu.MaNCC) == null)
                return KetQua.Loi("Nhà cung cấp đã chọn không còn tồn tại.");
            var thuocBUS = new ThuocBUS();
            foreach (CTPhieuNhap dong in chiTiet)
            {
                if (thuocBUS.LayTheoMa(dong.MaThuoc) == null)
                    return KetQua.Loi($"Thuốc {dong.MaThuoc} không còn tồn tại.");
            }

            return KetQua.Ok();
        }
        catch (Exception ex)
        {
            return KetQua.Loi(ex.Message);
        }
    }

    public DuLieuPhieuNhap LayDeSua(string maPN)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(maPN);
        if (maPN.Trim().Length > 10) throw new ArgumentException("Mã phiếu không hợp lệ.");
        DuLieuPhieuNhap duLieu = dao.LayDeSua(maPN.Trim());
        NhanVienBUS.TaiLai();
        NhaCungCapBUS.TaiLai();
        ThuocBUS.TaiLai();
        duLieu.Phieu.TenNhanVien = new NhanVienBUS().LayTheoMa(duLieu.Phieu.MaNV)?.HoTenNV ?? "";
        duLieu.Phieu.TenNCC = new NhaCungCapBUS().LayTheoMa(duLieu.Phieu.MaNCC)?.TenNCC ?? "";
        var thuocBUS = new ThuocBUS();
        foreach (CTPhieuNhap dong in duLieu.ChiTiet)
            dong.TenThuoc = thuocBUS.LayTheoMa(dong.MaThuoc)?.TenThuoc ?? dong.MaThuoc;
        return duLieu;
    }

    public KetQua Them(PhieuNhap phieu, ArrayList chiTiet) => Luu(phieu, chiTiet, false, null);

    public KetQua Sua(PhieuNhap phieu, ArrayList chiTiet, DuLieuPhieuNhap? banGoc = null) =>
        Luu(phieu, chiTiet, true, banGoc);

    public KetQua Xoa(string maPN, DuLieuPhieuNhap? banGoc = null)
    {
        if (string.IsNullOrWhiteSpace(maPN) || maPN.Trim().Length > 10)
            return KetQua.Loi("Hãy chọn phiếu nhập cần xóa.");
        try
        {
            dao.Xoa(maPN.Trim(), banGoc);
            TaiLai();
            ThuocBUS.TaiLai();
            return KetQua.Ok("Đã xóa phiếu nhập, chi tiết và hoàn tồn kho.");
        }
        catch (Exception ex)
        {
            return KetQua.Loi("Không xóa được phiếu nhập: " + ex.Message);
        }
    }

    private KetQua Luu(PhieuNhap phieu, ArrayList chiTiet, bool laSua, DuLieuPhieuNhap? banGoc)
    {
        try
        {
            if (laSua && (phieu == null || string.IsNullOrWhiteSpace(phieu.MaPN) || phieu.MaPN.Trim().Length > 10))
                return KetQua.Loi("Hãy chọn phiếu nhập cần sửa.");
            // Xác minh lại dữ liệu liên quan trước khi ghi; khóa ngoại vẫn bảo vệ trong giao dịch.
            NhanVienBUS.TaiLai();
            NhaCungCapBUS.TaiLai();
            ThuocBUS.TaiLai();
            KetQua kiemTra = KiemTraDuLieu(phieu, chiTiet);
            if (!kiemTra.ThanhCong)
                return kiemTra;

            var chiTietLuu = new ArrayList();
            foreach (CTPhieuNhap dong in chiTiet)
            {
                chiTietLuu.Add(new CTPhieuNhap
                {
                    MaThuoc = dong.MaThuoc.Trim(),
                    TenThuoc = dong.TenThuoc,
                    SoLuong = dong.SoLuong,
                    DonGia = dong.DonGia
                });
            }

            var phieuLuu = new PhieuNhap
            {
                MaPN = phieu.MaPN.Trim(),
                MaNV = phieu.MaNV.Trim(),
                MaNCC = phieu.MaNCC.Trim(),
                NgayNhap = phieu.NgayNhap.Date,
                TongTien = TinhTongTien(chiTietLuu)
            };
            string maPN;
            if (laSua)
            {
                dao.Sua(phieuLuu, chiTietLuu, banGoc);
                maPN = phieuLuu.MaPN;
            }
            else
                maPN = dao.Them(phieuLuu, chiTietLuu);

            // Chỉ cập nhật đối tượng và cache sau khi toàn bộ giao dịch commit thành công.
            phieu.MaPN = maPN;
            phieu.TongTien = phieuLuu.TongTien;
            TaiLai();
            ThuocBUS.TaiLai();
            return KetQua.Ok($"Đã lưu phiếu {maPN} và {(laSua ? "điều chỉnh" : "tăng")} tồn kho.");
        }
        catch (Exception ex)
        {
            return KetQua.Loi("Không lưu được phiếu nhập: " + ex.Message);
        }
    }
}

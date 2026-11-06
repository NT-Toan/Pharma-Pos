using System;
using System.Collections;
using System.Data;

public class CTKhuyenMaiBUS
{
    private static ArrayList? danhSachCTKM;
    private readonly KhuyenMaiDAO dao = new KhuyenMaiDAO();

    private static ArrayList LayDanhSach()
    {
        if (danhSachCTKM != null) return danhSachCTKM;

        KhuyenMaiDAO kmDao = new KhuyenMaiDAO();
        DataTable dt = kmDao.LayTatCaCTKM();
        ArrayList ds = new ArrayList();

        foreach (DataRow row in dt.Rows)
        {
            DateTime? bd = row["NGAYBATDAU"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYBATDAU"]);
            DateTime? kt = row["NGAYKETTHUC"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYKETTHUC"]);
            ds.Add(new CTKhuyenMai(
                row["MACTKM"].ToString() ?? "",
                row["TENCTKM"].ToString() ?? "",
                bd, kt));
            ((CTKhuyenMai)ds[ds.Count - 1]!).TheLoai = row["THELOAI"].ToString() ?? "Chưa cấu hình";
        }

        danhSachCTKM = ds;
        return ds;
    }

    public ArrayList LayTatCa() => new ArrayList(LayDanhSach());

    public static void TaiLai() => danhSachCTKM = null;

    public string TaoMaMoi()
    {
        ArrayList ds = LayDanhSach();
        List<string> dsMa = new List<string>();
        foreach (CTKhuyenMai km in ds)
        {
            dsMa.Add(km.MaCTKM);
        }
        return MaTuDong.TaoMaMoi(dsMa, "KM", 4);
    }

    public CTKhuyenMai? LayTheoMa(string maCTKM)
    {
        foreach (CTKhuyenMai km in LayDanhSach())
        {
            if (km.MaCTKM.Equals(maCTKM, StringComparison.OrdinalIgnoreCase))
                return km;
        }
        return null;
    }

    public bool KiemTraHieuLuc(string maCTKM, DateTime ngay)
    {
        CTKhuyenMai? km = LayTheoMa(maCTKM);
        if (km == null) return false;
        DateTime ngayXet = ngay.Date;
        if (km.NgayBatDau.HasValue && ngayXet < km.NgayBatDau.Value.Date) return false;
        if (km.NgayKetThuc.HasValue && ngayXet > km.NgayKetThuc.Value.Date) return false;
        return true;
    }

    public KetQua Them(CTKhuyenMai km)
    {
        if (string.IsNullOrWhiteSpace(km.TenCTKM))
            return KetQua.Loi("Tên chương trình khuyến mãi không được để trống.");

        if (km.NgayBatDau.HasValue && km.NgayKetThuc.HasValue && km.NgayKetThuc.Value.Date < km.NgayBatDau.Value.Date)
            return KetQua.Loi("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

        if (string.IsNullOrWhiteSpace(km.MaCTKM))
            km.MaCTKM = TaoMaMoi();

        if (LayTheoMa(km.MaCTKM) != null)
            return KetQua.Loi($"Mã khuyến mãi '{km.MaCTKM}' đã tồn tại.");

        int rows = dao.ThemCTKM(km);
        if (rows > 0)
        {
            TaiLai();
            return KetQua.Ok("Thêm chương trình khuyến mãi thành công.");
        }
        return KetQua.Loi("Lỗi khi thêm vào cơ sở dữ liệu.");
    }

    public KetQua Sua(CTKhuyenMai km)
    {
        if (string.IsNullOrWhiteSpace(km.TenCTKM))
            return KetQua.Loi("Tên chương trình khuyến mãi không được để trống.");

        if (km.NgayBatDau.HasValue && km.NgayKetThuc.HasValue && km.NgayKetThuc.Value.Date < km.NgayBatDau.Value.Date)
            return KetQua.Loi("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

        if (LayTheoMa(km.MaCTKM) == null)
            return KetQua.Loi("Chương trình khuyến mãi không tồn tại để sửa.");

        int rows = dao.SuaCTKM(km);
        if (rows > 0)
        {
            TaiLai();
            return KetQua.Ok("Cập nhật chương trình khuyến mãi thành công.");
        }
        return KetQua.Loi("Lỗi khi cập nhật cơ sở dữ liệu.");
    }

    public KetQua Xoa(string maCTKM)
    {
        if (LayTheoMa(maCTKM) == null)
            return KetQua.Loi("Chương trình khuyến mãi không tồn tại.");

        int rows = dao.XoaCTKM(maCTKM);
        if (rows > 0)
        {
            TaiLai();
            return KetQua.Ok("Xóa chương trình khuyến mãi thành công.");
        }
        return KetQua.Loi("Lỗi khi xóa chương trình khuyến mãi.");
    }
    public ArrayList TimKiem(string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
            return LayTatCa();

        string tuKhoaChuan = tuKhoa.Trim().ToLower();
        var ketQua = new ArrayList();

        foreach (CTKhuyenMai km in LayTatCa())
        {
            if (km.MaCTKM.ToLower().Contains(tuKhoaChuan) ||
                km.TenCTKM.ToLower().Contains(tuKhoaChuan) ||
                km.TheLoai.Contains(tuKhoaChuan, StringComparison.OrdinalIgnoreCase))
            {
                ketQua.Add(km);
            }
        }

        return ketQua;
    }
}
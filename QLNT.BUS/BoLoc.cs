using System.Collections;
using System.Globalization;
using System.Reflection;

public static class BoLoc
{
    public static ArrayList Loc(
        ArrayList danhSach,
        List<DieuKien> dieuKien,
        PhepNoi noi)
    {
        ArgumentNullException.ThrowIfNull(danhSach);
        ArgumentNullException.ThrowIfNull(dieuKien);

        if (dieuKien.Count == 0)
            return new ArrayList(danhSach);

        var ketQua = new ArrayList();

        foreach (object? doiTuong in danhSach)
        {
            if (doiTuong == null)
                continue;

            bool phuHop = noi == PhepNoi.And;

            foreach (DieuKien dk in dieuKien)
            {
                bool dung = KiemTra(doiTuong, dk);

                if (noi == PhepNoi.And && !dung)
                {
                    phuHop = false;
                    break;
                }

                if (noi == PhepNoi.Or && dung)
                {
                    phuHop = true;
                    break;
                }
            }

            if (phuHop)
                ketQua.Add(doiTuong);
        }

        return ketQua;
    }

    private static bool KiemTra(object doiTuong, DieuKien dk)
    {
        if (string.IsNullOrWhiteSpace(dk.TenTruong))
            throw new ArgumentException("Tên trường lọc không được để trống.");

        PropertyInfo? thuocTinh = doiTuong.GetType().GetProperty(
            dk.TenTruong,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (thuocTinh == null)
            throw new ArgumentException(
                $"Không tìm thấy trường '{dk.TenTruong}' trong {doiTuong.GetType().Name}.");

        object? giaTriThuc = thuocTinh.GetValue(doiTuong);
        object? giaTriCanTim = dk.GiaTri;

        bool dung;

        if (dk.Phep == PhepSoSanh.Chua)
        {
            string? chuoi = Convert.ToString(giaTriThuc, CultureInfo.CurrentCulture);
            string? canTim = Convert.ToString(giaTriCanTim, CultureInfo.CurrentCulture);

            dung = chuoi != null &&
                   canTim != null &&
                   chuoi.Contains(canTim, StringComparison.OrdinalIgnoreCase);
        }
        else if (giaTriThuc == null || giaTriCanTim == null)
        {
            dung = dk.Phep switch
            {
                PhepSoSanh.Bang => giaTriThuc == null && giaTriCanTim == null,
                PhepSoSanh.Khac => giaTriThuc != giaTriCanTim,
                _ => false
            };
        }
        else
        {
            int soSanh = SoSanh(giaTriThuc, giaTriCanTim);

            dung = dk.Phep switch
            {
                PhepSoSanh.Bang => soSanh == 0,
                PhepSoSanh.Khac => soSanh != 0,
                PhepSoSanh.Lon => soSanh > 0,
                PhepSoSanh.LonHoacBang => soSanh >= 0,
                PhepSoSanh.Nho => soSanh < 0,
                PhepSoSanh.NhoHoacBang => soSanh <= 0,
                _ => false
            };
        }

        return dk.Not ? !dung : dung;
    }

    private static int SoSanh(object giaTriThuc, object giaTriCanTim)
    {
        if (giaTriThuc is string chuoi)
        {
            string canTim = Convert.ToString(
                giaTriCanTim, CultureInfo.CurrentCulture) ?? string.Empty;

            return string.Compare(
                chuoi, canTim, StringComparison.OrdinalIgnoreCase);
        }

        Type kieu = giaTriThuc.GetType();
        object giaTriDaDoi = Convert.ChangeType(
            giaTriCanTim, kieu, CultureInfo.CurrentCulture);

        if (giaTriThuc is not IComparable coTheSoSanh)
            throw new ArgumentException(
                $"Kiểu dữ liệu '{kieu.Name}' không thể so sánh.");

        return coTheSoSanh.CompareTo(giaTriDaDoi);
    }
}
public static class MaTuDong
{
    public static string TaoMaMoi(
        IEnumerable<string> dsMaHienCo,
        string tienTo,
        int doRong = 4)
    {
        int soLonNhat = 0;

        foreach (string ma in dsMaHienCo)
        {
            if (ma == null ||
                !ma.StartsWith(tienTo, StringComparison.OrdinalIgnoreCase))
                continue;

            string phanSo = ma.Substring(tienTo.Length);

            if (phanSo.Length == doRong &&
                int.TryParse(phanSo, out int so) &&
                so > soLonNhat)
            {
                soLonNhat = so;
            }
        }

        string soMoi = (soLonNhat + 1).ToString().PadLeft(doRong, '0');

        if (soMoi.Length > doRong)
            throw new InvalidOperationException("Đã hết mã cho tiền tố " + tienTo);

        return tienTo + soMoi;
    }
}
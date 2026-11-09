using System.Collections;
using System.Globalization;
using ClosedXML.Excel;

// Chỉ chuyển đổi file; kiểm tra nghiệp vụ và ghi database do ThuocBUS/ThuocDAO thực hiện.
internal static class ThuocExcel
{
    private static readonly string[] CacCot = ["TENTHUOC", "MALOAI", "HSD", "DONGIA"];

    internal static ArrayList Doc(string duongDan)
    {
        KiemTraDuongDan(duongDan);
        if (!File.Exists(duongDan)) throw new ArgumentException("Không tìm thấy file Excel.");
        if (new FileInfo(duongDan).Length > 20 * 1024 * 1024)
            throw new ArgumentException("File nhập thuốc không được vượt quá 20 MB.");
        // Tự sở hữu stream để vẫn đóng file nếu constructor gặp file .xlsx hỏng.
        using var stream = File.OpenRead(duongDan);
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.FirstOrDefault(s => s.Name.Equals("THUOC", StringComparison.OrdinalIgnoreCase))
                    ?? workbook.Worksheets.FirstOrDefault()
                    ?? throw new ArgumentException("File Excel không có trang dữ liệu.");
        int cotCuoi = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        if (cotCuoi != CacCot.Length)
            throw new ArgumentException("Dòng 1 phải có đúng 4 cột: TENTHUOC, MALOAI, HSD, DONGIA. Hãy dùng file mẫu.");
        var viTri = new int[CacCot.Length];
        for (int cot = 1; cot <= cotCuoi; cot++)
        {
            var cell = sheet.Cell(1, cot);
            string ten = cell.GetString().Trim().ToUpperInvariant();
            int index = Array.IndexOf(CacCot, ten);
            if (cell.HasFormula || index < 0 || viTri[index] != 0)
                throw new ArgumentException($"Tiêu đề cột {cot} không hợp lệ hoặc bị trùng. Hãy dùng file mẫu.");
            viTri[index] = cot;
        }
        if (viTri.Any(c => c == 0)) throw new ArgumentException("File thiếu cột bắt buộc.");
        int dongCuoi = sheet.LastRowUsed()?.RowNumber() ?? 1;
        if (dongCuoi > 10001) throw new ArgumentException("Chỉ nhập tối đa 10.000 dòng mỗi file.");
        var ketQua = new ArrayList();
        for (int hang = 2; hang <= dongCuoi; hang++)
        {
            IXLCell[] cells = viTri.Select(c => sheet.Cell(hang, c)).ToArray();
            if (cells.All(c => c.IsEmpty())) continue;
            try
            {
                if (cells.Any(c => c.HasFormula || c.DataType == XLDataType.Error))
                    throw new ArgumentException("Hãy nhập giá trị trực tiếp, không dùng công thức hoặc ô lỗi.");
                string ten = cells[0].GetString().Trim();
                string loai = cells[1].GetString().Trim();
                DateTime? hsd = null;
                if (!cells[2].IsEmpty())
                {
                    if (cells[2].DataType == XLDataType.DateTime)
                        hsd = cells[2].GetDateTime().Date;
                    else if (DateTime.TryParseExact(cells[2].GetString().Trim(), ["dd/MM/yyyy", "yyyy-MM-dd"],
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngay))
                        hsd = ngay.Date;
                    else throw new ArgumentException("HSD phải là ngày Excel, dd/MM/yyyy, yyyy-MM-dd hoặc để trống.");
                    if (hsd.Value.Year < 1753 || hsd.Value.Year > 9998)
                        throw new ArgumentException("HSD nằm ngoài khoảng ngày chương trình hỗ trợ (01/01/1753–31/12/9998).");
                }
                decimal gia;
                if (cells[3].DataType == XLDataType.Number)
                    gia = checked((decimal)cells[3].GetDouble());
                else if (!decimal.TryParse(cells[3].GetString().Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out gia))
                    throw new ArgumentException("DONGIA phải là số; nếu nhập dạng chữ, dùng dấu chấm thập phân và không có dấu phân cách hàng nghìn.");
                ketQua.Add((hang, new Thuoc { TenThuoc = ten, MaLoai = loai, HSD = hsd, DonGia = gia }));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidCastException or FormatException or OverflowException)
            {
                throw new ArgumentException($"Dòng {hang}: {ex.Message}", ex);
            }
        }
        if (ketQua.Count == 0) throw new ArgumentException("File không có dòng thuốc để nhập.");
        return ketQua;
    }

    internal static void TaoMau(string duongDan, ArrayList cacLoai)
    {
        KiemTraDuongDan(duongDan);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("THUOC");
        for (int i = 0; i < CacCot.Length; i++) sheet.Cell(1, i + 1).Value = CacCot[i];
        sheet.Range("A1:D1").Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(1);
        sheet.Column(1).Width = 35;
        sheet.Column(2).Width = 18;
        sheet.Column(3).Width = 18;
        sheet.Column(4).Width = 20;
        sheet.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";
        sheet.Column(4).Style.NumberFormat.Format = "0.00";

        var loaiSheet = workbook.Worksheets.Add("LOAITHUOC");
        loaiSheet.Cell(1, 1).Value = "MALOAI";
        loaiSheet.Cell(1, 2).Value = "TENLOAI";
        int hang = 2;
        foreach (LoaiThuoc loai in cacLoai)
        {
            loaiSheet.Cell(hang, 1).Value = loai.MaLoai;
            loaiSheet.Cell(hang++, 2).Value = loai.TenLoai;
        }
        loaiSheet.Columns(1, 2).Width = 30;
        var huongDan = workbook.Worksheets.Add("HUONGDAN");
        string[] noiDung =
        [
            "Điền thuốc ở trang THUOC, từ dòng 2. Giữ nguyên 4 tiêu đề.",
            "TENTHUOC: 1–100 ký tự. MALOAI: mã có trong trang LOAITHUOC.",
            "HSD: ngày Excel, dd/MM/yyyy, yyyy-MM-dd hoặc để trống.",
            "DONGIA: giá bán > 0, tối đa 2 chữ số thập phân. Nhập ô số Excel.",
            "Nếu DONGIA là chữ: dùng dấu chấm thập phân, không phân cách hàng nghìn.",
            "Mã thuốc được tự tạo. Tồn ban đầu là 0; tăng kho bằng phiếu nhập.",
            "Không dùng công thức. Không nhập trùng tên + loại + hạn dùng.",
            "Chỉ cần một dòng sai thì toàn bộ file không được lưu. Hỗ trợ .xlsx."
        ];
        for (int i = 0; i < noiDung.Length; i++) huongDan.Cell(i + 1, 1).Value = noiDung[i];
        huongDan.Column(1).Width = 110;
        workbook.SaveAs(duongDan);
    }

    private static void KiemTraDuongDan(string duongDan)
    {
        if (string.IsNullOrWhiteSpace(duongDan) || !Path.GetExtension(duongDan).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Chỉ hỗ trợ file Excel .xlsx. Hãy lưu file .xls thành .xlsx trước khi nhập.");
    }
}

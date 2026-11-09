using System.Data;
using ClosedXML.Excel;

namespace QLNT.GUI;

public static class ExcelHelper
{
    public static void XuatBang(DataTable bang, string duongDan)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thống kê");

        // Đổ dữ liệu DataTable vào bảng Excel
        var table = worksheet.Cell(1, 1).InsertTable(bang);
        
        // Thiết lập giao diện màu xanh trang nhã
        table.Theme = XLTableTheme.TableStyleMedium9;

        // Tự động căn chỉnh độ rộng cột theo nội dung chữ
        worksheet.Columns().AdjustToContents();

        // Lưu trực tiếp thành file .xlsx
        workbook.SaveAs(duongDan);
    }
}
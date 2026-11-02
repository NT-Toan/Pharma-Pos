using System.Data;
using System.IO;
using System.Text;

namespace QLNT.GUI;

public static class ExcelHelper
{
    public static void XuatBang(DataTable bang, string duongDan)
    {
        var sb = new StringBuilder();

        // Ghi tiêu đề cột
        for (int i = 0; i < bang.Columns.Count; i++)
        {
            sb.Append(ChuanHoaGiaTri(bang.Columns[i].ColumnName));
            if (i < bang.Columns.Count - 1) sb.Append(",");
        }
        sb.AppendLine();

        // Ghi các dòng dữ liệu
        foreach (DataRow row in bang.Rows)
        {
            for (int i = 0; i < bang.Columns.Count; i++)
            {
                sb.Append(ChuanHoaGiaTri(row[i]?.ToString() ?? ""));
                if (i < bang.Columns.Count - 1) sb.Append(",");
            }
            sb.AppendLine();
        }

        // Ghi file với mã hóa UTF-8 kèm BOM để Excel hiển thị tiếng Việt có dấu chuẩn
        File.WriteAllText(duongDan, sb.ToString(), Encoding.UTF8);
    }

    private static string ChuanHoaGiaTri(string giaTri)
    {
        if (giaTri.Contains(",") || giaTri.Contains("\"") || giaTri.Contains("\n"))
        {
            giaTri = giaTri.Replace("\"", "\"\"");
            return $"\"{giaTri}\"";
        }
        return giaTri;
    }
}
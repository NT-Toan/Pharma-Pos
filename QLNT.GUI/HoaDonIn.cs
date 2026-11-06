using System.Drawing.Printing;

namespace QLNT.GUI;

public static class HoaDonIn
{
    private const string MayInPdf = "Microsoft Print to PDF";

    public static void In(Form chu, HoaDon hoaDon, IReadOnlyList<CTHoaDon> chiTiet)
    {
        using var taiLieu = TaoTaiLieu(hoaDon, chiTiet);
        using var hopThoai = new PrintDialog { Document = taiLieu, UseEXDialog = true };
        try
        {
            if (hopThoai.ShowDialog(chu) == DialogResult.OK)
                taiLieu.Print();
        }
        catch (Exception ex)
        {
            MessageBox.Show(chu, $"Không thể in hóa đơn: {ex.Message}",
                "Lỗi in hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void XuatPdf(Form chu, HoaDon hoaDon, IReadOnlyList<CTHoaDon> chiTiet)
    {
        using var hopThoai = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "pdf",
            FileName = $"HoaDon-{hoaDon.MaHD}.pdf",
            Filter = "Tệp PDF (*.pdf)|*.pdf",
            OverwritePrompt = true,
            Title = "Xuất hóa đơn PDF"
        };
        if (hopThoai.ShowDialog(chu) != DialogResult.OK)
            return;

        try
        {
            XuatPdf(hopThoai.FileName, hoaDon, chiTiet);
            MessageBox.Show(chu, $"Đã xuất hóa đơn PDF:\n{hopThoai.FileName}",
                "Xuất PDF thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(chu, $"Không thể xuất PDF hóa đơn: {ex.Message}",
                "Lỗi xuất PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void XuatPdf(string duongDan, HoaDon hoaDon, IReadOnlyList<CTHoaDon> chiTiet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(duongDan);
        ArgumentNullException.ThrowIfNull(hoaDon);
        ArgumentNullException.ThrowIfNull(chiTiet);

        string? mayIn = PrinterSettings.InstalledPrinters
            .Cast<string>()
            .FirstOrDefault(ten => ten.Equals(MayInPdf, StringComparison.OrdinalIgnoreCase));
        if (mayIn == null)
            throw new InvalidOperationException($"Không tìm thấy máy in PDF '{MayInPdf}'.");

        string tepPdf = Path.GetFullPath(duongDan);
        using var taiLieu = TaoTaiLieu(hoaDon, chiTiet);
        taiLieu.PrinterSettings.PrinterName = mayIn;
        if (!taiLieu.PrinterSettings.IsValid)
            throw new InvalidOperationException($"Máy in PDF '{mayIn}' không khả dụng.");
        taiLieu.PrinterSettings.PrintToFile = true;
        taiLieu.PrinterSettings.PrintFileName = tepPdf;
        taiLieu.Print();

        using var tep = new FileStream(tepPdf, FileMode.Open, FileAccess.Read, FileShare.Read);
        Span<byte> dauTep = stackalloc byte[5];
        if (tep.Length < dauTep.Length || tep.Read(dauTep) != dauTep.Length ||
            !dauTep.SequenceEqual("%PDF-"u8))
            throw new InvalidOperationException("Máy in không tạo được tệp PDF hợp lệ.");
    }

    private static PrintDocument TaoTaiLieu(HoaDon hoaDon, IReadOnlyList<CTHoaDon> chiTiet)
    {
        var taiLieu = new PrintDocument { DocumentName = $"HoaDon-{hoaDon.MaHD}" };
        int chiTietDangIn = 0;
        taiLieu.PrintPage += (_, e) =>
        {
            Graphics? graphics = e.Graphics;
            if (graphics == null)
            {
                e.Cancel = true;
                return;
            }

            Rectangle bounds = e.MarginBounds;
            using var fontTieuDe = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var fontDam = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var fontThuong = new Font("Segoe UI", 9F);
            using var brush = new SolidBrush(Color.Black);
            using var dinhDangTen = new StringFormat(StringFormat.GenericDefault)
            {
                Trimming = StringTrimming.EllipsisWord
            };
            using var canPhai = new StringFormat(StringFormat.GenericDefault)
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };

            float cotSoLuong = bounds.Left + bounds.Width * 0.58F;
            float cotDonGia = bounds.Left + bounds.Width * 0.68F;
            float cotThanhTien = bounds.Left + bounds.Width * 0.83F;
            float rongTen = cotSoLuong - bounds.Left - 8;
            float rongSoLuong = cotDonGia - cotSoLuong - 6;
            float rongDonGia = cotThanhTien - cotDonGia - 6;
            float rongThanhTien = bounds.Right - cotThanhTien;
            float y = bounds.Top;

            graphics.DrawString("HÓA ĐƠN BÁN THUỐC", fontTieuDe, brush, bounds.Left, y);
            y += 36;
            graphics.DrawString($"Mã hóa đơn: {hoaDon.MaHD}", fontThuong, brush, bounds.Left, y);
            graphics.DrawString($"Ngày lập: {hoaDon.NgayLap:dd/MM/yyyy}", fontThuong, brush,
                bounds.Left + bounds.Width / 2F, y);
            y += 22;
            graphics.DrawString($"Nhân viên: {hoaDon.TenNhanVien} ({hoaDon.MaNV})", fontThuong, brush, bounds.Left, y);
            y += 22;
            graphics.DrawString($"Khách hàng: {hoaDon.TenKhachHang}", fontThuong, brush, bounds.Left, y);
            y += 28;

            graphics.DrawString("Thuốc", fontDam, brush, bounds.Left, y);
            graphics.DrawString("SL", fontDam, brush, new RectangleF(cotSoLuong, y, rongSoLuong, 18), canPhai);
            graphics.DrawString("Đơn giá", fontDam, brush, new RectangleF(cotDonGia, y, rongDonGia, 18), canPhai);
            graphics.DrawString("Thành tiền", fontDam, brush, new RectangleF(cotThanhTien, y, rongThanhTien, 18), canPhai);
            y += 21;
            graphics.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
            y += 7;

            float chanBang = bounds.Bottom - 105;
            int soDongTrang = 0;
            while (chiTietDangIn < chiTiet.Count)
            {
                CTHoaDon dong = chiTiet[chiTietDangIn];
                string tenThuoc = $"{dong.TenThuoc} ({dong.MaThuoc})";
                float caoTen = graphics.MeasureString(tenThuoc, fontThuong, (int)rongTen, dinhDangTen).Height;
                float caoDong = Math.Max(20, caoTen);
                if (y + caoDong > chanBang && soDongTrang > 0)
                    break;

                graphics.DrawString(tenThuoc, fontThuong, brush,
                    new RectangleF(bounds.Left, y, rongTen, caoDong), dinhDangTen);
                graphics.DrawString(dong.SoLuong.ToString(), fontThuong, brush,
                    new RectangleF(cotSoLuong, y, rongSoLuong, caoDong), canPhai);
                graphics.DrawString(dong.DonGia.ToString("N2"), fontThuong, brush,
                    new RectangleF(cotDonGia, y, rongDonGia, caoDong), canPhai);
                graphics.DrawString(dong.ThanhTien.ToString("N2"), fontThuong, brush,
                    new RectangleF(cotThanhTien, y, rongThanhTien, caoDong), canPhai);
                y += caoDong + 4;
                chiTietDangIn++;
                soDongTrang++;
            }

            if (chiTietDangIn < chiTiet.Count)
            {
                e.HasMorePages = true;
                return;
            }

            y = Math.Max(y + 8, chanBang + 8);
            graphics.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
            y += 10;
            graphics.DrawString("Thành tiền từng dòng đã áp dụng giảm giá sản phẩm.", fontThuong, brush, bounds.Left, y);
            y += 21;
            graphics.DrawString($"Tiền giảm hóa đơn: {hoaDon.TienGiam:N2} đ", fontThuong, brush, bounds.Left, y);
            y += 24;
            graphics.DrawString($"TỔNG THANH TOÁN: {hoaDon.TongTien:N2} đ", fontDam, brush, bounds.Left, y);
            e.HasMorePages = false;
        };

        return taiLieu;
    }
}

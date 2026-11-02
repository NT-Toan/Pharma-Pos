using System.Drawing.Printing;

namespace QLNT.GUI;

internal static class HoaDonIn
{
    public static void In(Form chu, HoaDon hoaDon, IReadOnlyList<CTHoaDon> chiTiet)
    {
        using var taiLieu = new PrintDocument { DocumentName = $"HoaDon-{hoaDon.MaHD}" };
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
            using var fontDam = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var fontThuong = new Font("Segoe UI", 10F);
            using var brush = new SolidBrush(Color.Black);
            float cotSoLuong = bounds.Left + bounds.Width * 0.58F;
            float cotDonGia = bounds.Left + bounds.Width * 0.68F;
            float cotThanhTien = bounds.Left + bounds.Width * 0.82F;
            float y = bounds.Top;

            graphics.DrawString("HÓA ĐƠN BÁN THUỐC", fontTieuDe, brush, bounds.Left, y);
            y += 38;
            graphics.DrawString($"Mã hóa đơn: {hoaDon.MaHD}", fontThuong, brush, bounds.Left, y);
            graphics.DrawString($"Ngày lập: {hoaDon.NgayLap:dd/MM/yyyy}", fontThuong, brush, bounds.Left + bounds.Width / 2F, y);
            y += 24;
            graphics.DrawString($"Nhân viên: {hoaDon.TenNhanVien} ({hoaDon.MaNV})", fontThuong, brush, bounds.Left, y);
            y += 24;
            graphics.DrawString($"Khách hàng: {hoaDon.TenKhachHang}", fontThuong, brush, bounds.Left, y);
            y += 32;

            graphics.DrawString("Thuốc", fontDam, brush, bounds.Left, y);
            graphics.DrawString("SL", fontDam, brush, cotSoLuong, y);
            graphics.DrawString("Đơn giá", fontDam, brush, cotDonGia, y);
            graphics.DrawString("Thành tiền", fontDam, brush, cotThanhTien, y);
            y += 22;
            graphics.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
            y += 7;

            while (chiTietDangIn < chiTiet.Count && y + 22 < bounds.Bottom - 105)
            {
                CTHoaDon dong = chiTiet[chiTietDangIn++];
                graphics.DrawString($"{dong.TenThuoc} ({dong.MaThuoc})", fontThuong, brush, bounds.Left, y);
                graphics.DrawString(dong.SoLuong.ToString(), fontThuong, brush, cotSoLuong, y);
                graphics.DrawString(dong.DonGia.ToString("N2"), fontThuong, brush, cotDonGia, y);
                graphics.DrawString(dong.ThanhTien.ToString("N2"), fontThuong, brush, cotThanhTien, y);
                y += 22;
            }

            if (chiTietDangIn < chiTiet.Count)
            {
                e.HasMorePages = true;
                return;
            }

            y += 14;
            graphics.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
            y += 12;
            graphics.DrawString($"Tiền giảm: {hoaDon.TienGiam:N2} đ", fontThuong, brush, bounds.Left, y);
            y += 24;
            graphics.DrawString($"TỔNG THANH TOÁN: {hoaDon.TongTien:N2} đ", fontDam, brush, bounds.Left, y);
            e.HasMorePages = false;
        };

        using var hopThoai = new PrintDialog { Document = taiLieu, UseEXDialog = true };
        if (hopThoai.ShowDialog(chu) != DialogResult.OK)
            return;

        try
        {
            taiLieu.Print();
        }
        catch (Exception ex)
        {
            MessageBox.Show(chu, $"Không thể in hoặc xuất PDF hóa đơn: {ex.Message}",
                "Lỗi in hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

using System.Collections;

// Bản chụp đầu phiếu và chi tiết, dùng để phát hiện người khác đã sửa/xóa phiếu.
public sealed class DuLieuPhieuNhap
{
    public PhieuNhap Phieu { get; set; } = new();
    public ArrayList ChiTiet { get; set; } = new();
}

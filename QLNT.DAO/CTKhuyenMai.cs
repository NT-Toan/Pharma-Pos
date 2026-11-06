using System;

public class CTKhuyenMai
{
    public string MaCTKM { get; set; } = string.Empty;
    public string TenCTKM { get; set; } = string.Empty;
    public string TheLoai { get; set; } = "Chưa cấu hình";
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }

    public CTKhuyenMai() { }

    public CTKhuyenMai(string ma, string ten, DateTime? batDau, DateTime? ketThuc)
    {
        MaCTKM = ma;
        TenCTKM = ten;
        NgayBatDau = batDau;
        NgayKetThuc = ketThuc;
    }
}
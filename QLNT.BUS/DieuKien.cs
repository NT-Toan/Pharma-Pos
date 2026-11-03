public enum PhepSoSanh
{
    Bang,        // =
    Khac,        // !=
    Lon,         // >
    LonHoacBang, // >=
    Nho,         // <
    NhoHoacBang, // <=
    Chua         // Chuỗi có chứa giá trị cần tìm
}

public enum PhepNoi
{
    And, // Phải đúng tất cả điều kiện
    Or   // Chỉ cần đúng một điều kiện
}

public class DieuKien
{
    // Tên thuộc tính của đối tượng, ví dụ: "Luong", "HoTenNV"
    public string TenTruong { get; set; } = string.Empty;

    public PhepSoSanh Phep { get; set; }

    public object? GiaTri { get; set; }

    // true: đảo ngược kết quả điều kiện
    public bool Not { get; set; }
}
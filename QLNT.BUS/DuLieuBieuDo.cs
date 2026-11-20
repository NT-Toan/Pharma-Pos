public sealed record CotBieuDo(string Nhan, decimal GiaTriChinh, decimal? GiaTriPhu);
public sealed record LatBieuDo(string Nhan, decimal GiaTri);

public sealed class DuLieuBieuDo
{
    public string TieuDe { get; init; } = string.Empty;
    public string TenDayChinh { get; init; } = string.Empty;
    public string? TenDayPhu { get; init; }
    public string TieuDeTron { get; init; } = string.Empty;
    public IReadOnlyList<CotBieuDo> Cot { get; init; } = [];
    public IReadOnlyList<LatBieuDo> Tron { get; init; } = [];
}

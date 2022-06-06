public interface IPriceFilter
{
    string? ArmRegionName { get; }
    string? ArmSkuName { get; }
    string? Location { get; }
    string? MeterId { get; }
    string? MeterName { get; }
    string? PriceType { get; }
    string? ProductId { get; }
    string? ProductName { get; }
    string? ServiceFamily { get; }
    string? ServiceId { get; }
    string? ServiceName { get; }
    string? SkuId { get; }
    string? SkuName { get; }
}
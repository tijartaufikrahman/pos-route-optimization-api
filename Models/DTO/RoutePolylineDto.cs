public class RoutePolylineDto
{
    public LocationDto? start { get; set; }

    public LocationDto? end { get; set; }

    public string? polyline { get; set; }
}

public class RoutePolylineResponseDto
{
    public List<RoutePolylineDto> routes { get; set; } = new();
}

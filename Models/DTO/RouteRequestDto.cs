public class RouteRequestDto
{
    public List<LocationDto> locations { get; set; }
}

public class LocationDto
{
    public double lat { get; set; }
    public double lon { get; set; }
}
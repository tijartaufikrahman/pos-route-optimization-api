public class CalculateSegmentDto
{
    public int fromIndex { get; set; }

    public int toIndex { get; set; }

    public double distance { get; set; }

    public double duration { get; set; }
}

public class CalculateResultDto
{
    public double totalDistance { get; set; }

    public double totalDuration { get; set; }

    public List<CalculateSegmentDto> segments { get; set; } = new();

    public List<string> polylines { get; set; } = new();

    public int totalPoints { get; set; }
}

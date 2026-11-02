public class SegmentDto
{
    public string from { get; set; } = string.Empty;

    public string to { get; set; } = string.Empty;

    public double distance { get; set; }

    public double duration { get; set; }
}

public class OrderedPaketDto
{
    public int order { get; set; }

    public int paketId { get; set; }

    public double lat { get; set; }

    public double lon { get; set; }
}

public class OptimizationResultDto
{
    public List<OrderedPaketDto> orderedPakets { get; set; } = new();

    public double totalDistance { get; set; }

    public double totalDuration { get; set; }

    public List<SegmentDto> segments { get; set; } = new();

    public List<IterationHistory> histories { get; set; } = new();

    public List<IterationHistory> histories2Opt { get; set; } = new();
}

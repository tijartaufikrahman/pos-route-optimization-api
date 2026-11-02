public class AcoResult
{
    public List<int> solution { get; set; }

    public double distance { get; set; }

    public double duration { get; set; }

    public List<IterationHistory> histories { get; set; }


}

public class IterationHistory
{
    public int Iteration { get; set; }

    public double BestDistance { get; set; }

    public double BestDuration { get; set; }

    public List<int> BestRoute { get; set; }

    public int? SwapFromIndex { get; set; }

    public int? SwapToIndex { get; set; }

    public string? SwapValue { get; set; }

    public List<int>? RouteBeforeSwap { get; set; }

    public List<int>? RouteAfterSwap { get; set; }

    public double? DistanceBeforeSwap { get; set; }

    public double? DistanceAfterSwap { get; set; }
}
public class OptimizationRequestDto
{
    public double depotLat { get; set; }
    public double depotLon { get; set; }
    public string? algoritma { get; set; } = "aco";
    public int? ant { get; set; } = 65;


    //public string? algo { get; set; } = "aco";
    
    public bool? is_2opt { get; set; } = true;

    public List<PaketDto> pakets { get; set; }
}
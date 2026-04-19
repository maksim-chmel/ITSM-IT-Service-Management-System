namespace ITSM.ViewModels.Manage;

public class ChartData
{
    public string[] Labels { get; set; } = Array.Empty<string>();
    public int[] Data { get; set; } = Array.Empty<int>();
    public string[] BackgroundColors { get; set; } = Array.Empty<string>();
}
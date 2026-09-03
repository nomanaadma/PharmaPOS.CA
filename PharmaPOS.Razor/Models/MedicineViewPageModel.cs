namespace PharmaPOS.Razor.Models;

public class MedicineViewPageModel
{
    public MedicineViewModel Medicine { get; init; } = null!;
    public IEnumerable<StockViewModel> LiveStock { get; init; } = [];
    public IEnumerable<StockViewModel> BadStock { get; init; } = [];
}

namespace PharmaPOS_MVC.Models;

public class MedicineViewPageModel
{
    public MedicineViewModel Medicine { get; set; } = null!;
    public IEnumerable<StockViewModel> LiveStock { get; set; } = new List<StockViewModel>();
    public IEnumerable<StockViewModel> BadStock { get; set; } = new List<StockViewModel>();
}

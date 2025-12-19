namespace ShopHerePJ.Models;

public class ShopListVM
{
    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public double? MinRating { get; set; }

    public List<CategoryOpt> Categories { get; set; } = new();
    public List<ProductCardVM> Items { get; set; } = new();
}

public class CategoryOpt
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

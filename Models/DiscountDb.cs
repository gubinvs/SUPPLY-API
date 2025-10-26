using System.ComponentModel.DataAnnotations;


namespace SUPPLY_API
{
    public class DiscountDb
    {
        [Key]
        public int Id { get; set; }

        public string Manufacturer { get; set; } = string.Empty;

        public decimal Discount { get; set; } = 1m; // коэффициент скидки
    }
}


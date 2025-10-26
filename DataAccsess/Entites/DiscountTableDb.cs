using System.ComponentModel.DataAnnotations;



namespace SUPPLY_API
{
    public class DiscountTableDb
    {


        public DiscountTableDb() { }
        /// <summary>
        /// Уникальный ключ в таблице данных
        /// </summary>
        [Key]
        public int? Id { get; set; }

        /// <summary>
        /// Идентификатор производителя
        /// </summary>
        ///
        public string? GuidIdManufacturer { get; set; }


        /// <summary>
        /// Наименование производителя
        /// </summary>
        ///
        public string? Manufacturer { get; set; } 

        /// <summary>
        /// Скидка
        /// </summary>
        /// 
        public decimal? Discount { get; set; }

    }
}
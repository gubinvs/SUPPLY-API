
using System.ComponentModel.DataAnnotations;

namespace SUPPLY_API
{
    public class GoodsTableDb
    {

        public GoodsTableDb() { }
        /// <summary>
        /// Уникальный ключ в таблице данных
        /// </summary>
        
        [Key]
        public int? Id { get; set; }

        /// <summary>
        /// Ссылка на картинку для страницы описания товара
        /// </summary>
        ///
        public string? ImgLinkPage { get; set; }

        /// <summary>
        /// Артикул товара
        /// </summary>
        /// 
        public string? VendorCode { get; set; }

        /// <summary>
        /// Наменование товара
        /// </summary>
        /// 
        public string? NameComponent { get; set; }

        /// <summary>
        /// Производитель
        /// </summary>
        /// 
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Количество товара в наличии
        /// </summary>
        /// 
        public int? Quantity { get; set; }

        /// <summary>
        /// Доставка в сутках
        /// </summary>
        public int? DeliveryТime { get; set; }


        /// <summary>
        /// Стоимость товара
        /// </summary>
        public int? Price { get; set; }

        /// <summary>
        /// Является ли хитом продаж
        /// </summary>
        public int? Bestseller { get; set; }

        /// <summary>
        /// Каталог или группа товара
        /// </summary>
        public string? Chapter { get; set; }

        /// <summary>
        /// Страница описания товара
        /// </summary>
        public string? LinkPage { get; set; }

        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public string? Guid { get; set; }

        /// <summary>
        /// Cсылка на маленьую картинку для корзины
        /// </summary>
        public string? BasketImgPath { get; set; }

        /// <summary>
        /// Ссылка на картинку для карточки товара
        /// </summary>
        public string? ImgLinkIconCard { get; set; }

        /// <summary>
        /// Описание товара
        /// </summary>
        public string? ProductDescription { get; set; }
    }
}
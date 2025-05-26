using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace SUPPLY_API.Controllers
{

    /// <summary>
    /// Данный контроллер отвечает за изменение данных о предлагаемой цене и сроке поставки от конкретного ПОСТАВЩИКА:
    /// Принцип работы:
    /// - контроллер принимает АРТИКУЛ изменяемого компонента "VendorCodeComponent", "GuidIdProvider" идентификатор поставщика, 
    ///     новая цена "PriceComponent", срок доставки в неделях "DeliveryTimeComponent", дата присваивается автоматически текущая.
    /// - проверяется наличие записи с аналогичным артикулом в базе данных, если его нет отправляет соответствующий ответ.
    /// - если такой артикул есть, изымается информация о "GuidIdComponent" соответствующая артикулу. 
    /// - далее, проверяется наличие записи в таблице данных "PriceComponent" по двум критериям:
    ///     совпадение в записи и "GuidIdComponent" и "GuidIdProvider", если запись с такими полями присутствует, 
    ///     то изменяет эту запись новыми данными, если нет создается новая запись.
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class ChangePriceController : ControllerBase
    {

        private readonly ILogger<AddComponentController> _logger;

        // База данных с информацией о комплектующих
        private readonly SupplyComponentContext _db;

        // База данных с ценами и сроками
        private readonly SupplyPriceComponentContext _dbPrice;

        // База данных с информацией о поставщиках
        private readonly SupplyProviderContext _dbProvider;

        public ChangePriceController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db,
            SupplyPriceComponentContext dbPrice,
            SupplyProviderContext dbProvider

            )
        {
            _logger = logger;
            _db = db;
            _dbPrice = dbPrice;
            _dbProvider = dbProvider;
        }

        [HttpPost]
        public async Task<IActionResult> ReadComponent([FromBody] ChangePriceModel model)
        {
            try
            {
                var existing = await _db.SupplyComponent
                    .AnyAsync(c => c.VendorCodeComponent == model.VendorCodeComponent);

                if (!existing)
                {
                    return Conflict(new { message = $"Компонент с артикулом {model.VendorCodeComponent} отсутствует в базе данных" });
                }

                // Получаем GuidIdComponent по артикулу
                var guidIdComponent = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent == model.VendorCodeComponent)
                    .Select(c => c.GuidIdComponent)
                    .FirstOrDefaultAsync();

                // Получаем все записи по этому компоненту
                var offers = await _dbPrice.PriceComponent
                    .Where(p => p.GuidIdComponent == guidIdComponent)
                    .ToListAsync();

                // Ищем, есть ли уже предложение от этого поставщика
                var existingOffer = offers.FirstOrDefault(o => o.GuidIdProvider == model.GuidIdProvider);

                if (existingOffer != null)
                {
                    // Обновляем существующую запись
                    existingOffer.PriceComponent = model.PriceComponent;
                    existingOffer.DeliveryTimeComponent = model.DeliveryTimeComponent;
                    existingOffer.SaveDataPrice = DateTime.UtcNow;

                    _dbPrice.PriceComponent.Update(existingOffer);
                    await _dbPrice.SaveChangesAsync();

                    return Ok(new { message = "Запись успешно обновлена." });
                }
                else
                {
                    // Создаем новую запись
                    var newOffer = new PriceDb
                    {
                        GuidIdComponent = guidIdComponent,
                        GuidIdProvider = model.GuidIdProvider,
                        PriceComponent = model.PriceComponent,
                        DeliveryTimeComponent = model.DeliveryTimeComponent,
                        SaveDataPrice = DateTime.UtcNow
                    };

                    await _dbPrice.PriceComponent.AddAsync(newOffer);
                    await _dbPrice.SaveChangesAsync();

                    return Ok(new { message = "Новая запись успешно добавлена." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса: {article}", model.VendorCodeComponent);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }

    };

    public record ChangePriceModel(
        string VendorCodeComponent,
        string GuidIdProvider,
        string PriceComponent,
        string DeliveryTimeComponent
    );
};
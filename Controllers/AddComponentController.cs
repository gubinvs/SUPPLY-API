using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddComponentController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;
        private readonly SupplyComponentContext _db;
        private readonly SupplyPriceComponentContext _dbPrice;
        private readonly ManufacturerComponentContext _dbManufact;
        private readonly UnitMeasurementComponentContext _dbUnit;

        public AddComponentController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db,
            SupplyPriceComponentContext dbPrice,
            ManufacturerComponentContext dbManufact,
            UnitMeasurementComponentContext dbUnit)
        {
            _logger = logger;
            _db = db;
            _dbPrice = dbPrice;
            _dbManufact = dbManufact;
            _dbUnit = dbUnit;
        }

        [HttpPost]
        public async Task<IActionResult> AddComponent([FromBody] AddComponentModel model)
        {

            if (model.VendorCodeComponent.Contains("/"))
            {
                return BadRequest(new { message = "Артикул не должен содержать символ '/'." });
            }
            
            try
            {
                var existing = await _db.SupplyComponent
                    .FirstOrDefaultAsync(c => c.VendorCodeComponent == model.VendorCodeComponent);

                if (existing != null)
                {
                    existing.NameComponent = model.NameComponent;
                    _db.SupplyComponent.Update(existing);
                    await _db.SaveChangesAsync();

                    // Обновление производителя
                    var manufacturerRelation = await _dbManufact.ManufacturerComponent
                        .FirstOrDefaultAsync(x => x.GuidIdComponent == existing.GuidIdComponent);

                    if (manufacturerRelation != null)
                    {
                        manufacturerRelation.GuidIdManufacturer = model.guidIdManufacturer ?? "";
                        _dbManufact.ManufacturerComponent.Update(manufacturerRelation);
                    }
                    else
                    {
                        _dbManufact.ManufacturerComponent.Add(new ManufacturerComponentDb
                        {
                            GuidIdComponent = existing.GuidIdComponent,
                            GuidIdManufacturer = model.guidIdManufacturer ?? ""
                        });
                    }

                    // Обновление единицы измерения
                    var unitRelation = await _dbUnit.UnitMeasurementComponent
                        .FirstOrDefaultAsync(x => x.GuidIdComponent == existing.GuidIdComponent);

                    if (unitRelation != null)
                    {
                        unitRelation.GuidIdUnitMeasurement = model.guidIdUnitMeasurement ?? "";
                        _dbUnit.UnitMeasurementComponent.Update(unitRelation);
                    }
                    else
                    {
                        _dbUnit.UnitMeasurementComponent.Add(new UnitMeasurementComponentDb
                        {
                            GuidIdComponent = existing.GuidIdComponent,
                            GuidIdUnitMeasurement = model.guidIdUnitMeasurement ?? ""
                        });
                    }

                    await _dbManufact.SaveChangesAsync();
                    await _dbUnit.SaveChangesAsync();

                    return Ok(new { message = "Компонент обновлён", id = existing.GuidIdComponent });
                }

                // Добавление нового компонента
                var newComponent = new ComponentDb
                {
                    GuidIdComponent = Guid.NewGuid().ToString(),
                    VendorCodeComponent = model.VendorCodeComponent,
                    NameComponent = model.NameComponent
                };

                _db.SupplyComponent.Add(newComponent);
                await _db.SaveChangesAsync();

                // Производитель
                _dbManufact.ManufacturerComponent.Add(new ManufacturerComponentDb
                {
                    GuidIdComponent = newComponent.GuidIdComponent,
                    GuidIdManufacturer = model.guidIdManufacturer ?? ""
                });

                // Единица измерения
                _dbUnit.UnitMeasurementComponent.Add(new UnitMeasurementComponentDb
                {
                    GuidIdComponent = newComponent.GuidIdComponent,
                    GuidIdUnitMeasurement = model.guidIdUnitMeasurement ?? ""
                });

                await _dbManufact.SaveChangesAsync();
                await _dbUnit.SaveChangesAsync();

                return Ok(new { message = "Компонент успешно добавлен", id = newComponent.GuidIdComponent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении/обновлении компонента: {VendorCode}", model.VendorCodeComponent);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }

        [HttpDelete("{vendorCode}")]
        public async Task<IActionResult> DeleteComponent(string vendorCode)
        {
            try
            {
                var component = await _db.SupplyComponent
                    .FirstOrDefaultAsync(c => c.VendorCodeComponent == vendorCode);

                if (component == null)
                    return NotFound(new { message = "Компонент не найден." });

                var relatedPrices = await _dbPrice.PriceComponent
                    .Where(p => p.GuidIdComponent == component.GuidIdComponent)
                    .ToListAsync();

                if (relatedPrices.Any())
                {
                    _dbPrice.PriceComponent.RemoveRange(relatedPrices);
                    await _dbPrice.SaveChangesAsync();
                }

                var relatedManufacturers = await _dbManufact.ManufacturerComponent
                    .Where(x => x.GuidIdComponent == component.GuidIdComponent)
                    .ToListAsync();

                if (relatedManufacturers.Any())
                {
                    _dbManufact.ManufacturerComponent.RemoveRange(relatedManufacturers);
                    await _dbManufact.SaveChangesAsync();
                }

                var relatedUnits = await _dbUnit.UnitMeasurementComponent
                    .Where(x => x.GuidIdComponent == component.GuidIdComponent)
                    .ToListAsync();

                if (relatedUnits.Any())
                {
                    _dbUnit.UnitMeasurementComponent.RemoveRange(relatedUnits);
                    await _dbUnit.SaveChangesAsync();
                }

                _db.SupplyComponent.Remove(component);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Компонент и все связанные данные успешно удалены." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении компонента: {VendorCode}", vendorCode);
                return StatusCode(500, new { message = "Произошла ошибка при удалении компонента." });
            }
        }
    }
}
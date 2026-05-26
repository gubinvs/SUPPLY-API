using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace SUPPLY_API
{

    /// <summary>
    /// Сервис возвращает максимальное по цене предложение поставщиков
    /// </summary>
    public class ReturnMaxPriceProviderService
    {

        private readonly ILogger<ReturnMaxPriceProviderService> _logger;
        private readonly SupplyComponentContext _db;
        private readonly SupplyPriceComponentContext _dbPrice;
        private readonly SupplyProviderContext _dbProvider;
        private readonly ManufacturerComponentContext _dbManufact;
        private readonly UnitMeasurementComponentContext _dbUnit;
        private readonly SupplyManufacturerContext _dbSupplyManufact;
        private readonly SupplyUnitMeasurementContext _dbSupplyUnit;

        public ReturnMaxPriceProviderService (
                ILogger<ReturnMaxPriceProviderService> logger,
                SupplyComponentContext db,
                SupplyPriceComponentContext dbPrice,
                SupplyProviderContext dbProvider,
                ManufacturerComponentContext dbManufact,
                SupplyManufacturerContext dbSupplyManufact,
                UnitMeasurementComponentContext dbUnit,
                SupplyUnitMeasurementContext dbSupplyUnit
            )
        {
            _logger = logger;
            _db = db;
            _dbPrice = dbPrice;
            _dbProvider = dbProvider;
            _dbManufact = dbManufact;
            _dbSupplyManufact = dbSupplyManufact;
            _dbUnit = dbUnit;
            _dbSupplyUnit = dbSupplyUnit;
        }


        public async Task<ReturnOffer?> GetMaxPriceProvider (string article)
        {
             try
            {

                ReturnOffer errorOffer = new ReturnOffer
                (
                    1, // Если 0 то это означает ,что нет ошибки, если 1 то ошибка
                    "",
                    "",
                    0,
                    "",
                    "",
                    "",
                    "",
                    ""
                );

                // Загрузили данные о номенклатуре согласно запрашиваемому артикулу
                var component = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent == article)
                    .Select(c => new 
                    {
                        c.GuidIdComponent,
                        c.NameComponent
                    })
                    .FirstOrDefaultAsync();

                if (component == null)
                {
                    return (errorOffer );
                }

                // Загрузили все предложения имеющиеся по данной номенклатуре
                var offers = await _dbPrice.PriceComponent
                    .Where(p => p.GuidIdComponent == component.GuidIdComponent)
                    .ToListAsync();

                var providerIds = offers.Select(o => o.GuidIdProvider).Distinct().ToList();

                var providers = await _dbProvider.SupplyProvider
                    .Where(pr => providerIds.Contains(pr.GuidIdProvider))
                    .ToListAsync();

                // Находим максимальную цену
                int? maxPrice = offers.Max(o => o.PriceComponent);

                // Отбираем только первое предложение с максимальной ценой
                var offerWithName = offers
                    .Where(o => o.PriceComponent == maxPrice)
                    .Select(offer =>
                    {
                        var provider = providers.FirstOrDefault(p => p.GuidIdProvider == offer.GuidIdProvider);
                        return new
                        {
                            NameProvider = provider?.NameProvider ?? "Неизвестный поставщик",
                            offer.PriceComponent,
                            offer.DeliveryTimeComponent,
                            offer.SaveDataPrice
                        };
                    })
                    .FirstOrDefault();

                // Загрузили данные о производителе
                var manufacturerComponent = await _dbManufact.ManufacturerComponent
                    .Where(c => c.GuidIdComponent == component.GuidIdComponent)
                    .Select(c => c.GuidIdManufacturer)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(manufacturerComponent))
                    return (errorOffer );;

                var manufacturerName = await _dbSupplyManufact.SupplyManufacturer
                    .Where(sm => sm.GuidIdManufacturer == manufacturerComponent)
                    .Select(sm => sm.NameManufacturer)
                    .FirstOrDefaultAsync();

                var unitComponent = await _dbUnit.UnitMeasurementComponent
                    .Where(c => c.GuidIdComponent == component.GuidIdComponent)
                    .Select(c => c.GuidIdUnitMeasurement)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(unitComponent))
                    return (errorOffer );

                var unitName = await _dbSupplyUnit.SupplyUnitMeasurement
                    .Where(su => su.GuidIdUnitMeasurement == unitComponent)
                    .Select(su => su.NameUnitMeasurement)
                    .FirstOrDefaultAsync();

                
                if (offerWithName == null)
                {
                    return (errorOffer);
                }
                
               
                ReturnOffer newOffer = new ReturnOffer
                (
                    0,
                    article,
                    component.NameComponent ?? "",
                    offerWithName.PriceComponent ?? 0,
                    offerWithName.DeliveryTimeComponent ?? "",
                    offerWithName.SaveDataPrice.ToString("dd.MM.yyyy"),
                    manufacturerName ?? "",
                    unitName ?? "",
                    "Максимальная цена предложений"
                    
                );

                return (newOffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса: {article}", article);
                return null;
            }
        }
    }
}
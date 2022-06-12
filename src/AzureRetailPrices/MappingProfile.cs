using AutoMapper;
using Knapcode.AzureRetailPrices.Client;
using Normalized = Knapcode.AzureRetailPrices.Database.Normalized;
using Denormalized = Knapcode.AzureRetailPrices.Database.Denormalized;

namespace Knapcode.AzureRetailPrices;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PriceResponse, PriceFilter>();

        CreateMap<PriceResponse, Normalized.ArmSkuName>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.ArmSkuName));
        CreateMap<PriceResponse, Normalized.Meter>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.UnitOfMeasureId, opt => opt.Ignore())
            .ForMember(x => x.UnitOfMeasure, opt => opt.MapFrom(x => x));
        CreateMap<PriceResponse, Normalized.MeterName>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.MeterName));
        CreateMap<PriceResponse, Normalized.PriceType>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.PriceType));
        CreateMap<PriceResponse, Normalized.Product>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.ProductId, opt => opt.MapFrom(x => x.ProductId))
            .ForMember(x => x.ProductName, opt => opt.MapFrom(x => x.ProductName))
            .ForMember(x => x.ServiceId, opt => opt.Ignore())
            .ForMember(x => x.Service, opt => opt.MapFrom(x => x));
        CreateMap<PriceResponse, Normalized.Region>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.ArmRegionName, opt => opt.MapFrom(x => x.ArmRegionName))
            .ForMember(x => x.Location, opt => opt.MapFrom(x => x.Location));
        CreateMap<PriceResponse, Normalized.ReservationTerm>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.ReservationTerm));
        CreateMap<PriceResponse, Normalized.Service>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.ServiceFamilyId, opt => opt.Ignore())
            .ForMember(x => x.ServiceFamily, opt => opt.MapFrom(x => x))
            .ForMember(x => x.ServiceId, opt => opt.MapFrom(x => x.ServiceId))
            .ForMember(x => x.ServiceName, opt => opt.MapFrom(x => x.ServiceName));
        CreateMap<PriceResponse, Normalized.ServiceFamily>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.ServiceFamily));
        CreateMap<PriceResponse, Normalized.Sku>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.ArmSkuNameId, opt => opt.Ignore())
            .ForMember(x => x.ArmSkuName, opt => opt.MapFrom(x => x))
            .ForMember(x => x.ProductId, opt => opt.Ignore())
            .ForMember(x => x.Product, opt => opt.MapFrom(x => x))
            .ForMember(x => x.RegionId, opt => opt.Ignore())
            .ForMember(x => x.Region, opt => opt.MapFrom(x => x))
            .ForMember(x => x.ReservationTermId, opt => opt.Ignore())
            .ForMember(x => x.ReservationTerm, opt => opt.MapFrom(x => x))
            .ForMember(x => x.SkuIdSuffix, opt => opt.MapFrom(x => ExtractSkuIdPrefix(x)))
            .ForMember(x => x.SkuNameId, opt => opt.Ignore())
            .ForMember(x => x.SkuName, opt => opt.MapFrom(x => x));
        CreateMap<PriceResponse, Normalized.SkuName>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.SkuName));
        CreateMap<PriceResponse, Normalized.UnitOfMeasure>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.UnitOfMeasure));

        CreateMap<PriceResponse, Normalized.Price>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.EffectiveStartDate, opt => opt.MapFrom(x => x.EffectiveStartDate!.Value.ToUnixTimeSeconds()))
            .ForMember(x => x.EffectiveEndDate, opt => opt.MapFrom(x => ToNullableUnixTimeSeconds(x.EffectiveEndDate)))
            .ForMember(x => x.MeterId, opt => opt.Ignore())
            .ForMember(x => x.Meter, opt => opt.MapFrom(x => x))
            .ForMember(x => x.MeterNameId, opt => opt.Ignore())
            .ForMember(x => x.MeterName, opt => opt.MapFrom(x => x))
            .ForMember(x => x.PriceTypeId, opt => opt.Ignore())
            .ForMember(x => x.PriceType, opt => opt.MapFrom(x => x))
            .ForMember(x => x.SkuId, opt => opt.Ignore())
            .ForMember(x => x.Sku, opt => opt.MapFrom(x => x));

        CreateMap<PriceResponse, Denormalized.Price>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.EffectiveStartDate, opt => opt.MapFrom(x => x.EffectiveStartDate!.Value.ToUnixTimeSeconds()))
            .ForMember(x => x.EffectiveEndDate, opt => opt.MapFrom(x => ToNullableUnixTimeSeconds(x.EffectiveEndDate)));
    }

    private static long? ToNullableUnixTimeSeconds(DateTimeOffset? x)
    {
        return x?.ToUnixTimeSeconds();
    }

    private static string ExtractSkuIdPrefix(PriceResponse x)
    {
        if (!x.SkuId.StartsWith(x.ProductId)
            || x.SkuId.Length < x.ProductId.Length + 2
            || x.SkuId[x.ProductId.Length] != '/')
        {
            throw new InvalidDataException($"The SKU ID '{x.SkuId}' does not have the format 'PRODUCT_ID/SUFFIX'. The product ID is '{x.ProductId}'.");
        }

        return x.SkuId.Substring(x.ProductId.Length + 1);
    }
}

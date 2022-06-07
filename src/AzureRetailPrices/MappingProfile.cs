using AutoMapper;
using Knapcode.AzureRetailPrices.Client;
using Knapcode.AzureRetailPrices.Database;

namespace Knapcode.AzureRetailPrices;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PriceResponse, PriceFilter>();

        CreateMap<PriceResponse, ArmSkuName>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.ArmSkuName));
        CreateMap<PriceResponse, Meter>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.UnitOfMeasureId, opt => opt.Ignore())
            .ForMember(x => x.UnitOfMeasure, opt => opt.MapFrom(x => x));
        CreateMap<PriceResponse, MeterName>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.MeterName));
        CreateMap<PriceResponse, PriceType>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.PriceType));
        CreateMap<PriceResponse, Product>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.ProductId, opt => opt.MapFrom(x => x.ProductId))
            .ForMember(x => x.ProductName, opt => opt.MapFrom(x => x.ProductName))
            .ForMember(x => x.ServiceId, opt => opt.Ignore())
            .ForMember(x => x.Service, opt => opt.MapFrom(x => x));
        CreateMap<PriceResponse, Region>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.ArmRegionName, opt => opt.MapFrom(x => x.ArmRegionName))
            .ForMember(x => x.Location, opt => opt.MapFrom(x => x.Location));
        CreateMap<PriceResponse, ReservationTerm>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.ReservationTerm));
        CreateMap<PriceResponse, Service>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.ServiceFamilyId, opt => opt.Ignore())
            .ForMember(x => x.ServiceFamily, opt => opt.MapFrom(x => x))
            .ForMember(x => x.ServiceId, opt => opt.MapFrom(x => x.ServiceId))
            .ForMember(x => x.ServiceName, opt => opt.MapFrom(x => x.ServiceName));
        CreateMap<PriceResponse, ServiceFamily>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.ServiceFamily));
        CreateMap<PriceResponse, Sku>()
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
        CreateMap<PriceResponse, SkuName>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.SkuName));
        CreateMap<PriceResponse, UnitOfMeasure>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Value, opt => opt.MapFrom(x => x.UnitOfMeasure));

        CreateMap<PriceResponse, Price>()
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

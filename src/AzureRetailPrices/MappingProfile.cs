using AutoMapper;
using Knapcode.AzureRetailPrices.Client;
using Knapcode.AzureRetailPrices.Database;

namespace Knapcode.AzureRetailPrices;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PriceResponse, PriceFilter>();

        CreateMap<PriceResponse, Price>()
            .ForMember(x => x.PriceId, x => x.Ignore());
    }
}

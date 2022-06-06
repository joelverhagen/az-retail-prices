using AutoMapper;
using Xunit;

namespace Knapcode.AzureRetailPrices;

public class MappingProfileTest
{
    [Fact]
    public void IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        config.AssertConfigurationIsValid();
    }
}

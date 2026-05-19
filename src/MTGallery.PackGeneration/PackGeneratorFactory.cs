using MTGallery.PackGeneration.Generators;
using MTGallery.Persistence;

namespace MTGallery.PackGeneration;

public class PackGeneratorFactory(PostgreSqlRepository repository)
{
    internal IPackGenerator GetPackGenerator(PackGeneratorConfiguration configuration)
        => configuration switch
        {
            SetWithSpgPackGeneratorConfiguration generatorConfiguration => 
                GetSetWithSpgPackGenerator(generatorConfiguration),
            SetWithMysticalArchiveAndSpecialGuestConfiguration generatorConfiguration => 
                GetSetWithMysticalArchiveAndSpecialGuestPackGenerator(generatorConfiguration),
            _ => throw new ArgumentException("Unsupported configuration!")
        };

    private IPackGenerator GetSetWithMysticalArchiveAndSpecialGuestPackGenerator(
        SetWithMysticalArchiveAndSpecialGuestConfiguration configuration)
        => new SetWithMysticalArchivePackGenerator(
            configuration.SetCode,
            configuration.MysticalArchiveSetCode,
            configuration.SpecialGuestRateNumerator,
            configuration.SpecialGuestRateDenominator,
            configuration.SpecialGuestCollectorNumberLowerBound,
            configuration.SpecialGuestCollectorNumberUpperBound,
            configuration.PullRates,
            repository);

    private IPackGenerator GetSetWithSpgPackGenerator(SetWithSpgPackGeneratorConfiguration configuration)
        => new SetWithSpgPackGenerator(
            configuration.SetCode,
            configuration.SpecialGuestRateNumerator,
            configuration.SpecialGuestRateDenominator,
            configuration.SpecialGuestCollectorNumberLowerBound,
            configuration.SpecialGuestCollectorNumberUpperBound,
            configuration.PullRates,
            repository);
}
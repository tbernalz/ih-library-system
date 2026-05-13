using Bogus;

namespace IH.LibrarySystem.IntegrationTests.Support;

public static class TestDataFactory
{
    private static readonly Faker Faker = new();

    public static string UniqueEmail(string prefix = "user") =>
        $"{prefix}-{Guid.NewGuid():N}@test.local";

    public static string Isbn() => Faker.Commerce.Ean13();

    public static string PersonName() => Faker.Name.FullName();
}

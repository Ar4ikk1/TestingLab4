using FsCheck;
using FsCheck.Fluent;
using System;
using System.Linq;
using WalletPropertyTesting.Domain;

namespace WalletPropertyTesting.Tests.Arbitraries
{
    public static class MoneyGenerator
    {
        public static Arbitrary<Money> Money()
        {
            Gen<decimal> boundary = Gen.Elements(0m, 0.01m, 1m, 1000m);
            Gen<decimal> random = Gen.Choose(0, 100_000).Select(c => c / 100m);

            return Gen.OneOf(boundary, random)
                .Select(amount => new Money(amount))
                .ToArbitrary();
        }
    }
}
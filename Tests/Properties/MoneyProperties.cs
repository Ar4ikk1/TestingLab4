using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using System;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests.Properties
{
    public class MoneyProperties
    {

        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public bool Add_then_subtract_returns_original(Money a, Money b)
        {
            Money result = (a + b) - b;
            return result.Amount == a.Amount;
        }


        [Property]
        public Property Negative_amount_throws_ArgumentException(decimal amount)
        {
            bool applicable = amount < 0;
            bool ok = true;
            if (applicable)
            {
                Action act = () => new Money(amount);
                act.Should().ThrowExactly<ArgumentException>();
            }
            return ok.When(applicable);
        }

        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public Property Subtracting_larger_throws_InvalidOperationException(Money a, Money b)
        {
            bool applicable = a.Amount < b.Amount;
            bool ok = true;
            if (applicable)
            {
                Action act = () => { var _ = a - b; };
                act.Should().ThrowExactly<InvalidOperationException>();
            }
            return ok.When(applicable);
        }
    }
}
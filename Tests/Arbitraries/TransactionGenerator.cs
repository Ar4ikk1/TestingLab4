using FsCheck;
using FsCheck.Fluent;
using System;
using System.Linq;
using WalletPropertyTesting.Domain;

namespace WalletPropertyTesting.Tests.Arbitraries
{
    public static class TransactionGenerator
    {
        public static Arbitrary<Domain.Transaction> Transaction()
        {
            Gen<Domain.Transaction> gen =
                from money in MoneyGenerator.Money().Generator
                from type in Gen.Elements(TransactionType.Deposit, TransactionType.Withdraw)
                select type == TransactionType.Deposit
                    ? Domain.Transaction.Deposit(money)
                    : Domain.Transaction.Withdraw(money);

            return gen.ToArbitrary();
        }
    }
}
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using System;
using System.Linq;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests.Properties
{
    public class WalletProperties
    {
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public void Deposit_of_any_money_does_not_throw(Money amount)
        {
            var wallet = new Wallet();
            Action act = () => wallet.Deposit(amount);
            act.Should().NotThrow();
        }
        
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public Property Balance_is_deposits_minus_withdrawals(Money[] deposits, Money[] withdrawals)
        {
            decimal totalIn = deposits.Sum(m => m.Amount);
            decimal totalOut = withdrawals.Sum(m => m.Amount);
            bool applicable = totalOut <= totalIn;
            bool ok = true;
            if (applicable)
            {
                var wallet = new Wallet();
                foreach (var d in deposits) wallet.Deposit(d);
                foreach (var w in withdrawals) wallet.Withdraw(w);
                ok = wallet.Balance.Amount == totalIn - totalOut;
            }
            return ok.When(applicable);
        }
        
        [Property(Arbitrary = new[] { typeof(MoneyGenerator), typeof(TransactionGenerator) })]
        public bool Balance_never_negative_after_any_operations(Transaction[] operations)
        {
            var wallet = new Wallet();
            wallet.Deposit(new Money(operations.Sum(op => op.Amount.Amount)));

            bool ok = true;
            int expectedHistory = 1;
            foreach (var op in operations)
            {
                if (op.Type == TransactionType.Deposit)
                    wallet.Deposit(op.Amount);
                else
                    wallet.Withdraw(op.Amount);

                expectedHistory++;
                ok = ok && wallet.Balance.Amount >= 0 && wallet.History.Count == expectedHistory;
            }
            return ok;
        }
        
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public Property Withdraw_more_than_balance_throws_and_keeps_state(Money funds, Money request)
        {
            bool applicable = request.Amount > funds.Amount;
            bool ok = true;
            if (applicable)
            {
                var wallet = new Wallet();
                wallet.Deposit(funds);
                int historyBefore = wallet.History.Count;

                Action act = () => wallet.Withdraw(request);
                act.Should().ThrowExactly<InvalidOperationException>();

                ok = wallet.Balance.Amount == funds.Amount && wallet.History.Count == historyBefore;
            }
            return ok.When(applicable);
        }
        
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public bool Two_deposits_equal_one_combined_deposit(Money a, Money b)
        {
            var twoSteps = new Wallet();
            twoSteps.Deposit(a);
            twoSteps.Deposit(b);

            var oneStep = new Wallet();
            oneStep.Deposit(a + b);

            return twoSteps.Balance.Amount == oneStep.Balance.Amount;
        }
        
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public bool Repeating_every_deposit_doubles_balance(Money[] deposits)
        {
            var once = new Wallet();
            var twice = new Wallet();
            foreach (var d in deposits)
            {
                once.Deposit(d);
                twice.Deposit(d);
                twice.Deposit(d);
            }
            return twice.Balance.Amount == once.Balance.Amount * 2;
        }
    }
}
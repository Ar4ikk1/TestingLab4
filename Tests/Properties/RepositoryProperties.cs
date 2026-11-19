using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests.Properties
{
    public class RepositoryProperties
    {
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public bool Saving_same_wallet_twice_equals_saving_once(Money amount)
        {
            var repo = new InMemoryWalletRepository();
            var wallet = new Wallet();
            wallet.Deposit(amount);

            repo.Save(wallet);
            int countAfterFirst = repo.All().Count();
            repo.Save(wallet);

            return repo.All().Count() == countAfterFirst
                && repo.Get(wallet.Id).Balance.Amount == amount.Amount;
        }

        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public bool Service_matches_direct_wallet_calls(Money[] deposits)
        {
            var service = new WalletService(new InMemoryWalletRepository());
            var viaService = service.CreateWallet();
            var direct = new Wallet();

            foreach (var d in deposits)
            {
                service.Deposit(viaService.Id, d.Amount);
                direct.Deposit(d);
            }
            bool sameAfterDeposits =
                service.GetBalance(viaService.Id).Amount == direct.Balance.Amount;

            service.Withdraw(viaService.Id, service.GetBalance(viaService.Id).Amount);
            direct.Withdraw(direct.Balance);

            return sameAfterDeposits
                && service.GetBalance(viaService.Id).Amount == direct.Balance.Amount;
        }
        
        [Property]
        public Property Get_unknown_id_throws_KeyNotFoundException(Guid unknownId)
        {
            var repo = new InMemoryWalletRepository();
            var stored = new Wallet();
            repo.Save(stored);

            bool applicable = unknownId != stored.Id;
            bool ok = true;
            if (applicable)
            {
                Action act = () => repo.Get(unknownId);
                act.Should().ThrowExactly<KeyNotFoundException>();
            }
            return ok.When(applicable);
        }
    }
}
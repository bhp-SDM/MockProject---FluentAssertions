using FluentAssertions;
using MockProject.Core.Interfaces;
using MockProject.Core.Model;
using MockProject.Core.Services;
using Moq;

using System;
using System.Collections.Generic;
using Xunit;

namespace XUnitTestProject
{
    public class BankAccountMangerTest
    { 
        // Fake storage for repository
        private readonly SortedDictionary<int, IBankAccount> dataStore;

        private readonly Mock<IRepository<int, IBankAccount>> repoMock;

        // the constructor is executed befor EACH test
        public BankAccountMangerTest()
        {
            dataStore = new SortedDictionary<int, IBankAccount>();

            repoMock = new Mock<IRepository<int, IBankAccount>>();

            repoMock.SetupAllProperties();

            // setup the mock repository to use the fake storage
            repoMock.SetupGet(repo => repo.Count).Returns(dataStore.Count);

            repoMock.Setup(repo => repo.Add(It.IsAny<IBankAccount>())).Callback<IBankAccount>((acc) =>
                dataStore.Add(acc.AccountNumber, acc));

            repoMock.Setup(repo => repo.Update(It.IsAny<IBankAccount>())).Callback<IBankAccount>((acc) =>
                dataStore[acc.AccountNumber] = acc);

            repoMock.Setup(repo => repo.Remove(It.IsAny<IBankAccount>())).Callback<IBankAccount>((acc) =>
                dataStore.Remove(acc.AccountNumber));

            repoMock.Setup(repo => repo.GetByID(It.IsAny<int>())).Returns<int>((accNum) =>
                dataStore.ContainsKey(accNum) ? dataStore[accNum] : null);

            repoMock.Setup(repo => repo.GetAll()).Returns(() => new List<IBankAccount>(dataStore.Values));
        }

        [Fact]
        public void CreateBankAccountManager()
        {
            // arrange
            IRepository<int, IBankAccount> repo = repoMock.Object;

            // act
            BankAccountManager bam = new BankAccountManager(repo);

            // assert
            dataStore.Should().BeEmpty();
        }

        [Fact]
        public void CreateBankAccountManagerMissingRepositoryExpectArgumentException()
        {
            BankAccountManager bam = null;

            // act
            Action ac = () => bam = new BankAccountManager(null);

            // assert
            ac.Should().Throw<ArgumentException>().WithMessage("Missing BankAccount Repository");
            bam.Should().BeNull();
        }

        [Fact]
        public void AddNonExistingBankAccount()
        {
            IBankAccount acc = new BankAccount(1);

            IRepository<int, IBankAccount> repo = repoMock.Object;
            BankAccountManager bam = new BankAccountManager(repo);

            dataStore.Should().BeEmpty();

            // act
            bam.AddBankAccount(acc);

            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc));

            repoMock.Verify(repo => repo.Add(It.IsAny<IBankAccount>()), Times.Once);
        }

        [Fact]
        public void AddBankAccountIsNullExpectArgumentException()
        {
            IRepository<int, IBankAccount> repo = repoMock.Object;
            BankAccountManager bam = new BankAccountManager(repo);

            dataStore.Should().BeEmpty();

            // act
            Action ac = () => bam.AddBankAccount(null);

            // assert
            ac.Should().Throw<ArgumentException>().WithMessage("Bank account cannot be null");
            dataStore.Should().BeEmpty();
            repoMock.Verify(repo => repo.Add(It.IsAny<IBankAccount>()), Times.Never);
        }

        [Fact]
        public void AddExistingBankAccountExpectArgumentException()
        {
            // arrange
            IRepository<int, IBankAccount> repo = repoMock.Object;
            BankAccountManager bam = new BankAccountManager(repo);

            IBankAccount acc = new BankAccount(1);
            dataStore.Add(1, acc);

            dataStore.Should().HaveCount(1).And.Contain(new KeyValuePair<int, IBankAccount>(1, acc));

            // act 
            Action ac = () => bam.AddBankAccount(acc);

            // assert
            ac.Should().Throw<ArgumentException>().WithMessage("Account number already in use");

            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc));

            repoMock.Verify(repo => repo.Add(It.IsAny<IBankAccount>()), Times.Never);
        }

        [Fact]
        public void RemoveExistingBankAccountWithEmptyBalance()
        {
            // Arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create bank account with zero balance
            IBankAccount acc = new BankAccount(1);

            // add it to the datastore
            dataStore.Add(acc.AccountNumber, acc);

            // act
            bam.RemoveBankAccount(acc);

            // assert
            dataStore.Should().BeEmpty();
            repoMock.Verify(repo => repo.Remove(It.IsAny<IBankAccount>()), Times.Once);
        }

        [Fact]
        public void RemoveExistingBankAccountWithPositiveBalanceExpectInvalidOperationException()
        {
            // Arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create bank account with positive balance
            IBankAccount acc = new BankAccount(1, 0.01);

            // and add it to the datastore
            dataStore.Add(acc.AccountNumber, acc);

            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc));

            // act
            Action ac = () => bam.RemoveBankAccount(acc);

            // assert
            ac.Should().
                Throw<InvalidOperationException>()
                .WithMessage("Bank account must be empty before removal");

            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc));

            repoMock.Verify(repo => repo.Remove(It.IsAny<IBankAccount>()), Times.Never);
        }

        [Fact]
        public void RemoveNonExistingBankAccountExpectInvalidOperationException()
        {
            // Arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create two bank accounts
            IBankAccount acc1 = new BankAccount(1);
            IBankAccount acc2 = new BankAccount(2);

            // add acc1 to the datastore
            dataStore.Add(1, acc1);

            // state before act
            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            // act
            // try to remove acc2
            Action ac = () => bam.RemoveBankAccount(acc2);

            // assert
            ac.Should().
                Throw<InvalidOperationException>()
                .WithMessage("Bank account does not exist");

            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            repoMock.Verify(repo => repo.Remove(It.IsAny<IBankAccount>()), Times.Never);
        }

        [Fact]
        public void RemoveBankAccountIsNullExpectArgumentException()
        {
            // Arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create a bank account
            IBankAccount acc1 = new BankAccount(1);

            // add acc1 to the datastore
            dataStore.Add(1, acc1);

            // state before act
            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            // act
            // try to remove null account
            Action ac = () => bam.RemoveBankAccount(null);

            // assert
            ac.Should().
                Throw<ArgumentException>()
                .WithMessage("No Bank account to remove (null)");

            // datastore must stay unchanged
            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            repoMock.Verify(repo => repo.Remove(It.IsAny<IBankAccount>()), Times.Never);
        }

        [Fact]
        public void GetBankAccountByIdExistingBankAccount()
        {
            // arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create a bank account
            IBankAccount acc1 = new BankAccount(1);

            // and add it to the datastore
            dataStore.Add(1, acc1);

            // state before act
            dataStore.Should().HaveCount(1).And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            // act
            IBankAccount result = bam.GetBankAccountById(1);

            // assert
            result.Should().Be(acc1);

            // datastore stays unchanged
            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            repoMock.Verify(repo => repo.GetByID(It.Is<int>((id) => id == 1)), Times.Once);
        }

        [Fact]
        public void getBankAccountByIdNonExistingBankAccountExpectNull()
        {
            // arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create a bank account
            IBankAccount acc1 = new BankAccount(1);

            // and add it to the datastore
            dataStore.Add(1, acc1);

            // state before act
            dataStore.Should().HaveCount(1).And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            // act
            // look for non-existing account (accountNumber = 2)
            IBankAccount result = bam.GetBankAccountById(2);

            // assert
            result.Should().BeNull();

            // datastore stays unchanged
            dataStore.Should()
                .HaveCount(1)
                .And.Contain(new KeyValuePair<int, IBankAccount>(1, acc1));

            repoMock.Verify(repo => repo.GetByID(It.Is<int>((id) => id == 2)), Times.Once);
        }

        [Fact]
        public void GetAllBankAccounts()
        {
            // arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create two bank accounts
            IBankAccount acc1 = new BankAccount(1);
            IBankAccount acc2 = new BankAccount(2);

            // add them both to the datastore
            dataStore.Add(1, acc1);
            dataStore.Add(2, acc2);

            // state of datastore before act
            dataStore.Should()
                .HaveCount(2)
                .And.ContainValues(acc1, acc2);

            // act
            List<IBankAccount> result = bam.GetAllBankAccounts();

            // assert
            result.Should().HaveCount(2).And.ContainInOrder(acc1, acc2);

            // datastore stays unchanged
            dataStore.Should()
                .HaveCount(2)
                .And.ContainValues(acc1, acc2);

            repoMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Theory]
        [InlineData(123.45, 200, 123.45, 0, 323.45)]
        [InlineData(123.45, 0, 0, 123.45, 0)]
        [InlineData(123.45, 0, 0.01, 123.44, 0.01)]
        public void TransferAmount(
            double initialAmount1,
            double initialAmount2,
            double transferAmount,
            double expectedAmount1,
            double expectedAmount2)
        {
            // arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create two bank accounts
            IBankAccount acc1 = new BankAccount(1, initialAmount1);
            IBankAccount acc2 = new BankAccount(2, initialAmount2);
            // and add them to the data store
            dataStore.Add(1, acc1);
            dataStore.Add(2, acc2);

            // act
            bam.TransferAmount(acc1.AccountNumber, acc2.AccountNumber, transferAmount);

            // assert
            dataStore.Should().HaveCount(2).And.ContainValues(acc1, acc2);
            dataStore[1].Should().Match<IBankAccount>((acc) => acc.Balance == expectedAmount1);
            dataStore[2].Should().Match<IBankAccount>((acc) => acc.Balance == expectedAmount2);
        }

        [Theory]
        [InlineData(3, 2)]
        [InlineData(1, 3)]
        public void TransferAmountNonExistingAccountExpectArgumentException(int accNumber1, int accNumber2)
        {
            // arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create two accounts
            IBankAccount acc1 = new BankAccount(1);
            IBankAccount acc2 = new BankAccount(2);

            // add to datastore
            dataStore.Add(1, acc1);
            dataStore.Add(2, acc2);

            // act
            Action ac = () => bam.TransferAmount(accNumber1, accNumber2, 1234.55);

            // assert
            ac.Should().Throw<ArgumentException>().WithMessage("Non-Existing account number");
            dataStore[1].Should().Be(acc1);
            dataStore[2].Should().Be(acc2);
        }


        [Fact]
        public void TransferAmountNegativeAmountExpectArgumentException()
        {
            // arrange
            BankAccountManager bam = new BankAccountManager(repoMock.Object);

            // create two accounts
            IBankAccount acc1 = new BankAccount(1);
            IBankAccount acc2 = new BankAccount(2);

            // add to datastore
            dataStore.Add(1, acc1);
            dataStore.Add(2, acc2);

            // act
            Action ac = () => bam.TransferAmount(1, 2, -0.01);

            // assert
            
            /// throw ArgumentException
            ac.Should().Throw<ArgumentException>().WithMessage("Amount to transfer cannot be negative");
            
            // don't update the bankaccounts in the repo
            repoMock.Verify(repo => repo.Update(It.Is<IBankAccount>(acc => acc.AccountNumber == 1)), Times.Never);
            repoMock.Verify(repo => repo.Update(It.Is<IBankAccount>(acc => acc.AccountNumber == 2)), Times.Never);
        }
    }
}

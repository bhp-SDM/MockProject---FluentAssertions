using MockProject.Core.Interfaces;
using MockProject.Core.Model;
using System;
using Xunit;

namespace XUnitTestProject
{
    public class BankAccountTest
    {
        [Fact]
        public void CreateValidDefaultBankAccount()
        {
            // arrange
            int accNumber = 1;

            // Act
            IBankAccount acc = new BankAccount(accNumber);

            // assert
            Assert.Equal(accNumber, acc.AccountNumber);
            Assert.Equal(0.0, acc.Balance);
            Assert.Equal(BankAccount.DEFAULT_INTEREST_RATE, acc.InterestRate);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void CreateBankAccountInvalidAccountNumberExpectArgumentException(int accNumber)
        {
            // Arrange
            IBankAccount acc = null;

            // Act + Assert
            var ex = Assert.Throws<ArgumentException>(() => acc = new BankAccount(accNumber));

            Assert.Null(acc);
            Assert.Equal("Account number must be positive", ex.Message);
        }

        [Fact]
        public void CreateBankAccountWithValidInitialBalance()
        {
            int accNumber = 1;
            double initialBalance = 123.45;

            IBankAccount acc = new BankAccount(accNumber, initialBalance);

            Assert.Equal(accNumber, acc.AccountNumber);
            Assert.Equal(initialBalance, acc.Balance);
            Assert.Equal(BankAccount.DEFAULT_INTEREST_RATE, acc.InterestRate);
        }

        [Fact]
        public void CreateBankAccountWithInvalidInitialBalanceExpectArgumentException()
        {
            IBankAccount acc = null;
            double initialBalance = -0.01;

            var ex = Assert.Throws<ArgumentException>(() => acc = new BankAccount(1, initialBalance));

            Assert.Null(acc);
            Assert.Equal("Initial balance must be a positive amount", ex.Message);
        }

        [Theory]
        [InlineData(0, 0.00)]
        [InlineData(1000, 0.00)]
        [InlineData(0, 0.10)]
        [InlineData(1000, 0.10)]
        public void CreateBankAccountWithValidInitialBalanceAndInterestRate(double initialBalance, double interestRate)
        {
            IBankAccount acc = new BankAccount(1, initialBalance, interestRate);

            Assert.Equal(initialBalance, acc.Balance);
            Assert.Equal(interestRate, acc.InterestRate);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(0.11)]
        public void CreateBankAccountWithInvalidInterestRate(double initialInterestRate)
        {
            IBankAccount acc = null;

            var ex = Assert.Throws<ArgumentException>(() => acc = new BankAccount(1, 2, initialInterestRate));

            Assert.Null(acc);
            Assert.Equal("Interest rate must be between 0.00 - 0.10", ex.Message);
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(0.10)]
        public void SetValidInterestRate(double newInterestRate)
        {
            IBankAccount acc = new BankAccount(1);
            acc.InterestRate = newInterestRate;

            Assert.Equal(newInterestRate, acc.InterestRate);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(0.11)]
        public void SetInvalidInterestRateExpectArgumentException(double newInterestRate)
        {
            IBankAccount acc = new BankAccount(1, 2000, 0.03);
            double oldInterestRate = acc.InterestRate;

            var ex = Assert.Throws<ArgumentException>(() => acc.InterestRate = newInterestRate);

            Assert.Equal(oldInterestRate, acc.InterestRate);
            Assert.Equal("Interest rate must be between 0.00 - 0.10", ex.Message);
        }

        [Fact]
        public void DepositValidAmount()
        {
            double initialBalance = 1234.56;
            double amount = 123.45;

            IBankAccount acc = new BankAccount(1, initialBalance);

            acc.Deposit(amount);

            Assert.Equal(initialBalance + amount, acc.Balance);

        }

        [Fact]
        public void DepositInvalidAmountExpectArgumentException()
        {
            double initialBalance = 123.45;

            IBankAccount acc = new BankAccount(1, initialBalance);

            var ex = Assert.Throws<ArgumentException>(() => acc.Deposit(-0.01));

            Assert.Equal(initialBalance, acc.Balance);
            Assert.Equal("Amount to deposit cannot be negative", ex.Message);
        }

        [Theory]
        [InlineData(123.45, 0.00)]
        [InlineData(123.45, 100.00)]
        [InlineData(123.45, 123.45)]
        public void WithdrawValidAmount(double initialBalance, double amount)
        {
            IBankAccount acc = new BankAccount(1, initialBalance);

            acc.Withdraw(amount);

            Assert.Equal(initialBalance - amount, acc.Balance);
        }

        [Fact]
        public void WithdrawNegativeAmountExpectArgumentException()
        {
            double initialBalance = 123.45;

            IBankAccount acc = new BankAccount(1, initialBalance);

            var ex = Assert.Throws<ArgumentException>(() => acc.Withdraw(-0.01));

            Assert.Equal(initialBalance, acc.Balance);
            Assert.Equal("Amount to withdraw cannot be negative", ex.Message);
        }

        [Fact]
        public void WithdrawAmountExceedingBalanceExpectArgumentException()
        {
            double initialBalance = 123.45;

            IBankAccount acc = new BankAccount(1, initialBalance);

            var ex = Assert.Throws<ArgumentException>(() => acc.Withdraw(initialBalance + 0.01));

            Assert.Equal(initialBalance, acc.Balance);
            Assert.Equal("Amount to withdraw cannot exceed the balance", ex.Message);
        }

        [Fact]
        public void AddInterest()
        {
            IBankAccount acc = new BankAccount(1, 123.45);
            double expectedBalance = 123.45 * (1 + acc.InterestRate);

            acc.AddInterest();

            Assert.Equal(expectedBalance, acc.Balance);
        }

    }
}

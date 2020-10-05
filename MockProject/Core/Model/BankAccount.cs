using MockProject.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockProject.Core.Model
{
    public class BankAccount : IBankAccount
    {
        public const double DEFAULT_INTEREST_RATE = 0.01;

        private double _interestRate;

        public int AccountNumber { get; private set; }

        public double Balance { get; private set; }
        public double InterestRate
        {
            get { return _interestRate; }
            set
            {
                if (value < 0.00 || value > 0.10)
                    throw new ArgumentException("Interest rate must be between 0.00 - 0.10");
                _interestRate = value;
            }
        }


        public BankAccount(int accNumber)
        : this(accNumber, 0.00, DEFAULT_INTEREST_RATE)
        {

        }

        public BankAccount(int accNumber, double initialBalance)
        :this(accNumber, initialBalance, DEFAULT_INTEREST_RATE)
        {

        }

        public BankAccount(int accNumber,double initialBalance, double interestRate)
        {
            if (accNumber <= 0)
                throw new ArgumentException("Account number must be positive");
            if (initialBalance < 0)
                throw new ArgumentException("Initial balance must be a positive amount");
            AccountNumber = accNumber;
            Balance = initialBalance;
            InterestRate = interestRate;
        }
        public void AddInterest()
        {
            Balance += Balance * InterestRate;
        }

        public void Deposit(double amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount to deposit cannot be negative");
            Balance += amount;
        }

        public void Withdraw(double amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount to withdraw cannot be negative");
            if (amount > Balance)
                throw new ArgumentException("Amount to withdraw cannot exceed the balance");
            Balance -= amount;
        }
    }
}

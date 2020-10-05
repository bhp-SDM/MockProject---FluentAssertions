using System;
using System.Collections.Generic;
using System.Text;

namespace MockProject.Core.Interfaces
{
    public interface IBankAccount
    {
        int AccountNumber { get;}
        double Balance { get;}
        double InterestRate { get; set;}

        void Deposit(double amount);
        void Withdraw(double amount);

        void AddInterest();
    }
}

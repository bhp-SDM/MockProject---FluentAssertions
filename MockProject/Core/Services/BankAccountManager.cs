using MockProject.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockProject.Core.Services
{
    public class BankAccountManager
    {
        private IRepository<int, IBankAccount> accountsRepo;

        public BankAccountManager(IRepository<int, IBankAccount> repo)
        {
            this.accountsRepo = repo ?? throw new ArgumentException("Missing BankAccount Repository");
        }

        public void AddBankAccount(IBankAccount acc)
        {
            if (acc == null)
            {
                throw new ArgumentException("Bank account cannot be null");
            }
            if (accountsRepo.GetByID(acc.AccountNumber) != null)
            {
                throw new ArgumentException("Account number already in use");
            }

            accountsRepo.Add(acc);
        }

        public void RemoveBankAccount(IBankAccount acc)
        {
            // Bank account must be instance
            if (acc == null)
            {
                throw new ArgumentException("No Bank account to remove (null)");
            }
            // Bank account must exist
            if (accountsRepo.GetByID(acc.AccountNumber) == null)
            { 
                throw new InvalidOperationException("Bank account does not exist");
            }
            // and have a zero balance
            if (acc.Balance > 0)
            { 
                throw new InvalidOperationException("Bank account must be empty before removal");
            }
            accountsRepo.Remove(acc);
        }

        public IBankAccount GetBankAccountById(int accountNumber)
        {
            return accountsRepo.GetByID(accountNumber);
        }

        public List<IBankAccount> GetAllBankAccounts()
        {
            return accountsRepo.GetAll();
        }

        public void TransferAmount(int accountNumber1, int accountNumber2, double amount)
        {
            IBankAccount acc1 = accountsRepo.GetByID(accountNumber1);
            IBankAccount acc2 = accountsRepo.GetByID(accountNumber2);
            // accounts must exist
            if (acc1 == null || acc2 == null)
            {
                throw new ArgumentException("Non-Existing account number");
            }
            // amount to transfer must not be negative
            if (amount < 0)
            {
                throw new ArgumentException("Amount to transfer cannot be negative");
            }

            acc1.Withdraw(amount);
            acc2.Deposit(amount);
            accountsRepo.Update(acc1);
            accountsRepo.Update(acc2);
        }
    }
}

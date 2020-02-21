using System;
using System.Collections.Generic;
using System.Text;

namespace Minx.Aspects.Example.Service
{
    public class BankAccount : IBankAccount
    {
        public double Balance { get; private set; }

        public void Deposit(double amount)
        {
            Balance += amount;
        }

        public void Withdraw(double amount)
        {
            Balance -= amount;
        }
    }
}

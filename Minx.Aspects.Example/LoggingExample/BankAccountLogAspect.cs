using Castle.DynamicProxy;
using Minx.Aspects.Example.Service;
using System;

namespace Minx.Aspects.Example
{
    [Aspect(typeof(IBankAccount))]
    public class BankAccountLogAspect : IBankAccount
    {
        [AspectInvocation]
        public IInvocation Invocation { get; set; }

        public void Deposit(double amount)
        {
            Console.WriteLine($"Start - Deposit({amount})");

            Invocation.Proceed();

            Console.WriteLine($"End - Deposit({amount})");
        }

        public void Withdraw(double amount)
        {
            Console.WriteLine($"Start - Withdraw({amount})");

            Invocation.Proceed();

            Console.WriteLine($"End - Withdraw({amount})");
        }
    }
}

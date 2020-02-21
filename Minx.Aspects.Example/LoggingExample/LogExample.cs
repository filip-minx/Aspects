using Minx.Aspects.Example.Service;

namespace Minx.Aspects.Example
{
    public static class LogExample
    {
        public static void Run()
        {
            IBankAccount account = Aspects.AddAspects<IBankAccount>(new BankAccount());

            account.Deposit(1000);
            account.Withdraw(500);
        }
    }
}

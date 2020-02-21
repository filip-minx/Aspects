namespace Minx.Aspects.Example.Service
{
    public interface IBankAccount
    {
        void Withdraw(double amount);

        void Deposit(double amount);
    }
}

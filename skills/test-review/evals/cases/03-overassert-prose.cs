// AccountService.cs
public record Account(Guid Id, string Owner, decimal Balance, DateTime CreatedAt);

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Cannot withdraw {requested:C}: only {available:C} available in the account.")
    {
    }
}

public class AccountService
{
    private readonly Dictionary<Guid, Account> accounts = new();

    public Account Open(string owner, decimal initialDeposit)
    {
        var account = new Account(Guid.NewGuid(), owner, initialDeposit, DateTime.UtcNow);
        accounts[account.Id] = account;
        return account;
    }

    public Account Withdraw(Guid id, decimal amount)
    {
        var account = accounts[id];
        if (amount > account.Balance)
            throw new InsufficientFundsException(amount, account.Balance);
        var updated = account with { Balance = account.Balance - amount };
        accounts[id] = updated;
        return updated;
    }

    /// Owners are returned in no particular order.
    public IReadOnlyCollection<string> Owners() =>
        accounts.Values.Select(a => a.Owner).ToList();
}

// AccountServiceTests.cs
public class AccountServiceTests
{
    [Fact]
    public void Withdraw_ReducesBalance()
    {
        var service = new AccountService();
        var account = service.Open("Priya", 100m);

        var updated = service.Withdraw(account.Id, 30m);

        Assert.Equal(70m, updated.Balance);
        Assert.Equal("Priya", updated.Owner);
        Assert.Equal(account.Id, updated.Id);
        Assert.Equal(account.CreatedAt, updated.CreatedAt);
    }

    [Fact]
    public void Withdraw_OverdraftThrowsWithMessage()
    {
        var service = new AccountService();
        var account = service.Open("Priya", 50m);

        var exception = Assert.Throws<InsufficientFundsException>(
            () => service.Withdraw(account.Id, 80m));

        Assert.Equal(
            "Cannot withdraw $80.00: only $50.00 available in the account.",
            exception.Message);
    }

    [Fact]
    public void Owners_ListsEveryAccountHolder()
    {
        var service = new AccountService();
        service.Open("Priya", 10m);
        service.Open("Ahmed", 10m);

        var owners = service.Owners();

        Assert.Equal(new[] { "Priya", "Ahmed" }, owners);
    }

    [Fact]
    public void Open_ReturnsAccount()
    {
        var service = new AccountService();

        var account = service.Open("Priya", 25m);

        Assert.NotNull(account);
    }
}

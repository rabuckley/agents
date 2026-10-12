// Case 02 — review the public API of this library.

namespace Contoso.Billing;

public interface IInvoiceRepository
{
    Task<Invoice?> GetAsync(InvoiceId id, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
}

public sealed class SqlInvoiceRepository : IInvoiceRepository
{
    private readonly BillingDbContext _db;
    public SqlInvoiceRepository(BillingDbContext db) => _db = db;

    public Task<Invoice?> GetAsync(InvoiceId id, CancellationToken cancellationToken = default)
        => _db.Invoices.FindAsync(new object[] { id }, cancellationToken).AsTask();

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public interface IInvoiceService
{
    Task<Invoice?> GetInvoiceAsync(InvoiceId id, CancellationToken cancellationToken = default);
    Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
}

public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;
    public InvoiceService(IInvoiceRepository repository) => _repository = repository;

    public Task<Invoice?> GetInvoiceAsync(InvoiceId id, CancellationToken cancellationToken = default)
        => _repository.GetAsync(id, cancellationToken);

    public Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
        => _repository.AddAsync(invoice, cancellationToken);
}

public static class InvoiceServiceCollectionExtensions
{
    public static IServiceCollection AddInvoicing(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, SqlInvoiceRepository>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        return services;
    }
}

public readonly record struct InvoiceId(Guid Value);
public sealed class Invoice { public InvoiceId Id { get; init; } public decimal Total { get; init; } }

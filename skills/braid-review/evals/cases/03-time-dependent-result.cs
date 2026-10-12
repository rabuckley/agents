// Case 03 — review the design of this pricing module.

public static class PromotionEngine
{
    private static readonly List<Promotion> ActivePromotions = new();

    public static void Refresh(IEnumerable<Promotion> promotions)
    {
        ActivePromotions.Clear();
        ActivePromotions.AddRange(promotions);
    }

    public static decimal FinalPrice(Order order)
    {
        var price = order.Lines.Sum(line => line.UnitPrice * line.Quantity);

        foreach (var promotion in ActivePromotions)
        {
            if (promotion.StartsAt <= DateTime.Now && DateTime.Now < promotion.EndsAt
                && promotion.AppliesTo(order))
            {
                price -= promotion.Discount(price);
            }
        }

        if (order.Customer.Tier == CustomerTier.Gold && DateTime.Now.DayOfWeek == DayOfWeek.Friday)
        {
            price *= 0.95m;
        }

        return decimal.Round(price, 2);
    }
}

// checkout/CheckoutService.cs
public class CheckoutService
{
    public Receipt Complete(Order order)
    {
        var price = PromotionEngine.FinalPrice(order);
        var receipt = new Receipt(order.Id, price, issuedAt: DateTime.UtcNow);
        _receipts.Save(receipt);
        return receipt;
    }

    private readonly IReceiptStore _receipts;
    public CheckoutService(IReceiptStore receipts) => _receipts = receipts;
}

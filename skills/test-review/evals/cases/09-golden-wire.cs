// WebhookPayload.cs
//
// Partners consume this webhook body with hand-written parsers; the byte-exact
// JSON shape (field names, order, number formatting) is published in the
// partner API reference v2 and is a compatibility contract.
public static class WebhookSerializer
{
    public static string OrderShipped(string orderId, string carrier, decimal weightKg)
    {
        return "{"
            + "\"event\":\"order.shipped\","
            + "\"version\":2,"
            + $"\"order_id\":\"{orderId}\","
            + $"\"carrier\":\"{carrier}\","
            + $"\"weight_kg\":{weightKg.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}"
            + "}";
    }
}

// WebhookSerializerTests.cs
public class WebhookSerializerTests
{
    [Fact]
    public void OrderShipped_MatchesPublishedWireFormat()
    {
        var payload = WebhookSerializer.OrderShipped("ord-19", "DHL", 1.5m);

        Assert.Equal(
            "{\"event\":\"order.shipped\",\"version\":2,\"order_id\":\"ord-19\"," +
            "\"carrier\":\"DHL\",\"weight_kg\":1.5}",
            payload);
    }

    [Fact]
    public void OrderShipped_TrimsTrailingZerosFromWeight()
    {
        var payload = WebhookSerializer.OrderShipped("ord-20", "UPS", 2.000m);

        Assert.Equal(
            "{\"event\":\"order.shipped\",\"version\":2,\"order_id\":\"ord-20\"," +
            "\"carrier\":\"UPS\",\"weight_kg\":2}",
            payload);
    }
}

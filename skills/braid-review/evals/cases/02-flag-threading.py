# Case 02 — review the design of this change (a flag added end-to-end in one PR).

# api/handlers.py
def handle_quote_request(request, catalog, tax_tables):
    use_regional_rounding = request.headers.get("X-Region") in EU_REGIONS
    quote = build_quote(request.items, catalog, tax_tables, use_regional_rounding)
    return to_response(quote)


# quoting/builder.py
def build_quote(items, catalog, tax_tables, use_regional_rounding):
    lines = []
    for item in items:
        product = catalog.lookup(item.sku)
        lines.append(price_line(product, item.quantity, tax_tables, use_regional_rounding))
    return Quote(lines=lines, total=sum_lines(lines, use_regional_rounding))


# quoting/pricing.py
def price_line(product, quantity, tax_tables, use_regional_rounding):
    subtotal = product.unit_price * quantity
    tax = tax_tables.rate_for(product.category) * subtotal
    return QuoteLine(
        sku=product.sku,
        subtotal=round_amount(subtotal, use_regional_rounding),
        tax=round_amount(tax, use_regional_rounding),
    )


def sum_lines(lines, use_regional_rounding):
    return round_amount(sum(l.subtotal + l.tax for l in lines), use_regional_rounding)


# money/rounding.py
def round_amount(amount, use_regional_rounding):
    if use_regional_rounding:
        return round_half_even(amount, 2)
    return round_half_up(amount, 2)

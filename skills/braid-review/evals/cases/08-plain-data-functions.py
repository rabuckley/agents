# Case 08 — review the design of this order-total module.

from dataclasses import dataclass
from decimal import Decimal


@dataclass(frozen=True)
class LineItem:
    sku: str
    unit_price: Decimal
    quantity: int


@dataclass(frozen=True)
class Discount:
    code: str
    percent_off: Decimal  # 0-100


@dataclass(frozen=True)
class Order:
    items: tuple[LineItem, ...]
    discounts: tuple[Discount, ...]


def subtotal(order: Order) -> Decimal:
    return sum((item.unit_price * item.quantity for item in order.items), Decimal(0))


def discount_amount(order: Order) -> Decimal:
    base = subtotal(order)
    total_percent = min(sum(d.percent_off for d in order.discounts), Decimal(50))
    return (base * total_percent / 100).quantize(Decimal("0.01"))


def total(order: Order) -> Decimal:
    return subtotal(order) - discount_amount(order)


def add_item(order: Order, item: LineItem) -> Order:
    return Order(items=order.items + (item,), discounts=order.discounts)

# receipt.py

_WIDTH = 32


def _wrap_text(text: str, width: int) -> list[str]:
    words = text.split()
    lines: list[str] = []
    current = ""
    for word in words:
        candidate = f"{current} {word}".strip()
        if len(candidate) <= width:
            current = candidate
        else:
            lines.append(current)
            current = word
    if current:
        lines.append(current)
    return lines


def format_receipt(store_name: str, items: list[tuple[str, int]]) -> str:
    lines = _wrap_text(store_name, _WIDTH)
    lines.append("-" * _WIDTH)
    for name, cents in items:
        price = f"{cents / 100:.2f}"
        lines.append(f"{name[: _WIDTH - len(price) - 1]:<{_WIDTH - len(price)}}{price}")
    total = sum(cents for _, cents in items)
    lines.append("-" * _WIDTH)
    lines.append(f"{'TOTAL':<{_WIDTH - len(f'{total / 100:.2f}')}}{total / 100:.2f}")
    return "\n".join(lines)


# test_receipt.py
from receipt import _wrap_text, format_receipt


def test_wrap_text_short_line_stays_whole():
    assert _wrap_text("Corner Shop", 32) == ["Corner Shop"]


def test_wrap_text_splits_at_width():
    assert _wrap_text("The Very Long Store Name Emporium", 16) == [
        "The Very Long",
        "Store Name",
        "Emporium",
    ]


def test_wrap_text_empty_string():
    assert _wrap_text("", 32) == []


def test_receipt_totals_items():
    receipt = format_receipt("Corner Shop", [("Tea", 250), ("Milk", 130)])
    assert "TOTAL" in receipt
    assert "3.80" in receipt

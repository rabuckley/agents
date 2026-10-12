# Case 06 — review the design of this module.

# slugs.py
"""URL slug generation for article titles."""

import re
import unicodedata

MAX_SLUG_LENGTH = 80


def slugify(title: str) -> str:
    """Turn an article title into a URL-safe slug.

    Lowercases, strips accents, replaces runs of non-alphanumerics with a
    single hyphen, and truncates to MAX_SLUG_LENGTH at a hyphen boundary.
    """
    normalized = unicodedata.normalize("NFKD", title)
    ascii_only = normalized.encode("ascii", "ignore").decode("ascii")
    hyphenated = re.sub(r"[^a-z0-9]+", "-", ascii_only.lower()).strip("-")
    if len(hyphenated) <= MAX_SLUG_LENGTH:
        return hyphenated
    truncated = hyphenated[:MAX_SLUG_LENGTH]
    last_hyphen = truncated.rfind("-")
    if last_hyphen > 0:
        return truncated[:last_hyphen]
    return truncated


def ensure_unique(slug: str, taken: set[str]) -> str:
    """Append -2, -3, ... until the slug is not in `taken`."""
    if slug not in taken:
        return slug
    suffix = 2
    while f"{slug}-{suffix}" in taken:
        suffix += 1
    return f"{slug}-{suffix}"

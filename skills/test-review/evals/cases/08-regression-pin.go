// pricecache.go
package pricing

import "sync"

type PriceFetcher interface {
	CurrentPrice(sku string) int
}

// PriceCache caches prices and supports explicit invalidation when a
// merchant edits a price.
type PriceCache struct {
	mu      sync.Mutex
	fetcher PriceFetcher
	prices  map[string]int
}

func NewPriceCache(fetcher PriceFetcher) *PriceCache {
	return &PriceCache{fetcher: fetcher, prices: make(map[string]int)}
}

func (c *PriceCache) Price(sku string) int {
	c.mu.Lock()
	defer c.mu.Unlock()
	if price, ok := c.prices[sku]; ok {
		return price
	}
	price := c.fetcher.CurrentPrice(sku)
	c.prices[sku] = price
	return price
}

// Invalidate drops the cached entry so the next Price call refetches.
func (c *PriceCache) Invalidate(sku string) {
	c.mu.Lock()
	defer c.mu.Unlock()
	delete(c.prices, sku)
}

// pricecache_test.go
package pricing

import "testing"

type countingFetcher struct {
	price int
	calls int
}

func (f *countingFetcher) CurrentPrice(sku string) int {
	f.calls++
	return f.price
}

// Regression pin for issue #482: after a merchant price edit, Invalidate
// left the cached entry in place and customers saw the old price for up
// to an hour. Fixed in commit 9c41d7e. The refetched price may well equal
// the old one, so the returned value cannot detect a broken Invalidate —
// the fetch count is deliberately the observable here. If this test
// breaks, re-read issue #482 before changing it.
func TestInvalidate_ForcesRefetch(t *testing.T) {
	fetcher := &countingFetcher{price: 100}
	cache := NewPriceCache(fetcher)

	first := cache.Price("SKU-1")
	cached := cache.Price("SKU-1")
	cache.Invalidate("SKU-1")
	refreshed := cache.Price("SKU-1")

	if first != 100 || cached != 100 || refreshed != 100 {
		t.Errorf("prices = %d, %d, %d; want all 100", first, cached, refreshed)
	}
	if fetcher.calls != 2 {
		t.Errorf("fetcher called %d times, want 2 (once before Invalidate, once after)", fetcher.calls)
	}
}

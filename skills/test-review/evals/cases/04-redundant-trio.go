// cart.go
package cart

type Item struct {
	Name       string
	PriceCents int
	Quantity   int
}

type Cart struct {
	items []Item
}

func (c *Cart) Add(item Item) {
	c.items = append(c.items, item)
}

// TotalCents sums price times quantity across all items.
func (c *Cart) TotalCents() int {
	total := 0
	for _, item := range c.items {
		total += item.PriceCents * item.Quantity
	}
	return total
}

// cart_test.go
package cart

import "testing"

func TestTotalCents_EmptyCartIsZero(t *testing.T) {
	c := &Cart{}
	if got := c.TotalCents(); got != 0 {
		t.Errorf("TotalCents() = %d, want 0", got)
	}
}

func TestTotalCents_SingleItem(t *testing.T) {
	c := &Cart{}
	c.Add(Item{Name: "Tea", PriceCents: 250, Quantity: 1})
	if got := c.TotalCents(); got != 250 {
		t.Errorf("TotalCents() = %d, want 250", got)
	}
}

func TestTotalCents_TwoItems(t *testing.T) {
	c := &Cart{}
	c.Add(Item{Name: "Tea", PriceCents: 250, Quantity: 1})
	c.Add(Item{Name: "Milk", PriceCents: 130, Quantity: 1})
	if got := c.TotalCents(); got != 380 {
		t.Errorf("TotalCents() = %d, want 380", got)
	}
}

func TestTotalCents_MultipleItemsWithQuantities(t *testing.T) {
	c := &Cart{}
	c.Add(Item{Name: "Tea", PriceCents: 250, Quantity: 2})
	c.Add(Item{Name: "Milk", PriceCents: 130, Quantity: 3})
	c.Add(Item{Name: "Bread", PriceCents: 199, Quantity: 1})
	if got := c.TotalCents(); got != 1089 {
		t.Errorf("TotalCents() = %d, want 1089", got)
	}
}

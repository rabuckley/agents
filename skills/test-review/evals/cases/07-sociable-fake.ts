// wishlist.ts
export interface ProductCatalog {
  isDiscontinued(sku: string): boolean;
}

export interface WishlistRepository {
  itemsFor(customerId: string): string[];
  save(customerId: string, skus: string[]): void;
}

export class WishlistService {
  constructor(
    private readonly repository: WishlistRepository,
    private readonly catalog: ProductCatalog,
  ) {}

  add(customerId: string, sku: string): void {
    if (this.catalog.isDiscontinued(sku)) {
      throw new Error(`Cannot wishlist discontinued product ${sku}`);
    }
    const items = this.repository.itemsFor(customerId);
    if (!items.includes(sku)) {
      this.repository.save(customerId, [...items, sku]);
    }
  }

  items(customerId: string): string[] {
    return this.repository.itemsFor(customerId);
  }
}

// wishlist.test.ts
import { WishlistService, WishlistRepository, ProductCatalog } from "./wishlist";

class InMemoryWishlistRepository implements WishlistRepository {
  private store = new Map<string, string[]>();

  itemsFor(customerId: string): string[] {
    return this.store.get(customerId) ?? [];
  }

  save(customerId: string, skus: string[]): void {
    this.store.set(customerId, skus);
  }
}

const catalogWithDiscontinued = (discontinued: string[]): ProductCatalog => ({
  isDiscontinued: (sku) => discontinued.includes(sku),
});

describe("WishlistService", () => {
  it("added product appears in the customer's wishlist", () => {
    const service = new WishlistService(
      new InMemoryWishlistRepository(),
      catalogWithDiscontinued([]),
    );

    service.add("cust-1", "SKU-42");

    expect(service.items("cust-1")).toEqual(["SKU-42"]);
  });

  it("adding the same product twice keeps a single entry", () => {
    const service = new WishlistService(
      new InMemoryWishlistRepository(),
      catalogWithDiscontinued([]),
    );

    service.add("cust-1", "SKU-42");
    service.add("cust-1", "SKU-42");

    expect(service.items("cust-1")).toEqual(["SKU-42"]);
  });

  it("accumulates distinct products in the order they were added", () => {
    const service = new WishlistService(
      new InMemoryWishlistRepository(),
      catalogWithDiscontinued([]),
    );

    service.add("cust-1", "SKU-42");
    service.add("cust-1", "SKU-7");

    expect(service.items("cust-1")).toEqual(["SKU-42", "SKU-7"]);
  });

  it("keeps each customer's wishlist separate", () => {
    const repository = new InMemoryWishlistRepository();
    const service = new WishlistService(repository, catalogWithDiscontinued([]));

    service.add("cust-1", "SKU-42");
    service.add("cust-2", "SKU-7");

    expect(service.items("cust-1")).toEqual(["SKU-42"]);
    expect(service.items("cust-2")).toEqual(["SKU-7"]);
  });

  it("rejects discontinued products and leaves the wishlist unchanged", () => {
    const service = new WishlistService(
      new InMemoryWishlistRepository(),
      catalogWithDiscontinued(["SKU-99"]),
    );

    expect(() => service.add("cust-1", "SKU-99")).toThrow();
    expect(service.items("cust-1")).toEqual([]);
  });
});

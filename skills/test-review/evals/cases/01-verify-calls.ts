// checkout.ts
export interface Inventory {
  reserve(sku: string, quantity: number): void;
}

export interface Notifier {
  send(recipient: string, subject: string, body: string): void;
}

export interface Order {
  id: string;
  lines: { sku: string; quantity: number }[];
  status: "placed" | "failed";
  customerEmail: string;
}

let nextId = 0;

export function placeOrder(
  lines: { sku: string; quantity: number }[],
  customerEmail: string,
  inventory: Inventory,
  notifier: Notifier,
): Order {
  for (const line of lines) {
    inventory.reserve(line.sku, line.quantity);
  }
  const order: Order = {
    id: `ord-${nextId++}`,
    lines,
    status: "placed",
    customerEmail,
  };
  notifier.send(
    customerEmail,
    "Your order is confirmed",
    `Order ${order.id} with ${lines.length} item(s) is on its way.`,
  );
  return order;
}

// checkout.test.ts
import { placeOrder, Inventory, Notifier } from "./checkout";

describe("placeOrder", () => {
  it("reserves inventory and sends confirmation", () => {
    const inventory = { reserve: jest.fn() };
    const notifier = { send: jest.fn() };

    placeOrder(
      [
        { sku: "A-1", quantity: 2 },
        { sku: "B-9", quantity: 1 },
      ],
      "sam@example.com",
      inventory,
      notifier,
    );

    expect(inventory.reserve).toHaveBeenCalledTimes(2);
    expect(inventory.reserve).toHaveBeenNthCalledWith(1, "A-1", 2);
    expect(inventory.reserve).toHaveBeenNthCalledWith(2, "B-9", 1);
    expect(notifier.send).toHaveBeenCalledTimes(1);
    expect(notifier.send).toHaveBeenCalledWith(
      "sam@example.com",
      "Your order is confirmed",
      expect.stringContaining("2 item(s)"),
    );
  });
});

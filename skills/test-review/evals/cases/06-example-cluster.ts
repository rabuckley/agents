// discount.ts

/**
 * Volume discount: 0% below 10 units, 5% for 10-49, 10% for 50-99,
 * 15% for 100 or more. Result is rounded down to whole cents.
 */
export function discountedCents(unitCents: number, quantity: number): number {
  const gross = unitCents * quantity;
  let rate = 0;
  if (quantity >= 100) rate = 0.15;
  else if (quantity >= 50) rate = 0.1;
  else if (quantity >= 10) rate = 0.05;
  return Math.floor(gross * (1 - rate));
}

// discount.test.ts
import { discountedCents } from "./discount";

describe("discountedCents", () => {
  it("applies no discount below ten units", () => {
    expect(discountedCents(200, 9)).toBe(1800);
  });

  it("applies five percent at ten units", () => {
    expect(discountedCents(200, 10)).toBe(1900);
  });

  it("applies five percent at forty-nine units", () => {
    expect(discountedCents(200, 49)).toBe(9310);
  });

  it("applies ten percent at fifty units", () => {
    expect(discountedCents(200, 50)).toBe(9000);
  });

  it("applies ten percent at ninety-nine units", () => {
    expect(discountedCents(200, 99)).toBe(17820);
  });

  it("applies fifteen percent at one hundred units", () => {
    expect(discountedCents(200, 100)).toBe(17000);
  });

  it("applies fifteen percent at two hundred units", () => {
    expect(discountedCents(200, 200)).toBe(34000);
  });

  it("rounds down to whole cents", () => {
    expect(discountedCents(199, 10)).toBe(1890);
  });
});

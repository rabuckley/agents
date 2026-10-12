// Source: /Users/ryanbuckley/dev/dsegroup/apps-webs/apps/storefront/src/lib/i18n/format.ts and format.test.ts

export interface Money {
  readonly amount: number
  readonly currency: string
}

export type DatePreset = 'short' | 'medium' | 'long'

const DATE_PRESETS: Record<DatePreset, Intl.DateTimeFormatOptions> = {
  short: { dateStyle: 'short' },
  medium: { dateStyle: 'medium' },
  long: { dateStyle: 'long' },
}

// `Intl.*` constructors load ICU data; reuse instances across calls so that
// rendering e.g. a product grid doesn't rebuild a formatter per row. Keys are
// composed from closed enumerations (locale × currency, locale × preset, locale)
// so the keyspace is bounded per Workers isolate.
function memo<V>(cache: Map<string, V>, key: string, build: () => V): V {
  let value = cache.get(key)
  if (!value) {
    value = build()
    cache.set(key, value)
  }
  return value
}

const moneyFormatters = new Map<string, Intl.NumberFormat>()
const dateFormatters = new Map<string, Intl.DateTimeFormat>()

export function formatMoney(money: Money, locale: string): string {
  return memo(
    moneyFormatters,
    `${locale}\0${money.currency}`,
    () =>
      new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: money.currency,
      }),
  ).format(money.amount)
}

export function formatDate(value: Date | string | number, locale: string, preset: DatePreset = 'medium'): string {
  const date = value instanceof Date ? value : new Date(value)
  return memo(
    dateFormatters,
    `${locale}\0${preset}`,
    () => new Intl.DateTimeFormat(locale, DATE_PRESETS[preset]),
  ).format(date)
}

// ---- tests ----

import { describe, expect, it } from 'vitest'

describe('formatMoney', () => {
  it('renders GBP with two fraction digits in en-GB', () => {
    const out = formatMoney({ amount: 9.99, currency: 'GBP' }, 'en-gb')
    expect(out).toContain('£')
    expect(out).toContain('9.99')
  })

  it('renders EUR with a comma decimal mark and trailing symbol in fr-FR', () => {
    const out = formatMoney({ amount: 9.99, currency: 'EUR' }, 'fr-fr')
    expect(out).toContain('€')
    expect(out).toContain('9,99')
    // Symbol is placed after the number in French convention.
    expect(out.lastIndexOf('€')).toBeGreaterThan(out.indexOf('9'))
  })

  it('renders JPY with zero fraction digits', () => {
    const out = formatMoney({ amount: 1234, currency: 'JPY' }, 'en-us')
    expect(out).toContain('¥')
    expect(out).toContain('1,234')
    expect(out).not.toContain('.')
  })

  it('renders BHD with three fraction digits', () => {
    const out = formatMoney({ amount: 1.234, currency: 'BHD' }, 'en-gb')
    expect(out).toContain('1.234')
  })

  it('renders USD with a leading symbol in en-US', () => {
    const out = formatMoney({ amount: 12.34, currency: 'USD' }, 'en-us')
    expect(out.indexOf('$')).toBeLessThan(out.indexOf('1'))
    expect(out).toContain('12.34')
  })
})

describe('formatDate', () => {
  const date = new Date(Date.UTC(2025, 0, 15, 12, 0, 0))

  it('renders short dates in en-GB as day-first', () => {
    const out = formatDate(date, 'en-gb', 'short')
    // en-GB short is dd/mm/yy, so the day comes first. The literal year
    // digits `25` appear at the end; we only assert structure.
    expect(out.startsWith('15')).toBe(true)
  })

  it('renders short dates in en-US as month-first', () => {
    const out = formatDate(date, 'en-us', 'short')
    // en-US short is m/d/yy — month leads with `1` for January.
    expect(out.startsWith('1/15')).toBe(true)
  })

  it('renders medium dates with an abbreviated month in en-GB', () => {
    const out = formatDate(date, 'en-gb', 'medium')
    expect(out).toContain('Jan')
    expect(out).toContain('2025')
  })

  it('renders long dates with a full month name in fr-FR', () => {
    const out = formatDate(date, 'fr-fr', 'long')
    expect(out.toLowerCase()).toContain('janvier')
    expect(out).toContain('2025')
  })

  it('accepts epoch milliseconds and ISO strings', () => {
    const ms = formatDate(date.getTime(), 'en-gb', 'medium')
    const iso = formatDate(date.toISOString(), 'en-gb', 'medium')
    expect(ms).toBe(iso)
  })
})

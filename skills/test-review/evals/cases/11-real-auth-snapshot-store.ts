// Source: /Users/ryanbuckley/dev/dsegroup/apps-webs/packages/auth/src/shared/store.ts, types.ts (excerpt) and store.test.ts

export type AuthStatus = 'loading' | 'authenticated' | 'unauthenticated' | 'error'

export interface DseUser {
  readonly subject: string
  readonly name: string | undefined
  readonly email: string | undefined
  readonly roles: readonly string[]
  readonly claims: Readonly<Record<string, unknown>>
}

export interface AuthSnapshot {
  readonly status: AuthStatus
  readonly user: DseUser | null
}

export const LOADING_SNAPSHOT: AuthSnapshot = Object.freeze({ status: 'loading', user: null })
export const UNAUTHENTICATED_SNAPSHOT: AuthSnapshot = Object.freeze({ status: 'unauthenticated', user: null })

// A tiny observable snapshot holder for the auth adapter. It implements the
// `useSyncExternalStore` contract directly: `getSnapshot` returns a
// referentially-stable value that only changes when `setSnapshot` swaps in a
// genuinely different snapshot, and every swap notifies all subscribers. The
// BFF adapter drives one of these from its `/bff/user` seed, keeping the React
// provider a thin reader over it.

export interface SnapshotStore {
  readonly getSnapshot: () => AuthSnapshot
  readonly setSnapshot: (next: AuthSnapshot) => void
  readonly subscribe: (listener: () => void) => () => void
}

export function createSnapshotStore(initial: AuthSnapshot): SnapshotStore {
  let current = initial
  const listeners = new Set<() => void>()
  return {
    getSnapshot: () => current,
    // Compare by status and user identity so setting an equivalent snapshot
    // (e.g. re-seeding to the same shared constant) is a no-op that keeps the
    // cached reference, and React is only woken for real transitions.
    setSnapshot: (next) => {
      if (next.status === current.status && next.user === current.user) return
      current = next
      // Snapshot the listener set before notifying: a listener that re-enters
      // `subscribe`/unsubscribe (React can, mid-render) would otherwise mutate
      // the `Set` while we iterate it.
      const listenersToNotify = [...listeners]
      for (const listener of listenersToNotify) listener()
    },
    subscribe: (listener) => {
      listeners.add(listener)
      return () => {
        listeners.delete(listener)
      }
    },
  }
}

// ---- tests ----

import { describe, expect, it, vi } from 'vitest'

const AUTHENTICATED: AuthSnapshot = {
  status: 'authenticated',
  user: { subject: 'u', name: undefined, email: undefined, roles: [], claims: {} },
}

describe('createSnapshotStore', () => {
  it('treats an equivalent snapshot as a no-op (same reference, no notify)', () => {
    const store = createSnapshotStore(LOADING_SNAPSHOT)
    const listener = vi.fn()
    store.subscribe(listener)

    // A fresh object with the same status and user identity must not replace the
    // cached reference or wake subscribers — that is what keeps React quiet.
    store.setSnapshot({ status: 'loading', user: null })

    expect(store.getSnapshot()).toBe(LOADING_SNAPSHOT)
    expect(listener).not.toHaveBeenCalled()
  })

  it('notifies once on a real transition', () => {
    const store = createSnapshotStore(LOADING_SNAPSHOT)
    const listener = vi.fn()
    store.subscribe(listener)

    store.setSnapshot(UNAUTHENTICATED_SNAPSHOT)

    expect(store.getSnapshot()).toBe(UNAUTHENTICATED_SNAPSHOT)
    expect(listener).toHaveBeenCalledTimes(1)
  })

  it('stops notifying after unsubscribe', () => {
    const store = createSnapshotStore(LOADING_SNAPSHOT)
    const listener = vi.fn()
    const unsubscribe = store.subscribe(listener)

    unsubscribe()
    store.setSnapshot(UNAUTHENTICATED_SNAPSHOT)

    expect(listener).not.toHaveBeenCalled()
  })

  it('is safe when a listener unsubscribes itself during notification', () => {
    const store = createSnapshotStore(LOADING_SNAPSHOT)
    const order: string[] = []
    const unsubscribeSelf = store.subscribe(() => {
      order.push('self')
      unsubscribeSelf()
    })
    const other = vi.fn(() => {
      order.push('other')
    })
    store.subscribe(other)

    // Removing a listener mid-notify must not skip the still-subscribed `other`;
    // the store iterates a snapshot of the set rather than the live set.
    store.setSnapshot(UNAUTHENTICATED_SNAPSHOT)
    expect(order).toEqual(['self', 'other'])

    // The self-removed listener must not fire on the next change.
    store.setSnapshot(AUTHENTICATED)
    expect(order.filter((entry) => entry === 'self')).toHaveLength(1)
    expect(other).toHaveBeenCalledTimes(2)
  })
})

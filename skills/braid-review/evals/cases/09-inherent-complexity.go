// Case 09 — review the design of this payment-retry module.

package payments

import (
	"context"
	"errors"
	"math/rand"
	"time"
)

// Charge attempts a payment against the provider, retrying transient
// failures with exponential backoff and jitter. Every attempt carries the
// same idempotency key, so a retry after an ambiguous failure (timeout,
// connection reset) cannot double-charge: the provider replays the original
// outcome for a key it has already seen.
func Charge(ctx context.Context, provider Provider, req ChargeRequest) (Receipt, error) {
	key := req.IdempotencyKey
	backoff := 200 * time.Millisecond
	const maxAttempts = 5

	var lastErr error
	for attempt := 1; attempt <= maxAttempts; attempt++ {
		receipt, err := provider.Charge(ctx, req, key)
		if err == nil {
			return receipt, nil
		}

		var provErr *ProviderError
		if errors.As(err, &provErr) && !provErr.Transient {
			// Declines, invalid cards, fraud blocks: retrying cannot succeed
			// and hammering the provider risks penalties.
			return Receipt{}, err
		}
		lastErr = err

		if attempt == maxAttempts {
			break
		}
		// Full jitter: sleep a uniform random duration up to the current
		// backoff, doubling the cap each attempt. Uniform jitter avoids
		// synchronized retry storms across concurrent callers.
		sleep := time.Duration(rand.Int63n(int64(backoff)))
		select {
		case <-ctx.Done():
			return Receipt{}, ctx.Err()
		case <-time.After(sleep):
		}
		backoff *= 2
	}

	return Receipt{}, &AmbiguousOutcomeError{IdempotencyKey: key, Cause: lastErr}
}

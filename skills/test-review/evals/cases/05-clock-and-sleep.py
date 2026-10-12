# session.py
import random
import string
import threading
import time
from datetime import datetime, timedelta


class SessionStore:
    """Sessions expire thirty minutes after creation."""

    TTL = timedelta(minutes=30)

    def __init__(self):
        self._sessions: dict[str, datetime] = {}
        self._lock = threading.Lock()

    def create(self) -> str:
        token = "".join(random.choices(string.ascii_lowercase, k=16))
        with self._lock:
            self._sessions[token] = datetime.now() + self.TTL
        return token

    def is_active(self, token: str) -> bool:
        with self._lock:
            expiry = self._sessions.get(token)
        return expiry is not None and datetime.now() < expiry

    def sweep_expired_async(self) -> None:
        def sweep():
            with self._lock:
                now = datetime.now()
                self._sessions = {
                    token: expiry
                    for token, expiry in self._sessions.items()
                    if expiry > now
                }

        threading.Thread(target=sweep, daemon=True).start()


# test_session.py
import time
from datetime import datetime, timedelta

from session import SessionStore

store = SessionStore()


def test_new_session_is_active():
    token = store.create()
    assert store.is_active(token)


def test_expired_session_is_swept():
    token = store.create()
    store._sessions[token] = datetime.now() - timedelta(seconds=1)
    store.sweep_expired_async()
    time.sleep(0.2)
    assert not store.is_active(token)


def test_tokens_are_unique():
    tokens = {store.create() for _ in range(50)}
    assert len(tokens) == 50

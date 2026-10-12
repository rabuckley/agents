"""Score a doc-audit result tree against the before tree and the expected (target commit) tree.

Usage: score.py BEFORE_ROOT EXPECTED_ROOT RESULT_ROOT [files...]
Counts comment lines per file (///, //, -- , /* */) and checks named markers.
"""
import re, sys, os, subprocess

def comment_lines(text, ext):
    n = 0; block = False
    for raw in text.splitlines():
        s = raw.strip()
        if block:
            n += 1
            if '*/' in s: block = False
            continue
        if s.startswith('/*'):
            n += 1; block = '*/' not in s; continue
        if ext in ('.cs',) and (s.startswith('///') or s.startswith('//')): n += 1
        elif ext == '.sql' and s.startswith('--'): n += 1
        elif ext == '.md': n += 1 if s else 0
    return n

MARKERS = {
 # lies the target fixed (should be ABSENT after)
 'lie:validator proves lease bound': r'proves|proof the host started|startup guarantee',
 'lie:lease.lost=publisher overran': r'publisher overran the PublishTimeout',
 'lie:parked=exhausting attempts': r'parked as failed after exhausting the publish attempts',
 'lie:purged=unacknowledged destroyed': r"non-zero 'failed' rate is an unacknowledged event destroyed",
 'lie:skew-free by construction': r'skew-free by construction',
 # truths the target added (should be PRESENT after)
 'truth:cooperative/observe cancellation': r'observ(e|ing) (the )?(cancellation|token)|cooperative',
 'truth:no lease renewal/deadline check': r'not renew leases|no deadline check|lease renewal',
 'truth:claims count unattempted': r'unattempted|never reached publishing|even if publishing never reaches',
 # house style (target applies csharp-xml-docs)
 'style:Asynchronously summaries': r'<summary>Asynchronously',
 'style:empty cancellationToken tag': r'<param name="cancellationToken"></param>',
 'style:The default is': r'The default is',
 'style:token boilerplate remains': r'A token that can be used to request cancellation',
 'style:em dashes in comments': r'^\s*(///|//|--).*—',
}

def count_markers(text):
    return {k: len(re.findall(v, text, re.M)) for k, v in MARKERS.items()}

def read(root, f):
    p = os.path.join(root, f)
    return open(p, encoding='utf-8').read() if os.path.exists(p) else ''

def main():
    before, expected, result = sys.argv[1:4]
    allfiles = sorted(os.path.relpath(os.path.join(d,f),expected) for d,_,fs in os.walk(expected) for f in fs)
    files = allfiles if len(sys.argv)<=4 else ([f for f in allfiles if f.startswith('src/core/src')] if sys.argv[4]=='src' else sys.argv[4:])
    files = [f for f in files if os.path.exists(os.path.join(expected, f))]
    tb=te=tr=0
    print(f"{'file':60} {'before':>6} {'target':>6} {'result':>6}  tgt%  res%")
    for f in files:
        ext = os.path.splitext(f)[1]
        b, e, r = (comment_lines(read(x, f), ext) for x in (before, expected, result))
        tb+=b; te+=e; tr+=r
        print(f"{f[-60:]:60} {b:6} {e:6} {r:6} {100*e//max(b,1):4}% {100*r//max(b,1):4}%")
    print(f"{'TOTAL':60} {tb:6} {te:6} {tr:6} {100*te//max(tb,1):4}% {100*tr//max(tb,1):4}%")
    print()
    agg = {}
    for root, name in ((before,'before'),(expected,'target'),(result,'result')):
        text = '\n'.join(read(root, f) for f in files)
        agg[name] = count_markers(text)
    print(f"{'marker':45} {'before':>6} {'target':>6} {'result':>6}")
    for k in MARKERS:
        print(f"{k:45} {agg['before'][k]:6} {agg['target'][k]:6} {agg['result'][k]:6}")

if __name__ == '__main__': main()

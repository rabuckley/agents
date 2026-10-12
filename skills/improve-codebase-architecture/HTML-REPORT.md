# HTML Report Format

The architectural review is rendered as a single self-contained HTML file in the OS temp directory. Tailwind and Mermaid both come from CDNs. Use Mermaid for graph-shaped relationships (dependencies, call flow, sequences) and hand-built divs or inline SVG for anything it can't draw.

## Scaffold

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Architecture review for {{repo name}}</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <script type="module">
      import mermaid from "https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs";
      mermaid.initialize({ startOnLoad: true, theme: "neutral" });
    </script>
  </head>
  <body class="bg-stone-50 text-slate-900 font-sans">
    <main class="max-w-5xl mx-auto px-6 py-12 space-y-12">
      <header>...</header>
      <section id="candidates" class="space-y-10">...</section>
      <section id="top-recommendation">...</section>
    </main>
  </body>
</html>
```

## Header

Repo name, date, and one line on which areas were scanned, then straight into the candidates.

## Candidate card

Each candidate is one `<article>`:

- **Title**: short, names the change.
- **Badge**: recommendation strength (`Strong` = emerald, `Worth exploring` = amber, `Speculative` = slate).
- **Files**: monospaced list, `font-mono text-sm`.
- **Before / After diagram** (where a picture clarifies the change): two columns, side by side. Where the problem is the shape of an interface, a short code excerpt may show it better.
- **Problem**: what hurts, and the evidence you saw: commits that touched the same files together, a bug traced to the leak, `file:line`.
- **Solution**: what changes.
- **Wins**: bullets.
- **Cost**: migration effort, blast radius, tests rewritten.
- **ADR callout** (if applicable): one line in an amber-tinted box.

## Top recommendation section

Only when one candidate clearly leads. One larger card. Candidate name, one sentence on why, anchor link to its card. That's it.

## Tone

**Wins bullets** name a concrete gain: *"locality: bugs concentrate in one module"*, *"leverage: one interface, N call sites"*, *"locality: a pricing-rule change touches one module, not four call sites"*. Don't write *"easier to maintain"* or *"cleaner code"*; they don't say what changes.

No throat-clearing, no "it's worth noting that…". Where you're unsure, say what you didn't verify, once, where it matters. If a bullet could be cut, cut it.

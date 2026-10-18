# Upgrade Options — Specs

Assessment: 8 SDK-style projects (4 × net8.0 test projects, 4 × netstandard2.1 libraries), no incompatible packages, 5 minor source-incompatible API usages.

## Strategy

### Upgrade Strategy
Only 4 small net8.0 test projects need a TFM change; dependency graph is shallow.

| Value | Description |
|-------|-------------|
| All-at-Once | Upgrade all net8.0 projects together in a single pass, then validate. |
| Bottom-Up | Upgrade project tiers from leaves to roots, validating each tier. |
| **Top-Down** (selected) | Upgrade applications first, libraries later. |

# xUnit Test Architecture References

This folder contains focused reference material that supports the
`xunit-test-arch` skill.

Use these files when an agent needs narrower context than the main `SKILL.md`
provides, such as xUnit version-specific package selection, Microsoft Testing
Platform integration, async cancellation token patterns, or VSTest/coverlet
setup for legacy .NET 6/7 projects.

## Index

- [xunit-v3.md](./xunit-v3.md) — xUnit v3 packages, MTP integration, `TestContext.Current.CancellationToken`, and MTP filter syntax
- [xunit-v2.md](./xunit-v2.md) — xUnit v2 packages, VSTest integration, and `coverlet.collector` coverage setup

## Intent

These references are intentionally supplemental:

- `SKILL.md` remains the authoritative instruction set for the agent
- Files in this folder provide deeper background, external source links,
  and side-by-side comparisons between the two runner models
- Topic files are organized to mirror the framework split in `Directory.Build.props`


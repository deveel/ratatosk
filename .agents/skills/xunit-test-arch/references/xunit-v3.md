# xUnit v3 Reference

This reference supports the `xunit-test-arch` skill's guidance for .NET 8+
projects using xUnit v3 and the Microsoft Testing Platform (MTP).

## Official Resources

- xUnit v3 getting started: <https://xunit.net/docs/getting-started/v3/getting-started>
- `xunit.v3` NuGet package: <https://www.nuget.org/packages/xunit.v3>
- `xunit.runner.visualstudio` (v3) NuGet package: <https://www.nuget.org/packages/xunit.runner.visualstudio>
- Microsoft Testing Platform introduction: <https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro>
- `Microsoft.Testing.Extensions.CodeCoverage` NuGet package: <https://www.nuget.org/packages/Microsoft.Testing.Extensions.CodeCoverage>
- xUnit v3 `TestContext` API: <https://xunit.net/docs/v3-test-context>

## Package setup (.NET 8+)

```xml
<!-- In test/Directory.Build.props — under the net8+ condition -->
<ItemGroup Condition="'$(IsTestProject)' == 'true'
         and $([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net8.0'))">
    <PackageReference Include="xunit.v3" Version="1.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.*" />
    <PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" Version="17.*" />
</ItemGroup>

<PropertyGroup Condition="'$(IsTestProject)' == 'true'
             and $([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net8.0'))">
    <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
    <TestingPlatformCaptureOutput>false</TestingPlatformCaptureOutput>
</PropertyGroup>
```

**Do NOT add `coverlet.collector` to .NET 8+ xUnit v3 projects.** The
`coverlet.collector` is a VSTest data collector and is incompatible with the
MTP runner model. Coverage for MTP projects is handled exclusively by
`Microsoft.Testing.Extensions.CodeCoverage`.

**Do NOT add `Microsoft.NET.Test.Sdk`** to .NET 8+ projects — it conflicts
with the MTP host and will break the build.

## MTP command-line (`--` divider)

With `TestingPlatformDotnetTestSupport=true`, `dotnet test` passes arguments
after the `--` separator directly to the MTP runner binary:

```bash
# Run all tests
dotnet test

# Filter by xUnit trait (Category)
dotnet test -- --filter "Trait[Category]=Unit"

# Filter by feature trait
dotnet test -- --filter "Trait[Feature]=OrderProcessing"

# Filter by class name (substring)
dotnet test -- --filter "FullyQualifiedName~OrderServiceTests"

# List discovered tests without running
dotnet test -- --list-tests

# Coverage + filter
dotnet test --coverage --coverage-output ./coverage \
            --coverage-output-format cobertura \
            -- --filter "Trait[Category]=Unit"

# Diagnostic log
dotnet test -- --output-diag ./test-diag.log
```

Arguments **before** `--` are consumed by `dotnet test` / MSBuild (e.g.
`--configuration`, `--no-build`, `--coverage`, `--coverage-output`).
Arguments **after** `--` are forwarded verbatim to the MTP runner.

## `TestContext.Current.CancellationToken` in async tests

xUnit v3 exposes a per-test cancellation token through `TestContext.Current.CancellationToken`.
Always forward this token into every awaited API call that accepts a
`CancellationToken`:

```csharp
[Fact]
public async Task Should_ReturnOrder_When_OrderExistsAsync()
{
    // Always capture the runner token at the start of the test method
    var cancellationToken = TestContext.Current.CancellationToken;

    var order = _fixture.BuildValidOrder();
    await _fixture.Repository.SaveAsync(order, cancellationToken);

    var result = await _fixture.Repository.GetByIdAsync(order.Id, cancellationToken);

    Assert.NotNull(result);
    Assert.Equal(order.Id, result.Id);
}
```

**Why this matters:** When the MTP runner cancels the test run (timeout, Ctrl+C,
or test abort), omitting the token leaves async operations running in the
background. The test process then hangs until the OS kills it, producing
confusing partial results or timeouts in CI.

Rules:
- Capture `TestContext.Current.CancellationToken` at the top of every async test method
- Pass it through every `await` call that accepts a `CancellationToken`
- Do not use `CancellationToken.None` in test methods — always prefer the runner token
- `TestContext` is **xUnit v3-only** — do not use it in xUnit v2 projects

## Key differences from xUnit v2

| Concern | xUnit v3 (.NET 8+) | xUnit v2 (.NET 6/7) |
|---------|-------------------|---------------------|
| Package | `xunit.v3` | `xunit` |
| Visual Studio runner | `xunit.runner.visualstudio` v3 | `xunit.runner.visualstudio` v2 |
| Test runner host | Microsoft Testing Platform | VSTest (`Microsoft.NET.Test.Sdk`) |
| Coverage | `Microsoft.Testing.Extensions.CodeCoverage` | `coverlet.collector` |
| Filter syntax | `dotnet test -- --filter "Trait[Category]=Unit"` | `dotnet test --filter "Category=Unit"` |
| Async cancellation | `TestContext.Current.CancellationToken` | Not available |
| MTP properties | `TestingPlatformDotnetTestSupport=true` | Not applicable |


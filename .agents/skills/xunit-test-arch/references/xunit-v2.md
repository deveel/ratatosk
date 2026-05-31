# xUnit v2 Reference

This reference supports the `xunit-test-arch` skill's legacy guidance for
.NET 6/7 projects using xUnit v2 and the VSTest runner model. These patterns
apply **only** when upgrading to .NET 8+ is not possible.

## Official Resources

- xUnit v2 getting started: <https://xunit.net/docs/getting-started/v2/getting-started>
- `xunit` NuGet package: <https://www.nuget.org/packages/xunit>
- `xunit.runner.visualstudio` (v2) NuGet package: <https://www.nuget.org/packages/xunit.runner.visualstudio>
- `Microsoft.NET.Test.Sdk` NuGet package: <https://www.nuget.org/packages/Microsoft.NET.Test.Sdk>
- `coverlet.collector` NuGet package: <https://www.nuget.org/packages/coverlet.collector>

## Package setup (.NET 6/7)

```xml
<!-- In test/Directory.Build.props — under the net6/7 condition -->
<ItemGroup Condition="'$(IsTestProject)' == 'true'
         and !$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net8.0'))">
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="coverlet.collector" Version="6.*">
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        <PrivateAssets>all</PrivateAssets>
    </PackageReference>
</ItemGroup>
```

**Do NOT add `TestingPlatformDotnetTestSupport`** to .NET 6/7 projects — MTP
is not supported on those target frameworks.

**Do NOT add `Microsoft.Testing.Extensions.CodeCoverage`** to .NET 6/7 projects
— use `coverlet.collector` with the `XPlat Code Coverage` data collector instead.

## VSTest command-line (no `--` divider)

VSTest-based projects use `dotnet test` flags **directly**, without the `--`
divider used by MTP. All filter arguments are passed before any `--`:

```bash
# Run all tests
dotnet test

# Filter by trait (VSTest style — no "--" divider)
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Feature=OrderProcessing"

# Collect coverage with Coverlet
dotnet test --collect:"XPlat Code Coverage" \
            --settings test/coverlet.runsettings \
            --results-directory ./coverage
```

## Coverage with `coverlet.runsettings`

Place a `coverlet.runsettings` file in the `test/` folder to configure
exclusions and format:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName="XPlat Code Coverage">
                <Configuration>
                    <Format>cobertura</Format>
                    <Exclude>[*.XUnit]*,[*.Testing]*,[*.Migrations]*</Exclude>
                    <ExcludeByAttribute>GeneratedCodeAttribute,ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
                    <ExcludeByFile>**/Program.cs,**/Migrations/**</ExcludeByFile>
                    <SingleHit>false</SingleHit>
                    <IncludeTestAssembly>false</IncludeTestAssembly>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>
```

## Async tests in xUnit v2

xUnit v2 does **not** have `TestContext.Current.CancellationToken`. Async test
methods in v2 simply return `Task` without cooperative cancellation support:

```csharp
[Fact]
public async Task Should_ReturnOrder_When_OrderExistsAsync()
{
    // xUnit v2 — no runner CancellationToken available
    var result = await _fixture.Repository.GetByIdAsync(orderId);
    Assert.NotNull(result);
}
```

If cooperative cancellation is needed in v2, wire a `CancellationTokenSource`
manually in the fixture. When upgrading to xUnit v3, replace it with
`TestContext.Current.CancellationToken`.

## Key differences from xUnit v3

| Concern | xUnit v2 (.NET 6/7) | xUnit v3 (.NET 8+) |
|---------|---------------------|-------------------|
| Package | `xunit` | `xunit.v3` |
| Visual Studio runner | `xunit.runner.visualstudio` v2 | `xunit.runner.visualstudio` v3 |
| Test runner host | VSTest (`Microsoft.NET.Test.Sdk`) | Microsoft Testing Platform |
| Coverage | `coverlet.collector` | `Microsoft.Testing.Extensions.CodeCoverage` |
| Filter syntax | `dotnet test --filter "Category=Unit"` | `dotnet test -- --filter "Trait[Category]=Unit"` |
| Async cancellation | Not available | `TestContext.Current.CancellationToken` |
| MTP properties | Not applicable | `TestingPlatformDotnetTestSupport=true` |


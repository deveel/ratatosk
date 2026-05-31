using System.ComponentModel.DataAnnotations;
using Deveel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Kista;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "EntityFrameworkSenderStore")]
public class EntityFrameworkSenderStoreTests
{
    private static DbContextOptions<SenderDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<SenderDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }

    private static DbSender CreateEntity(string id, string name, EndpointType endpointType = EndpointType.PhoneNumber, string address = "+1234567890", bool isActive = true)
    {
        var entity = new DbSender
        {
            Id = id,
            Name = name,
            DisplayName = name,
            Address = address,
            Type = endpointType.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        if (isActive)
            entity.Activate();
        else
            entity.Deactivate();
            
        return entity;
    }

    private static IServiceProvider CreateServices(Action<DbContextOptionsBuilder> optionsAction)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<SenderDbContext>(optionsAction);
        services.AddScoped<ISenderRepository<DbSender>, EntitySenderRepository>();
        services.AddScoped<SenderManager<DbSender>>(sp =>
        {
            var repo = sp.GetRequiredService<ISenderRepository<DbSender>>();
            return new SenderManager<DbSender>(repo, services: sp);
        });
        services.AddSingleton<ISenderCache>(sp => new Mock<ISenderCache>().Object);
        services.AddScoped<ISenderResolver, SenderResolver<DbSender>>();
        return services.BuildServiceProvider();
    }

    private static async Task SeedAsync(IServiceProvider services, params DbSender[] entities)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SenderDbContext>();
        context.Senders.AddRange(entities);
        await context.SaveChangesAsync();
    }

    // ── Registration Tests ──────────────────────────────────────────────────────

    [Fact]
    public void Should_RegisterSenderDbContext()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("register-context"));
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetService<SenderDbContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public void Should_RegisterSenderRepository()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("register-repo"));
        using var scope = services.CreateScope();

        var repository = scope.ServiceProvider.GetService<ISenderRepository<DbSender>>();
        Assert.NotNull(repository);
        Assert.IsType<EntitySenderRepository>(repository);
    }

    [Fact]
    public void Should_RegisterSenderManager()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("register-manager"));
        using var scope = services.CreateScope();

        var manager = scope.ServiceProvider.GetService<SenderManager<DbSender>>();
        Assert.NotNull(manager);
    }

    [Fact]
    public void Should_RegisterSenderResolver()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("register-resolver"));
        using var scope = services.CreateScope();

        var resolver = scope.ServiceProvider.GetService<ISenderResolver>();
        Assert.NotNull(resolver);
        Assert.IsType<SenderResolver<DbSender>>(resolver);
    }

    [Fact]
    public void Should_RegisterEntityCache()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("register-cache"));
        using var scope = services.CreateScope();

        var cache = scope.ServiceProvider.GetService<ISenderCache>();
        Assert.NotNull(cache);
    }

    // ── Repository Query Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task Repository_FindByNameAsync_ShouldReturnSender_WhenExists()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("query-find-by-name"));
        await SeedAsync(services, CreateEntity("name-1", "unique-name"));

        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISenderRepository<DbSender>>();
        var result = await repository.FindByNameAsync("unique-name");

        Assert.NotNull(result);
        Assert.Equal("unique-name", result.Name);
    }

    [Fact]
    public async Task Repository_FindByNameAsync_ShouldReturnNull_WhenNotFound()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("query-name-missing"));
        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISenderRepository<DbSender>>();

        var result = await repository.FindByNameAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task Repository_FindByEndpointAsync_ShouldReturnSender_WhenMatch()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("query-find-by-endpoint"));
        await SeedAsync(services, CreateEntity("ep-1", "email-sender", EndpointType.EmailAddress, "test@example.com"));

        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISenderRepository<DbSender>>();
        var result = await repository.FindByEndpointAsync("test@example.com", EndpointType.EmailAddress);

        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);
    }

    [Fact]
    public async Task Repository_FindByEndpointAsync_ShouldReturnNull_WhenNoMatch()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("query-endpoint-missing"));
        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISenderRepository<DbSender>>();

        var result = await repository.FindByEndpointAsync("missing@example.com", EndpointType.EmailAddress);

        Assert.Null(result);
    }

    [Fact]
    public async Task Repository_GetAllActiveAsync_ShouldReturnOnlyActiveSenders()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("query-get-all-active"));
        await SeedAsync(services,
            CreateEntity("active-1", "active-sender-1", isActive: true),
            CreateEntity("active-2", "active-sender-2", isActive: true),
            CreateEntity("inactive-1", "inactive-sender", isActive: false)
        );

        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISenderRepository<DbSender>>();
        var results = await repository.GetAllActiveAsync();

        Assert.Equal(2, results.Count);
        Assert.All(results, s => Assert.True(s.IsActive));
    }

    [Fact]
    public async Task Repository_SetActiveAsync_ShouldActivateSender()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("query-set-active"));
        var sender = CreateEntity("activate-1", "to-activate", isActive: false);
        await SeedAsync(services, sender);

        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISenderRepository<DbSender>>();
        var found = await repository.FindByNameAsync("to-activate");
        Assert.NotNull(found);
        Assert.False(found.IsActive);

        await repository.SetActiveAsync(found, true);
        Assert.True(found.IsActive);
        Assert.NotNull(found.UpdatedAt);
    }

    [Fact]
    public async Task Repository_SetActiveAsync_ShouldDeactivateSender()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("query-set-inactive"));
        var sender = CreateEntity("deactivate-1", "to-deactivate", isActive: true);
        await SeedAsync(services, sender);

        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISenderRepository<DbSender>>();
        var found = await repository.FindByNameAsync("to-deactivate");
        Assert.NotNull(found);
        Assert.True(found.IsActive);

        await repository.SetActiveAsync(found, false);
        Assert.False(found.IsActive);
        Assert.NotNull(found.UpdatedAt);
    }

    // ── SenderManager Tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task Manager_FindByNameAsync_ShouldReturnSuccess_WhenSenderExists()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("manager-find-by-name"));
        await SeedAsync(services, CreateEntity("mgr-1", "managed-sender"));

        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
        var result = await manager.FindByNameAsync("managed-sender");

        Assert.True(result.IsSuccess());
        Assert.Equal("managed-sender", result.Value!.Name);
    }

    [Fact]
    public async Task Manager_FindByNameAsync_ShouldReturnFail_WhenSenderNotFound()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("manager-name-missing"));
        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();

        var result = await manager.FindByNameAsync("nonexistent");

        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task Manager_FindByEndpointAsync_ShouldReturnSuccess_WhenMatch()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("manager-find-endpoint"));
        await SeedAsync(services, CreateEntity("mgr-ep-1", "endpoint-sender", EndpointType.EmailAddress, "mgr@example.com"));

        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
        var result = await manager.FindByEndpointAsync("mgr@example.com", EndpointType.EmailAddress);

        Assert.True(result.IsSuccess());
        Assert.Equal("endpoint-sender", result.Value!.Name);
    }

    [Fact]
    public async Task Manager_GetAllActiveAsync_ShouldReturnActiveSenders()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("manager-get-active"));
        await SeedAsync(services,
            CreateEntity("mgr-active-1", "active-1", isActive: true),
            CreateEntity("mgr-active-2", "active-2", isActive: true),
            CreateEntity("mgr-inactive-1", "inactive-1", isActive: false)
        );

        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
        var result = await manager.GetAllActiveAsync();

        Assert.True(result.IsSuccess());
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task Manager_ActivateAsync_ShouldActivateSender()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("manager-activate"));
        await SeedAsync(services, CreateEntity("mgr-activate-1", "to-activate", isActive: false));

        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
        var result = await manager.ActivateAsync("mgr-activate-1");

        Assert.True(result.IsSuccess());

        await using var verifyScope = services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<SenderDbContext>();
        var updated = await verifyContext.Senders.FindAsync("mgr-activate-1");
        Assert.NotNull(updated);
        Assert.True(updated.IsActive);
    }

    [Fact]
    public async Task Manager_DeactivateAsync_ShouldDeactivateSender()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("manager-deactivate"));
        await SeedAsync(services, CreateEntity("mgr-deactivate-1", "to-deactivate", isActive: true));

        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
        var result = await manager.DeactivateAsync("mgr-deactivate-1");

        Assert.True(result.IsSuccess());

        await using var verifyScope = services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<SenderDbContext>();
        var updated = await verifyContext.Senders.FindAsync("mgr-deactivate-1");
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Manager_ActivateAsync_ShouldReturnFail_WhenSenderNotFound()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("manager-activate-missing"));
        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();

        var result = await manager.ActivateAsync("nonexistent");

        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderNotFound, result.Error?.Code);
    }

    // ── Resolver Integration Tests ──────────────────────────────────────────────

    [Fact]
    public async Task Resolver_ShouldResolveSender_WhenBackedByEntityFramework()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("resolver-ef-integration"));
        await SeedAsync(services, CreateEntity("res-1", "resolved-sender", EndpointType.EmailAddress, "resolved@example.com"));

        await using var scope = services.CreateAsyncScope();
        var resolver = scope.ServiceProvider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("resolved-sender"), new ConnectionSettings()));

        Assert.NotNull(result);
        Assert.Equal("resolved-sender", result.Name);
    }

    [Fact]
    public async Task Resolver_ShouldReturnNull_WhenSenderInactive()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("resolver-inactive-ef"));
        await SeedAsync(services, CreateEntity("res-inactive-1", "inactive-sender", isActive: false));

        await using var scope = services.CreateAsyncScope();
        var resolver = scope.ServiceProvider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("inactive-sender"), new ConnectionSettings()));

        Assert.Null(result);
    }

    [Fact]
    public async Task Resolver_ShouldResolveByConcreteSender_WhenBackedByEntityFramework()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("resolver-concrete-ef"));
        await SeedAsync(services, CreateEntity("res-concrete-1", "concrete-sender", EndpointType.EmailAddress, "concrete@example.com"));

        await using var scope = services.CreateAsyncScope();
        var resolver = scope.ServiceProvider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new EmailSender("concrete@example.com"), new ConnectionSettings()));

        Assert.NotNull(result);
        Assert.Equal("concrete-sender", result.Name);
    }

    [Fact]
    public async Task Resolver_ShouldResolveByEndpoint_WhenBackedByEntityFramework()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("resolver-endpoint-ef"));
        await SeedAsync(services, CreateEntity("res-ep-1", "endpoint-sender", EndpointType.PhoneNumber, "+15551234567"));

        await using var scope = services.CreateAsyncScope();
        var resolver = scope.ServiceProvider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new PhoneSender("+15551234567"), new ConnectionSettings()));

        Assert.NotNull(result);
        Assert.Equal("endpoint-sender", result.Name);
    }

    // ── End-to-End Lifecycle Tests ──────────────────────────────────────────────

    [Fact]
    public async Task EndToEnd_ResolveAfterCreate_ShouldWork()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("e2e-create-resolve"));
        
        // Seed sender directly via DbContext
        await SeedAsync(services, CreateEntity("e2e-resolve-1", "resolvable-sender", EndpointType.EmailAddress, "resolvable@example.com"));

        // Resolve by name
        await using (var resolveScope1 = services.CreateAsyncScope())
        {
            var resolver = resolveScope1.ServiceProvider.GetRequiredService<ISenderResolver>();
            var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("resolvable-sender"), new ConnectionSettings()));
            Assert.NotNull(result);
            Assert.Equal("resolvable-sender", result.Name);
        }

        // Resolve by email
        await using (var resolveScope2 = services.CreateAsyncScope())
        {
            var resolver = resolveScope2.ServiceProvider.GetRequiredService<ISenderResolver>();
            var result = await resolver.ResolveAsync(new SenderResolutionContext(new EmailSender("resolvable@example.com"), new ConnectionSettings()));
            Assert.NotNull(result);
            Assert.Equal("resolvable-sender", result.Name);
        }
    }

    [Fact]
    public async Task EndToEnd_UpdateAndResolve_ShouldReflectChanges()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("e2e-update-resolve"));
        
        // Seed sender
        await SeedAsync(services, CreateEntity("e2e-update-1", "update-sender", EndpointType.EmailAddress, "old@example.com"));

        // Update sender via DbContext
        await using (var updateScope = services.CreateAsyncScope())
        {
            var context = updateScope.ServiceProvider.GetRequiredService<SenderDbContext>();
            var sender = await context.Senders.FindAsync("e2e-update-1");
            Assert.NotNull(sender);
            
            sender.Address = "new@example.com";
            sender.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        // Resolve with new address
        await using (var resolveScope1 = services.CreateAsyncScope())
        {
            var resolver = resolveScope1.ServiceProvider.GetRequiredService<ISenderResolver>();
            var result = await resolver.ResolveAsync(new SenderResolutionContext(new EmailSender("new@example.com"), new ConnectionSettings()));
            Assert.NotNull(result);
            Assert.Equal("update-sender", result.Name);
        }
    }

    [Fact]
    public async Task EndToEnd_ActivateDeactivateAndResolve_ShouldWork()
    {
        var services = CreateServices(opt => opt.UseInMemoryDatabase("e2e-activate-resolve"));
        
        // Seed inactive sender
        await SeedAsync(services, CreateEntity("e2e-activate-1", "inactive-sender", EndpointType.EmailAddress, "inactive@example.com", isActive: false));

        // Should not resolve when inactive
        await using (var resolveScope1 = services.CreateAsyncScope())
        {
            var resolver = resolveScope1.ServiceProvider.GetRequiredService<ISenderResolver>();
            var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("inactive-sender"), new ConnectionSettings()));
            Assert.Null(result);
        }

        // Activate via Manager
        await using (var activateScope = services.CreateAsyncScope())
        {
            var manager = activateScope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
            var result = await manager.ActivateAsync("e2e-activate-1");
            Assert.True(result.IsSuccess());
        }

        // Should resolve when active
        await using (var resolveScope2 = services.CreateAsyncScope())
        {
            var resolver = resolveScope2.ServiceProvider.GetRequiredService<ISenderResolver>();
            var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("inactive-sender"), new ConnectionSettings()));
            Assert.NotNull(result);
            Assert.Equal("inactive-sender", result.Name);
        }

        // Deactivate via Manager
        await using (var deactivateScope = services.CreateAsyncScope())
        {
            var manager = deactivateScope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
            var result = await manager.DeactivateAsync("e2e-activate-1");
            Assert.True(result.IsSuccess());
        }
    }

    // ── Entity Framework Store Direct Registration Tests ─────────────────────

    [Fact]
    public void UseEntityFramework_ShouldRegisterSenderDbContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-register-context"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        var provider = services.BuildServiceProvider();
        var context = provider.GetService<SenderDbContext>();

        Assert.NotNull(context);
    }

    [Fact]
    public void UseEntityFramework_ShouldRegisterSenderRepository()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-register-repo"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetService<ISenderRepository<DbSender>>();

        Assert.NotNull(repository);
        Assert.IsType<EntitySenderRepository>(repository);
    }

    [Fact]
    public void UseEntityFramework_ShouldRegisterSenderManager()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-register-manager"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetService<SenderManager<DbSender>>();

        Assert.NotNull(manager);
    }

    [Fact]
    public void UseEntityFramework_ShouldRegisterSenderResolver()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-register-resolver"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetService<ISenderResolver>();

        Assert.NotNull(resolver);
        Assert.IsType<SenderResolver<DbSender>>(resolver);
    }

    [Fact]
    public async Task UseEntityFramework_ShouldResolveSender_When_SenderExists()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-resolve-sender"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        // Seed sender
        await using (var seedScope = services.BuildServiceProvider().CreateAsyncScope())
        {
            var context = seedScope.ServiceProvider.GetRequiredService<SenderDbContext>();
            context.Senders.Add(CreateEntity("ef-resolve-1", "ef-sender", EndpointType.EmailAddress, "ef@example.com"));
            await context.SaveChangesAsync();
        }

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("ef-sender"), new ConnectionSettings()));

        Assert.NotNull(result);
        Assert.Equal("ef-sender", result.Name);
        Assert.Equal("ef@example.com", result.Address);
    }

    [Fact]
    public async Task UseEntityFramework_ShouldReturnNull_When_SenderNotFound()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-not-found"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("nonexistent"), new ConnectionSettings()));

        Assert.Null(result);
    }

    [Fact]
    public async Task UseEntityFramework_ShouldResolveByEndpoint_When_SenderExists()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-resolve-endpoint"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        // Seed sender
        await using (var seedScope = services.BuildServiceProvider().CreateAsyncScope())
        {
            var context = seedScope.ServiceProvider.GetRequiredService<SenderDbContext>();
            context.Senders.Add(CreateEntity("ef-ep-1", "endpoint-sender", EndpointType.EmailAddress, "endpoint@example.com"));
            await context.SaveChangesAsync();
        }

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new EmailSender("endpoint@example.com"), new ConnectionSettings()));

        Assert.NotNull(result);
        Assert.Equal("endpoint-sender", result.Name);
    }

    [Fact]
    public async Task UseEntityFramework_ShouldNotResolveInactiveSender()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-inactive"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        // Seed inactive sender
        await using (var seedScope = services.BuildServiceProvider().CreateAsyncScope())
        {
            var context = seedScope.ServiceProvider.GetRequiredService<SenderDbContext>();
            context.Senders.Add(CreateEntity("ef-inactive-1", "inactive-sender", isActive: false));
            await context.SaveChangesAsync();
        }

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<ISenderResolver>();
        var result = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("inactive-sender"), new ConnectionSettings()));

        Assert.Null(result);
    }

    [Fact]
    public async Task UseEntityFramework_ShouldSupportFullLifecycle()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-lifecycle"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        var provider = services.BuildServiceProvider();

        // Seed inactive sender
        await using (var seedScope = provider.CreateAsyncScope())
        {
            var context = seedScope.ServiceProvider.GetRequiredService<SenderDbContext>();
            context.Senders.Add(CreateEntity("ef-lifecycle-1", "lifecycle-sender", EndpointType.EmailAddress, "lifecycle@example.com", isActive: false));
            await context.SaveChangesAsync();
        }

        // Should not resolve when inactive
        await using (var scope1 = provider.CreateAsyncScope())
        {
            var resolver1 = scope1.ServiceProvider.GetRequiredService<ISenderResolver>();
            var result1 = await resolver1.ResolveAsync(new SenderResolutionContext(new SenderRef("lifecycle-sender"), new ConnectionSettings()));
            Assert.Null(result1);
        }

        // Activate via Manager
        await using (var activateScope = provider.CreateAsyncScope())
        {
            var manager = activateScope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
            var activateResult = await manager.ActivateAsync("ef-lifecycle-1");
            Assert.True(activateResult.IsSuccess());
        }

        // Should resolve when active
        await using (var scope2 = provider.CreateAsyncScope())
        {
            var resolver2 = scope2.ServiceProvider.GetRequiredService<ISenderResolver>();
            var result2 = await resolver2.ResolveAsync(new SenderResolutionContext(new SenderRef("lifecycle-sender"), new ConnectionSettings()));
            Assert.NotNull(result2);
            Assert.Equal("lifecycle-sender", result2.Name);
        }

        // Deactivate via Manager
        await using (var deactivateScope = provider.CreateAsyncScope())
        {
            var manager = deactivateScope.ServiceProvider.GetRequiredService<SenderManager<DbSender>>();
            var deactivateResult = await manager.DeactivateAsync("ef-lifecycle-1");
            Assert.True(deactivateResult.IsSuccess());
        }
    }

    [Fact]
    public async Task UseEntityFramework_ShouldSupportMultipleConnectors()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessaging().AddSenders<DbSender>();
        services.AddDbContext<SenderDbContext>(opt => opt.UseInMemoryDatabase("test-multi-1"));
        services.AddRepositoryContext()
            .AddRepository<EntitySenderRepository>();

        // Seed senders for both connectors
        await using (var seedScope = services.BuildServiceProvider().CreateAsyncScope())
        {
            var context = seedScope.ServiceProvider.GetRequiredService<SenderDbContext>();
            context.Senders.Add(CreateEntity("multi-1", "sender-1", EndpointType.EmailAddress, "sender1@example.com"));
            context.Senders.Add(CreateEntity("multi-2", "sender-2", EndpointType.EmailAddress, "sender2@example.com"));
            await context.SaveChangesAsync();
        }

        var provider = services.BuildServiceProvider();
        
        // Both connectors should share the same SenderDbContext (single registration)
        var resolver = provider.GetRequiredService<ISenderResolver>();
        var result1 = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("sender-1"), new ConnectionSettings()));
        var result2 = await resolver.ResolveAsync(new SenderResolutionContext(new SenderRef("sender-2"), new ConnectionSettings()));

        Assert.NotNull(result1);
        Assert.NotNull(result2);
    }

    // ── Test Connector Types ──────────────────────────────────────────────────

    [ChannelSchema(typeof(TestEfSchemaFactory))]
    private class TestEfConnector : IChannelConnector
    {
        public TestEfConnector(IChannelSchema schema, ConnectionSettings? settings = null)
        {
            Schema = schema;
            ConnectionSettings = settings ?? new ConnectionSettings();
        }

        public IChannelSchema Schema { get; }
        public ConnectionSettings ConnectionSettings { get; }
        public ConnectorState State => ConnectorState.Uninitialized;

        public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken ct) => new(OperationResult<bool>.Success(true));
        public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => new(OperationResult<bool>.Success(true));
        public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotSupportedException();
        public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotSupportedException();
        public ValueTask ShutdownAsync(CancellationToken ct) => default;
    }

    [ChannelSchema(typeof(AnotherTestEfSchemaFactory))]
    private class AnotherTestEfConnector : IChannelConnector
    {
        public AnotherTestEfConnector(IChannelSchema schema, ConnectionSettings? settings = null)
        {
            Schema = schema;
            ConnectionSettings = settings ?? new ConnectionSettings();
        }

        public IChannelSchema Schema { get; }
        public ConnectionSettings ConnectionSettings { get; }
        public ConnectorState State => ConnectorState.Uninitialized;

        public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken ct) => new(OperationResult<bool>.Success(true));
        public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => new(OperationResult<bool>.Success(true));
        public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotSupportedException();
        public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
        public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotSupportedException();
        public ValueTask ShutdownAsync(CancellationToken ct) => default;
    }

    private class TestEfSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema() => new DummyEfSchema("TestEf", "TestEf");
    }

    private class AnotherTestEfSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema() => new DummyEfSchema("AnotherTestEf", "AnotherTestEf");
    }

    private class DummyEfSchema : IChannelSchema
    {
        public DummyEfSchema(string channelProvider, string channelType)
        {
            ChannelProvider = channelProvider;
            ChannelType = channelType;
        }
        public string ChannelProvider { get; }
        public string ChannelType { get; }
        public string Version => "1.0";
        public string? DisplayName => null;
        public bool IsStrict => false;
        public ChannelCapability Capabilities => ChannelCapability.SendMessages;
        public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => new List<ChannelEndpointConfiguration>();
        public IReadOnlyList<ChannelParameter> Parameters => new List<ChannelParameter>();
        public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => new List<MessagePropertyConfiguration>();
        public IReadOnlyList<MessageContentType> ContentTypes => new List<MessageContentType>();
        public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => new List<AuthenticationConfiguration>();
    }
}

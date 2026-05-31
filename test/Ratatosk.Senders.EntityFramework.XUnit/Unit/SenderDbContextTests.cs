using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "SenderDbContext")]
public class SenderDbContextTests
{
    private static DbContextOptions<SenderDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<SenderDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }

    [Fact]
    public void Should_CreateDatabase()
    {
        var options = CreateOptions("test-create");
        using var context = new SenderDbContext(options);

        Assert.NotNull(context);
        Assert.NotNull(context.Senders);
    }

    [Fact]
    public void Should_AddAndRetrieveSender()
    {
        var options = CreateOptions("test-add-retrieve");
        var entity = new DbSender
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test-sender",
            DisplayName = "Test Sender",
            Address = "test@example.com",
            Type = EndpointType.EmailAddress.ToString()
        };
        entity.Activate();

        using (var context = new SenderDbContext(options))
        {
            context.Senders.Add(entity);
            context.SaveChanges();
        }

        using (var context = new SenderDbContext(options))
        {
            var retrieved = context.Senders.Find(entity.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("test-sender", retrieved.Name);
            Assert.Equal("test@example.com", retrieved.Address);
        }
    }

    [Fact]
    public void Should_HaveUniqueNameIndex_Configured()
    {
        var options = CreateOptions("test-unique-name");
        using var context = new SenderDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(DbSender));
        Assert.NotNull(entityType);

        var nameIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(DbSender.Name)));

        Assert.NotNull(nameIndex);
        Assert.True(nameIndex.IsUnique);
    }

    [Fact]
    public void Should_MapEntityToSendersTable()
    {
        var options = CreateOptions("test-table-name");
        using var context = new SenderDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(DbSender));
        Assert.NotNull(entityType);
        Assert.Equal("senders", entityType.GetTableName());
    }

    [Fact]
    public void Should_ConfigureStringProperties()
    {
        var options = CreateOptions("test-properties");
        using var context = new SenderDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(DbSender));
        Assert.NotNull(entityType);

        var idProp = entityType.FindProperty(nameof(DbSender.Id));
        Assert.NotNull(idProp);
        Assert.Equal(50, idProp.GetMaxLength());

        var nameProp = entityType.FindProperty(nameof(DbSender.Name));
        Assert.NotNull(nameProp);
        Assert.Equal(100, nameProp.GetMaxLength());

        var addressProp = entityType.FindProperty(nameof(DbSender.Address));
        Assert.NotNull(addressProp);
        Assert.Equal(500, addressProp.GetMaxLength());
    }

    [Fact]
    public void Should_HaveIsActiveDefaultValue()
    {
        var options = CreateOptions("test-active-default");
        using var context = new SenderDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(DbSender));
        Assert.NotNull(entityType);

        var isActiveProp = entityType.FindProperty(nameof(DbSender.IsActive));
        Assert.NotNull(isActiveProp);
        Assert.NotNull(isActiveProp.GetDefaultValue());
    }
}

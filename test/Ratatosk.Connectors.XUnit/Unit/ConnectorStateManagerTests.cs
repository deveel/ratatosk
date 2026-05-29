namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "ConnectorLifecycle")]
public class ConnectorStateManagerTests
{
    [Fact]
    public void Should_StartInUninitializedState()
    {
        var manager = new ConnectorStateManager();
        Assert.Equal(ConnectorState.Uninitialized, manager.Current);
    }

    [Fact]
    public void Should_TransitionToNewState()
    {
        var manager = new ConnectorStateManager();
        manager.TransitionTo(ConnectorState.Ready);
        Assert.Equal(ConnectorState.Ready, manager.Current);
    }

    [Theory]
    [InlineData(ConnectorState.Ready)]
    [InlineData(ConnectorState.Error)]
    public void Should_BeOperational_When_InOperationalStates(ConnectorState state)
    {
        var manager = new ConnectorStateManager();
        manager.TransitionTo(state);
        manager.EnsureOperational();
    }

    [Theory]
    [InlineData(ConnectorState.Uninitialized)]
    [InlineData(ConnectorState.Initializing)]
    [InlineData(ConnectorState.ShuttingDown)]
    [InlineData(ConnectorState.Shutdown)]
    public void Should_Throw_When_NotOperational(ConnectorState state)
    {
        var manager = new ConnectorStateManager();
        manager.TransitionTo(state);
        Assert.Throws<MessagingException>(() => manager.EnsureOperational());
    }
}

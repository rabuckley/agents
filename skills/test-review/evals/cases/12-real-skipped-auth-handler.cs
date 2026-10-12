// Source: /Users/ryanbuckley/dev/dsegroup/code/src/identity — src/DSE.Identity.Domain/Commands/Users/GetClientUserCommandHandler.cs, src/DSE.Identity.Domain/Commands/Users/GetClientUserCommand.cs, test/DSE.Identity.Domain.Tests/Commands/Users/GetClientUserCommandHandlerTests.cs

// GetClientUserCommand.cs
public sealed class GetClientUserCommand : Command
{
    /// <summary>
    /// The id of the user. If <see cref="UserId"/> and <see cref="LoginName"/> are <see langword="null"/>
    /// then the data for the current user is returned.
    /// </summary>
    public ClientIdentifier? UserId { get; init; }

    public string? LoginName { get; init; }
}

// IClientUserDocumentStore.cs (member used here)
public interface IClientUserDocumentStore
{
    ValueTask<ClientUser?> GetByIdAsync(
        ClientIdentifier id,
        bool filterForDeleted = false,
        CancellationToken cancellationToken = default);
}

// GetClientUserCommandHandler.cs (logging and login-name lookup elided)
public sealed partial class GetClientUserCommandHandler
    : AsyncValueResultCommandHandler<GetClientUserCommand, ClientUser>,
      IGetClientUserCommandHandler
{
    public GetClientUserCommandHandler(
        IClientUserDocumentStore documentStore,
        ILogger<GetClientUserCommandHandler> logger)
        : base(logger)
    {
        DocumentStore = documentStore;
    }

    public IClientUserDocumentStore DocumentStore { get; }

    protected override async Task DoExecuteAsync(
        GetClientUserCommand command,
        ValueResultBuilder<ClientUser> builder,
        CancellationToken cancellationToken = default)
    {
        ClientUser? user = null;

        if (command.UserId is not null)
        {
            user = await DocumentStore
                .GetByIdAsync(command.UserId.Value, false, cancellationToken)
                .ConfigureAwait(false);
        }

        if (user is null)
        {
            builder.Status = ResultStatus.NotFound;
            builder.Notifications.Add(
                Notification.Error("IDNT001001", "No client user found."));
            return;
        }

        builder.Value = user;
    }
}

// GetClientUserCommandHandlerTests.cs
public sealed class GetClientUserCommandHandlerTests(ITestOutputHelper output) : LoggedTestsBase(output)
{
    internal string[] ValidIssuers { get; } = [Guid.NewGuid().ToString()];

    public ClientUser CreateClientUser()
    {
        return ClientUser.Create(
        new Documents.Users.EmailAddress(DSE.Core.Values.EmailAddress.Parse("test@dsegroup.org", null)),
        new Secret(BinaryValue.GetRandomValue())).Value!;
    }

    public GetClientUserCommandHandler CreateHandler(IEnumerable<Claim> claims, ClientUser clientUser)
    {
        var clientUserDocumentStoreMock = new Mock<IClientUserDocumentStore>();

        _ = clientUserDocumentStoreMock.Setup(s => s.GetByIdAsync(clientUser.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(clientUser.ToValueTask()!);

        var handler = new GetClientUserCommandHandler(
            clientUserDocumentStoreMock.Object,
            LoggerFactory.CreateLogger<GetClientUserCommandHandler>());

        return handler;
    }

    [Fact]
    public async Task GivenUnknownUserIdReturnsNotFoundResult()
    {
        var userId = ClientIdentifier.New();
        var clientUserDocumentStore = new Mock<IClientUserDocumentStore>();

        _ = clientUserDocumentStore
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<ClientUser?>(null));

        var handler = new GetClientUserCommandHandler(
            clientUserDocumentStore.Object,
            LoggerFactory.CreateLogger<GetClientUserCommandHandler>());

        var result = await handler.ExecuteAsync(
            new GetClientUserCommand
            {
                UserId = userId,
                IdempotencyKey = IdempotencyKey.New(),
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
        Assert.Contains(result.Notifications, notification => notification.Code == "IDNT001001");
    }

    [Fact(Skip = "TODO: rework authorization")]
    public async Task UserWithoutRequiredScopesThrowsUnauthorizedException()
    {
        var clientUser = CreateClientUser();

        var claims = new[]
        {
            new Claim(IdentityClaimsTypes.Subject, clientUser.Id.ToString(), null, ValidIssuers[0])
        };

        var handler = CreateHandler(claims, clientUser);

        var command = new GetClientUserCommand()
        {
            IdempotencyKey = IdempotencyKey.New()
        };

        _ = await Assert.ThrowsAsync<UnauthorizedException>(() => handler.ExecuteAsync(command, TestContext.Current.CancellationToken));
    }

    [Fact(Skip = "TODO: rework authorization")]

    public async Task UserWithoutRequiredScopesThrowsUnauthorizedException2()
    {
        var clientUser = CreateClientUser();

        var claims = new[]
        {
            new Claim(IdentityClaimsTypes.Subject, clientUser.Id.ToString(), null, ValidIssuers[0]),
            new Claim("scope", ApiScopes.Identity.PreferencesRead, null, ValidIssuers[0])
        };

        var handler = CreateHandler(claims, clientUser);

        var command = new GetClientUserCommand()
        {
            IdempotencyKey = IdempotencyKey.New()
        };

        _ = await Assert.ThrowsAsync<UnauthorizedException>(() => handler.ExecuteAsync(command, TestContext.Current.CancellationToken));
    }

    [Fact(Skip = "TODO: rework authorization")]

    public async Task GivenNoIdOrLoginNameReturnsClientUserForCurrentUser()
    {
        var clientUser = CreateClientUser();

        var claims = new[]
        {
            new Claim(IdentityClaimsTypes.Subject, clientUser.Id.ToString(), null, ValidIssuers[0]),
            new Claim("scope", ApiScopes.Identity.UserRead, null, ValidIssuers[0])
        };

        var handler = CreateHandler(claims, clientUser);

        var command = new GetClientUserCommand()
        {
            IdempotencyKey = IdempotencyKey.New()
        };

        var result = await handler.ExecuteAsync(command, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.HasValueAndNoErrorNotifications());
        Assert.Equal(clientUser, result.Value);
    }
}

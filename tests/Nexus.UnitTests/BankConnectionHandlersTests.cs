using Nexus.Application.Banking.Commands;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Interfaces;
using Nexus.UnitTests.Fakes;
using Xunit;

namespace Nexus.UnitTests;

public class BankConnectionHandlersTests
{
    private readonly FakeBankConnectionRepository _connectionRepo = new();
    private readonly FakeConnectedBankAccountRepository _accountRepo = new();
    private readonly FakeBankAccountRepository _internalAccountRepo = new();
    private readonly FakeBankTransactionRepository _txRepo = new();
    private readonly FakeStatementImportRepository _importRepo = new();
    private readonly FakeEnableBankingClient _client = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task StartBankAuthSession_ShouldCreatePendingConnectionAndReturnUrl()
    {
        var handler = new StartBankAuthSessionCommandHandler(_client, _connectionRepo, _uow);
        var command = new StartBankAuthSessionCommand(_userId, "bbva_es", "http://localhost:5173/banking/callback");

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Contains("bbva_es", result.AuthorizationUrl);
        Assert.Equal("eb_sess_test_123", result.SessionId);

        var conn = await _connectionRepo.GetByIdAsync(result.ConnectionId);
        Assert.NotNull(conn);
        Assert.Equal(_userId, conn.UserId);
        Assert.Equal(BankConnectionStatus.PendingAuthorization, conn.Status);
    }

    [Fact]
    public async Task CompleteBankAuthCallback_ShouldActivateConnectionAndDiscoverAccounts()
    {
        var startHandler = new StartBankAuthSessionCommandHandler(_client, _connectionRepo, _uow);
        var startResult = await startHandler.HandleAsync(new StartBankAuthSessionCommand(_userId, "ing_es", null));

        var callbackHandler = new CompleteBankAuthCallbackCommandHandler(
            _client,
            _connectionRepo,
            _accountRepo,
            _internalAccountRepo,
            _uow
        );

        var callbackResult = await callbackHandler.HandleAsync(
            new CompleteBankAuthCallbackCommand(_userId, "code_xyz", startResult.SessionId)
        );

        Assert.NotNull(callbackResult);
        Assert.Equal("Active", callbackResult.Status);
        Assert.Single(callbackResult.Accounts);
        Assert.Equal("Cuenta Online", callbackResult.Accounts[0].AccountName);
        Assert.Equal(1500m, callbackResult.Accounts[0].CurrentBalance);

        // Verifica que se creó la cuenta interna en Nexus
        var internalAccounts = await _internalAccountRepo.GetByUserIdAsync(_userId);
        Assert.Single(internalAccounts);
        Assert.Equal("ES9121000418450200051332", internalAccounts[0].Iban);
    }

    [Fact]
    public async Task SyncBankConnection_ShouldDownloadTransactionsAndSkipDuplicates()
    {
        // 1. Setup conexión y cuenta interna
        var connection = new BankConnection
        {
            UserId = _userId,
            InstitutionCode = "bbva_es",
            InstitutionName = "BBVA",
            Status = BankConnectionStatus.Active
        };
        await _connectionRepo.AddAsync(connection);

        var internalAccount = new BankAccount
        {
            UserId = _userId,
            Name = "Cuenta BBVA",
            Currency = "EUR",
            CurrentBalance = 1000m
        };
        await _internalAccountRepo.AddAsync(internalAccount);

        var connectedAccount = new ConnectedBankAccount
        {
            BankConnectionId = connection.Id,
            ExternalAccountId = "ext_bbva_1",
            AccountName = "Cuenta BBVA Online",
            InternalBankAccountId = internalAccount.Id
        };
        connection.Accounts.Add(connectedAccount);
        await _accountRepo.AddAsync(connectedAccount);

        // 2. Prepara transacciones en el cliente mock
        _client.TransactionsToReturn = new List<RawBankTransaction>
        {
            new("REF-1", DateTime.UtcNow, null, -15.50m, "EUR", "CAFETERIA OCHOA", null, null, null),
            new("REF-2", DateTime.UtcNow, null, -80.00m, "EUR", "ZARA COMPRA", null, null, null)
        };

        var handler = new SyncBankConnectionCommandHandler(
            _connectionRepo,
            _accountRepo,
            _internalAccountRepo,
            _txRepo,
            _importRepo,
            _client,
            _uow
        );

        // Primer sync: 2 nuevas transacciones
        var result1 = await handler.HandleAsync(new SyncBankConnectionCommand(_userId, connection.Id));
        Assert.NotNull(result1);
        Assert.Equal(2, result1.NewTransactionsImported);
        Assert.Equal(2, _txRepo.Transactions.Count);

        // Segundo sync con las mismas transacciones: debe omitir duplicados
        var result2 = await handler.HandleAsync(new SyncBankConnectionCommand(_userId, connection.Id));
        Assert.NotNull(result2);
        Assert.Equal(0, result2.NewTransactionsImported);
        Assert.Equal(2, _txRepo.Transactions.Count); // No se duplica ninguna
    }
}

using Nexus.Application.Rules.Commands.ApplyRulesRetroactive;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.UnitTests.Fakes;
using Xunit;

namespace Nexus.UnitTests;

public class ApplyRulesRetroactiveCommandHandlerTests
{
    private readonly FakeBankTransactionRepository _txRepo;
    private readonly FakeCategorizationRuleRepository _ruleRepo;
    private readonly FakeCategoryRepository _catRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly ApplyRulesRetroactiveCommandHandler _handler;

    public ApplyRulesRetroactiveCommandHandlerTests()
    {
        _txRepo = new FakeBankTransactionRepository();
        _ruleRepo = new FakeCategorizationRuleRepository();
        _catRepo = new FakeCategoryRepository();
        _uow = new FakeUnitOfWork();

        _handler = new ApplyRulesRetroactiveCommandHandler(
            _txRepo,
            _ruleRepo,
            _catRepo,
            _uow
        );
    }

    [Fact]
    public async Task HandleAsync_ShouldNeverOverrideManualTransactions()
    {
        var userId = Guid.NewGuid();
        var manualCatId = Guid.NewGuid();
        var ruleCatId = Guid.NewGuid();

        var manualTx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            Description = "COMPRA CABIFY",
            CleanMerchantName = "Cabify",
            IsManual = true,
            CategoryId = manualCatId,
            CategorizationSource = CategorizationSource.Manual
        };

        var rule = new CategorizationRule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Pattern = "CABIFY",
            NormalizedPattern = "CABIFY",
            TargetCategoryId = ruleCatId,
            Priority = 100,
            IsActive = true
        };

        _ruleRepo.Rules[rule.Id] = rule;
        _txRepo.Transactions[manualTx.Id] = manualTx;

        var result = await _handler.HandleAsync(new ApplyRulesRetroactiveCommand(userId));

        Assert.Equal(0, result.TotalUpdated);
        Assert.Equal(manualCatId, manualTx.CategoryId);
        Assert.Equal(CategorizationSource.Manual, manualTx.CategorizationSource);
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyRuleToAutomaticTransactions()
    {
        var userId = Guid.NewGuid();
        var ruleCatId = Guid.NewGuid();

        var autoTx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            Description = "PAGO TPV CABIFY MADRID",
            CleanMerchantName = null,
            IsManual = false,
            CategoryId = null,
            CategorizationSource = CategorizationSource.None
        };

        var rule = new CategorizationRule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Pattern = "CABIFY",
            NormalizedPattern = "CABIFY",
            TargetCategoryId = ruleCatId,
            Priority = 100,
            IsActive = true
        };

        _ruleRepo.Rules[rule.Id] = rule;
        _txRepo.Transactions[autoTx.Id] = autoTx;

        var result = await _handler.HandleAsync(new ApplyRulesRetroactiveCommand(userId));

        Assert.Equal(1, result.TotalUpdated);
        Assert.Equal(ruleCatId, autoTx.CategoryId);
        Assert.Equal(CategorizationSource.UserRule, autoTx.CategorizationSource);
        Assert.Equal("Cabify", autoTx.CleanMerchantName);
        Assert.Equal(1, _uow.SaveChangesCalls);
    }
}

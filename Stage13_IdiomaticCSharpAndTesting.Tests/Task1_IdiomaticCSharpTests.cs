using System.Runtime.CompilerServices;
using Stage13_IdiomaticCSharpAndTesting.Task1_IdiomaticCSharp;

namespace Stage13_IdiomaticCSharpAndTesting.Tests;

public class Task1_IdiomaticCSharpTests
{
    // =====================================================================
    // 1. 封裝性與現代 C# 結構守門測試
    // =====================================================================

    [Fact]
    public void MemberAccount_ShouldNotExposeAnyPublicSetters()
    {
        var properties = typeof(MemberAccount).GetProperties();

        var publicSetProperties = properties
            .Where(p => p.SetMethod?.IsPublic == true)
            .Select(p => p.Name)
            .ToList();

        Assert.Empty(publicSetProperties);
    }

    [Fact]
    public void PointTransaction_ShouldBeImmutableRecordWithValueEquality()
    {
        var txType = typeof(PointTransaction);
        var properties = txType.GetProperties();

        // 所有公開 setter 都必須是 init-only（具 IsExternalInit 標記）
        foreach (var prop in properties)
        {
            if (prop.SetMethod is { IsPublic: true } setter)
            {
                var hasIsExternalInit = setter.ReturnParameter
                    .GetRequiredCustomModifiers()
                    .Contains(typeof(IsExternalInit));

                Assert.True(hasIsExternalInit, $"屬性 {prop.Name} 不可使用可變的 public set，請改為 init 或唯讀。");
            }
        }

        // C# record 編譯後會產生 <Clone>$ 方法，且具備值相等性
        Assert.NotNull(txType.GetMethod("<Clone>$"));

        var occurredAt = new DateTimeOffset(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);
        var tx1 = new PointTransaction(TransactionType.Earn, 100, "每日簽到", "首日獎勵", occurredAt);
        var tx2 = new PointTransaction(TransactionType.Earn, 100, "每日簽到", "首日獎勵", occurredAt);

        Assert.Equal(tx1, tx2);
        Assert.Equal(tx1.GetHashCode(), tx2.GetHashCode());
    }

    [Fact]
    public void Transactions_ShouldNotBeCastableToMutableList()
    {
        var account = new MemberAccount("Alice", "alice@example.com");

        Assert.False(account.Transactions is List<PointTransaction>);
    }

    // =====================================================================
    // 2. Result<T> 契約測試
    // =====================================================================

    [Fact]
    public void Result_Success_ShouldExposeValueAndNullErrors()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Result_Failure_ShouldExposeErrorsAndThrowWhenReadingValue()
    {
        var result = Result<int>.Failure("INSUFFICIENT_POINTS", "點數餘額不足");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("INSUFFICIENT_POINTS", result.ErrorCode);
        Assert.Equal("點數餘額不足", result.ErrorMessage);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Theory]
    [InlineData("", "有效訊息")]
    [InlineData("   ", "有效訊息")]
    [InlineData("ERR_CODE", "")]
    [InlineData("ERR_CODE", "   ")]
    public void Result_Failure_ShouldRejectBlankErrorCodeOrMessage(string errorCode, string errorMessage)
    {
        Assert.Throws<ArgumentException>(() => Result<string>.Failure(errorCode, errorMessage));
    }

    // =====================================================================
    // 3. MemberAccount 建構子與狀態變更守門測試
    // =====================================================================

    [Fact]
    public void Constructor_WithValidArgs_ShouldInitializeAccount()
    {
        var account = new MemberAccount("  Alice  ", "alice@example.com", 250);

        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal("Alice", account.DisplayName);
        Assert.Equal("alice@example.com", account.Email);
        Assert.Equal(250, account.PointsBalance);
        Assert.False(account.IsSuspended);
        Assert.Empty(account.Transactions);
    }

    [Theory]
    [InlineData("", "alice@example.com")]
    [InlineData("   ", "alice@example.com")]
    [InlineData("Alice", "")]
    [InlineData("Alice", "   ")]
    [InlineData("Alice", "invalid-email")]
    public void Constructor_WithInvalidDisplayNameOrEmail_ShouldThrowArgumentException(
        string displayName,
        string email)
    {
        Assert.Throws<ArgumentException>(() => new MemberAccount(displayName, email));
    }

    [Fact]
    public void Constructor_WithNegativeInitialPoints_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MemberAccount("Alice", "alice@example.com", -1));
    }

    [Fact]
    public void UpdateEmail_ValidAndInvalidCases()
    {
        var account = new MemberAccount("Alice", "alice@example.com");

        account.UpdateEmail("  alice.new@example.com  ");
        Assert.Equal("alice.new@example.com", account.Email);

        Assert.Throws<ArgumentException>(() => account.UpdateEmail(""));
        Assert.Throws<ArgumentException>(() => account.UpdateEmail("not-an-email"));
        Assert.Equal("alice.new@example.com", account.Email);
    }

    // =====================================================================
    // 4. 商業行為與 Result Pattern 測試
    // =====================================================================

    [Fact]
    public void EarnPoints_WhenValid_ShouldIncreaseBalanceAndRecordTransaction()
    {
        var account = new MemberAccount("Alice", "alice@example.com", 100);

        var r1 = account.EarnPoints(50, "  購物回饋  ", null);
        var r2 = account.EarnPoints(30, "活動贈點", "  雙十一加碼  ");

        Assert.True(r1.IsSuccess);
        Assert.Null(r1.Value.Note);
        Assert.Equal("購物回饋", r1.Value.Reason);

        Assert.True(r2.IsSuccess);
        Assert.Equal("雙十一加碼", r2.Value.Note);

        Assert.Equal(180, account.PointsBalance);
        Assert.Equal(2, account.Transactions.Count);
        Assert.All(account.Transactions, tx => Assert.Equal(TransactionType.Earn, tx.Type));
    }

    [Fact]
    public void RedeemPoints_WhenSufficientBalance_ShouldDeductBalanceAndRecordTransaction()
    {
        var account = new MemberAccount("Alice", "alice@example.com", 200);

        var result = account.RedeemPoints(120, "  折抵訂單  ", "  訂單 #1001 ");

        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionType.Redeem, result.Value.Type);
        Assert.Equal(120, result.Value.Amount);
        Assert.Equal("折抵訂單", result.Value.Reason);
        Assert.Equal("訂單 #1001", result.Value.Note);
        Assert.Equal(80, account.PointsBalance);
        Assert.Single(account.Transactions);
    }

    [Fact]
    public void RedeemPoints_WhenInsufficientPoints_ShouldReturnFailureWithoutMutatingState()
    {
        var account = new MemberAccount("Alice", "alice@example.com", 50);

        var result = account.RedeemPoints(100, "兌換禮券");

        Assert.True(result.IsFailure);
        Assert.Equal("INSUFFICIENT_POINTS", result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(50, account.PointsBalance);
        Assert.Empty(account.Transactions);
    }

    [Fact]
    public void EarnAndRedeem_WhenAccountSuspended_ShouldReturnFailureWithoutMutatingState()
    {
        var account = new MemberAccount("Alice", "alice@example.com", 100);
        account.Suspend();

        var earnResult = account.EarnPoints(50, "補發");
        var redeemResult = account.RedeemPoints(20, "兌換");

        Assert.True(account.IsSuspended);
        Assert.True(earnResult.IsFailure);
        Assert.Equal("ACCOUNT_SUSPENDED", earnResult.ErrorCode);
        Assert.True(redeemResult.IsFailure);
        Assert.Equal("ACCOUNT_SUSPENDED", redeemResult.ErrorCode);
        Assert.Equal(100, account.PointsBalance);
        Assert.Empty(account.Transactions);

        account.Reactivate();
        var afterReactivate = account.RedeemPoints(20, "恢復後兌換");

        Assert.False(account.IsSuspended);
        Assert.True(afterReactivate.IsSuccess);
        Assert.Equal(80, account.PointsBalance);
        Assert.Single(account.Transactions);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void EarnAndRedeem_WhenAmountNonPositive_ShouldReturnInvalidAmountFailure(int invalidAmount)
    {
        var account = new MemberAccount("Alice", "alice@example.com", 100);

        var earnResult = account.EarnPoints(invalidAmount, "測試");
        var redeemResult = account.RedeemPoints(invalidAmount, "測試");

        Assert.True(earnResult.IsFailure);
        Assert.Equal("INVALID_AMOUNT", earnResult.ErrorCode);
        Assert.True(redeemResult.IsFailure);
        Assert.Equal("INVALID_AMOUNT", redeemResult.ErrorCode);
        Assert.Equal(100, account.PointsBalance);
        Assert.Empty(account.Transactions);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EarnAndRedeem_WhenReasonBlank_ShouldReturnInvalidReasonFailure(string blankReason)
    {
        var account = new MemberAccount("Alice", "alice@example.com", 100);

        var earnResult = account.EarnPoints(10, blankReason);
        var redeemResult = account.RedeemPoints(10, blankReason);

        Assert.True(earnResult.IsFailure);
        Assert.Equal("INVALID_REASON", earnResult.ErrorCode);
        Assert.True(redeemResult.IsFailure);
        Assert.Equal("INVALID_REASON", redeemResult.ErrorCode);
        Assert.Equal(100, account.PointsBalance);
        Assert.Empty(account.Transactions);
    }
}

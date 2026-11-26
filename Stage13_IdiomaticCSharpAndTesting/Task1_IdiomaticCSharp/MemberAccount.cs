namespace Stage13_IdiomaticCSharpAndTesting.Task1_IdiomaticCSharp;

public enum TransactionType
{
    Earn,
    Redeem
}

// TODO: 將 PointTransaction 重構為不可變的 sealed record（使用位置參數或 init-only 屬性），
// 確保具備 Value Equality（值相等性）且建立後不可被外部竄改。
// 注意：請保留 5 參數建構順序 (TransactionType, int, string, string?, DateTimeOffset) 與對應屬性名稱。
public class PointTransaction
{
    public TransactionType Type { get; set; }
    public int Amount { get; set; }
    public string Reason { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public PointTransaction(
        TransactionType type,
        int amount,
        string reason,
        string? note,
        DateTimeOffset occurredAt)
    {
        Type = type;
        Amount = amount;
        Reason = reason;
        Note = note;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// 會員點數帳戶聚合根。
/// 請落實現代 C# 慣用語：
///   1. 私有欄位使用 _camelCase + readonly（如適用），建構子/方法參數使用 camelCase。
///   2. 消除所有 public set，僅透過受保護的行為方法改變狀態。
///   3. 使用 Backing Field 暴露唯讀交易紀錄集合。
///   4. 建構子與參數防呆拋出精確的 ArgumentException / ArgumentOutOfRangeException；
///      可預期的業務規則拒絕則回傳 Result&lt;PointTransaction&gt;.Failure(...)。
/// </summary>
public sealed class MemberAccount
{
    // TODO: 補齊 private readonly List<PointTransaction> _transactions = new();

    public Guid Id { get; private set; }
    public string DisplayName { get; set; } = string.Empty; // TODO: 收斂封裝，消除 public set
    public string Email { get; private set; } = string.Empty;
    public int PointsBalance { get; set; } // TODO: 收斂封裝，消除 public set
    public bool IsSuspended { get; private set; }

    public IReadOnlyList<PointTransaction> Transactions
        => throw new NotImplementedException("TODO: 實作唯讀 Backing Field（回傳 _transactions.AsReadOnly()）");

    /// <summary>
    /// 建立會員點數帳戶。
    /// - displayName 不可為空白（否則拋出 ArgumentException）
    /// - email 不可為空白且必須包含 '@'（否則拋出 ArgumentException）
    /// - initialPoints 不可小於 0（否則拋出 ArgumentOutOfRangeException）
    /// - Id 必須以 Guid.NewGuid() 產生
    /// </summary>
    public MemberAccount(string displayName, string email, int initialPoints = 0)
    {
        throw new NotImplementedException("TODO: 實作建構子參數守門與狀態初始化");
    }

    /// <summary>
    /// 更新電子郵件。
    /// 若 newEmail 為空白或不包含 '@'，必須拋出 <see cref="ArgumentException"/>。
    /// </summary>
    public void UpdateEmail(string newEmail)
    {
        throw new NotImplementedException("TODO: 實作 UpdateEmail 與格式驗證");
    }

    /// <summary>停權會員帳戶。</summary>
    public void Suspend()
    {
        throw new NotImplementedException("TODO: 實作 Suspend");
    }

    /// <summary>恢復會員帳戶使用權限。</summary>
    public void Reactivate()
    {
        throw new NotImplementedException("TODO: 實作 Reactivate");
    }

    /// <summary>
    /// 累積會員點數。
    /// 預期失敗錯誤碼：
    ///   - 帳號停權："ACCOUNT_SUSPENDED"
    ///   - 點數 &lt;= 0："INVALID_AMOUNT"
    ///   - 原因為空白："INVALID_REASON"
    /// 成功時更新餘額並寫入交易明細（reason 需 Trim；note 若為空白則存為 null，否則 Trim）。
    /// </summary>
    public Result<PointTransaction> EarnPoints(int amount, string reason, string? note = null)
    {
        throw new NotImplementedException("TODO: 實作 EarnPoints 與 Result<PointTransaction> 回傳");
    }

    /// <summary>
    /// 兌換扣除會員點數。
    /// 預期失敗錯誤碼：
    ///   - 帳號停權："ACCOUNT_SUSPENDED"
    ///   - 點數 &lt;= 0："INVALID_AMOUNT"
    ///   - 原因為空白："INVALID_REASON"
    ///   - 點數餘額不足："INSUFFICIENT_POINTS"
    /// 成功時扣除餘額並寫入交易明細（reason 需 Trim；note 若為空白則存為 null，否則 Trim）。
    /// </summary>
    public Result<PointTransaction> RedeemPoints(int amount, string reason, string? note = null)
    {
        throw new NotImplementedException("TODO: 實作 RedeemPoints 與 Result<PointTransaction> 回傳");
    }
}

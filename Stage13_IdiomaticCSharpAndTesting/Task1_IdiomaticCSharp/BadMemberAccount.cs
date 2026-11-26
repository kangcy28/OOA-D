namespace Stage13_IdiomaticCSharpAndTesting.Task1_IdiomaticCSharp;

// =========================================================================
// ❌ 反模式對照組：充滿非慣用語與封裝漏洞的會員點數帳戶
// 請在閱讀這份檔案時，找出刻意埋入的 5 個地雷：
//   1. 命名慣例顛倒（建構子參數加 _、私有欄位沒加 _ 也沒 readonly）
//   2. 殘留 public set 且直接暴露可變 List<T>
//   3. 交易明細使用可變 class 而非不可變 record / init
//   4. 濫用 null! 壓抑編譯器警告，埋下執行期 NullReferenceException 地雷
//   5. 用通用 Exception 處理可預期的商業規則失敗
// =========================================================================

public class BadPointTransaction
{
    public string Type { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Note { get; set; } = null!; // 💣 地雷：濫用 null-forgiving (!) 掩蓋可能為 null 的事實
    public DateTimeOffset OccurredAt { get; set; }
}

public class BadMemberAccount
{
    // 💣 地雷：私有欄位未遵循 _camelCase 與 readonly 慣例
    private List<BadPointTransaction> transactions = new();

    // 💣 地雷：殘留大量 public set，外部可直接繞過商業規則亂改餘額與停權狀態
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PointsBalance { get; set; }
    public bool IsSuspended { get; set; }

    // 💣 地雷：直接回傳可變 List<T>，呼叫端可執行 account.Transactions.Clear() 或 Add()
    public List<BadPointTransaction> Transactions => transactions;

    // 💣 地雷：建構子參數誤加底線前綴
    public BadMemberAccount(string _displayName, string _email, int _initialPoints = 0)
    {
        Id = new Guid(); // 💣 順帶一提：這會產生全零 GUID，而非 Guid.NewGuid()
        DisplayName = _displayName;
        Email = _email;
        PointsBalance = _initialPoints;
    }

    public void EarnPoints(int _amount, string _reason, string _note)
    {
        if (IsSuspended)
        {
            throw new Exception("帳號已停權"); // 💣 地雷：可預期業務失敗卻丟出寬泛的 Exception
        }

        PointsBalance += _amount;
        transactions.Add(new BadPointTransaction
        {
            Type = "Earn",
            Amount = _amount,
            Reason = _reason,
            Note = _note.Trim(), // 💣 當外部傳入 null 時會在執行期直接 NullReferenceException 爆炸！
            OccurredAt = DateTimeOffset.UtcNow
        });
    }

    public void RedeemPoints(int _amount, string _reason)
    {
        if (IsSuspended)
        {
            throw new Exception("帳號已停權");
        }

        if (_amount > PointsBalance)
        {
            throw new Exception("點數不足");
        }

        PointsBalance -= _amount;
        transactions.Add(new BadPointTransaction
        {
            Type = "Redeem",
            Amount = _amount,
            Reason = _reason,
            Note = null!,
            OccurredAt = DateTimeOffset.UtcNow
        });
    }
}

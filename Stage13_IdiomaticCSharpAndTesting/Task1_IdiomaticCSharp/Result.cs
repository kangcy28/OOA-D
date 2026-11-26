namespace Stage13_IdiomaticCSharpAndTesting.Task1_IdiomaticCSharp;

/// <summary>
/// 代表商業操作結果的 Result Pattern 容器。
/// 用於顯式表達「可預期的業務失敗」，避免將一般業務拒絕（如餘額不足、帳號停權）混同為系統例外。
/// </summary>
public sealed class Result<T>
{
    // TODO: 補齊私有建構子與必要的私有欄位（遵循 _camelCase 慣例）

    /// <summary>操作是否成功。</summary>
    public bool IsSuccess => throw new NotImplementedException("TODO: 實作 Result<T>.IsSuccess");

    /// <summary>操作是否失敗。</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 取得成功結果值。
    /// 當 <see cref="IsFailure"/> 為 true 時存取此屬性，必須拋出 <see cref="InvalidOperationException"/>。
    /// </summary>
    public T Value => throw new NotImplementedException("TODO: 實作 Result<T>.Value（失敗時必須拋出 InvalidOperationException）");

    /// <summary>失敗時的錯誤代碼（成功時為 null）。</summary>
    public string? ErrorCode => throw new NotImplementedException("TODO: 實作 Result<T>.ErrorCode");

    /// <summary>失敗時的錯誤訊息（成功時為 null）。</summary>
    public string? ErrorMessage => throw new NotImplementedException("TODO: 實作 Result<T>.ErrorMessage");

    /// <summary>建立成功結果。</summary>
    public static Result<T> Success(T value)
        => throw new NotImplementedException("TODO: 實作 Result<T>.Success");

    /// <summary>
    /// 建立失敗結果。
    /// 若 <paramref name="errorCode"/> 或 <paramref name="errorMessage"/> 為 null 或空白，必須拋出 <see cref="ArgumentException"/>。
    /// </summary>
    public static Result<T> Failure(string errorCode, string errorMessage)
        => throw new NotImplementedException("TODO: 實作 Result<T>.Failure（需驗證 errorCode 與 errorMessage 不可為空白）");
}

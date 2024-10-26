namespace HippoServer.DataModel;

public class Transaction
{
    public int Id { get; set; }
    public int? FromAccountId { get; set; }
    public int? ToAccountId { get; set; }
    public int Value { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public string Note { get; set; } = null!;
}

public enum TransactionType
{
    Loan,
    Withdrawal,
    Deposit,
    Transfer,
    GameResult
}
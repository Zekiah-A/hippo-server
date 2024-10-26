namespace HippoServer.DataModel;

public class Transaction
{
    public int Id { get; set; }
    
    public int? FromAccountId { get; set; }
    // Navigation property to sender account
    public Account? FromAccount { get; set; }
    
    public int? ToAccountId { get; set; }
    // Navigation property to receiver account
    public Account? ToAccount { get; set; }

    public int Value { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Note { get; set; }
}

public enum TransactionType
{
    // Deduction types:
    // Withdrawals from casino system, money transferred into the void in exchange for real world cash
    Withdrawal,
 
    // Insertion types:
    // Deposits into casino system, money is created from void in exchange for real world cash
    Deposit,
    
    // Transfer types:
    // Generic currency transfer, can include unrecorded game winnings and loans, or sharing between accounts
    Transfer,
    // Game results, from casino account or another's account, will have an associated GameResult in the GameResults table
    GameResult,
    // Loans from casino account, will have an associated Loan record in the Loans table
    Loan
}
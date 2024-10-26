namespace HippoServer.DataModel;

public class Loan
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int Value { get; set; }
    public DateTime Created { get; set; }
}
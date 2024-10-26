namespace HippoServer.DataModel;

public class AccountNote
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Note { get; set; } = null!;
    public DateTime Created { get; set; }
}
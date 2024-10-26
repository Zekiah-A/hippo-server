namespace HippoServer.DataModel;

public class Activation
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public DateTime Created { get; set; }
    
    public Account Account { get; set; } = null!;
    // Navigation property for account
    public int AccountId { get; set; }
}
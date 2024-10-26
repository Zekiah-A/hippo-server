namespace HippoServer.DataModel;

public class Verification
{
    public int Id { get; set; }
    
    public int AccountId { get; set; }
    // Navigation property to target Account
    public Account Account { get; set; } = null!;
    
    public string Code { get; set; }
    public string Token { get; set; }
    public DateTime Created { get; set; }
}
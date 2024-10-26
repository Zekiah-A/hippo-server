namespace HippoServer.DataModel;

public class Account
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public int Balance { get; set; }
    public int Total { get; set; }
    public DateTime? Activated { get; set; }
    public DateTime Created { get; set; }
    
    // Navigation property to groups which account is a member of
    public List<Group> Groups { get; set; } = [ ];
    
    // Navigation property to individual account permissions
    public List<Permission> Permissions { get; set; } = [ ];
    
    // Navigation property to all account verifications
    public List<Verification> Verifications { get; set; } = [ ];
}
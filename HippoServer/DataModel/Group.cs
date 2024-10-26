namespace HippoServer.DataModel;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    
    // Navigation property to group members
    public List<Account> Members { get; set; } = [];
    // Navigation property to group permissions
    public List<Permission> Permissions { get; set; } = [];
}
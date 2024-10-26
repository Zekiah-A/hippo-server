namespace HippoServer.DataModel;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public int? GroupId { get; set; }
    // Navigation property to group owner of this permission
    public Group? Group { get; set; }
    
    public int? AccountId { get; set; }
    // Navigation property to account owner of this permission
    public Account? Account { get; set; }
}
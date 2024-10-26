namespace HippoServer;

public class RequiredPermissionsMetadata
{
    public RequiredPermissionsMetadata(string[] permissions)
    {
        Permissions = permissions;
    }
    
    public string[] Permissions { get; }
}

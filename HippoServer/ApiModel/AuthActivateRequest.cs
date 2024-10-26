namespace HippoServer.ApiModel;

public class AuthActivateRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
}
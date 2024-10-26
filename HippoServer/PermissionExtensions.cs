using HippoServer.DataModel;

namespace HippoServer;

public static class PermissionExtensions
{
    public static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(new RequiredPermissionsMetadata(permissions));
        });
        return builder;
    }
}
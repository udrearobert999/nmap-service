using System.Net;

namespace NetworkMapper.Application.Validation.Shared;

public static class TargetValidationHelper
{
    public static bool IsValidTarget(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return false;
        
        if (IPAddress.TryParse(target, out _))
        {
            return true;
        }
        
        var hostNameType = Uri.CheckHostName(target);
        
        if (hostNameType == UriHostNameType.Dns)
        {
            return target.Contains('.') || 
                   target.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
using System.Text.RegularExpressions;

namespace NetworkMapper.WebAPI.Config;

internal class SpinalCaseRouteNameTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null)
        {
            return null;
        }

        var str = value.ToString();
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }

        return Regex.Replace(str, "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}
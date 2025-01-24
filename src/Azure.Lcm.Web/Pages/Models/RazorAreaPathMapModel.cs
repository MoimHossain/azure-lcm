using AzLcm.Shared.Storage;

namespace Azure.Lcm.Web.Pages.Models;

public class RazorAreaPathMapModel
{
    public string CommaSeparatedServices { get; set; } = string.Empty;
    public string RouteToAreaPath { get; set; } = string.Empty;

    public static RazorAreaPathMapModel CreateFrom(IEnumerable<string> services, string routeToAreaPath)
    {
        return new RazorAreaPathMapModel
        {
            CommaSeparatedServices = string.Join(",", services),
            RouteToAreaPath = routeToAreaPath
        };
    }

    public AreaPathServiceMap ConvertTo() 
    {
        var services = new List<string>();
        if (!string.IsNullOrWhiteSpace(CommaSeparatedServices))
        {
            services.AddRange(CommaSeparatedServices.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
        }

        return new AreaPathServiceMap(services, RouteToAreaPath);
    }
}

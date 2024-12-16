

using AzLcm.Shared.Storage;
using Azure.Lcm.Web.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure.Lcm.Web.Pages;

public class ConfigModel(ConfigurationStorage ConfigurationStorage) : PageModel
{
    [BindProperty]
    public AreaPathMapConfig? Config { get; set; }

    [BindProperty]
    public List<RazorAreaPathMapModel> ServiceHealthMap { get; set; } = new List<RazorAreaPathMapModel>();

    [BindProperty]
    public List<RazorAreaPathMapModel> FeedMap { get; set; } = new List<RazorAreaPathMapModel>();

    [BindProperty]
    public List<RazorAreaPathMapModel> PolicyMap { get; set; } = new List<RazorAreaPathMapModel>();

    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Config = await ConfigurationStorage.LoadConfigAsync(cancellationToken);

            ServiceHealthMap.Clear();
            FeedMap.Clear();
            PolicyMap.Clear();

            foreach (var map in Config.ServiceHealthMap.Map)
            {
                ServiceHealthMap.Add(RazorAreaPathMapModel.CreateFrom(map.Services, map.RouteToAreaPath));
            }
            foreach (var map in Config.AzureUpdatesMap.Map)
            {
                FeedMap.Add(RazorAreaPathMapModel.CreateFrom(map.Services, map.RouteToAreaPath));
            }
            foreach (var map in Config.PolicyMap.Map)
            {
                PolicyMap.Add(RazorAreaPathMapModel.CreateFrom(map.Services, map.RouteToAreaPath));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading configuration: {ex.Message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        //if (!ModelState.IsValid)
        //{
        //    StatusMessage = "Invalid data. Please review the form.";
        //    return Page();
        //}

        try
        {
            if (Config != null) 
            {
                Config.ServiceHealthMap.Map = [];
                foreach(var map in ServiceHealthMap)
                {
                    Config.ServiceHealthMap.Map.Add(map.ConvertTo());
                }
                Config.AzureUpdatesMap.Map = [];
                foreach (var map in FeedMap)
                {
                    Config. AzureUpdatesMap .Map.Add(map.ConvertTo());
                }
                Config.PolicyMap.Map = [];
                foreach (var map in PolicyMap)
                {
                    Config.PolicyMap.Map.Add(map.ConvertTo());
                }

                await ConfigurationStorage.SaveConfigAsync(Config, cancellationToken);

                StatusMessage = "Configuration saved successfully!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving configuration: {ex.Message}";
        }

        return Page();
    }
}

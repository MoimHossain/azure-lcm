using AzLcm.Shared.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure.Lcm.Web.Pages
{
    public class WorkItemConfigModel(ConfigurationStorage ConfigurationStorage) : PageModel
    {
        [BindProperty]
        public WorkItemTempates? WorkItemTempates { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                WorkItemTempates = await ConfigurationStorage.GetWorkItemTemplatesAsync(cancellationToken);
            }
            catch (Exception )
            {
                
            }

            return Page();
        }
    }
}

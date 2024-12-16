

using AzLcm.Shared.ServiceHealth;
using AzLcm.Shared.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure.Lcm.Web.Pages
{
    public class SvcHealthConfigModel(ConfigurationStorage ConfigurationStorage) : PageModel
    {
        [BindProperty]
        public ServiceHealthConfig? ServiceHealthConfig { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                ServiceHealthConfig = await ConfigurationStorage.LoadServiceHealthConfigAsync(cancellationToken);
            }
            catch (Exception)
            {
                
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (ServiceHealthConfig != null) 
                {
                    await ConfigurationStorage.SaveServiceHealthConfigAsync(ServiceHealthConfig, cancellationToken);
                }                
            }
            catch (Exception )
            {
                
            }

            return Page();
        }
    }
}

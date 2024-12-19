using AzLcm.Shared.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure.Lcm.Web.Pages
{
    public class GeneralConfigModel(ConfigurationStorage ConfigurationStorage) : PageModel
    {
        [BindProperty]
        public GeneralConfig GeneralConfigObject { get; set; }

        [BindProperty]
        public bool ProcessServiceHealth
        {
            get
            {
                return GeneralConfigObject.ProcessServiceHealth;
            }
        }

        [BindProperty]
        public bool ProcessFeed
        {
            get
            {
                return GeneralConfigObject.ProcessFeed;
            }
        }

        [BindProperty]
        public bool ProcessPolicy
        {
            get
            {
                return GeneralConfigObject.ProcessPolicy;
            }
        }


        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                GeneralConfigObject = await ConfigurationStorage.LoadGeneralConfigAsync(cancellationToken);
            }
            catch (Exception)
            {

            }

            return Page();
        }
    }
}

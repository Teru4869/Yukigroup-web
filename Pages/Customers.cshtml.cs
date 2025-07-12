using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Pages.Services;

namespace Yukigroup_WEB.Pages
{
    public class CustomersModel : PageModel
    {
        private readonly GoogleSheetService _sheetService;

        public CustomersModel()
        {
            _sheetService = new GoogleSheetService();
        }

        public List<Participant> CustomerList { get; set; } = new();

        public async Task OnGetAsync(string? searchTerm)
        {
            var allCustomers = await _sheetService.LoadParticipantsAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // 名前でフィルター
                CustomerList = allCustomers
                    .Where(c => c.Name != null && c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                CustomerList = allCustomers;
            }
        }
    }
}

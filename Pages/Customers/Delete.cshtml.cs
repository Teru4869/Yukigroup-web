using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Pages.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Yukigroup_WEB.Pages.Customers
{
    public class DeleteModel : PageModel
    {
        private readonly GoogleSheetService _sheetService;

        public DeleteModel()
        {
            _sheetService = new GoogleSheetService();
        }

        [BindProperty]
        public Participant Customer { get; set; }

        // 削除確認画面表示  
        public async Task<IActionResult> OnGetAsync(string id)
        {
            var participants = await _sheetService.LoadParticipantsAsync();
            Customer = participants.Find(p => p.ID.ToString() == id);

            if (Customer == null)
            {
                return NotFound();
            }

            return Page();
        }

        // 削除実行  
        public async Task<IActionResult> OnPostAsync()
        {
            if (Customer == null || Customer.ID == 0)             {
                return NotFound();
            }

            await _sheetService.DeleteRowAsync(Customer.ID);  
            return RedirectToPage("./Index");
        }
    }
}

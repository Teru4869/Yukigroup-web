using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Pages.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Yukigroup_WEB.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly GoogleSheetService _sheetService;

        public EditModel()
        {
            _sheetService = new GoogleSheetService();
        }

        [BindProperty]
        public Participant Customer { get; set; }

        public List<SelectListItem> ParticipationOptions { get; set; }
        public List<SelectListItem> StatusOptions { get; set; }
        public List<SelectListItem> PlaceOptions { get; set; }
        public List<SelectListItem> LanguageSkillOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var participants = await _sheetService.LoadParticipantsAsync();
            if (int.TryParse(id, out int parsedId))
            {
                Customer = participants.Find(p => p.ID == parsedId);
            }

            if (Customer == null)
            {
                return NotFound();
            }

            LoadSelectOptions();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadSelectOptions();
                return Page();
            }

            await _sheetService.UpdateRowAsync(Customer);
            return RedirectToPage("./Index");
        }

        private void LoadSelectOptions()
        {
            ParticipationOptions = new List<SelectListItem>
        {
            new SelectListItem("1回", "1回"),
            new SelectListItem("2回", "2回"),
            new SelectListItem("3回", "3回"),
            new SelectListItem("4回以上", "4回以上")
        };

            StatusOptions = new List<SelectListItem>
        {
            new SelectListItem("学生", "学生"),
            new SelectListItem("社会人", "社会人"),
        };

            PlaceOptions = new List<SelectListItem>
        {
            new SelectListItem("WeBase", "WeBase"),
            new SelectListItem("コモエス", "コモエス"),
        };

            LanguageSkillOptions = new List<SelectListItem>
        {
            new SelectListItem("全く話せない", "全く話せない"),
            new SelectListItem("少し話せる", "少し話せる"),
            new SelectListItem("日常会話可能", "日常会話可能"),
            new SelectListItem("流暢", "流暢")
        };
        }
    }

}

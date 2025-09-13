using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Pages.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using System.Security.Cryptography.Xml;

namespace Yukigroup_WEB.Pages
{
    public class CustomersModel : PageModel
    {
        private readonly GoogleSheetService _sheetService;

        public CustomersModel()
        {
            _sheetService = new GoogleSheetService();
        }

        public List<Participant> CustomerList { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Country { get; set; }

        [BindProperty]
        public string ParticipationCount { get; set; }

        [BindProperty]
        public string Status { get; set; }

        [BindProperty]
        public string Place { get; set; }

        [BindProperty]
        public string LanguageSkill { get; set; }

        [BindProperty]
        public string Note { get; set; }

        // ページ初期表示（GET）
        public async Task OnGetAsync(string searchTerm)
        {
            var participants = await _sheetService.LoadParticipantsAsync();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                CustomerList = participants.FindAll(p => p.Name.Contains(searchTerm));
            }
            else
            {
                CustomerList = participants;
            }
        }


        // フォーム送信時（POST）
        public async Task<IActionResult> OnPostAsync()
        {
            // サービスから次IDを取得
            int nextId = await _sheetService.GetNextIdAsync();

            // シートに追加（A列から順番に 8 項目）
            await _sheetService.AppendRowAsync(new List<object>
            {
                nextId,              // A: 参加者ID（自動採番）
                Name,                // B: 名前
                Country,             // C: 出身国（プルダウンの選択肢と一致させる）
                ParticipationCount,  // D: 参加回数
                Status,              // E: 学生か社会人
                Place,               // F: 場所
                LanguageSkill,       // G: 外国語のレベル
                Note                 // H: 備考
            });

            // 送信後にGETで再表示
            return RedirectToPage();
        }
    }
}

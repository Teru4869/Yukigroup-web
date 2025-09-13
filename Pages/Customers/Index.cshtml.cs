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

        // �y�[�W�����\���iGET�j
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


        // �t�H�[�����M���iPOST�j
        public async Task<IActionResult> OnPostAsync()
        {
            // �T�[�r�X���玟ID���擾
            int nextId = await _sheetService.GetNextIdAsync();

            // �V�[�g�ɒǉ��iA�񂩂珇�Ԃ� 8 ���ځj
            await _sheetService.AppendRowAsync(new List<object>
            {
                nextId,              // A: �Q����ID�i�����̔ԁj
                Name,                // B: ���O
                Country,             // C: �o�g���i�v���_�E���̑I�����ƈ�v������j
                ParticipationCount,  // D: �Q����
                Status,              // E: �w�����Љ�l
                Place,               // F: �ꏊ
                LanguageSkill,       // G: �O����̃��x��
                Note                 // H: ���l
            });

            // ���M���GET�ōĕ\��
            return RedirectToPage();
        }
    }
}

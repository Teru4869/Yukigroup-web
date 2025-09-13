using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace Yukigroup_WEB.Pages.Account
{
    public class daybudgetModel : PageModel
    {
        private readonly AccountingSheetService _sheetService;

        public IList<IList<object>> Rows { get; set; }

        public daybudgetModel(AccountingSheetService sheetService)
        {
            _sheetService = sheetService;
        }

        [BindProperty]
        public string InputDate { get; set; }

        [BindProperty]
        public string InputPlace { get; set; }  // B列プルダウン用

        [BindProperty]
        public decimal? StartCash { get; set; }
        [BindProperty]
        public decimal? EndCash { get; set; }
        [BindProperty]
        public decimal? DiscountTicket { get; set; }
        [BindProperty]
        public decimal? DailySales { get; set; }
        [BindProperty]
        public decimal? VenueCost { get; set; }
        [BindProperty]
        public decimal? MiscCost { get; set; }
        [BindProperty]
        public decimal? MonthlySales { get; set; }
        [BindProperty]
        public decimal? FinalCash { get; set; }
        [BindProperty]
        public decimal? Visitors { get; set; }
        [BindProperty]
        public string? InputNote { get; set; }      // 備考

        public async Task OnGetAsync()
        {
            Rows = await _sheetService.GetSheetAsync("daybudget!A:Z");

            if (Rows != null && Rows.Count > 3)
            {
                // 1〜3行目を固定でそのまま保持
                var fixedRows = Rows.Take(3).ToList();

                // 4行目以降を逆順に
                var reversedRows = Rows.Skip(3).Reverse().ToList();

                // 結合
                var result = new List<IList<object>>();
                result.AddRange(fixedRows);
                result.AddRange(reversedRows);

                Rows = result;
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // 再描画用にデータ取得
                return Page();
            }

            var newRow = new List<object>
            {
                InputDate,
                InputPlace,
                StartCash?.ToString() ?? "",
                EndCash?.ToString() ?? "",
                DiscountTicket?.ToString() ?? "",
                DailySales?.ToString() ?? "",
                VenueCost?.ToString() ?? "",
                MiscCost?.ToString() ?? "",
                MonthlySales?.ToString() ?? "",
                FinalCash?.ToString() ?? "",
                Visitors?.ToString() ?? "",
                InputNote ?? ""
            };

            await _sheetService.AppendDayBudgetRowAsync(newRow);

            return RedirectToPage(); // 再読み込みして一覧に反映
        }
    }
}

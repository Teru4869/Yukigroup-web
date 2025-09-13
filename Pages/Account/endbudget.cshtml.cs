using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Yukigroup_WEB.Pages.Account
{
    public class endbudgetModel : PageModel
    {
        private readonly AccountingSheetService _sheetService;

        public IList<IList<object>> Rows { get; set; }

        public endbudgetModel(AccountingSheetService sheetService)
        {
            _sheetService = sheetService;
        }


        [BindProperty]
        public string EndDate { get; set; }

        [BindProperty]
        public string EndConfimer { get; set; }  // B列プルダウン用

        [BindProperty]
        public decimal? EndItimanen { get; set; }
        [BindProperty]
        public decimal? EndGosenen { get; set; }
        [BindProperty]
        public decimal? EndNisenen { get; set; }
        [BindProperty]
        public decimal? EndSenen { get; set; }
        [BindProperty]
        public decimal? EndGohyakuen { get; set; }
        [BindProperty]
        public decimal? EndHyakuen { get; set; }
        [BindProperty]
        public decimal? EndGozyuen { get; set; }
        [BindProperty]
        public decimal? EndZyuen { get; set; }
        [BindProperty]
        public decimal? EndGoen { get; set; }
        [BindProperty]
        public decimal? EndItien { get; set; }

        public async Task OnGetAsync()
        {
            Rows = await _sheetService.GetSheetAsync("endbudget!A:Z");

            if (Rows != null && Rows.Count > 4)
            {
                // 1〜4行目を固定でそのまま保持
                var fixedRows = Rows.Take(4).ToList();

                // 5行目以降を逆順に
                var reversedRows = Rows.Skip(4).Reverse().ToList();

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

            // 未入力を0に変換
            var enditimanen = EndItimanen ?? 0;
            var endgosenen = EndGosenen ?? 0;
            var endnisenen = EndNisenen ?? 0;
            var endsenen = EndSenen ?? 0;
            var endgohyakuen = EndGohyakuen ?? 0;
            var endhyakuen = EndHyakuen ?? 0;
            var endgozyuen = EndGozyuen ?? 0;
            var endzyuen = EndZyuen ?? 0;
            var endgoen = EndGoen ?? 0;
            var enditien = EndItien ?? 0;

            // 合計計算
            var endtotal =
                (enditimanen * 10000) +
                (endgosenen * 5000) +
                (endnisenen * 2000) +
                (endsenen * 1000) +
                (endgohyakuen * 500) +
                (endhyakuen * 100) +
                (endgozyuen * 50) +
                (endzyuen * 10) +
                (endgoen * 5) +
                (enditien * 1);

            var newRow = new List<object>
            {
                EndDate,
                EndConfimer,
                enditimanen,
                endgosenen,
                endnisenen,
                endsenen,
                endgohyakuen,
                endhyakuen,
                endgozyuen,
                endzyuen,
                endgoen,
                enditien,
                endtotal
            };

            await _sheetService.AppendEndBudgetRowAsync(newRow);

            return RedirectToPage(); // 再読み込みして一覧に反映
        }
    }
}

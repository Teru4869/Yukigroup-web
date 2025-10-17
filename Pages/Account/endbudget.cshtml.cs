using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Components.Forms;

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
        public string EndEditId { get; set; }
        [BindProperty]
        public DateTime? EndDate { get; set; }

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

            if (Rows != null && Rows.Count > 0)
            {
                // 全行を逆順に
                var reversedRows = Rows.Reverse().ToList();

                // 直近3か月（12行）だけ残す
                var limitedRows = reversedRows.Take(12).ToList();

                Rows = limitedRows;
            }
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            await OnGetAsync();
            var target = Rows.FirstOrDefault(r => r.Count > 0 && r[0]?.ToString() == id);
            if (target != null)
            {
                EndEditId = id;
                if (DateTime.TryParse(target[1]?.ToString(), out var parsedDate))
                {
                    EndDate = parsedDate;
                }
                else
                {
                    EndDate = null;
                }
                EndConfimer = target[2]?.ToString();
                EndItimanen = ParseDecimal(target.ElementAtOrDefault(3));
                EndGosenen = ParseDecimal(target.ElementAtOrDefault(4));
                EndNisenen = ParseDecimal(target.ElementAtOrDefault(5));
                EndSenen = ParseDecimal(target.ElementAtOrDefault(6));
                EndGohyakuen = ParseDecimal(target.ElementAtOrDefault(7));
                EndHyakuen = ParseDecimal(target.ElementAtOrDefault(8));
                EndGozyuen = ParseDecimal(target.ElementAtOrDefault(9));
                EndZyuen = ParseDecimal(target.ElementAtOrDefault(10));
                EndGoen = ParseDecimal(target.ElementAtOrDefault(11));
                EndItien = ParseDecimal(target.ElementAtOrDefault(12));
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            Console.WriteLine("[OnPostSaveAsync] 実行開始（endbudget）");

            // EndEditId のエラーを除外（新規登録時は空でもOKにする）
            ModelState.Remove(nameof(EndEditId));

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[OnPostSaveAsync] ModelState 無効 → ページ再表示");
                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Count > 0)
                        Console.WriteLine($"  ❌ {kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                await OnGetAsync();
                return Page();
            }

            Console.WriteLine("[OnPostSaveAsync] 入力値 OK。新規/更新処理へ進む。");

            // --- 金種集計 ---
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

            Console.WriteLine($"[OnPostSaveAsync] endtotal 計算完了: {endtotal}");

            // --- ID 採番処理 ---
            string id;
            if (string.IsNullOrEmpty(EndEditId))
            {
                // Googleスプレッドシート上の最大IDを調べて +1
                int nextId = await _sheetService.GetNextIdAsync("endbudget");
                id = nextId.ToString("D6"); // 例: 000047
                Console.WriteLine($"[OnPostSaveAsync] 新規ID採番: {id}");
            }
            else
            {
                id = EndEditId;
                Console.WriteLine($"[OnPostSaveAsync] 既存ID使用: {id}");
            }

            // --- 行データ作成 ---
            var newRow = new List<object>
    {
        id,
        EndDate?.ToString("yyyy/MM/dd"),
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
            Console.WriteLine($"[OnPostSaveAsync] newRow 作成完了: {string.Join(", ", newRow)}");

            // --- Google Sheets への登録処理 ---
            if (string.IsNullOrEmpty(EndEditId))
            {
                Console.WriteLine("[OnPostSaveAsync] 新規登録処理 → AppendEndBudgetRowAsync()");
                await _sheetService.AppendEndBudgetRowAsync(newRow);
                Console.WriteLine("[OnPostSaveAsync] AppendEndBudgetRowAsync() 完了");
            }
            else
            {
                Console.WriteLine("[OnPostSaveAsync] 既存更新処理 → UpdateRowByIdAsync()");
                await _sheetService.UpdateRowByIdAsync("endbudget", id, newRow);
                Console.WriteLine("[OnPostSaveAsync] UpdateRowByIdAsync() 完了");
            }

            Console.WriteLine("[OnPostSaveAsync] Redirect 実行");
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _sheetService.DeleteRowByIdAsync("endbudget", id);
            return RedirectToPage();
        }

        private decimal? ParseDecimal(object? obj)
            => decimal.TryParse(obj?.ToString(), out var d) ? d : (decimal?)null;
    }
}

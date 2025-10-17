using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace Yukigroup_WEB.Pages.Account
{
    public class DayBudgetModel : PageModel
    {
        private readonly AccountingSheetService _sheetService;

        public IList<IList<object>> Rows { get; set; }

        public DayBudgetModel(AccountingSheetService sheetService)
        {
            _sheetService = sheetService;
        }
        [BindProperty] 
        public string InputEditId { get; set; }
        [BindProperty]
        public DateTime? InputDate { get; set; }

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

            if (Rows != null && Rows.Count > 0)
            {
                // 全行を逆順に
                var reversedRows = Rows.Reverse().ToList();

                // 直近3か月＝12行分だけ残す
                var limitedRows = reversedRows.Take(12).ToList();

                // Rowsに反映
                Rows = limitedRows;
            }
        }
        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            await OnGetAsync();
            var target = Rows.FirstOrDefault(r => r.Count > 0 && r[0]?.ToString() == id);
            if (target != null)
            {
                InputEditId = id;
                if (DateTime.TryParse(target[1]?.ToString(), out var parsedDate))
                {
                    InputDate = parsedDate;
                }
                else
                {
                    InputDate = null;
                }
                InputPlace = target[2]?.ToString();
                StartCash = ParseDecimal(target.ElementAtOrDefault(3));
                EndCash = ParseDecimal(target.ElementAtOrDefault(4));
                DiscountTicket = ParseDecimal(target.ElementAtOrDefault(5));
                DailySales = ParseDecimal(target.ElementAtOrDefault(6));
                VenueCost = ParseDecimal(target.ElementAtOrDefault(7));
                MiscCost = ParseDecimal(target.ElementAtOrDefault(8));
                MonthlySales = ParseDecimal(target.ElementAtOrDefault(9));
                FinalCash = ParseDecimal(target.ElementAtOrDefault(10));
                Visitors = ParseDecimal(target.ElementAtOrDefault(11));
                InputNote = target.ElementAtOrDefault(12)?.ToString();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            Console.WriteLine("[OnPostSaveAsync] 実行開始");

            // InputEditId のエラーを除外（新規時に空でもOKにする）
            ModelState.Remove(nameof(InputEditId));

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[OnPostSaveAsync] ModelState 無効 → ページ再表示");

                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"  ❌ {kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                await OnGetAsync();
                return Page();
            }

            Console.WriteLine("[OnPostSaveAsync] 入力値 OK。新規/更新処理へ進む。");

            // --- 各種計算処理 ---
            var startcash = StartCash ?? 0;
            var endcash = EndCash ?? 0;
            var discountticket = DiscountTicket ?? 0;
            DailySales = endcash + discountticket - startcash;
            Console.WriteLine($"[OnPostSaveAsync] DailySales 計算完了: {DailySales}");

            // --- ID 採番処理 ---
            string id;
            if (string.IsNullOrEmpty(InputEditId))
            {
                // Googleスプレッドシートから現在の最大IDを取得して +1
                int nextId = await _sheetService.GetNextIdAsync("daybudget");
                id = nextId.ToString("D6"); // 6桁固定表示（例: 000047）
                Console.WriteLine($"[OnPostSaveAsync] 新規ID採番: {id}");
            }
            else
            {
                id = InputEditId;
                Console.WriteLine($"[OnPostSaveAsync] 既存ID使用: {id}");
            }

            // --- 新しい行データを構築 ---
            var newRow = new List<object>
    {
        id,
        InputDate?.ToString("yyyy/MM/dd"),
        InputPlace,
        StartCash,
        EndCash,
        DiscountTicket,
        DailySales,
        VenueCost,
        MiscCost,
        MonthlySales,
        FinalCash,
        Visitors,
        InputNote
    };
            Console.WriteLine($"[OnPostSaveAsync] newRow 作成完了: {string.Join(", ", newRow)}");

            // --- Google Sheets 登録処理 ---
            if (string.IsNullOrEmpty(InputEditId))
            {
                Console.WriteLine("[OnPostSaveAsync] 新規登録処理 → AppendDayBudgetRowAsync()");
                await _sheetService.AppendDayBudgetRowAsync(newRow);
                Console.WriteLine("[OnPostSaveAsync] AppendDayBudgetRowAsync() 完了");
            }
            else
            {
                Console.WriteLine("[OnPostSaveAsync] 既存更新処理 → UpdateRowByIdAsync()");
                await _sheetService.UpdateRowByIdAsync("daybudget", id, newRow);
                Console.WriteLine("[OnPostSaveAsync] UpdateRowByIdAsync() 完了");
            }

            Console.WriteLine("[OnPostSaveAsync] Redirect 実行");
            return RedirectToPage();
        }



        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _sheetService.DeleteRowByIdAsync("daybudget", id);
            return RedirectToPage();
        }

        private decimal? ParseDecimal(object? obj)
            => decimal.TryParse(obj?.ToString(), out var d) ? d : (decimal?)null;

    }
}

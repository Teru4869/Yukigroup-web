using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Yukigroup_WEB.Pages.Services
{
    public class GoogleSheetService
    {
        private readonly string spreadsheetId = "1PyB66kBkKy1kXHlrHcXkMM1W9Yq5PKd5bEmwLOx-LpI";
        private readonly string sheetName = "Yukigroupcustomersystem";
        private readonly string apiKey = "AIzaSyDU9pS8wZL35ABoS1kf3PclMaNv-Uy4sVA";

        public async Task<List<Participant>> LoadParticipantsAsync()
        {
            string encodedSheetName = Uri.EscapeDataString(sheetName);
            string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{encodedSheetName}?key={apiKey}";

            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<Participant>();

            var json = await response.Content.ReadAsStringAsync();
            var sheetData = JsonSerializer.Deserialize<GoogleSheetResponse>(json);

            var participants = new List<Participant>();

            foreach (var row in sheetData.values)
            {
                if (row.Count < 8) continue;

                participants.Add(new Participant
                {
                    ID = int.TryParse(row[0]?.ToString(), out var id) ? id : 0,
                    Name = row[1],
                    Country = row[2],
                    ParticipationCount = row[3],
                    Status = row[4],
                    Place = row[5],
                    LanguageSkill = row[6],
                    Note = row[7]
                });
            }

            return participants;
        }

        public async Task AppendRowAsync(IList<object> rowData)
        {
            var service = GetSheetService();
            var range = sheetName; // （"!A1"など固定しない）

            var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
            {
                Values = new List<IList<object>> { rowData }
            };

            var request = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            await request.ExecuteAsync();
        }



        private SheetsService GetSheetService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Yukigroup Web",
            });
        }

        public async Task<int> GetNextIdAsync()
        {
            var service = GetSheetService();

            // A列（ID列）の2行目以降を取得（ヘッダ行は除外）
            var range = $"{sheetName}!A2:A";
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = await request.ExecuteAsync();

            // まだデータが無い場合の初期値（お好みで変更OK）
            if (response.Values == null || response.Values.Count == 0)
                return 100000;

            // A列の数値の最大値を求めて +1
            int maxId = 0;
            foreach (var row in response.Values)
            {
                if (row.Count > 0 && int.TryParse(row[0]?.ToString(), out int id))
                {
                    if (id > maxId) maxId = id;
                }
            }
            return maxId + 1;
        }

        // 更新処理
        public async Task UpdateRowAsync(Participant updated)
        {
            var range = $"{sheetName}!A:H"; // 全列を対象に検索
            var service = GetSheetService();

            // まず全データを取得
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = await request.ExecuteAsync();

            if (response.Values == null) return;

            // IDで対象の行を探す
            int rowIndex = -1;
            for (int i = 0; i < response.Values.Count; i++)
            {

                if (response.Values[i].Count > 0 && response.Values[i][0].ToString() == updated.ID.ToString())
                {
                    rowIndex = i + 1; // シートは1始まり
                    break;
                }
            }

            if (rowIndex == -1) return;

            // 更新データを用意
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>>
        {
            new List<object>
            {
                updated.ID,
                updated.Name,
                updated.Country,
                updated.ParticipationCount,
                updated.Status,
                updated.Place,
                updated.LanguageSkill,
                updated.Note
            }
        }
            };

            // Update API 呼び出し
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"{sheetName}!A{rowIndex}:H{rowIndex}");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await updateRequest.ExecuteAsync();
        }

        // 削除処理
        public async Task DeleteRowAsync(int id)
        {
            var range = $"{sheetName}!A:H";
            var service = GetSheetService();

            // 全データを取得
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = await request.ExecuteAsync();

            if (response.Values == null) return;

            // IDで行番号を探す
            int rowIndex = -1;
            for (int i = 0; i < response.Values.Count; i++)
            {

                if (response.Values[i].Count > 0 && response.Values[i][0].ToString() == id.ToString())
                {
                    rowIndex = i + 1; // シートは1始まり
                    break;
                }
            }

            if (rowIndex == -1) return;

            // Clear API で対象行を削除
            var clearRequest = service.Spreadsheets.Values.Clear(new ClearValuesRequest(), spreadsheetId, $"{sheetName}!A{rowIndex}:H{rowIndex}");
            await clearRequest.ExecuteAsync();
        }


    }
}

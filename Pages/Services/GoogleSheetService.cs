using System.Text.Json;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Yukigroup_WEB.Pages.Services;
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
                        ID = row[0],
                        Name = row[1],
                        Country = row[2],
                        ParticipationCount = row[3],
                        Status = row[4],
                        Place = row[5],
                        JapaneseSkill = row[6],
                        Note = row[7]
                    });
                }

                return participants;
            }
    }
}

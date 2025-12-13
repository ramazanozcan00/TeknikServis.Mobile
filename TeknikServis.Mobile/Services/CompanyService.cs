using System.Net.Http.Json;

namespace TeknikServis.Mobile.Services
{
    public class CompanyService
    {
        private readonly HttpClient _httpClient;

        // Android emülatörü için localhost 10.0.2.2'dir.
        // Gerçek cihazda veya iOS'ta sunucunuzun gerçek IP adresini yazmalısınız.
        private const string BaseUrl = "http://10.0.2.2:5158"; // Port numaranızı (5000/5001 vb.) buraya yazın.

        public CompanyService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<List<string>> GetCompanyNamesAsync()
        {
            try
            {
                // API'deki GetAll metoduna istek atıyoruz
                var response = await _httpClient.GetAsync("/api/CompanySetting/GetAll");

                if (response.IsSuccessStatusCode)
                {
                    // Gelen JSON listesini string listesine çeviriyoruz
                    var companies = await response.Content.ReadFromJsonAsync<List<string>>();
                    return companies ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapılabilir
                Console.WriteLine($"Hata: {ex.Message}");
            }

            return new List<string>();
        }
    }
}
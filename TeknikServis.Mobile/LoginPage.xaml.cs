using System.Text;
using System.Text.Json;

namespace TeknikServis.Mobile;

public partial class LoginPage : ContentPage
{
    // Web API Adresi (Port: 5158)
    private const string ApiUrl = "http://10.0.2.2:5158/api/Auth/Test";

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void BtnLogin_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtUser.Text) || string.IsNullOrWhiteSpace(TxtPass.Text))
        {
            await DisplayAlert("Uyarý", "Kullanýcý adý ve þifre giriniz.", "Tamam");
            return;
        }

        Loading.IsRunning = true;
        BtnLogin.IsEnabled = false;

        try
        {
            var loginData = new { Username = TxtUser.Text.Trim(), Password = TxtPass.Text.Trim() };
            string json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // SSL Bypass
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };

            using (HttpClient client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.PostAsync(ApiUrl, content);

                // --- BAÞARILI ÝSE ---
                if (response.IsSuccessStatusCode)
                {
                    var resString = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<LoginResponse>(resString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data != null)
                    {
                        UserSession.UserId = data.UserId;
                        UserSession.CurrentBranchId = data.BranchId;
                        UserSession.FullName = data.FullName;
                        UserSession.Role = data.Role;
                        UserSession.IsLoggedIn = true;

                        Application.Current.MainPage = new MainPage();
                    }
                }
                // --- HATA VARSA (DETAYLI GÖSTER) ---
                else
                {
                    // Sunucudan gelen hata mesajýný oku
                    string sunucuMesaji = await response.Content.ReadAsStringAsync();

                    // Status Code (400, 401, 404, 500 vb.) bilgisini al
                    string kod = response.StatusCode.ToString();

                    // Ekrana tüm detaylarý bas
                    string hataMetni = $"Durum Kodu: {kod}\n";
                    hataMetni += $"Sunucu Mesajý: {(string.IsNullOrEmpty(sunucuMesaji) ? "Boþ Ýçerik" : sunucuMesaji)}";

                    await DisplayAlert("Giriþ Yapýlamadý", hataMetni, "Tamam");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Baðlantý Hatasý", $"Uygulama sunucuya eriþemedi.\nHata: {ex.Message}", "Tamam");
        }
        finally
        {
            Loading.IsRunning = false;
            BtnLogin.IsEnabled = true;
        }
    }

    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Message { get; set; }
    }
}
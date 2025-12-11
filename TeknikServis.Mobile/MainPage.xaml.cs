using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace TeknikServis.Mobile;

public partial class MainPage : TabbedPage
{
    // PORT NUMARASINA DİKKAT
    private const string ApiUrlBase = "http://10.0.2.2:5158/api/TicketApi";

    // Ekranda gösterilecek liste
    public ObservableCollection<TicketListItem> MyTickets { get; set; } = new ObservableCollection<TicketListItem>();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this; // XAML ile C# bağlantısını kurar

        // Giriş yapan kişinin adını başlığa yazabiliriz (İsteğe bağlı)
        this.Title = $"Hoşgeldin, {UserSession.FullName}";

        // Sayfa açılınca verileri çek
        LoadTickets();
    }

    // --- LİSTELEME ---
    private async void LoadTickets()
    {
        try
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
            using (HttpClient client = new HttpClient(handler))
            {
                // Şube ID'sini URL'e ekleyerek gönderiyoruz
                var url = $"{ApiUrlBase}/List/{UserSession.CurrentBranchId}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonSerializer.Deserialize<List<TicketListItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    MyTickets.Clear();
                    if (list != null)
                    {
                        foreach (var item in list) MyTickets.Add(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Hata olursa sessiz kal veya logla
            Console.WriteLine(ex.Message);
        }
    }

    // --- YENİ KAYIT ---
    private async void KaydetBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtAd.Text) || string.IsNullOrWhiteSpace(TxtTel.Text))
        {
            await DisplayAlert("Eksik Bilgi", "Müşteri adı ve telefon zorunludur.", "Tamam");
            return;
        }

        YukleniyorKayit.IsRunning = true;
        KaydetBtn.IsEnabled = false;

        // API'ye gönderilecek veri paketi
        var yeniKayit = new
        {
            Name = TxtAd.Text,
            Phone = TxtTel.Text,
            DeviceModel = TxtCihaz.Text,
            Problem = TxtSorun.Text,
            BranchId = UserSession.CurrentBranchId // <--- BU ÇOK ÖNEMLİ
        };

        try
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
            using (HttpClient client = new HttpClient(handler))
            {
                string json = JsonSerializer.Serialize(yeniKayit);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{ApiUrlBase}/Create", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Başarılı", "Kayıt oluşturuldu.", "Tamam");

                    // Formu Temizle
                    TxtAd.Text = ""; TxtTel.Text = ""; TxtCihaz.Text = ""; TxtSorun.Text = "";

                    // Listeyi Güncelle ve İlk Sekmeye Dön
                    LoadTickets();
                    CurrentPage = Children[0];
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Hata", $"Kayıt başarısız: {err}", "Tamam");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Bağlantı Hatası", ex.Message, "Tamam");
        }
        finally
        {
            YukleniyorKayit.IsRunning = false;
            KaydetBtn.IsEnabled = true;
        }
    }

    private void SorgulaBtn_Clicked(object sender, EventArgs e)
    {
        LoadTickets();
    }
}

// Liste için veri modeli
public class TicketListItem
{
    public string FisNo { get; set; }
    public string Musteri { get; set; }
    public string Cihaz { get; set; }
    public string Durum { get; set; }
    public string Tarih { get; set; }
}
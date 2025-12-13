using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace TeknikServis.Mobile;

public partial class MainPage : TabbedPage
{
    // PORT NUMARASINI KONTROL EDİN (Siyah ekrandakiyle aynı olmalı)
    private const string BaseUrl = "http://10.0.2.2:57584/api";

    // Ekrana Bağlı Listeler (Selectboxlar ve Liste için)
    public ObservableCollection<TicketListItem> MyTickets { get; set; } = new ObservableCollection<TicketListItem>();
    public ObservableCollection<IdNameDto> BrandList { get; set; } = new ObservableCollection<IdNameDto>();
    public ObservableCollection<IdNameDto> TypeList { get; set; } = new ObservableCollection<IdNameDto>();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        // 1. Arıza Kaydı Selectboxları (Marka ve Tür) Kaynağını Belirle
        PickerBrand.ItemsSource = BrandList;
        PickerType.ItemsSource = TypeList;

        // 2. Müşteri Kaydı İLLER (CityData.cs'den Statik)
        if (CityData.Iller != null)
        {
            foreach (var il in CityData.Iller.Keys) PickerCity.Items.Add(il);
        }

        // 3. MÜŞTERİ TİPLERİ (StaticData.cs'den Statik)
        PickerCustomerType.Items.Clear();
        foreach (var tip in StaticData.MusteriTipleri)
        {
            PickerCustomerType.Items.Add(tip);
        }
        PickerCustomerType.SelectedIndex = 0; // Varsayılan: Normal

        // Verileri Yükle
        LoadFormOptions();      // Arıza formu için veriler
        LoadCompanies();        // <-- YENİ: Firmaları Çeken Metod
        LoadTickets();          // Kayıt listesi
    }

    // ---------------------------------------------------------
    // 1. VERİ YÜKLEME METODLARI
    // ---------------------------------------------------------

    // Arıza Kaydı İçin Marka ve Cihaz Türlerini Çeker
    private async void LoadFormOptions()
    {
        try
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
            using (HttpClient client = new HttpClient(handler))
            {
                var response = await client.GetAsync($"{BaseUrl}/TicketApi/FormOptions");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<FormOptionsDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data != null)
                    {
                        BrandList.Clear();
                        TypeList.Clear();
                        foreach (var item in data.Brands) BrandList.Add(item);
                        foreach (var item in data.Types) TypeList.Add(item);
                    }
                }
            }
        }
        catch { }
    }

    // Müşteri Kaydı İçin Firmaları Çeker (CompanySetting API'den)
    // MainPage.xaml.cs içindeki LoadCompanies metodu
    private async void LoadCompanies()
    {
        try
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
            using (HttpClient client = new HttpClient(handler))
            {
                // Timeout eklemek, bağlantı sorununu daha hızlı anlamanızı sağlar
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync($"{BaseUrl}/CompanySettingApi/GetAll");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var companies = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (companies != null)
                    {
                        PickerCompany.Items.Clear();
                        foreach (var comp in companies)
                        {
                            PickerCompany.Items.Add(comp);
                        }
                    }
                }
                else
                {
                    // API hatası dönerse görmek için:
                    await DisplayAlert("Hata", $"Firma listesi alınamadı. Kod: {response.StatusCode}", "Tamam");
                }
            }
        }
        catch (Exception ex)
        {
            // Bağlantı hatasını ekranda görmek için:
            await DisplayAlert("Firma Yükleme Hatası", ex.Message, "Tamam");
        }
    }

    // Liste Sekmesi İçin Kayıtları Çeker
    private async void LoadTickets()
    {
        try
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
            using (HttpClient client = new HttpClient(handler))
            {
                var response = await client.GetAsync($"{BaseUrl}/TicketApi/List/{UserSession.CurrentBranchId}");
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
        catch { }
    }

    // ---------------------------------------------------------
    // 2. MÜŞTERİ KAYIT İŞLEMLERİ (SEKME 2)
    // ---------------------------------------------------------

    // İl Seçilince İlçeleri Doldur
    private void PickerCity_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (PickerCity.SelectedIndex == -1) return;

        string secilenIl = PickerCity.SelectedItem.ToString();

        PickerDistrict.Items.Clear();
        PickerDistrict.IsEnabled = true;

        if (CityData.Iller.ContainsKey(secilenIl))
        {
            foreach (var ilce in CityData.Iller[secilenIl])
            {
                PickerDistrict.Items.Add(ilce);
            }
        }
    }

    private async void BtnMusteriKaydet_Clicked(object sender, EventArgs e)
    {
        // Zorunlu alan kontrolü
        if (string.IsNullOrWhiteSpace(TxtAd.Text) || string.IsNullOrWhiteSpace(TxtSoyad.Text) || string.IsNullOrWhiteSpace(TxtTel.Text))
        {
            await DisplayAlert("Eksik Bilgi", "Ad, Soyad ve Telefon alanları zorunludur.", "Tamam");
            return;
        }

        LoaderMusteri.IsRunning = true;
        BtnMusteriKaydet.IsEnabled = false;

        // Selectbox'lardan seçilen değerleri al (Seçilmediyse boş string gönder)
        string firma = PickerCompany.SelectedIndex != -1 ? PickerCompany.SelectedItem.ToString() : "";
        string tip = PickerCustomerType.SelectedIndex != -1 ? PickerCustomerType.SelectedItem.ToString() : "Normal";
        string il = PickerCity.SelectedIndex != -1 ? PickerCity.SelectedItem.ToString() : "";
        string ilce = PickerDistrict.SelectedIndex != -1 ? PickerDistrict.SelectedItem.ToString() : "";

        var musteriData = new
        {
            BranchId = UserSession.CurrentBranchId,
            FirstName = TxtAd.Text?.Trim(),
            LastName = TxtSoyad.Text?.Trim(),
            Phone = TxtTel.Text?.Trim(),
            Phone2 = TxtTel2.Text,
            Email = TxtEmail.Text,
            TCNo = TxtTC.Text,
            Address = TxtAdres.Text,
            City = il,
            District = ilce,
            CompanyName = firma,
            CustomerType = tip,
            TaxOffice = TxtVergiDairesi.Text,
            TaxNumber = TxtVergiNo.Text
        };

        try
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
            using (HttpClient client = new HttpClient(handler))
            {
                string json = JsonSerializer.Serialize(musteriData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{BaseUrl}/Customer/Create", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Başarılı", "Müşteri kaydedildi.", "Tamam");
                    TemizleMusteriFormu();
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Hata", $"Kayıt yapılamadı: {err}", "Tamam");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Bağlantı Hatası", ex.Message, "Tamam");
        }
        finally
        {
            LoaderMusteri.IsRunning = false;
            BtnMusteriKaydet.IsEnabled = true;
        }
    }

    private void TemizleMusteriFormu()
    {
        TxtAd.Text = ""; TxtSoyad.Text = ""; TxtTC.Text = "";
        TxtTel.Text = ""; TxtTel2.Text = ""; TxtEmail.Text = "";
        TxtAdres.Text = ""; TxtVergiDairesi.Text = ""; TxtVergiNo.Text = "";
        PickerCity.SelectedIndex = -1;
        PickerDistrict.Items.Clear();
        PickerDistrict.IsEnabled = false;
        PickerCompany.SelectedIndex = -1;
        PickerCustomerType.SelectedIndex = 0;
    }

    // ---------------------------------------------------------
    // 3. ARIZA KAYDI İŞLEMLERİ (SEKME 1)
    // ---------------------------------------------------------

    private async void BtnServisKaydet_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtHizliAd.Text) || string.IsNullOrWhiteSpace(TxtHizliTel.Text))
        {
            await DisplayAlert("Hata", "Ad ve Telefon zorunludur.", "Tamam");
            return;
        }

        LoaderServis.IsRunning = true;
        BtnServisKaydet.IsEnabled = false;

        // Seçilen Marka ve Tür objelerini al (ID'leri için)
        var selectedBrand = PickerBrand.SelectedItem as IdNameDto;
        var selectedType = PickerType.SelectedItem as IdNameDto;

        var servisData = new
        {
            Name = TxtHizliAd.Text,
            Phone = TxtHizliTel.Text,
            DeviceModel = TxtHizliModel.Text,
            Problem = TxtHizliSorun.Text,
            BranchId = UserSession.CurrentBranchId,

            // Seçilmediyse boş Guid gönder (Sunucu halleder)
            DeviceBrandId = selectedBrand?.Id ?? Guid.Empty,
            DeviceTypeId = selectedType?.Id ?? Guid.Empty
        };

        try
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
            using (HttpClient client = new HttpClient(handler))
            {
                string json = JsonSerializer.Serialize(servisData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{BaseUrl}/TicketApi/Create", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Başarılı", "Arıza kaydı oluşturuldu!", "Tamam");

                    // Formu Temizle
                    TxtHizliAd.Text = ""; TxtHizliTel.Text = ""; TxtHizliModel.Text = ""; TxtHizliSorun.Text = "";
                    PickerBrand.SelectedIndex = -1; PickerType.SelectedIndex = -1;

                    // Listeyi Güncelle
                    LoadTickets();
                }
                else
                {
                    await DisplayAlert("Hata", "Kayıt başarısız.", "Tamam");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Bağlantı Hatası", ex.Message, "Tamam");
        }
        finally
        {
            LoaderServis.IsRunning = false;
            BtnServisKaydet.IsEnabled = true;
        }
    }

    // ---------------------------------------------------------
    // 4. LİSTE YENİLEME BUTONU
    // ---------------------------------------------------------
    private void SorgulaBtn_Clicked(object sender, EventArgs e)
    {
        LoadTickets();
    }
}

// ---------------------------------------------------------
// YARDIMCI SINIFLAR (DTO)
// ---------------------------------------------------------

public class TicketListItem
{
    public string FisNo { get; set; }
    public string Musteri { get; set; }
    public string Cihaz { get; set; }
    public string Durum { get; set; }
    public string Tarih { get; set; }
}

public class FormOptionsDto
{
    public List<IdNameDto> Brands { get; set; }
    public List<IdNameDto> Types { get; set; }
}

public class IdNameDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
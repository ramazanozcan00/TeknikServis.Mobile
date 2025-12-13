using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TeknikServis.Mobile.Services;

namespace TeknikServis.Mobile.ViewModels
{
    public class CustomerRegistrationViewModel : INotifyPropertyChanged
    {
        private readonly CompanyService _companyService;

        // Ekranda gösterilecek firma listesi
        public ObservableCollection<string> CompanyList { get; set; } = new ObservableCollection<string>();

        // Seçilen firma
        private string _selectedCompany;
        public string SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                _selectedCompany = value;
                OnPropertyChanged();
                // Firma seçildiğinde yapılacak işlemler buraya (örn: Diğer alanları doldurma)
            }
        }

        // Sayfa yüklenirken çağrılacak komut
        public ICommand LoadCompaniesCommand { get; }

        public CustomerRegistrationViewModel()
        {
            _companyService = new CompanyService();
            LoadCompaniesCommand = new Command(async () => await LoadCompaniesAsync());

            // ViewModel oluştuğunda verileri çekmeye başla
            Task.Run(LoadCompaniesAsync);
        }

        private async Task LoadCompaniesAsync()
        {
            var companies = await _companyService.GetCompanyNamesAsync();

            // UI thread'inde listeyi güncellememiz gerekir
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CompanyList.Clear();
                foreach (var company in companies)
                {
                    CompanyList.Add(company);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
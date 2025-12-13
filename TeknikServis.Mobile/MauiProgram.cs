using Microsoft.Extensions.Logging;
using TeknikServis.Mobile.Services; // Namespace'i eklemeyi unutmayın
using TeknikServis.Mobile.ViewModels;

namespace TeknikServis.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Servisleri ve Sayfaları Kaydedelim
            builder.Services.AddSingleton<CompanyService>();
            builder.Services.AddTransient<CustomerRegistrationViewModel>();
            builder.Services.AddTransient<MainPage>(); // Eğer MainPage kullanıyorsanız

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
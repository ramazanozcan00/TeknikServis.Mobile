namespace TeknikServis.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new LoginPage());
        }

        public static class UserSession
        {
            public static Guid CurrentBranchId { get; set; }
            public static string FullName { get; set; }
            public static bool IsLoggedIn { get; set; } = false;
        }
    }
}
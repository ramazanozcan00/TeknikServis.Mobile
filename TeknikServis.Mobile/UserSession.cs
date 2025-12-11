using System;

namespace TeknikServis.Mobile
{
    public static class UserSession
    {
        public static Guid CurrentBranchId { get; set; } // En önemli kısım: Şube ID
        public static Guid UserId { get; set; }
        public static string FullName { get; set; }
        public static string Role { get; set; }
        public static bool IsLoggedIn { get; set; } = false;
    }
}
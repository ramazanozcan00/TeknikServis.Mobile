namespace TeknikServis.Mobile
{
    // Web API'den dönen JSON verisinin aynısı
    public class TicketResponse
    {
        public string FisNo { get; set; }
        public string Cihaz { get; set; }
        public string SeriNo { get; set; }
        public string Durum { get; set; }
        public string Ariza { get; set; }
        public string GirisTarihi { get; set; }
        public string TahminiTeslim { get; set; }
        public string Ucret { get; set; }
    }
}
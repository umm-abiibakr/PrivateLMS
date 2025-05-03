namespace PrivateLMS.Services
{
    public class LocationService : ILocationService
    {
        public List<string> GetCountries()
        {
            return new List<string>
            {
                "Nigeria"
            };
        }

        public List<string> GetNigerianStates()
        {
            return new List<string>
            {
                "Abia", "Adamawa", "Akwa Ibom", "Anambra", "Bauchi", "Bayelsa", "Benue", "Borno",
                "Cross River", "Delta", "Ebonyi", "Edo", "Ekiti", "Enugu", "Gombe", "Imo",
                "Jigawa", "Kaduna", "Kano", "Katsina", "Kebbi", "Kogi", "Kwara", "Lagos",
                "Nasarawa", "Niger", "Ogun", "Ondo", "Osun", "Oyo", "Plateau", "Rivers",
                "Sokoto", "Taraba", "Yobe", "Zamfara", "FCT"
            };
        }
    }
}
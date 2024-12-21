
namespace MyProject.Models
{
    public class CountryGeneralInfo
    {
        public Name Name { get; set; }
        public List<string> Capital { get; set; }
        public List<string> Borders { get; set; }
        public CountryGeneralInfo()
        {
            Borders = new List<string>();
            Capital = new List<string>();
        }
    }
}
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        public string CommonName { get; set; }
        public string Capital { get; set; }
        public List<string> Borders { get; set; }
        public Country()
        {
            Borders = new List<string>();
        }
    }
}

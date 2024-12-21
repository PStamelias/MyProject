using Microsoft.EntityFrameworkCore;
namespace MyProject.Models
{
    public class WebAppContext:DbContext
    {
        public WebAppContext(DbContextOptions<WebAppContext> options):base(options)
        {

        }
        public DbSet<Country> Countries { get; set; }
    }
}

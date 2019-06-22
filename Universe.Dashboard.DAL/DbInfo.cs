using System.ComponentModel.DataAnnotations.Schema;

namespace Universe.Dashboard.DAL
{
    public class DbInfo
    {
        public int Id { get; set; }
        
        public string Version { get; set; }
    }
}

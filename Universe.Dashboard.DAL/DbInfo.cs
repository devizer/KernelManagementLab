using System.ComponentModel.DataAnnotations.Schema;

namespace Universe.Dashboard.DAL
{
    [Table("W3Top_DbInfo")]
    public class DbInfo
    {
        public int Id { get; set; }
        
        public string Version { get; set; }
    }
}

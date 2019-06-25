
using System.ComponentModel.DataAnnotations.Schema;

namespace Universe.Dashboard.DAL
{
    [Table("W3Top_HistoryCopy")]
    public class HistoryCopy
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string JsonBlob { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Universe.Dashboard.DAL
{
    [Table("W3Top_HistoryCopy")]
    public class HistoryCopyEntity
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string JsonBlob { get; set; }
    }
}

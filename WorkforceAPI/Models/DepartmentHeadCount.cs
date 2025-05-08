using System.ComponentModel.DataAnnotations.Schema;

namespace WorkforceAPI.Models
{
    [Table("DepartmentHeadCount")]
    public class DepartmentHeadCount
    {
        [Column("Department")]
        public string Department { get; set; } = string.Empty;

        [Column("HeadCount")]
        public int HeadCount { get; set; }
    }
}
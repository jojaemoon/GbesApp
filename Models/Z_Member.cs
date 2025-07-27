using System.ComponentModel.DataAnnotations;

namespace GBES.Models
{
    public class Z_Member
    {
        [Key]
        public int Id { get; set; }
        public string? division { get; set; }
        public string? city { get; set; }
        public string? memberName { get; set; }
        public string? logID { get; set; }
        public string? logPW { get; set; }
        public string? name { get; set; }
        public string? phone { get; set; }
        public string? specialOne { get; set; }

        public string? etc { get; set; }
    }
}

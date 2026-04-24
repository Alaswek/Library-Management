using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("Libraries")]
    public class Model_Library
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        public int AvailableSeats { get; set; }

        public bool IsActive { get; set; }
    }
}

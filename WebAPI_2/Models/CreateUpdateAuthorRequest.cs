using System.ComponentModel.DataAnnotations;

namespace WebAPI_2.Models
{
    public class CreateUpdateAuthorRequest
    {
        [Required]
        [MinLength(2)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        public string LastName { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }
    }
}

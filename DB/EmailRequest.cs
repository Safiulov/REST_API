using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class EmailRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}

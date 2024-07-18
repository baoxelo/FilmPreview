using System.ComponentModel.DataAnnotations;

namespace FilmPreview.Model
{
    public class RegistryUserDTO 
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(15, ErrorMessage = "Your password is limited to {2} to {1} characters"), MinLength(6), MaxLength(15)]
        public string Password { get; set; }

        [Required]
        public string Address { get; set; }
    }
    public class UpdateUserDTO
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
    }
    public class LoginUserDTO
    {
        [Required]
        [DataType(DataType.Text)]
        public string Account { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "Your password is limited to {2} to {1} characters"), MinLength(6), MaxLength(15)]
        public string Password { get; set; }
    }
    public class UserDTO 
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string Image { get; set; }
        public string Status { get; set; }

        [Required]
        public string Address { get; set; }

        public IList<string>? Roles { get; set; }

        public IList<InvoiceItemDTO> Cart { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmPreview.Data
{
    public class ApiUser : IdentityUser
    {
        [DisplayName("Full Name")]
        [StringLength(50, MinimumLength = 3,ErrorMessage = "Full name must have the length from {2} to {1} characters.")]
        [Column(TypeName = "nvarchar(50)")]
        public string FullName { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Image { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Address { get; set; }

        [DisplayName("Phone number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public UserList UserList { get; set; }
    }
}

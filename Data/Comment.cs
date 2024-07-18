using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace FilmPreview.Data
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Comment")]
        [Column(TypeName = "nvarchar(500)")]
        public string Content { get; set; }

        [DisplayName("Timestamp")]
        public DateTime Timestamp { get; set; }

        [ForeignKey(nameof(Film))]
        public string FilmId { get; set; }
        public Film Film { get; set; }

        [ForeignKey(nameof(ApiUser))]
        public string ApiUserId { get; set; }
        public ApiUser ApiUser { get; set; }
        
    }
}

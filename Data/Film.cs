using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmPreview.Data
{
    public class Film
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Film name")]
        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Film name must have the length from {2} to {1} characters.")]
        public string Name { get; set; }

        [DisplayName("Film description")]
        [Column(TypeName = "nvarchar(500)")]
        public string Description { get; set; }

        [DisplayName("Image")]
        [Column(TypeName = "nvarchar(500)")]
        public string Image { get; set; }

        [DisplayName("Trailer")]
        [Column(TypeName = "nvarchar(500)")]
        public string Trailer { get; set; }

        [DisplayName("Slug")]
        [Column(TypeName = "nvarchar(500)")]
        public string Slug { get; set; }

        [DisplayName("Release date")]
        public DateTime ReleaseDate { get; set; }
        public List<FilmGenre> FilmGenres { get; set; }
        public List<Comment> Comments { get; set; }

    }
}

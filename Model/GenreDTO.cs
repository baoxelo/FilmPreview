using FilmPreview.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmPreview.Model
{
    public class CreateGenreDTO
    {
        [Required]
        [DisplayName("Genre")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Genre name must have the length from {2} to {1} characters.")]
        public string Name { get; set; }

        [DisplayName("Genre image")]
        [Column(TypeName = "nvarchar(500)")]
        public FormFile ImageFile { get; set; }

    }
    public class UpdateGenreDTO : CreateGenreDTO { }
    public class GenreDTO : CreateGenreDTO
    {
        public List<FilmGenreDTO> FilmList { get; set; }

        public string Image { get; set; }
        public string Slug { get; set; }
    }
}

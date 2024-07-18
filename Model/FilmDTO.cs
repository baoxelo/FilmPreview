using FilmPreview.Data;
using System.ComponentModel.DataAnnotations;

namespace FilmPreview.Model
{
    public class CreateFilmDTO
    {
        [Required]
        [StringLength(maximumLength: 100, ErrorMessage = "Film Name Is Too Long")]
        public string Name { get; set; }


        [StringLength(maximumLength: 500, ErrorMessage = "Film Description Is Too Long")]
        public string Description { get; set; }

        public string Image { get; set; }

        public string Trailer { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int GenreId { get; set; }
    }

    public class UpdateFilmDTO : CreateFilmDTO { }

    public class FilmDTO : CreateFilmDTO
    {
        public GenreDTO Genre { get; set; }

        public List<FilmGenreDTO> FilmGenres { get; set; }
    }
}

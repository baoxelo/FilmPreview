using System.ComponentModel.DataAnnotations;

namespace FilmPreview.Model
{
    public class CreateGenreDTO
    {
        [Required]
        public string Name { get; set; }
    }
    public class UpdateGenreDTO : CreateGenreDTO { }
    public class GenreDTO : CreateGenreDTO
    {
        public IList<FilmDTO> Products { get; set; }
    }
}

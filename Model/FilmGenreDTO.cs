using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmPreview.Data
{
    public class FilmGenreDTO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Film))]
        public int FilmId { get; set; }
        public Film Film { get; set;}

        [ForeignKey(nameof(Genre))]
        public int GenreId { get; set;}
        public Genre Genre { get; set;}
    }
}

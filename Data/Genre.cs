using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmPreview.Data
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Genre name must have the length from {2} to {1} characters.")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Image {  get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Slug { get; set; }

        public List<FilmGenre> FilmList { get; set; }
       
    }
}

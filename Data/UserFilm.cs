using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmPreview.Data
{
    public class UserFilm
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserList))]
        public int UserListId { get; set; }
        public UserList UserList { get; set; }

        [ForeignKey(nameof(Film))]
        public int FilmId { get; set;}
        public Film Film { get; set; }

    }
}

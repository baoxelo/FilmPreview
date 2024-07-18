using FilmPreview.Data;

namespace FilmPreview.Model
{
    public class UserFilmDTO
    {
        public UserListDTO UserList { get; set; }

        public FilmDTO Film { get; set; }
    }
}

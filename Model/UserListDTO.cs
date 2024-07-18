using FilmPreview.Data;

namespace FilmPreview.Model
{
    public class UserListDTO
    {
        public UserDTO ApiUser { get; set; }

        public List<UserFilmDTO> UserFilms { get; set; }
    }
}

using FilmPreview.Data;

namespace FilmPreview.Model
{
    public class UserListDTO
    {
        public UserDTO ApiUser { get; set; }

        public List<FilmDTO> UserFilms { get; set; }
    }
}

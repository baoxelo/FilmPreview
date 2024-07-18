using AutoMapper;
using FilmPreview.Data;
using FilmPreview.Model;

namespace FilmPreview.Configuration
{
    public class MapperInitializer : Profile
    {
        public MapperInitializer() 
        {
            CreateMap<Film, FilmDTO>().ReverseMap();
            CreateMap<Film, CreateFilmDTO>().ReverseMap();
            CreateMap<Film, UpdateFilmDTO>().ReverseMap();

            CreateMap<Genre, GenreDTO>().ReverseMap();
            CreateMap<Genre, CreateGenreDTO>().ReverseMap();
            CreateMap<Genre, UpdateGenreDTO>().ReverseMap();

            CreateMap<Comment, CommentDTO>().ReverseMap();

            CreateMap<UserList, UserListDTO>().ReverseMap();
            CreateMap<UserFilm, UserFilmDTO>();

            CreateMap<ApiUser, RegistryUserDTO>().ReverseMap();
            CreateMap<ApiUser, UserDTO>().ReverseMap();



        }
    }
}

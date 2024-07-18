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
            CreateMap<Film, CreateProductDTO>().ReverseMap();
            CreateMap<Film, UpdateProductDTO>().ReverseMap();

            CreateMap<Genre, GenreDTO>().ReverseMap();
            CreateMap<Genre, CreateCategoryDTO>().ReverseMap();
            CreateMap<Genre, UpdateCategoryDTO>().ReverseMap();

            CreateMap<Comment, CommentDTO>().ReverseMap();
            CreateMap<Comment, CreateInvoiceDTO>().ReverseMap();
            CreateMap<InvoiceItem, InvoiceItemDTO>().ReverseMap();

            CreateMap<UserList, InvoiceItemDTO>().ReverseMap();
            CreateMap<UserList, CartItemDTO>().ReverseMap();
            CreateMap<UserList, AddCartDTO>().ReverseMap();

            CreateMap<Income, UserFilmDTO>().ReverseMap();
            CreateMap<UserFilm, ProductStaticDTO>();

            CreateMap<ApiUser, RegistryUserDTO>().ReverseMap();
            CreateMap<ApiUser, UserDTO>().ReverseMap();



        }
    }
}

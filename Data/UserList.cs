using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FilmPreview.Data
{
    public class UserList
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(ApiUser))]
        public string ApiUserId { get; set; }
        public ApiUser ApiUser { get; set; }

        public List<UserFilm> UserFilms { get; set; }

    }
}


using FilmPreview.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmPreview.Model
{   
    public class CommentDTO
    {
        public UserDTO User { get; set; }

        public string Content { get; set; }

        public DateTime Timestamp { get; set; }

        public FilmDTO Film { get; set; }
    }
}

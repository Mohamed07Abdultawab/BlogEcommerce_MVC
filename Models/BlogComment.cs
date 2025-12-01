using System.ComponentModel.DataAnnotations;

namespace BlogEcommerce.Models
{
    public class BlogComment
    {
        public int Id { get; set; }

        [Required]
        public int BlogPostId { get; set; }
        public BlogPost? BlogPost { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Comment { get; set; } = string.Empty;

        public DateTime CommentDate { get; set; }
    }
}
namespace BlogEcommerce.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }

        // Add this line
        public List<BlogComment> Comments { get; set; } = new List<BlogComment>();
    }
}
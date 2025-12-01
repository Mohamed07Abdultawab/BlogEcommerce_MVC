namespace BlogEcommerce.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public int Stock { get; set; }

        // Add this line
        public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}
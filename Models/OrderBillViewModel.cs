namespace BlogEcommerce.Models
{
    public class OrderBillViewModel
    {
        public Order Order { get; set; } = new Order();
        public string CompanyName { get; set; } = "BlogEcommerce";
        public string CompanyAddress { get; set; } = "123 Business Street";
        public string CompanyCity { get; set; } = "New York, NY 10001";
        public string CompanyPhone { get; set; } = "+1 (234) 567-8900";
        public string CompanyEmail { get; set; } = "billing@blogecommerce.com";
        public string CompanyWebsite { get; set; } = "www.blogecommerce.com";
    }
}
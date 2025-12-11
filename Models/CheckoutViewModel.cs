namespace BlogEcommerce.Models
{
    public class CheckoutViewModel
    {
        // 1. معلومات الطلب (لأغراض الإدخال والتخزين)
        public Order Order { get; set; } = new Order();

        // 2. عناصر السلة (لأغراض العرض والحساب)
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        // 3. خصائص الإجماليات (لتبسيط العرض في View)
        public decimal Subtotal { get; set; }
        public int TotalItemsCount => CartItems.Sum(i => i.Quantity);
    }
}
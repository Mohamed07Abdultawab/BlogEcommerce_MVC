using BlogEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogEcommerce.Controllers
{
    [Authorize] // Require authentication for all cart actions
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = GetUserId();

            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity)
            {
                TempData["ErrorMessage"] = "Product is out of stock or invalid quantity.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null)
            {
                // Create new cart item
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                // Update existing cart item
                if (cartItem.Quantity + quantity > product.Stock)
                {
                    TempData["ErrorMessage"] = $"Cannot add more items. Only {product.Stock} available in stock.";
                    return RedirectToAction("Details", "Products", new { id = productId });
                }

                cartItem.Quantity += quantity;
                _context.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{product.Name} added to cart successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var userId = GetUserId();

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else if (quantity > cartItem.Product!.Stock)
            {
                TempData["ErrorMessage"] = $"Only {cartItem.Product.Stock} items available in stock.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                cartItem.Quantity = quantity;
                _context.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = GetUserId();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item removed from cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            // Check stock availability
            foreach (var item in cartItems)
            {
                if (item.Product!.Stock < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"{item.Product.Name} doesn't have enough stock. Please update your cart.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Pre-fill customer info if user email is available
            var order = new Order
            {
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
            };

            return View(order);
        }

        // POST: Cart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            var userId = GetUserId();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            // Validate stock again before placing order
            foreach (var item in cartItems)
            {
                if (item.Product!.Stock < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"{item.Product.Name} doesn't have enough stock.";
                    return RedirectToAction(nameof(Index));
                }
            }

            order.OrderDate = DateTime.Now;
            order.Status = "Pending";
            order.TotalAmount = cartItems.Sum(c => c.Product!.Price * c.Quantity);

            // Create order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product!.Price
                };
                order.OrderItems.Add(orderItem);

                // Update product stock
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Stock -= cartItem.Quantity;
                    _context.Update(product);
                }
            }

            _context.Orders.Add(order);

            // Clear user's cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // GET: Cart/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Cart/GetCartCount (for navbar badge)
        public async Task<IActionResult> GetCartCount()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Json(new { count = 0 });
            }

            var userId = GetUserId();
            var count = await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);

            return Json(new { count });
        }
    }
}
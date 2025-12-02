using BlogEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Calculate statistics
            var totalProducts = await _context.Products.CountAsync();
            var totalBlogPosts = await _context.BlogPosts.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var totalRevenue = await _context.Orders.Where(o => o.Status != "Cancelled").SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            // Get low stock products
            var lowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            // Get recent blog posts
            var recentPosts = await _context.BlogPosts
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalBlogPosts = totalBlogPosts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.LowStockProducts = lowStockProducts;
            ViewBag.RecentPosts = recentPosts;

            return View();
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Orders(string status)
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            ViewBag.CurrentStatus = status;
            var ordersList = await orders.OrderByDescending(o => o.OrderDate).ToListAsync();

            return View(ordersList);
        }

        // GET: Admin/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order status updated successfully!";
            return RedirectToAction(nameof(OrderDetails), new { id = id });
        }

        // GET: Admin/Products
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(products);
        }

        // GET: Admin/BlogPosts
        public async Task<IActionResult> BlogPosts()
        {
            var posts = await _context.BlogPosts
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(posts);
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            // Orders by status
            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Revenue by month (last 6 months)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var revenueByMonth = await _context.Orders
                .Where(o => o.OrderDate >= sixMonthsAgo && o.Status != "Cancelled")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Top selling products
            var topProducts = await _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product!.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();

            ViewBag.OrdersByStatus = ordersByStatus;
            ViewBag.RevenueByMonth = revenueByMonth;
            ViewBag.TopProducts = topProducts;

            return View();
        }
    }
}
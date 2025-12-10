using BlogEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogEcommerce.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Get user statistics
            var totalOrders = await _context.Orders
                .Where(o => o.Email == user.Email)
                .CountAsync();

            var totalSpent = await _context.Orders
                .Where(o => o.Email == user.Email && o.Status != "Cancelled")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var totalReviews = await _context.ProductReviews
                .Where(r => r.UserName == user.Email)
                .CountAsync();

            var totalComments = await _context.BlogComments
                .Where(c => c.UserName == user.Email)
                .CountAsync();

            var model = new ProfileViewModel
            {
                UserId = userId,
                Email = user.Email ?? string.Empty,
                PhoneNumber = userProfile?.PhoneNumber,
                FullName = userProfile?.FullName,
                Address = userProfile?.Address,
                City = userProfile?.City,
                ZipCode = userProfile?.ZipCode,
                Country = userProfile?.Country,
                ProfilePictureUrl = userProfile?.ProfilePictureUrl,
                Bio = userProfile?.Bio,
                DateOfBirth = userProfile?.DateOfBirth,
                MemberSince = user.LockoutEnd?.DateTime ?? DateTime.Now,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                TotalReviews = totalReviews,
                TotalComments = totalComments
            };

            return View(model);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var model = new ProfileViewModel
            {
                UserId = userId,
                Email = user.Email ?? string.Empty,
                PhoneNumber = userProfile?.PhoneNumber,
                FullName = userProfile?.FullName,
                Address = userProfile?.Address,
                City = userProfile?.City,
                ZipCode = userProfile?.ZipCode,
                Country = userProfile?.Country,
                ProfilePictureUrl = userProfile?.ProfilePictureUrl,
                Bio = userProfile?.Bio,
                DateOfBirth = userProfile?.DateOfBirth
            };

            return View(model);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetUserId();
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userProfile == null)
            {
                // Create new profile
                userProfile = new UserProfile
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                _context.UserProfiles.Add(userProfile);
            }

            // Update profile
            userProfile.FullName = model.FullName;
            userProfile.PhoneNumber = model.PhoneNumber;
            userProfile.Address = model.Address;
            userProfile.City = model.City;
            userProfile.ZipCode = model.ZipCode;
            userProfile.Country = model.Country;
            userProfile.ProfilePictureUrl = model.ProfilePictureUrl;
            userProfile.Bio = model.Bio;
            userProfile.DateOfBirth = model.DateOfBirth;
            userProfile.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Profile/Orders
        public async Task<IActionResult> Orders()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Email == user!.Email)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Profile/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.Email == user!.Email);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Profile/Reviews
        public async Task<IActionResult> Reviews()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var reviews = await _context.ProductReviews
                .Include(r => r.Product)
                .Where(r => r.UserName == user!.Email)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return View(reviews);
        }

        // GET: Profile/Comments
        public async Task<IActionResult> Comments()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var comments = await _context.BlogComments
                .Include(c => c.BlogPost)
                .Where(c => c.UserName == user!.Email)
                .OrderByDescending(c => c.CommentDate)
                .ToListAsync();

            return View(comments);
        }

        // GET: Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Profile/DeleteAccount
        public IActionResult DeleteAccount()
        {
            return View();
        }

        // POST: Profile/DeleteAccountConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed(string password)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Verify password
            var passwordCheck = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordCheck)
            {
                TempData["ErrorMessage"] = "Incorrect password.";
                return RedirectToAction(nameof(DeleteAccount));
            }

            // Delete user profile
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (userProfile != null)
            {
                _context.UserProfiles.Remove(userProfile);
            }

            // Delete cart items
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();
            _context.CartItems.RemoveRange(cartItems);

            // Delete reviews
            var reviews = await _context.ProductReviews
                .Where(r => r.UserName == user.Email)
                .ToListAsync();
            _context.ProductReviews.RemoveRange(reviews);

            // Delete comments
            var comments = await _context.BlogComments
                .Where(c => c.UserName == user.Email)
                .ToListAsync();
            _context.BlogComments.RemoveRange(comments);

            await _context.SaveChangesAsync();

            // Delete user account
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                TempData["SuccessMessage"] = "Your account has been deleted.";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Error deleting account.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Profile/Wishlist
        public IActionResult Wishlist()
        {
            // Placeholder for wishlist feature
            return View();
        }

        // GET: Profile/Settings
        public async Task<IActionResult> Settings()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            ViewBag.Email = user?.Email;
            ViewBag.EmailConfirmed = user?.EmailConfirmed ?? false;

            return View();
        }

        // مثال كود C# في الكنترولر (Controller)

        public IActionResult UpdatePreferences(IFormCollection form)
        {
            // ... منطق حفظ التفضيلات في قاعدة البيانات ...

            // تعيين رسالة النجاح في TempData
            TempData["PreferencesUpdated"] = "Your privacy preferences have been successfully saved!";

            // إعادة التوجيه لنفس الصفحة
            return RedirectToAction("Settings");
        }


        // GET: Profile/ViewBill/5
        public async Task<IActionResult> ViewBill(int id)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.Email == user!.Email);

            if (order == null)
            {
                return NotFound();
            }

            var billViewModel = new OrderBillViewModel
            {
                Order = order
            };

            return View(billViewModel);
        }
    }
}
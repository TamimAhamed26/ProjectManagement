using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;
using System.Diagnostics;

namespace ProjectManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
         
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View(); 
            }

        
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(); 
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Employee"))
            {
                return RedirectToAction(nameof(EIndex));
            }

            return RedirectToAction(nameof(AIndex));
        }


        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public IActionResult AIndex()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> EIndex()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Index)); 
            }

            var tasks = _context.AssignedTasks
                .Include(t => t.Task)
                .Where(t => t.UserId == user.Id)
                .ToList();

            
            foreach (var task in tasks)
            {
                task.UpdateOverdueStatus();
            }
            await _context.SaveChangesAsync();

            return View(tasks);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}

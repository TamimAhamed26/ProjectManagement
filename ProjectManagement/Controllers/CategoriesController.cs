using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectManagement.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Tasklists)
                .Include(c => c.Department)
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.Department != null ? c.Department.Name : "",
                    TaskCount = c.Tasklists.Count,
                    TaskTitles = c.Tasklists.Select(t => t.Title).ToList(),
                    TaskIds = c.Tasklists.Select(t => t.TasklistId).ToList() // <-- add this
                })
                .ToListAsync();


            ViewBag.Departments = _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name
                })
                .ToList();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create(int? departmentId)
        {
            ViewBag.Departments = new SelectList(_context.Departments.ToList(), "DepartmentId", "Name", departmentId);
            return View();
        }



        // AJAX Create Category
        [HttpPost]
        public async Task<IActionResult> CreateCategoryAjax(string name, int? departmentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Name is required" });

            var category = new Category
            {
                Name = name,
                DepartmentId = departmentId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                categoryId = category.CategoryId,
                name = category.Name,
                departmentName = category.DepartmentId != null
                    ? (await _context.Departments.FindAsync(category.DepartmentId))?.Name
                    : "None"
            });
        }

        // AJAX Edit Category
        [HttpPost]
        public async Task<IActionResult> EditCategoryAjax(int id, string name, int? departmentId)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return Json(new { success = false, message = "Category not found" });

            category.Name = name;
            category.DepartmentId = departmentId;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                categoryId = category.CategoryId,
                name = category.Name,
                departmentName = category.DepartmentId != null
                    ? (await _context.Departments.FindAsync(category.DepartmentId))?.Name
                    : "None"
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategoryAjax(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Tasklists)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return Json(new { success = false, message = "Category not found" });

            // unassign tasks first
            foreach (var task in category.Tasklists)
            {
                task.CategoryId = null;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, categoryId = id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return Json(new { success = false, message = "Category not found" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }


        [HttpGet]
        public IActionResult GetUnassignedCategories()
        {
            var categories = _context.Categories
                .Where(c => c.DepartmentId == null)
                .Select(c => new { c.CategoryId, c.Name })
                .ToList();

            return Json(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCategoryToDepartment(int categoryId, int departmentId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                return Json(new { success = false, message = "Category not found" });

            category.DepartmentId = departmentId;
            await _context.SaveChangesAsync();

            return Json(new { success = true, categoryId = category.CategoryId, name = category.Name });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax(string name, int departmentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Name is required" });

            var category = new Category { Name = name, DepartmentId = departmentId };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, categoryId = category.CategoryId, name = category.Name });
        }
        [HttpGet]
        public IActionResult GetUnassignedTasks()
        {
            var tasks = _context.Tasklists
                .Where(t => t.CategoryId == null)
                .Select(t => new { taskId = t.TasklistId, title = t.Title })
                .ToList();
            return Json(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> AssignTaskToCategory(int taskId, int categoryId)
        {
            var task = await _context.Tasklists.FindAsync(taskId);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            task.CategoryId = categoryId;
            await _context.SaveChangesAsync();

            var category = await _context.Categories
                .Include(c => c.Tasklists)
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new
                {
                    categoryId = c.CategoryId,
                    taskCount = c.Tasklists.Count,
                    tasks = c.Tasklists.Select(t => new { taskId = t.TasklistId, title = t.Title })
                })
                .FirstOrDefaultAsync();

            return Json(new { success = true, category });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTaskFromCategory(int taskId)
        {
            var task = await _context.Tasklists.FindAsync(taskId);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            if (task.CategoryId == null) return Json(new { success = false, message = "Task is not assigned." });

            int categoryId = task.CategoryId.Value;
            task.CategoryId = null;
            await _context.SaveChangesAsync();

            var category = await _context.Categories
                .Include(c => c.Tasklists)
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new
                {
                    categoryId = c.CategoryId,
                    taskCount = c.Tasklists.Count,
                    tasks = c.Tasklists.Select(t => new { taskId = t.TasklistId, title = t.Title })
                })
                .FirstOrDefaultAsync();

            return Json(new { success = true, category });
        }



    }

}

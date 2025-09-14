using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;

namespace ProjectManagement.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class TasklistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasklistController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateCategoriesDropDownList(object selectedCategory = null)
        {
            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            categories.Insert(0, new Category { CategoryId = 0, Name = "Uncategorized" });

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", selectedCategory);
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasklists
                .Include(t => t.Category)
                .ToListAsync();

            foreach (var task in tasks)
            {
                if (task.Category == null)
                {
                    task.Category = new Category
                    {
                        CategoryId = 0,
                        Name = "Uncategorized"
                    };
                }
            }

            return View(tasks);
        }

        public IActionResult Create()
        {
            PopulateCategoriesDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tasklist task)
        {
            if (task.CategoryId == 0) 
                task.CategoryId = null;

            if (ModelState.IsValid)
            {
                _context.Tasklists.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateCategoriesDropDownList(task.CategoryId ?? 0);
            return View(task);
        }

    
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.Tasklists.FindAsync(id);
            if (task == null) return NotFound();

            PopulateCategoriesDropDownList(task.CategoryId ?? 0);
            return View(task);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tasklist model)
        {
            if (id != model.TasklistId) return NotFound();

            if (model.CategoryId == 0) 
                model.CategoryId = null;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tasklists.Any(e => e.TasklistId == model.TasklistId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateCategoriesDropDownList(model.CategoryId ?? 0);
            return View(model);
        }

    
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasklists
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TasklistId == id);

            if (task == null) return NotFound();

            if (task.Category == null)
            {
                task.Category = new Category
                {
                    CategoryId = 0,
                    Name = "Uncategorized"
                };
            }

            return View(task);
        }

     
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasklists.FindAsync(id);
            if (task != null)
            {
                _context.Tasklists.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


    }
}

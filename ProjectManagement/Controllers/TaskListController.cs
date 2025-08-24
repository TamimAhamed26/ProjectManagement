using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;

[Authorize(Roles = "Admin, SuperAdmin")]
public class TasklistController : Controller
{
    private readonly ApplicationDbContext _context;

    public TasklistController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Tasklists.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Tasklist task)
    {
        if (ModelState.IsValid)
        {
            _context.Tasklists.Add(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(task);
    }

    // GET: Edit
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _context.Tasklists.FindAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

 
    [HttpPost]
    public async Task<IActionResult> Edit(Tasklist model)
    {
        if (ModelState.IsValid)
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }


    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.Tasklists.FindAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }


    [HttpPost, ActionName("Delete")]
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;

[Authorize(Roles = "SuperAdmin,Admin")]
public class DepartmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Categories)
            .Select(d => new {
                d.DepartmentId,
                d.Name,
                Users = d.Users.Select(u => new { u.Id, u.Name, u.UserName }), // Fixed: Include Id
                Categories = d.Categories.Select(c => new { c.CategoryId, c.Name }) // Fixed: Include CategoryId
            })
            .ToListAsync();

        return Json(departments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAjax([FromForm] Department department)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return Json(department);
    }

    [HttpPost]
    public async Task<IActionResult> EditAjax(int departmentId, string name)
    {
        var dept = await _context.Departments.FindAsync(departmentId);
        if (dept == null) return NotFound();

        dept.Name = name;
        await _context.SaveChangesAsync();

        return Json(dept);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        var dept = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Categories)
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

        if (dept == null) return NotFound();

        foreach (var user in dept.Users)
        {
            user.DepartmentId = null;
        }

        foreach (var cat in dept.Categories)
        {
            cat.DepartmentId = null;
        }

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();

        return Json(new { success = true, id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return Json(new { success = false, message = "User not found." });

        if (user.DepartmentId == null)
            return Json(new { success = false, message = "User is not assigned to any department." });

        var departmentId = user.DepartmentId;
        user.DepartmentId = null;
        await _context.SaveChangesAsync();

     
        var dept = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Categories)
            .Where(d => d.DepartmentId == departmentId)
            .Select(d => new
            {
                departmentId = d.DepartmentId,
                name = d.Name,
                users = d.Users.Select(u => new { id = u.Id, name = u.Name, userName = u.UserName }),
                categories = d.Categories.Select(c => new { categoryId = c.CategoryId, name = c.Name })
            })
            .FirstOrDefaultAsync();

        return Json(new { success = true, department = dept });
    }


    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return Json(new { success = false, message = "Category not found." });

        if (category.DepartmentId == null)
            return Json(new { success = false, message = "Category is not assigned to any department." });

        var departmentId = category.DepartmentId;
        category.DepartmentId = null; 
        await _context.SaveChangesAsync();

        var dept = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Categories)
            .Where(d => d.DepartmentId == departmentId)
            .Select(d => new
            {
                departmentId = d.DepartmentId,
                name = d.Name,
                users = d.Users.Select(u => new { id = u.Id, name = u.Name, userName = u.UserName }),
                categories = d.Categories.Select(c => new { categoryId = c.CategoryId, name = c.Name })
            })
            .FirstOrDefaultAsync();

        return Json(new { success = true, department = dept });
    }

}
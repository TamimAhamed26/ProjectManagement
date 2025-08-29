
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Models;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }


    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ApplicationUser model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.PicturePath = user.PicturePath;
            return View(model);
        }

        user.Name = model.Name?.Trim();
        user.PhoneNo = model.PhoneNo?.Trim();
        user.Email = model.Email?.Trim();

        if (model.Picture != null && model.Picture.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(model.Picture.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("Picture", "Only JPG, JPEG, and PNG formats are allowed.");
                model.PicturePath = user.PicturePath;
                return View(model);
            }

            if (model.Picture.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("Picture", "File size cannot exceed 5 MB.");
                model.PicturePath = user.PicturePath;
                return View(model);
            }

            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string safeFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadDir, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.Picture.CopyToAsync(stream);
            }

            if (!string.IsNullOrEmpty(user.PicturePath))
            {
                string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PicturePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old file: {ex.Message}");
                    }
                }
            }

            user.PicturePath = $"/Pictures/{safeFileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

}
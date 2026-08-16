using HealthWellnessMVC.Data;
using HealthWellnessMVC.Models;
using HealthWellnessMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthWellnessMVC.Controllers;

[Authorize]
public class MentorshipController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MentorshipController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Administrator: assign a mentor (lecturer) to a mentee (student).
    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public async Task<IActionResult> Assign()
    {
        var lecturers = await _userManager.GetUsersInRoleAsync("Lecturer");
        var students = await _userManager.GetUsersInRoleAsync("Student");

        var vm = new MentorshipAssignViewModel
        {
            Mentors = lecturers.Select(u => (u.Id, u.FullName)).ToList(),
            Mentees = students.Select(u => (u.Id, u.FullName)).ToList()
        };
        return View(vm);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(MentorshipAssignViewModel model)
    {
        _db.MentorshipAssignments.Add(new MentorshipAssignment
        {
            MentorId = model.MentorId,
            MenteeId = model.MenteeId
        });
        await _db.SaveChangesAsync();

        TempData["Message"] = "Mentorship pairing created.";
        return RedirectToAction("Assign");
    }

    // Mentor/mentee: view assignments and session history.
    [HttpGet]
    public async Task<IActionResult> MyAssignments()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var assignments = await _db.MentorshipAssignments
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.Sessions)
            .Where(m => m.MentorId == user.Id || m.MenteeId == user.Id)
            .ToListAsync();

        return View(assignments);
    }

    // Mentor: record a new wellness card session.
    [HttpGet]
    public IActionResult CreateSession(int mentorshipAssignmentId)
    {
        return View(new WellnessCardSessionViewModel { MentorshipAssignmentId = mentorshipAssignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSession(WellnessCardSessionViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.WellnessCardSessions.Add(new WellnessCardSession
        {
            MentorshipAssignmentId = model.MentorshipAssignmentId,
            WellnessDimension = model.WellnessDimension,
            Notes = model.Notes
        });
        await _db.SaveChangesAsync();

        return RedirectToAction("MyAssignments");
    }
}

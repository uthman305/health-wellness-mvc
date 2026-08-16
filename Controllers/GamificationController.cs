using HealthWellnessMVC.Data;
using HealthWellnessMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthWellnessMVC.Controllers;

[Authorize]
public class GamificationController : Controller
{
    private readonly ApplicationDbContext _db;
    private const int PageSize = 20;

    public GamificationController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Institution-wide leaderboard, ranked by points, with pagination
    // (added during testing per the report to prevent slow loads with large user counts).
    [HttpGet]
    public async Task<IActionResult> Leaderboard(int page = 1)
    {
        var query = _db.Points
            .Include(p => p.User)
            .OrderByDescending(p => p.Total);

        var totalCount = await query.CountAsync();
        var rows = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(p => new LeaderboardRowViewModel
            {
                FullName = p.User!.FullName,
                Total = p.Total
            })
            .ToListAsync();

        for (int i = 0; i < rows.Count; i++)
            rows[i].Rank = (page - 1) * PageSize + i + 1;

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        return View(rows);
    }
}

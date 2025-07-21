using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskingSystem.Data;
using TaskingSystem.Models;

namespace TaskingSystem.Controllers
{
    public class AssignmentHeadLinesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignmentHeadLinesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AssignmentHeadLines
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            IQueryable<AssignmentHeadLine> assignments = _context.AssignmentHeadLines
                .Include(a => a.Course)
                .Include(a => a.Professor);

            // لو المستخدم مش سوبر أدمن، فلتر بالـ ProfessorId
            if (!User.IsInRole(Roles.SuperAdmin))
            {
                assignments = assignments.Where(a => a.ProfessorId == userId);
            }

            return View(await assignments.ToListAsync());
        }



        // GET: AssignmentHeadLines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignmentHeadLine = await _context.AssignmentHeadLines
                .Include(a => a.Course)
                .Include(a => a.Professor)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignmentHeadLine == null)
            {
                return NotFound();
            }

            return View(assignmentHeadLine);
        }

        // GET: AssignmentHeadLines/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole(Roles.SuperAdmin))
            {
                // لو سوبر أدمن، يختار من كل الدكاترة
                var professors = await _userManager.GetUsersInRoleAsync(Roles.Manger);
                ViewData["ProfessorId"] = new SelectList(professors, "Id", "UserName");

                // عرض كل الكورسات
                ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "CourseName");
            }
            else
            {
                // لو دكتور، هنعرضه تلقائي
                var currentUserId = _userManager.GetUserId(User);
                var currentProfessor = await _context.Users.FindAsync(currentUserId);

                ViewData["ProfessorName"] = currentProfessor?.UserName ?? "Unknown";
                ViewData["ProfessorId"] = currentUserId;

                // فلترة الكورسات الخاصة بالدكتور فقط
                var professorCourses = await _context.Courses
                    .Where(c => c.ProfessorId == currentUserId)
                    .ToListAsync();

                ViewData["CourseCode"] = new SelectList(professorCourses, "CourseCode", "CourseName");
            }

            return View();
        }




        // POST: AssignmentHeadLines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignmentId,AssignmentName,AssignmentDate,ProfessorId,CourseCode")] AssignmentHeadLine assignmentHeadLine)
        {
            if (assignmentHeadLine.AssignmentDate is null)
                assignmentHeadLine.AssignmentDate = DateTime.UtcNow.AddHours(2);

            if (ModelState.IsValid)
            {
                _context.Add(assignmentHeadLine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "CourseCode", assignmentHeadLine.CourseCode);
            ViewData["ProfessorId"] = new SelectList(_context.Users, "Id", "UserName", assignmentHeadLine.ProfessorId);
            return View(assignmentHeadLine);
        }

        // GET: AssignmentHeadLines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignmentHeadLine = await _context.AssignmentHeadLines.FindAsync(id);
            if (assignmentHeadLine == null)
            {
                return NotFound();
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "CourseCode", assignmentHeadLine.CourseCode);
            ViewData["ProfessorId"] = new SelectList(_context.Users, "Id", "UserName", assignmentHeadLine.ProfessorId);
            return View(assignmentHeadLine);
        }

        // POST: AssignmentHeadLines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,AssignmentName,AssignmentDate,ProfessorId,CourseCode")] AssignmentHeadLine assignmentHeadLine)
        {

            if (id != assignmentHeadLine.AssignmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignmentHeadLine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentHeadLineExists(assignmentHeadLine.AssignmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "CourseCode", assignmentHeadLine.CourseCode);
            ViewData["ProfessorId"] = new SelectList(_context.Users, "Id", "UserName", assignmentHeadLine.ProfessorId);
            return View(assignmentHeadLine);
        }

        // GET: AssignmentHeadLines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignmentHeadLine = await _context.AssignmentHeadLines
                .Include(a => a.Course)
                .Include(a => a.Professor)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignmentHeadLine == null)
            {
                return NotFound();
            }

            return View(assignmentHeadLine);
        }

        // POST: AssignmentHeadLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignmentHeadLine = await _context.AssignmentHeadLines.FindAsync(id);
            if (assignmentHeadLine != null)
            {
                _context.AssignmentHeadLines.Remove(assignmentHeadLine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssignmentHeadLineExists(int id)
        {
            return _context.AssignmentHeadLines.Any(e => e.AssignmentId == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskingSystem.Data;
using TaskingSystem.Models;

namespace TaskingSystem.Controllers
{
    [Authorize]
    public class CoursesRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesRegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CoursesRegistration
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(Roles.SuperAdmin))
            {
                var studentsCourses = await _context.StudentsCourses
                    .Include(s => s.Course)
                    .Include(s => s.Student)
                    .ToListAsync();

                return View(studentsCourses);
            }

            var userId = await _context.Users
                .Where(a => a.UserName == User.Identity.Name)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            if (userId == null)
            {
                return NotFound(); // or handle however is appropriate
            }

            if (User.IsInRole(Roles.Manger))
            {
                var studentCourses = await _context.StudentsCourses
                .Include(s => s.Course)
                .Include(s => s.Student)
                .Where(a => a.Course.ProfessorId == userId)
                .ToListAsync();

                return View(studentCourses);
            }
            else
            {
                var studentCourses = _context.StudentsCourses
                    .Include(s => s.Student)
                            .Include(sc => sc.Course)
                            .ThenInclude(p => p.Professor)
                            .Where(sc => sc.StudentId == userId);

                return View(studentCourses);

            }


        }


        // GET: CoursesRegistration/Create
        [Authorize(Roles = Roles.SuperAdmin)]
        public IActionResult Create()
        {
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "CourseCode");
            // ViewData["StudentId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["StudentId"] = new SelectList(_context.Users
                , "Id", "UserName");
            return View();
        }
        public async Task<IActionResult> SearchByStudent(string searchString)
        {
            var registrations = _context.StudentsCourses
    .Include(r => r.Student)
    .Include(r => r.Course)
    .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                registrations = registrations
                    .Where(r => r.Student.UserName.Contains(searchString));
            }

            var result = await registrations.ToListAsync();
            return View("Index", result); // نعرض نفس صفحة Index بالبيانات المتفلترة
        }


        // POST: CoursesRegistration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> Create([Bind("StudentId,CourseCode")] StudentsCourses studentsCourses)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(studentsCourses);
                    await _context.SaveChangesAsync();

                }
                catch (Exception)
                {

                    return RedirectToAction("Error", "Home", new ErrorViewModel() { Error = "This Record already exsist!" });
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "CourseCode", studentsCourses.CourseCode);
            ViewData["StudentId"] = new SelectList(_context.Users, "Id", "UserName", studentsCourses.StudentId);
            return View(studentsCourses);
        }



        // GET: CoursesRegistration/Delete/5
        public async Task<IActionResult> Delete(string CourseCode, string StudentId)
        {
            if (CourseCode == null && StudentId == null)
            {
                return NotFound();
            }

            var studentsCourses = await _context.StudentsCourses
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => a.StudentId == StudentId && a.CourseCode == CourseCode)
                .SingleOrDefaultAsync();

            if (studentsCourses == null)
            {
                return NotFound();
            }

            return View(studentsCourses);
        }

        // POST: CoursesRegistration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string CourseCode, string StudentId)
        {
            var studentsCourses = await _context.StudentsCourses.FindAsync(StudentId, CourseCode);
            if (studentsCourses != null)
            {
                _context.StudentsCourses.Remove(studentsCourses);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentsCoursesExists(string id)
        {
            return _context.StudentsCourses.Any(e => e.StudentId == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskingSystem.Data;
using TaskingSystem.Models;

namespace TaskingSystem.Controllers
{
    [Authorize]
    public class lecturesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;

        public lecturesController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: lectures
        [HttpGet]
        public async Task<IActionResult> Index(string course, string professor)
        {
            var lectures = _context.lectures.Include(l => l.Professor).Include(l => l.Course).AsQueryable();

            if (!string.IsNullOrEmpty(course))
                lectures = lectures.Where(l => l.CourseCode == course);

            if (!string.IsNullOrEmpty(professor))
                lectures = lectures.Where(l => l.Professor.UserName.Contains(professor));

            ViewData["Courses"] = new SelectList(await _context.Courses.ToListAsync(), "CourseCode", "CourseName");
            return View(await lectures.ToListAsync());
        }

        // GET: lectures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lecture = await _context.lectures.Include(l => l.Professor).Include(l => l.Course).FirstOrDefaultAsync(m => m.lectureId == id);
            if (lecture == null) return NotFound();

            return View(lecture);
        }

        // GET: lectures/Create
        public async Task<IActionResult> Create()
        {
            var currentUserName = User.Identity.Name; // جلب اسم المستخدم المسجل حالياً

            // جلب الكورسات الخاصة بالدكتور بناءً على اسم المستخدم
            var courses = await _context.Courses
                .Where(c => c.Professor.UserName == currentUserName)
                .ToListAsync();

            // تمرير الكورسات الخاصة بالدكتور فقط إلى الـ View باستخدام ViewData
            ViewData["Courses"] = new SelectList(courses, "CourseCode", "CourseName");

            return View();
        }


        // POST: lectures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("lectureId,lectureName,ProfessorId,CourseCode,lectureFile")] lecture lecture)
        {
            if (lecture.lectureFile != null)
            {
                lecture.lectureURL = UploadFile(lecture.lectureFile);
            }
            lecture.ProfessorId = _context.Users.Where(a => a.UserName == User.Identity.Name).Select(a => a.Id).SingleOrDefault();

            _context.Add(lecture);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


            ViewData["Courses"] = new SelectList(_context.Courses, "CourseCode", "CourseName", lecture.CourseCode);
            return View(lecture);
        }


        // GET: lectures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lecture = await _context.lectures.FindAsync(id);
            if (lecture == null) return NotFound();

            ViewData["Courses"] = new SelectList(_context.Courses, "CourseCode", "CourseName", lecture.CourseCode);
            TempData["CurrentLectureFile"] = lecture.lectureURL;
            return View(lecture);
        }

        // POST: lectures/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("lectureId,lectureName,ProfessorId,CourseCode,lectureFile")] lecture lecture)
        {
            if (id != lecture.lectureId) return NotFound();

            var oldLecture = await _context.lectures.AsNoTracking().FirstOrDefaultAsync(l => l.lectureId == id);
            if (oldLecture == null) return NotFound();

            if (lecture.lectureFile != null)
            {
                if (TempData["CurrentLectureFile"]?.ToString() != null)
                {
                    var oldPath = Path.Combine(_hostingEnvironment.WebRootPath, "lectures", TempData["CurrentLectureFile"].ToString());
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                lecture.lectureURL = UploadFile(lecture.lectureFile);
            }
            else
            {
                lecture.lectureURL = oldLecture.lectureURL;
            }

            try
            {
                _context.Update(lecture);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!lectureExists(lecture.lectureId)) return NotFound();
                throw;
            }
        }

        // GET: lectures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lecture = await _context.lectures.Include(l => l.Professor).Include(l => l.Course).FirstOrDefaultAsync(m => m.lectureId == id);
            if (lecture == null) return NotFound();

            return View(lecture);
        }

        // POST: lectures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lecture = await _context.lectures.FindAsync(id);
            if (lecture != null)
            {
                if (!string.IsNullOrEmpty(lecture.lectureURL))
                {
                    var path = Path.Combine(_hostingEnvironment.WebRootPath, "lectures", lecture.lectureURL);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                _context.lectures.Remove(lecture);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool lectureExists(int id)
        {
            return _context.lectures.Any(e => e.lectureId == id);
        }

        private string UploadFile(IFormFile file)
        {
            var uniqueFileName = GetUniqueFileName(file.FileName);
            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "lectures");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            var filePath = Path.Combine(uploads, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return uniqueFileName;
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName) + "_" + Guid.NewGuid().ToString().Substring(0, 4) + Path.GetExtension(fileName);
        }
    }
}

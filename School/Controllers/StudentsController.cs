using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School.Data;
using School.Models;

namespace School.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context)
        {
            _context = context;    
        }



        // GET: Students
        public async Task<IActionResult> Index(string sortOrder, string searchString,string currentFilter, int? page)
        {

            //These are ternary statements. The first one specifies 
            //that if the sortOrder parameter is null or empty, NameSortParm should be set to "name_desc"; 
            //otherwise, it should be set to an empty string.

            ViewData["CurrrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }


            ViewData["CurrentFilter"] = searchString;
            
            var students = from s in _context.Students
                select s;

            /**
             You've added a searchString parameter to the Index method. 
            The search string value is received from a text box that you'll add to the Index view. 
            You've also added to the LINQ statement a where clause that selects only students whose first name or last name contains the search string. 
            The statement that adds the where clause is executed only if there's a value to search for.
             */

            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                               || s.FirstMidName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }
            /**
             The method uses LINQ to Entities to specify the column to sort by. The code creates an IQueryable variable before the switch statement, 
            modifies it in the switch statement, and calls the ToListAsync method after the switch statement. When you create and modify IQueryable variables, 
            no query is sent to the database. The query is not executed until you convert the IQueryable object into a collection by calling a method 
            such as ToListAsync. Therefore, this code results in a single query that is not executed until the return View statement.
             */
            int pageSize = 3;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), page ?? 1, pageSize));
        }




        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists " +
                                             "see your system administrator.");
            }
           
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /*
             * The following code reads the existing entity and calls TryUpdateModel to update fields in the retrieved entity based on user input in the posted form data. 
             * The Entity Framework's automatic change tracking sets the Modified flag on the fields that are changed by form input.
             * When the SaveChanges method is called, the Entity Framework creates SQL statements to update the database row. 
             * Concurrency conflicts are ignored, and only the table columns that were updated by the user are updated in the database
             */

            var studentToUpdate = await _context.Students.SingleOrDefaultAsync(s => s.ID == id);
            //The empty string preceding the list of fields in the parameter list is for a prefix to use with the form fields names
            if (await TryUpdateModelAsync<Student>(studentToUpdate,"", s => 
                                                                        s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                                                 "Try again, and if the problem persists, " +
                                                 "see your system administrator.");
                }
            }
            return View(studentToUpdate);
        }

        // GET: Students/Delete/5
       
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                /*
                 When a database context retrieves table rows and creates entity objects that represent them, 
                 by default it keeps track of whether the entities in memory are in sync with what's in the database. 
                 The data in memory acts as a cache and is used when you update an entity. 
                 This caching is often unnecessary in a web application because context instances are typically short-lived 
                 (a new one is created and disposed for each request) and the context that reads an entity is typically disposed before that entity is used again.
                 */
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }


            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return RedirectToAction("Index");
            }

            try
            {

            }
            catch (DbUpdateException /*e*/ )
            {
                //Log the error (uncomment ex variable name and write a log)
                //Console.WriteLine(e);
                return RedirectToAction("Delete", new {id, saveChangesError = true});
             
            }


            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}

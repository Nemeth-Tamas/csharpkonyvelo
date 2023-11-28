using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KonyveloIroda.Data;
using KonyveloIroda.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;

namespace KonyveloIroda.Controllers
{
    [Authorize(Roles = "Default, Admin")]
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Files
        public async Task<IActionResult> Index()
        {
              return _context.Files != null ? 
                          View(await _context.Files.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Files'  is null.");
        }

        // GET: Files/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Files == null)
            {
                return NotFound();
            }

            var files = await _context.Files
                .FirstOrDefaultAsync(m => m.ID == id);
            if (files == null)
            {
                return NotFound();
            }

            ViewData["ID"] = id;

            return View(files);
        }

        [HttpGet]
        public IActionResult Download(int? id) {
            if (id == null || _context.Files == null)
            {
                return NotFound();
            }

            var files = _context.Files
                .FirstOrDefault(m => m.ID == id);
            if (files == null)
            {
                return NotFound();
            }

            return File(System.IO.File.ReadAllBytes(files.URL), 
                "application/force-download", files.FileName + "." + files.URL.Split(".").Last());
        }

        // GET: Files/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Files/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePost(String fileName)
        {
            System.Diagnostics.Debug.WriteLine("Calling CreatePost");

            try
            {
                System.Diagnostics.Debug.WriteLine("Inside try");
                var form = await Request.ReadFormAsync(new FormOptions()
                {
                    BufferBody = false, // Disable buffered body reading
                    MultipartBodyLengthLimit = long.MaxValue // Allow large file uploads
                });

                var file = form.Files[0];

                if (file != null && file.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine("File found");
                    var filePath = Path.Combine("wwwroot", "Tartalom", fileName + "." + file.FileName.Split(".").Last());

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    System.Diagnostics.Debug.WriteLine("File copied");

                    Files dbFile = new Files();
                    dbFile.UploadDate = DateTime.Now;
                    dbFile.FileName = fileName;
                    dbFile.URL = filePath;
                    _context.Add(dbFile);
                    await _context.SaveChangesAsync();

                    ViewBag.Message = "Fájl sikeresen feltöltve!";
                }
                else
                {
                    ViewBag.Message = "Nincs kiválasztott fájl!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Hiba történt a fájl feltöltése közben: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Files/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Files == null)
            {
                return NotFound();
            }

            var files = await _context.Files
                .FirstOrDefaultAsync(m => m.ID == id);
            if (files == null)
            {
                return NotFound();
            }

            return View(files);
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Files == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Files'  is null.");
            }
            var files = await _context.Files.FindAsync(id);
            if (files != null)
            {
                if (System.IO.File.Exists(files.URL))
                {
                    System.IO.File.Delete(files.URL);
                }
                _context.Files.Remove(files);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilesExists(int id)
        {
          return (_context.Files?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}

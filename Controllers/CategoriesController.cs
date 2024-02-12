using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sunrice.Contexts;
using Sunrice.Models;
using System.Diagnostics;

namespace Sunrice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        /// <summary>
        /// Get All Category
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public ActionResult GetCategories(string? search, string sortType, string sortOrder, int pageSize = 5, int pageNumber = 1) 
        {
            IQueryable<Category> cat = _context.Categorys.AsQueryable();

            if (string.IsNullOrEmpty(search) == false)
            {
                cat = cat.Where(c => c.Name.Contains(search) || c.Description.Contains(search));
            }
            if (sortType == "Name" && sortOrder == "asc")
            {
                cat = cat.OrderBy(c => c.Name);
            }
            else if (sortType == "Name" && sortOrder == "desc")
            {
                cat = cat.OrderByDescending(p => p.Name);
            }
            else if (sortType == "Description" && sortOrder == "asc")
            {
                cat = cat.OrderBy(c => c.Description);
            }
            else if (sortType == "Description" && sortOrder == "desc")
            {
                cat = cat.OrderByDescending(c => c.Description);
            }
            if (pageSize > 50) pageSize = 50;
            if (pageSize < 1) pageSize = 1;
            if (pageNumber < 1) pageNumber = 1;
            cat = cat.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            return Ok(cat.Include(p => p.Products).ToList());
        }
        /// <summary>
        /// Get Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult GetCategories(int id)
        {
            if (id == 0) return BadRequest();
            Category cat = _context.Categorys.Include(c => c.Products).FirstOrDefault(c => c.Id == id);
            if (cat == null) return NotFound();
            return Ok(cat);
        }

        /// <summary>
        /// Add Category
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public ActionResult PostCategory([FromForm]Category cat)
        {
            if (cat == null) return BadRequest(); 

            if (_context.Categorys.Any(c => c.Name == cat.Name))
            {
                ModelState.AddModelError("DuplicateName", "This name is registered to another category. Enter a different name.");
                return BadRequest(ModelState);
            }
            if (cat.Image == null)
            {
                cat.ImagePath = "\\images\\No.jpg";
            }
            else
            {
                string imgExtension = Path.GetExtension(cat.Image.FileName);
                Guid imgGiud = Guid.NewGuid();
                string imgname = imgGiud + imgExtension;
                cat.ImagePath = "\\images\\Categorys\\" + imgname;
                string imgFullPath = _webHostEnvironment.WebRootPath + cat.ImagePath;
                FileStream imgStream = new FileStream(imgFullPath, FileMode.Create);
                cat.Image.CopyTo(imgStream);
                imgStream.Dispose();
            }

            cat.CreatedAt = DateTime.Now;
            _context.Categorys.Add(cat);
            _context.SaveChanges();
            return CreatedAtAction("GetCategories", new { id = cat.Id }, cat);
        }
        /// <summary>
        /// Update Category
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpPut]
        public ActionResult PutCategory(Category cat)
        {
            if (cat == null) return BadRequest();
            if (_context.Categorys.Any(c => c.Name == cat.Name && c.Id != cat.Id))
            {
                ModelState.AddModelError("DuplicateName", "The Name is Dublicate.");
                return BadRequest(ModelState);
            }
            if (cat.Name == cat.Description)
            {
                ModelState.AddModelError("NameAndDescription", "The Name and Description must be Different.");
                return BadRequest(ModelState);
            }
            if (cat.Image != null)
            {
                if (cat.ImagePath != "\\images\\No.jpg")
                {
                    string OldImgFullPath = _webHostEnvironment.WebRootPath + cat.ImagePath;
                    if (System.IO.File.Exists(OldImgFullPath))
                    {
                        System.IO.File.Delete(OldImgFullPath);
                    }
                }
                string imgExtenstion = Path.GetExtension(cat.Image.FileName);
                Guid imgGiud = Guid.NewGuid();
                string imgname = imgGiud + imgExtenstion;
                cat.ImagePath = "\\images\\Categorys\\" + imgname;
                string imgFullPath = _webHostEnvironment.WebRootPath + cat.ImagePath;
                FileStream imgStream = new FileStream(imgFullPath, FileMode.Create);
                cat.Image.CopyTo(imgStream);
                imgStream.Dispose();

            }

            cat.LastUpdatedAt = DateTime.Now;
            _context.Categorys.Update(cat);
            _context.SaveChanges();
            return NoContent();

        }

        /// <summary>
        /// Delete Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpDelete("{id}")]
        public ActionResult DeleteCategory(int id)
        {
            if (id == 0) return BadRequest();

            Category cat = _context.Categorys.Find(id);
            if (cat == null) return NotFound();
            if (cat.ImagePath != "\\images\\Categorys\\")
            {
                string imgFullPath = _webHostEnvironment.WebRootPath + cat.ImagePath;
                if (System.IO.File.Exists(imgFullPath))
                {
                    System.IO.File.Delete(imgFullPath);
                }
            }

            _context.Categorys.Remove(cat);
            _context.SaveChanges();
            return NoContent();
        }


    }
}

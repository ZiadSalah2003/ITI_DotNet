using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sunrice.Contexts;
using Sunrice.Models;
using System.IO;

namespace Sunrice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets All Products
        /// </summary>
        /// <returns>List Of Products</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public ActionResult GetProducts(int catid,string? search, string sortType, string sortOrder, int pageSize=5 ,int pageNumber=1)
        {
            IQueryable<Product>procs=_context.Products.AsQueryable();
            if (catid != 0)
            {
                procs = procs.Where(p => p.CategoryId == catid);
            }
            if (string.IsNullOrEmpty(search) == false)
            {
                procs= procs.Where(p=>p.Name.Contains(search) || p.Description.Contains(search));   
            }
            if (sortType == "Name" && sortOrder == "asc")
            {
                procs = procs.OrderBy(p => p.Name);
            }
            else if (sortType == "Name" && sortOrder == "desc")
            {
                procs = procs.OrderByDescending(p => p.Name);
            }
            else if (sortType == "Description" && sortOrder == "asc")
            {
                procs = procs.OrderBy(p => p.Description);
            }
            else if (sortType == "Description" && sortOrder == "desc")
            {
                procs = procs.OrderByDescending(p => p.Description);
            }

            if (pageSize > 50) pageSize = 50;
            if (pageSize < 1) pageSize = 1;
            if (pageNumber < 1) pageNumber = 1;
            procs = procs.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            return Ok(procs.Include(p => p.Category).ToList());
        }

        /// <summary>
        /// Gets The Produts Name Price And Dec
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Product Object </returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult GetProduct(int id)
        {
            if (id == 0) return BadRequest();
            Product prod = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (prod == null) return NotFound();
            return Ok(prod);
        }

        /// <summary>
        /// Add New Products
        /// </summary>
        /// <param name="prod"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public ActionResult PostProducts([FromForm] Product prod)
        {
            if (prod == null) return BadRequest();
            if (_context.Categorys.Any(p => p.Name == prod.Name))
            {
                ModelState.AddModelError("DuplicateName", "This name is registered to another category. Enter a different name.");
                return BadRequest(ModelState);
            }
            if (prod.Name == prod.Description)
            {
                ModelState.AddModelError("NameAndDescription", "The Name and Description must be Different.");
                return BadRequest(ModelState);
            }
            if (prod.ProductionDate > DateTime.Now)
            {
                ModelState.AddModelError("ProductionDate", "The ProductionDate can't  be a future date.");
                return BadRequest(ModelState);
            }
            if (prod.Image == null)
            {
                prod.ImagePath = "\\images\\No.jpg";
            }
            else
            {
                string imgExtension = Path.GetExtension(prod.Image.FileName);
                Guid imgGiud = Guid.NewGuid();
                string imgname = imgGiud + imgExtension;
                prod.ImagePath = "\\images\\Products\\" + imgname;
                string imgFullPath = _webHostEnvironment.WebRootPath + prod.ImagePath;
                FileStream imgStream = new FileStream(imgFullPath, FileMode.Create);
                prod.Image.CopyTo(imgStream);
                imgStream.Dispose();
            }

            prod.CreatedAt = DateTime.Now;
            _context.Products.Add(prod);
            _context.SaveChanges();
            return CreatedAtAction("GetProducts", new { id = prod.Id }, prod);
        }

        /// <summary>
        /// Update Products
        /// </summary>
        /// <param name="prod"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpPut]
        public ActionResult PutProduct([FromForm]Product prod)
        {
            if (prod == null) return BadRequest();
            if (_context.Products.Any(p => p.Name == prod.Name && p.Id != prod.Id))
            {
                ModelState.AddModelError("DuplicateName", "The Name is Dublicate.");
                return BadRequest(ModelState);
            }
            if (prod.Name == prod.Description)
            {
                ModelState.AddModelError("NameAndDescription", "The Name and Description must be Different.");
                return BadRequest(ModelState);
            }
            if (prod.ProductionDate > DateTime.Now)
            {
                ModelState.AddModelError("ProductionDate", "The ProductionDate can't  be a future date.");
                return BadRequest(ModelState);
            }
            if (prod.Image != null)
            {
                if (prod.ImagePath != "\\images\\No.jpg")
                {
                    string OldImgFullPath = _webHostEnvironment.WebRootPath + prod.ImagePath;
                    if (System.IO.File.Exists(OldImgFullPath))
                    {
                        System.IO.File.Delete(OldImgFullPath);
                    }
                }
                string imgExtenstion = Path.GetExtension(prod.Image.FileName);
                Guid imgGiud = Guid.NewGuid();
                string imgname = imgGiud + imgExtenstion;
                prod.ImagePath = "\\images\\Products\\" + imgname;
                string imgFullPath = _webHostEnvironment.WebRootPath + prod.ImagePath;
                FileStream imgStream = new FileStream(imgFullPath, FileMode.Create);
                prod.Image.CopyTo(imgStream);
                imgStream.Dispose();

            }
            prod.LastUpdatedAt = DateTime.Now;
            _context.Products.Update(prod);
            _context.SaveChanges();
            return NoContent();
        }

        /// <summary>
        /// Delete Products
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(int id)
        {
            if (id == 0) return BadRequest();

            Product prod = _context.Products.Find(id);
            if (prod == null) return NotFound();
            if (prod.ImagePath != "\\images\\Products\\")
            {
                string imgFullPath = _webHostEnvironment.WebRootPath + prod.ImagePath;
                if (System.IO.File.Exists(imgFullPath))
                {
                    System.IO.File.Delete(imgFullPath);
                }
            }
            _context.Products.Remove(prod);
            _context.SaveChanges();
            return NoContent();
        }
    }
}

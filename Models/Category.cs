using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sunrice.Models
{
    public class Category
    {
        /// <summary>
        /// A Unique Int auto-generated Identifier
        /// </summary>
        public int Id { get; set; }

        [Required(ErrorMessage = "You  have to  provide a valid full name")]
        [MinLength(3, ErrorMessage = "Full Name  musn't be  less than  10 charcters.")]
        [MaxLength(50, ErrorMessage = "Full Name  musn't be  less than  50 charcters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "You  have to  provide a valid Description")]
        [MinLength(2, ErrorMessage = "Position  musn't be  less than  2 charcters.")]
        [MaxLength(20, ErrorMessage = "Position  musn't be  less than  20 charcters.")]
        public string Description { get; set; }

        [ValidateNever]
        public List<Product> Products { get; set; }

        [ValidateNever]
        public DateTime CreatedAt { get; set; }

        [ValidateNever]
        public DateTime LastUpdatedAt { get; set; }

        [ValidateNever]
        public string ImagePath { get; set; }

        [ValidateNever]
        [NotMapped]
        public IFormFile Image { get; set; }

    }
}

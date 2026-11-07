using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Request
{
    public class AddressRequestDTO
    {
        [Required(ErrorMessage = "Street is required")]
        [StringLength(100, ErrorMessage = "Street must be at most 100 characters")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Number is required")]
        [StringLength(20, ErrorMessage = "Number must be at most 20 characters")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City must be at most 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "State must contain 2 characters")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip code is required")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Invalid zip code format")]
        public string ZipCode { get; set; } = string.Empty;
    }
}

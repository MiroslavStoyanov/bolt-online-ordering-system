using System.ComponentModel.DataAnnotations;

namespace Bolt.Web.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
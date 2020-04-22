using System;
using System.ComponentModel.DataAnnotations;

namespace The_Wall.Models
{
    public class LoginUser
    {
        [Required(ErrorMessage="You must enter an email.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string LoginEmail {get; set;}
        
        [Required(ErrorMessage="You must enter an password.")]
        [DataType(DataType.Password)]
        public string LoginPassword {get; set;}

        
    }
}
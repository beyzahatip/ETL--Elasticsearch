using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ETL_Haritha.Models
{
    public class Employee
    {
        [Key]
        public string id { get; set; }

        [Required(ErrorMessage ="Enter Employee's First Name")]
        [Display(Name ="Employee First Name")]
        public string firstName { get; set; }

        [Required(ErrorMessage = "Enter Employee's Last Name")]
        [Display(Name = "Employee Last Name")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Enter Employee's Title")]
        [Display(Name = "Employee Title")]
        public string title { get; set; }
    }

}

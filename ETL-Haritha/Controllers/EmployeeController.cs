using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ETL_Haritha.Models;

namespace ETL_Haritha.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public EmployeeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var displayData = _db.EmployeeTable.ToList();
            return View(displayData);
        }
        [HttpGet]
        public async Task<IActionResult> Index(string Empsearch)
        {
            ViewData["Getemployeedetails"] = Empsearch;
            var empquery=from x in _db.EmployeeTable select x;
            if(!String.IsNullOrEmpty(Empsearch))
            {
                empquery = empquery.Where(x => x.firstName.Contains(Empsearch) || x.title.Contains(Empsearch));
            }
            return View(await empquery.AsNoTracking().ToListAsync());
        }
    }
}

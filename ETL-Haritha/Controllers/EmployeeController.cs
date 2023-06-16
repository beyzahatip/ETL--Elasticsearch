using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ETL_Haritha.Models;
using Nest;

namespace ETL_Haritha.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ElasticClient _elasticClient;

        public EmployeeController(ApplicationDbContext db)
        {
            _db = db;
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).
                DefaultMappingFor<Employee>(m => m.IndexName("employee-index"))
            .CertificateFingerprint("3e8456a26af11c4531c6dfb58e75c664140949264c3664f5679a085891a55f58")
                .BasicAuthentication("elastic", "K7AbrXX9s_K5jNtlRG4K");
            _elasticClient = new ElasticClient(settings);

            _elasticClient.Indices.Create("employee-index", index => index
               .Map<Employee>(m => m.AutoMap())
           );

            // Verileri Elasticsearch'e yükleme
            var employees = _db.EmployeeTable.ToList();

            foreach (var employee in employees)
            {
                var indexResponse = _elasticClient.IndexDocument(employee);
                if (!indexResponse.IsValid)
                {
                    // İşlem başarısız oldu
                }
            }
        }
        public IActionResult Index()
        {
            var displayData = _db.EmployeeTable.ToList();
            return View(displayData);
        }


        public IActionResult ElasticsearchIndex()
        {
            // Elasticsearch Employee endeksi oluşturma
            _elasticClient.Indices.Create("employee-index", index => index
                .Map<Employee>(m => m.AutoMap())
            );

            // Verileri Elasticsearch'e yükleme
            var employees = _db.EmployeeTable.ToList();

            foreach (var employee in employees)
            {
                var indexResponse = _elasticClient.IndexDocument(employee);
                if (!indexResponse.IsValid)
                {
                    // İşlem başarısız oldu
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ElasticsearchSearch(string query)
        {
            var searchResponse = _elasticClient.Search<Employee>(s => s
                .Index("employee-index")
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.title)
                        .Query(query)
                    )
                )
            );

            var hits = searchResponse.Hits;
            var searchResults = hits.Select(hit => hit.Source).ToList();

            return PartialView("_EmployeeTable", searchResults);
        }

        public IActionResult ElasticsearchFilterByTitle(string title)
        {
            var searchResponse = _elasticClient.Search<Employee>(s => s
                .Index("employee-index")
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.title)
                        .Query(title)
                    )
                )
            );

            var hits = searchResponse.Hits;
            var searchResults = hits.Select(hit => hit.Source).ToList();

            return PartialView("_EmployeeTable", searchResults);
        }

        public IActionResult ElasticsearchFilterByExperience(string operation, string value)
        {
            ISearchResponse<Employee> searchResponse = null;

            switch (operation)
            {
                case ">":
                    searchResponse = _elasticClient.Search<Employee>(s => s
                        .Index("employee-index")
                        .Query(q => q
                            .Range(r => r
                                .Field(f => f.experience)
                                .GreaterThan(int.Parse(value))
                            )
                        )
                    );
                    break;
                case "<":
                    searchResponse = _elasticClient.Search<Employee>(s => s
                        .Index("employee-index")
                        .Query(q => q
                            .Range(r => r
                                .Field(f => f.experience)
                                .LessThan(int.Parse(value))
                            )
                        )
                    );
                    break;
                case "=":
                    searchResponse = _elasticClient.Search<Employee>(s => s
                        .Index("employee-index")
                        .Query(q => q
                            .Term(t => t
                                .Field(f => f.experience)
                                .Value(int.Parse(value))
                            )
                        )
                    );
                    break;
                default:
                    // Yanlış bir işlem operatörü verildiğinde boş bir yanıt döndür
                    break;
            }

            var hits = searchResponse.Hits;

            var searchResults = hits.Select(hit => hit.Source).ToList();

            return PartialView("_EmployeeTable", searchResults);
        }


        [HttpGet]
        public async Task<IActionResult> Index(string Empsearch)
        {
            ViewData["Getemployeedetails"] = Empsearch;
            var empquery = from x in _db.EmployeeTable select x;
            if (!String.IsNullOrEmpty(Empsearch))
            {
                empquery = empquery.Where(x => x.firstName.Contains(Empsearch) || x.title.Contains(Empsearch));
            }
            return View(await empquery.AsNoTracking().ToListAsync());
        }

       

        

        //[HttpPost]
        //public IActionResult FilterByTitle(string title)
        //{
        //    var filteredData = _db.EmployeeTable.Where(x => x.title.Contains(title)).ToList();
        //    return PartialView("_EmployeeTable", filteredData);
        //}

        //public IActionResult FilterByExperience(string operation, string value)
        //{
        //    List<Employee> filteredData;

        //    switch (operation)
        //    {
        //        case ">":
        //            filteredData = _db.EmployeeTable.Where(x => x.experience > int.Parse(value)).ToList();
        //            break;
        //        case "<":
        //            filteredData = _db.EmployeeTable.Where(x => x.experience < int.Parse(value)).ToList();
        //            break;
        //        case "=":
        //            filteredData = _db.EmployeeTable.Where(x => x.experience == int.Parse(value)).ToList();
        //            break;
        //        default:
        //            filteredData = new List<Employee>();
        //            break;
        //    }

        //    return PartialView("_EmployeeTable", filteredData);
        //}





    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MyProject.Models;
using Newtonsoft.Json;
using RestSharp;
using Microsoft.EntityFrameworkCore;
using Azure;
namespace MyProject.Controllers
{
    [Route("api")]
    [ApiController]
    public class BasicController : ControllerBase
    {
        private readonly WebAppContext _context;
        private readonly IMemoryCache _cache;
        public BasicController(WebAppContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
    }

        [HttpPost]
        [Route("getSecondLargestIntofArray")]
        public async Task<IActionResult> GetSecondLargestIntofArray(RequestObj requestObj)
        {
            /* check if list is empty*/
            if (!requestObj.RequestArrayObj.Any())
                return BadRequest("RequestList cannot be Empty");

            /*sort RequestArrayObj*/
            var sortedArray = requestObj.RequestArrayObj.OrderByDescending(x => x);

            /*Return status  ok with the second largest interger of the array*/
            return Ok(sortedArray.ElementAt(1));
        }

        [HttpGet]
        [Route("retrieveCountries")]
        public async Task<IActionResult> RetrieveCountries()
        {

            var key = "MemoryCache";
            /*check if cache has data*/
            if (_cache.TryGetValue(key,out List<Country> countriesList))
            {
                return Ok(countriesList.Select(c=>new {c.CommonName,c.Capital,c.Borders}));
            }
            else
            {
                /*cache has no data , so we check the Db */
                var countriesListFromDb= await _context.Countries.ToListAsync();

                if (countriesListFromDb.Count==0)
                {
                    /*The cache and the database do not contain data*/
                    /*So make the http call*/
                    var client = new RestClient("https://restcountries.com/v3.1/independent?status=true");

                    var request = new RestRequest("independent", Method.Get);
                    request.AddQueryParameter("status", "true");
                    request.Timeout = TimeSpan.FromSeconds(600);


                    try
                    {

                        var response = await client.ExecuteAsync(request);
                        if (response.IsSuccessful)
                        {
                            var countries = GetCountriesFromJson(response);
                            await SaveCountries(countries);
                            /*I added TimeSpan to Cache for Testing*/
                            _cache.Set(key, countries,TimeSpan.FromMinutes(1));
                            /*Return each country without Id*/
                            return Ok(countries.Select(c => new { c.CommonName, c.Capital, c.Borders }).ToList());
                        }
                        else
                        {
                            return StatusCode(500,$"Http call ResponseError: {response.StatusCode}, {response.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500,$"Http call Error:{ex.Message}");
                    }

                }
                else
                {
                    /*Db contains data , so save them to cache*/
                    var countriesCache = await _context.Countries.Select(c => new Country {CommonName=c.CommonName,Capital=c.Capital,Borders=c.Borders }).ToListAsync();
                    /*I added TimeSpan to Cache for Testing*/
                    _cache.Set(key, countriesCache, TimeSpan.FromMinutes(1));
                    /*Return each country without Id*/
                    return Ok(countriesCache.Select(c => new { c.CommonName, c.Capital, c.Borders }).ToList());

                }

            }
            
        }

        private List<Country> GetCountriesFromJson(RestResponse response)
        {
            var jsonhttpResponseMessageContent = JsonConvert.DeserializeObject<List<CountryGeneralInfo>>(response.Content);
            List<Country> countries = new List<Country>();
            foreach (var jsonObjectCountry in jsonhttpResponseMessageContent)
            {
                Country country = new Country()
                {
                    Capital = jsonObjectCountry.Capital.FirstOrDefault(),
                    Borders = jsonObjectCountry.Borders,
                    CommonName = jsonObjectCountry.Name.Common
                };
                countries.Add(country);
            }
            return countries;
        }

        private  async Task SaveCountries(List<Country> countriesList)
        {
            
            _context.Countries.AddRange(countriesList);
            await _context.SaveChangesAsync();

        }
    }
}

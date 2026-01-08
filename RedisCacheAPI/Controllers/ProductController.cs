using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisCacheAPI.Services;

namespace RedisCacheAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ICacheService cacheService, ILogger<ProductController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var cacheKey = $"product_{id}";

            // Try to get from cache
            var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);

            if (cachedProduct != null)
            {
                _logger.LogInformation("Product {Id} retrieved from cache", id);
                return Ok(new { source = "cache", product = cachedProduct });
            }

            // Simulate database call
            var product = GetProductFromDatabase(id);

            if (product == null)
                return NotFound(new { message = $"Product {id} not found" });

            // Cache the result
            await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10));

            _logger.LogInformation("Product {Id} retrieved from database and cached", id);
            return Ok(new { source = "database", product });
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var cacheKey = "all_products";

            // Try to get from cache
            var cachedProducts = await _cacheService.GetAsync<List<Product>>(cacheKey);

            if (cachedProducts != null)
            {
                _logger.LogInformation("All products retrieved from cache");
                return Ok(new { source = "cache", products = cachedProducts, count = cachedProducts.Count });
            }

            // Simulate database call
            var products = GetAllProductsFromDatabase();

            // Cache the result
            await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(5));

            _logger.LogInformation("All products retrieved from database and cached");
            return Ok(new { source = "database", products, count = products.Count });
        }

        // Simulate database operations
        private Product? GetProductFromDatabase(int id)
        {
            // Simulate delay
            Thread.Sleep(1000);

            var products = GetAllProductsFromDatabase();
            return products.FirstOrDefault(p => p.Id == id);
        }

        private List<Product> GetAllProductsFromDatabase()
        {
            // Simulate delay
            Thread.Sleep(2000);

            return new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 999.99m, Category = "Electronics" },
                new Product { Id = 2, Name = "Mouse", Price = 29.99m, Category = "Electronics" },
                new Product { Id = 3, Name = "Keyboard", Price = 79.99m, Category = "Electronics" },
                new Product { Id = 4, Name = "Monitor", Price = 299.99m, Category = "Electronics" },
                new Product { Id = 5, Name = "Desk Chair", Price = 199.99m, Category = "Furniture" }
            };
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}


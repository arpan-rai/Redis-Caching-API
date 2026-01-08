# Redis API Demo

A .NET Core Web API starter project demonstrating Redis distributed caching using `IDistributedCache` and StackExchange.Redis.

## Features

- ✅ Redis distributed caching implementation
- ✅ Generic cache service with CRUD operations
- ✅ Sample product API with cache-aside pattern
- ✅ Swagger/OpenAPI documentation
- ✅ Configurable cache expiration
- ✅ Error handling and logging
- ✅ Clean architecture with service layer

## Prerequisites

- .NET 6.0 or later
- Redis Server (local or remote)
- Visual Studio 2022, VS Code, or any .NET-compatible IDE

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd RedisApiDemo
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Configure Redis Connection

Update the `appsettings.json` file with your Redis server details:

```json
{
  "Redis": {
    "ConnectionString": "REMOTE_REDIS_SERVER,user=admin,password=YOUR_PASSWORD_HERE,abortConnect=false,ConnectTimeout=5000,SyncTimeout=5000"
  }
}
```

**Connection String Options:**

- **With Authentication (Redis 6.0+):**
  ```
  REMOTE_REDIS_SERVER,user=admin,password=YOUR_PASSWORD,abortConnect=false
  ```

- **Without Authentication:**
  ```
  REMOTE_REDIS_SERVER,abortConnect=false
  ```

- **Local Redis:**
  ```
  localhost:6379,abortConnect=false
  ```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`
- Swagger UI: `https://localhost:7xxx/swagger`

## Project Structure

```
RedisApiDemo/
├── Controllers/
│   ├── CacheController.cs      # Direct cache operations API
│   └── ProductsController.cs   # Sample API with caching
├── Services/
│   ├── ICacheService.cs        # Cache service interface
│   └── CacheService.cs         # Cache service implementation
├── appsettings.json            # Configuration file
└── Program.cs                  # Application startup
```

## API Endpoints

### Cache Controller

#### Get Cached Value
```http
GET /api/cache/{key}
```

#### Set Cache Value
```http
POST /api/cache
Content-Type: application/json

{
  "key": "my_key",
  "value": "my_value",
  "expirationMinutes": 10
}
```

#### Delete Cache Value
```http
DELETE /api/cache/{key}
```

#### Check if Key Exists
```http
GET /api/cache/exists/{key}
```

#### Health Check
```http
GET /api/cache/health
```

### Products Controller

#### Get Product by ID (with caching)
```http
GET /api/products/{id}
```

#### Get All Products (with caching)
```http
GET /api/products
```

## Usage Examples

### Basic Caching

```csharp
// Inject the service
private readonly ICacheService _cacheService;

// Set a value
await _cacheService.SetAsync("user_123", userData, TimeSpan.FromMinutes(30));

// Get a value
var user = await _cacheService.GetAsync<User>("user_123");

// Remove a value
await _cacheService.RemoveAsync("user_123");

// Check existence
bool exists = await _cacheService.ExistsAsync("user_123");
```

### Cache-Aside Pattern Example

```csharp
public async Task<Product> GetProductAsync(int id)
{
    var cacheKey = $"product_{id}";
    
    // Try cache first
    var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);
    if (cachedProduct != null)
        return cachedProduct;
    
    // Cache miss - get from database
    var product = await _database.GetProductAsync(id);
    
    // Store in cache for next time
    await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10));
    
    return product;
}
```

## Testing with cURL

### Set a cache value
```bash
curl -X POST "https://localhost:7xxx/api/cache" \
  -H "Content-Type: application/json" \
  -d '{
    "key": "test_key",
    "value": {"name": "John", "age": 30},
    "expirationMinutes": 5
  }'
```

### Get a cache value
```bash
curl -X GET "https://localhost:7xxx/api/cache/test_key"
```

### Test caching performance
```bash
# First call - slow (from "database")
curl -X GET "https://localhost:7xxx/api/products/1"

# Second call - fast (from cache)
curl -X GET "https://localhost:7xxx/api/products/1"
```

## Configuration Options

### Redis Connection String Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `host:port` | Redis server address | `localhost:6379` |
| `user` | Username (Redis 6.0+) | - |
| `password` | Password | - |
| `abortConnect` | Abort on connection failure | `true` |
| `ConnectTimeout` | Connection timeout (ms) | `5000` |
| `SyncTimeout` | Sync operation timeout (ms) | `5000` |
| `ssl` | Use SSL/TLS | `false` |

### Cache Service Options

```csharp
// In Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "RedisApiDemo_"; // Key prefix
});
```

## Dependencies

- **Microsoft.Extensions.Caching.StackExchangeRedis** - Redis cache provider
- **StackExchange.Redis** - Redis client library
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI support

## Troubleshooting

### Connection Issues

If you can't connect to Redis:

1. Verify Redis is running:
   ```bash
   redis-cli -h 192.168.10.111 -p 6379 ping
   ```

2. Check firewall rules allow port 6379

3. Verify authentication credentials

4. Check Redis logs for connection attempts

### Cache Not Working

1. Enable detailed logging in `appsettings.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug"
       }
     }
   }
   ```

2. Check application logs for cache errors

3. Verify Redis has sufficient memory

## Best Practices

- Use meaningful cache keys with prefixes (e.g., `user_123`, `product_456`)
- Set appropriate expiration times based on data volatility
- Handle cache misses gracefully
- Monitor cache hit rates
- Consider cache invalidation strategy
- Use cache for expensive operations (database queries, API calls, complex calculations)
- Don't cache sensitive data without encryption


## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.


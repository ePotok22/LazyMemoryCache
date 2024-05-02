# LazyMemoryCache

LazyMemoryCache is a versatile caching library tailored for .NET applications, utilizing the powerful MemoryCache features provided by Microsoft.Extensions.Caching.Memory. Designed to improve application performance by efficiently managing data caching, LazyMemoryCache introduces easy-to-use mechanisms for adding, retrieving, and removing cached data.

## Features

- **Efficient Data Caching**: Utilizes MemoryCache to store data, reducing database or API call frequency.
- **Simple API**: Easy-to-use methods for managing cache, requiring minimal setup.
- **Automatic Cache Invalidation**: Includes functionality to automatically remove outdated or irrelevant cache entries.
- **Thread-Safe**: Ensures that cache operations are safe in a multi-threaded environment.

## Getting Started

### Prerequisites

Before you begin, ensure you have the .NET SDK installed on your system. This library is compatible with projects using .NET Core (all versions). You can download the latest .NET SDK from:

[.NET Downloads](https://dotnet.microsoft.com/download)

### Installation

To use LazyMemoryCache in your project, install it via NuGet:

```bash
dotnet add package LazyMemoryCache
```

## Usage
Here's how you can integrate LazyMemoryCache into your .NET applications:

### Setting Up MemoryCache
First, set up an instance of MemoryCache which LazyMemoryCache will manage internally:

``` csharp
using Microsoft.Extensions.Caching.Memory;
using LazyMemoryCache;

// Configure cache options
var cacheOptions = new MemoryCacheOptions()
{
    SizeLimit = 1024, // Limit size to 1024 entries
    CompactionPercentage = 0.2 // Compact 20% when max size is reached
};

// Initialize the cache
var cache = new LazyMemoryCache(cacheOptions);
``` 
### Adding and Retrieving Cache Entries
Add entries to the cache with specific cache entry options:

``` csharp
// Data to cache
var userData = new { Name = "John Doe", Age = 30 };

// Set cache with a 1-hour absolute expiration
cache.Set("userDetails", userData, new MemoryCacheEntryOptions()
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
    SlidingExpiration = TimeSpan.FromMinutes(30),
    Size = 1 // Set size if using a size limit
});

// Retrieve cached data
var cachedData = cache.Get<dynamic>("userDetails");
if (cachedData != null)
{
    Console.WriteLine($"Name: {cachedData.Name}, Age: {cachedData.Age}");
}
```

### Removing Cache Entries
Explicitly remove entries from the cache when they are no longer needed:

```  csharp
// Remove the cache entry
cache.Remove("userDetails");
``` 
## Contributing
We welcome contributions to make LazyMemoryCache even better! Here are some ways you can contribute:

1. Report bugs and suggest features.
2. Contribute to the codebase with enhancements and fixes.

### Getting started with contributions
1. Fork the repository.
2. Create a new branch for your feature (git checkout -b feature/YourFeature).
3. Commit your changes (git commit -am 'Add some YourFeature').
4. Push to the branch (git push origin feature/YourFeature).
5. Open a pull request.

## License
LazyMemoryCache is open-sourced software licensed under the MIT license.

## Contact
Project Repository: https://github.com/yourusername/LazyMemoryCache
Creator's Contact: [your.email

namespace Yarpsul.Catalog.Products;

internal sealed class ProductService
{
    private readonly List<Product> _products;
    public ProductService()
    {
        _products = new List<Product>
        {
            new Product(1, "Domain-Driven Design: Tackling Complexity in the Heart of Software 1st Edition by Eric Evans", 35.99m),
            new Product(2, "Implementing Domain-Driven Design 1st Edition by Vaughn Vernon", 46.82m),
            new Product(3, "Software Architecture: The Hard Parts: Modern Trade-Off Analyses for Distributed Architectures 1st Edition by Neal Ford, Mark Richards", 24.00m),
            new Product(4, "Building Microservices: Designing Fine-Grained Systems 2nd Edition by Sam Newman", 47.99m),
            new Product(5, "Enterprise Integration Patterns: Designing, Building, and Deploying Messaging Solutions 1st Edition by Gregor Hohpe, Bobby Woolf", 47.91m)
        };
    }

    public List<Product> GetAllProducts() => _products;

    public Product? GetProduct(int id) => _products.FirstOrDefault(p => p.Id == id);
}
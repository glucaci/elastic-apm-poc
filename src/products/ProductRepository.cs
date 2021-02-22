using System.Collections.Generic;
using System.Linq;

namespace Demo.Products
{
    public class ProductRepository
    {
        private readonly Dictionary<int, Product> _products;

        public ProductRepository()
        {
            _products = new Product[]
            {
                // Simulate EXEC_NON_NULL_VIOLATION
                new Product(1, default, 899, 100),
                new Product(2, "Couch", 1299, 1000),
                new Product(3, "Chair", 54, 50),
                new Product(4, "Regal", 1599, 1300)
            }.ToDictionary(t => t.Upc);
        }

        public IEnumerable<Product> GetTopProducts(int first) => 
            _products.Values.OrderBy(t => t.Upc).Take(first);

        public Product GetProduct   (int upc) => _products[upc];
    }
}
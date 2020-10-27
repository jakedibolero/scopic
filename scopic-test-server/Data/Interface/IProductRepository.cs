using System;
using System.Collections.Generic;
using scopic_test_server.Data;
using scopic_test_server.DTO;

namespace scopic_test_server.Interface
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAllProducts(int Page, int Role, bool? Sort, string SearchString);
        Product GetProduct(Guid ProductId);
        Product AddProduct(ProductCreateDto Product);

    }
}
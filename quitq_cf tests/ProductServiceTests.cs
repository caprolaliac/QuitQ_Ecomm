using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using quitq_cf.Data;
using quitq_cf.Models;
using quitq_cf.DTO;
using quitq_cf.Repository;

namespace Quit_q_test
{
    [TestFixture]
    public class ProductServiceTests
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<IMapper> _mockMapper;
        private ProductService _productService;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockMapper = new Mock<IMapper>();
            _productService = new ProductService(_mockDbContext.Object, _mockMapper.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            var queryableData = data.AsQueryable();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            return mockSet;
        }

        [Test]
        public async Task GetAllProductsAsync_ShouldReturnProductDTOs()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Product 1", SellerId = "Seller1" },
                new Product { ProductId = 2, ProductName = "Product 2", SellerId = "Seller2" }
            };

            var productDTOs = new List<ProductDTO>
            {
                new ProductDTO { ProductId = 1, ProductName = "Product 1" },
                new ProductDTO { ProductId = 2, ProductName = "Product 2" }
            };

            var mockProductSet = CreateMockDbSet(products);
            _mockDbContext.Setup(db => db.Products).Returns(mockProductSet.Object);
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDTO>>(It.IsAny<IEnumerable<Product>>())).Returns(productDTOs);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Product 1", result.First().ProductName);
        }

        [Test]
        public async Task GetProductByIdAsync_ShouldReturnProductDTO()
        {
            // Arrange
            var product = new Product { ProductId = 1, ProductName = "Product 1", SellerId = "Seller1" };
            var productDTO = new ProductDTO { ProductId = 1, ProductName = "Product 1" };

            var mockProductSet = CreateMockDbSet(new List<Product> { product });
            _mockDbContext.Setup(db => db.Products).Returns(mockProductSet.Object);
            _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<Product>())).Returns(productDTO);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Product 1", result.ProductName);
        }

        //[Test]
        //public async Task CreateProductAsync_ShouldReturnSuccessResponse()
        //{
        //    // Arrange
        //    var createProductDto = new CreateProductDTO { ProductName = "New Product", Price = 10.0m };
        //    var sellerId = "Seller1";
        //    var expectedResponse = new Response { Status = "Success", Message = "Product created successfully" };

        //    _mockMapper.Setup(m => m.Map<Product>(It.IsAny<CreateProductDTO>())).Returns(new Product { ProductId = 1, ProductName = "New Product", SellerId = sellerId });
        //    _mockDbContext.Setup(db => db.Products.AddAsync(It.IsAny<Product>(), default)).ReturnsAsync(new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Product>(new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Product>>().Object));
        //    _mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1); // Simulating a successful save

        //    // Act
        //    var result = await _productService.CreateProductAsync(sellerId, createProductDto);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Success", result.Status);
        //    Assert.AreEqual("Product created successfully", result.Message);
        //}

        [Test]
        public async Task UpdateProductAsync_ShouldReturnSuccessResponse()
        {
            // Arrange
            var productDto = new UpdateProductDTO { ProductName = "Updated Product", Price = 15.0m };
            var sellerId = "Seller1";
            var productId = 1;
            var existingProduct = new Product { ProductId = 1, ProductName = "Old Product", SellerId = sellerId };
            var expectedResponse = new Response { Status = "Success", Message = "Product updated successfully" };

            var mockProductSet = CreateMockDbSet(new List<Product> { existingProduct });
            _mockDbContext.Setup(db => db.Products).Returns(mockProductSet.Object);
            _mockMapper.Setup(m => m.Map(It.IsAny<UpdateProductDTO>(), It.IsAny<Product>()));

            // Act
            var result = await _productService.UpdateProductAsync(productId, sellerId, productDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result.Status);
            Assert.AreEqual("Product updated successfully", result.Message);
        }

        [Test]
        public async Task DeleteProductAsync_ShouldReturnSuccessResponse()
        {
            // Arrange
            var productId = 1;
            var sellerId = "Seller1";
            var existingProduct = new Product { ProductId = 1, ProductName = "Product to delete", SellerId = sellerId };
            var expectedResponse = new Response { Status = "Success", Message = "Product deleted successfully" };

            var mockProductSet = CreateMockDbSet(new List<Product> { existingProduct });
            _mockDbContext.Setup(db => db.Products).Returns(mockProductSet.Object);
            _mockDbContext.Setup(db => db.Products.Remove(It.IsAny<Product>()));

            // Act
            var result = await _productService.DeleteProductAsync(productId, sellerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result.Status);
            Assert.AreEqual("Product deleted successfully", result.Message);
        }

        [Test]
        public async Task UpdateStockAsync_ShouldReturnSuccessResponse()
        {
            // Arrange
            var productId = 1;
            var newStock = 50;
            var existingProduct = new Product { ProductId = 1, ProductName = "Product", Stock = 10 };

            var expectedResponse = new Response { Status = "Success", Message = "Stock updated successfully" };

            _mockDbContext.Setup(db => db.Products.FindAsync(productId)).ReturnsAsync(existingProduct);
            _mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1); // Simulate successful update

            // Act
            var result = await _productService.UpdateStockAsync(productId, newStock);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result.Status);
            Assert.AreEqual("Stock updated successfully", result.Message);
        }
    }
}

using Moq;
using NUnit.Framework;
using AutoMapper;
using quitq_cf.Data;
using quitq_cf.DTO;
using quitq_cf.Models;
using quitq_cf.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;

[TestFixture]
public class OrderServiceTests
{
    private Mock<ApplicationDbContext> _mockContext;
    private Mock<IMapper> _mockMapper;
    private OrderService _orderService;

    [SetUp]
    public void SetUp()
    {
        _mockContext = new Mock<ApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _orderService = new OrderService(_mockContext.Object, _mockMapper.Object);
    }

    [Test]
    public async Task CancelOrderAsync_ShouldReturnSuccess_WhenOrderIsCancelled()
    {
        var orderId = 1;
        var mockOrder = new Order
        {
            OrderId = orderId,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { ProductId = 1, Quantity = 2 }
            }
        };

        // Mocking DbSet<Order> to return an IQueryable
        var mockOrders = new List<Order> { mockOrder }.AsQueryable();
        var mockOrderSet = new Mock<DbSet<Order>>();

        mockOrderSet.As<IQueryable<Order>>()
            .Setup(m => m.Provider)
            .Returns(mockOrders.Provider);
        mockOrderSet.As<IQueryable<Order>>()
            .Setup(m => m.Expression)
            .Returns(mockOrders.Expression);
        mockOrderSet.As<IQueryable<Order>>()
            .Setup(m => m.ElementType)
            .Returns(mockOrders.ElementType);
        mockOrderSet.As<IQueryable<Order>>()
            .Setup(m => m.GetEnumerator())
            .Returns(mockOrders.GetEnumerator());

        // Setting up the mock DbContext to return the mocked Orders
        _mockContext.Setup(c => c.Orders)
            .Returns(mockOrderSet.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _orderService.CancelOrderAsync(orderId);

        Assert.AreEqual("Success", result.Status);
        Assert.AreEqual("Order cancelled successfully", result.Message);
    }

    [Test]
    public async Task CreateOrderAsync_ShouldThrowException_WhenCartIsEmpty()
    {
        var userId = "user123";
        var createOrderDto = new CreateOrderDTO { ShippingAddress = "Test Address", PaymentMethod = "CreditCard" };

        // Mocking DbSet<Cart> to return an IQueryable
        var mockCarts = new List<Cart>().AsQueryable(); // Empty cart to simulate exception
        var mockCartSet = new Mock<DbSet<Cart>>();

        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.Provider)
            .Returns(mockCarts.Provider);
        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.Expression)
            .Returns(mockCarts.Expression);
        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.ElementType)
            .Returns(mockCarts.ElementType);
        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.GetEnumerator())
            .Returns(mockCarts.GetEnumerator());

        // Setting up the mock DbContext to return the mocked Carts
        _mockContext.Setup(c => c.Carts)
            .Returns(mockCartSet.Object);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, createOrderDto));
        Assert.AreEqual("Cart is empty", ex.Message);
    }

    [Test]
    public async Task CreateOrderAsync_ShouldCreateOrderSuccessfully()
    {
        var userId = "user123";
        var createOrderDto = new CreateOrderDTO { ShippingAddress = "Test Address", PaymentMethod = "CreditCard" };

        // Mocking DbSet<Cart> to return an IQueryable
        var mockCarts = new List<Cart>
        {
            new Cart { ProductId = 1, Quantity = 2, UserId = userId, Product = new Product { ProductId = 1, Price = 10.0m, Stock = 5 } }
        }.AsQueryable();
        var mockCartSet = new Mock<DbSet<Cart>>();
        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.Provider)
            .Returns(mockCarts.Provider);
        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.Expression)
            .Returns(mockCarts.Expression);
        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.ElementType)
            .Returns(mockCarts.ElementType);
        mockCartSet.As<IQueryable<Cart>>()
            .Setup(m => m.GetEnumerator())
            .Returns(mockCarts.GetEnumerator());

        _mockContext.Setup(c => c.Carts)
            .Returns(mockCartSet.Object);

        // Mocking DbSet<Product> to return an IQueryable
        var mockProducts = new List<Product>
        {
            new Product { ProductId = 1, Price = 10.0m, Stock = 5 }
        }.AsQueryable();
        var mockProductSet = new Mock<DbSet<Product>>();
        mockProductSet.As<IQueryable<Product>>()
            .Setup(m => m.Provider)
            .Returns(mockProducts.Provider);
        mockProductSet.As<IQueryable<Product>>()
            .Setup(m => m.Expression)
            .Returns(mockProducts.Expression);
        mockProductSet.As<IQueryable<Product>>()
            .Setup(m => m.ElementType)
            .Returns(mockProducts.ElementType);
        mockProductSet.As<IQueryable<Product>>()
            .Setup(m => m.GetEnumerator())
            .Returns(mockProducts.GetEnumerator());

        _mockContext.Setup(p => p.Products)
            .Returns(mockProductSet.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<OrderDTO>(It.IsAny<Order>()))
            .Returns(new OrderDTO());

        var result = await _orderService.CreateOrderAsync(userId, createOrderDto);

        Assert.IsNotNull(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}

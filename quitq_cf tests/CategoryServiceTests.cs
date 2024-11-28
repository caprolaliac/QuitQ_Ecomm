using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using quitq_cf.Data;
using quitq_cf.Models;
using quitq_cf.Repository;
using AutoMapper;

namespace quitq_cf_tests
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<DbSet<Category>> _mockCategorySet;
        private Mock<DbSet<SubCategory>> _mockSubCategorySet;
        private CategoryService _service;
        private Mock<IMapper> _mockMapper;

        [SetUp]
        public void SetUp()
        {
            _mockContext = new Mock<ApplicationDbContext>();
            _mockMapper = new Mock<IMapper>();
            _mockCategorySet = new Mock<DbSet<Category>>();
            _mockSubCategorySet = new Mock<DbSet<SubCategory>>();

            _mockContext.Setup(c => c.Categories).Returns(_mockCategorySet.Object);
            _mockContext.Setup(c => c.SubCategories).Returns(_mockSubCategorySet.Object);

            _service = new CategoryService(_mockContext.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Category1" },
                new Category { CategoryId = 2, CategoryName = "Category2" }
            }.AsQueryable();

            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(categories.Provider);
            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(categories.Expression);
            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(categories.ElementType);
            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categories.GetEnumerator());

            var result = await _service.GetAllCategoriesAsync();

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetSubcategoriesByCategoryIdAsync_ReturnsSubcategories()
        {
            var subcategories = new List<SubCategory>
            {
                new SubCategory { SubcategoryId = 1, SubcategoryName = "SubCategory1", CategoryId = 1 },
                new SubCategory { SubcategoryId = 2, SubcategoryName = "SubCategory2", CategoryId = 1 }
            }.AsQueryable();

            _mockSubCategorySet.As<IQueryable<SubCategory>>().Setup(m => m.Provider).Returns(subcategories.Provider);
            _mockSubCategorySet.As<IQueryable<SubCategory>>().Setup(m => m.Expression).Returns(subcategories.Expression);
            _mockSubCategorySet.As<IQueryable<SubCategory>>().Setup(m => m.ElementType).Returns(subcategories.ElementType);
            _mockSubCategorySet.As<IQueryable<SubCategory>>().Setup(m => m.GetEnumerator()).Returns(subcategories.GetEnumerator());

            var result = await _service.GetSubcategoriesByCategoryIdAsync(1);

            Assert.AreEqual(2, result.Count());
        }

        //[Test]
        //public async Task CreateCategoryAsync_CreatesNewCategory()
        //{
        //    var categoryName = "NewCategory";

        //    _mockCategorySet.Setup(m => m.AddAsync(It.IsAny<Category>(), default)).Returns(Task.FromResult((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Category>)null));
        //    _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

        //    var result = await _service.CreateCategoryAsync(categoryName);

        //    Assert.AreEqual("Success", result.Status);
        //}

        [Test]
        public async Task CreateCategoryAsync_ReturnsFailedIfCategoryExists()
        {
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "ExistingCategory" }
            }.AsQueryable();

            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(categories.Provider);
            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(categories.Expression);
            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(categories.ElementType);
            _mockCategorySet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categories.GetEnumerator());

            var result = await _service.CreateCategoryAsync("ExistingCategory");

            Assert.AreEqual("Failed", result.Status);
        }

        [Test]
        public async Task UpdateCategoryAsync_UpdatesCategory()
        {
            var category = new Category { CategoryId = 1, CategoryName = "OldName" };

            _mockCategorySet.Setup(m => m.FindAsync(1)).ReturnsAsync(category);
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _service.UpdateCategoryAsync(1, "NewName");

            Assert.AreEqual("Success", result.Status);
            Assert.AreEqual("NewName", category.CategoryName);
        }

        [Test]
        public async Task DeleteCategoryAsync_DeletesCategory()
        {
            var category = new Category { CategoryId = 1, CategoryName = "Category1", SubCategories = new List<SubCategory>() };

            _mockCategorySet.Setup(m => m.Include(It.IsAny<string>())).Returns(_mockCategorySet.Object);
            _mockCategorySet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>(), default)).ReturnsAsync(category);
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _service.DeleteCategoryAsync(1);

            Assert.AreEqual("Success", result.Status);
        }
    }
}

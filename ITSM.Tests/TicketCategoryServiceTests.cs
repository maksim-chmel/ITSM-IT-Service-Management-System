using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ITSM.Data;
using ITSM.Models;
using ITSM.Services.TicketCategory;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Tests;

public class TicketCategoryServiceTests
{
    private readonly DBaseContext _db;
    private readonly TicketCategoryService _sut;

    public TicketCategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();
        var cache = new MemoryCache(new MemoryCacheOptions());
        _sut = new TicketCategoryService(_db, cache);
    }

    // ── CreateCategory ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCategory_Fails_WhenDuplicateNameExists()
    {
        await _sut.CreateCategory("Networking");
        var result = await _sut.CreateCategory("Networking");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCategory_Fails_WhenDuplicateNameExistsCaseInsensitive()
    {
        await _sut.CreateCategory("Networking");
        var result = await _sut.CreateCategory("NETWORKING");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCategory_Succeeds_AndPersistsRecord()
    {
        var before = _db.TicketCategories.Count();
        var result = await _sut.CreateCategory("New Unique Category");

        result.IsSuccess.Should().BeTrue();
        _db.TicketCategories.Count().Should().Be(before + 1);
    }

    // ── AddSubCategoryAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task AddSubCategory_Fails_WhenNameIsWhitespace()
    {
        var model = new SubCategoryCreateViewModel { Name = "   ", CategoryId = 1 };
        var result = await _sut.AddSubCategoryAsync(model);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddSubCategory_Fails_WhenParentCategoryNotFound()
    {
        var model = new SubCategoryCreateViewModel { Name = "Valid Name", CategoryId = 9999 };
        var result = await _sut.AddSubCategoryAsync(model);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddSubCategory_Fails_WhenDuplicateNameInSameCategory()
    {
        var first = new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 };
        await _sut.AddSubCategoryAsync(first);

        var duplicate = new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 };
        var result = await _sut.AddSubCategoryAsync(duplicate);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddSubCategory_Fails_WhenDuplicateNameCaseInsensitive()
    {
        await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 });
        var result = await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "FRONTEND", CategoryId = 1 });
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddSubCategory_Succeeds_WhenSameNameInDifferentCategory()
    {
        await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 });
        var result = await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 2 });
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddSubCategory_Succeeds_AndPersistsRecord()
    {
        var model = new SubCategoryCreateViewModel { Name = "Backend", CategoryId = 1 };
        var result = await _sut.AddSubCategoryAsync(model);

        result.IsSuccess.Should().BeTrue();
        var sub = await _db.TicketSubCategories.FirstOrDefaultAsync(s => s.Name == "Backend");
        sub.Should().NotBeNull();
        sub!.CategoryId.Should().Be(1);
    }

    // ── SoftDeleteSubCategoryAsync ─────────────────────────────────────────────

    [Fact]
    public async Task SoftDeleteSubCategory_Fails_WhenNotFound()
    {
        var result = await _sut.SoftDeleteSubCategoryAsync(999);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteSubCategory_Succeeds_AndMarksDeleted()
    {
        var sub = new TicketSubCategory { Name = "Old Sub", CategoryId = 1 };
        _db.TicketSubCategories.Add(sub);
        await _db.SaveChangesAsync();

        var result = await _sut.SoftDeleteSubCategoryAsync(sub.Id);

        result.IsSuccess.Should().BeTrue();
        var archived = await _db.TicketSubCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == sub.Id);
        archived!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeleteSubCategory_HidesSubCategoryFromNormalQueries()
    {
        var sub = new TicketSubCategory { Name = "Old Sub", CategoryId = 1 };
        _db.TicketSubCategories.Add(sub);
        await _db.SaveChangesAsync();

        await _sut.SoftDeleteSubCategoryAsync(sub.Id);

        _db.ChangeTracker.Clear();
        var found = await _db.TicketSubCategories.FindAsync(sub.Id);
        found.Should().BeNull();
    }

    // ── GetAllCategoriesToList ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAllCategoriesToList_ReturnsAllSeededCategories()
    {
        var result = await _sut.GetAllCategoriesToList();

        result.Should().HaveCount(5); // 5 HasData seed categories
    }

    [Fact]
    public async Task GetAllCategoriesToList_IncludesSubCategories()
    {
        _db.TicketSubCategories.Add(new TicketSubCategory { Name = "Sub A", CategoryId = 1 });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllCategoriesToList();

        result.Should().Contain(c => c.Id == 1 && c.SubCategories.Count == 1);
    }

    // ── DeleteCategory ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteCategory_SoftDeletesCategory_AndHidesFromNormalQuery()
    {
        await _sut.CreateCategory("ToDelete");
        _db.ChangeTracker.Clear();
        var cat = _db.TicketCategories.First(c => c.Name == "ToDelete");

        var result = await _sut.DeleteCategory(cat.Id);

        result.IsSuccess.Should().BeTrue();
        _db.ChangeTracker.Clear();
        _db.TicketCategories.Any(c => c.Name == "ToDelete").Should().BeFalse();
        _db.TicketCategories.IgnoreQueryFilters().Any(c => c.Name == "ToDelete" && c.IsDeleted).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCategory_ReturnsFailure_WhenCategoryNotFound()
    {
        var result = await _sut.DeleteCategory(9999);
        result.IsSuccess.Should().BeFalse();
    }

    // ── GetCategorySelectListAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetCategorySelectListAsync_ReturnsItems_WhenNoListProvided()
    {
        var result = await _sut.GetCategorySelectListAsync();

        result.Should().HaveCount(5); // 5 seed categories
        result.Should().OnlyContain(item => item.Value != null && item.Text != null);
    }

    [Fact]
    public async Task GetCategorySelectListAsync_MapsProvidedList_WhenListSupplied()
    {
        var cats = new List<TicketCategory> { new() { Id = 42, Name = "Custom" } };

        var result = await _sut.GetCategorySelectListAsync(cats);

        result.Should().ContainSingle(item => item.Text == "Custom" && item.Value == "42");
    }

    // ── GetSubCategoryListAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetSubCategoryListAsync_ReturnsCategoryWithSubcategories()
    {
        _db.TicketSubCategories.Add(new TicketSubCategory { Name = "Sub", CategoryId = 1 });
        await _db.SaveChangesAsync();

        var result = await _sut.GetSubCategoryListAsync(1);

        result.Should().NotBeNull();
        result!.SubCategories.Should().ContainSingle(s => s.Name == "Sub");
    }

    [Fact]
    public async Task GetSubCategoryListAsync_ReturnsNull_WhenCategoryNotFound()
    {
        var result = await _sut.GetSubCategoryListAsync(9999);
        result.Should().BeNull();
    }

    // ── DeleteSubCategoryAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteSubCategoryAsync_SoftDeletesSubcategory()
    {
        var sub = new TicketSubCategory { Name = "ToDelete", CategoryId = 1 };
        _db.TicketSubCategories.Add(sub);
        await _db.SaveChangesAsync();

        var result = await _sut.DeleteSubCategoryAsync(sub.Id);

        result.IsSuccess.Should().BeTrue();
        var archived = await _db.TicketSubCategories.IgnoreQueryFilters().FirstAsync(s => s.Id == sub.Id);
        archived.IsDeleted.Should().BeTrue();
    }

    // ── GetSubCategoriesForCategory ────────────────────────────────────────────

    [Fact]
    public async Task GetSubCategoriesForCategory_ReturnsSubcategories_WhenIdProvided()
    {
        _db.TicketSubCategories.Add(new TicketSubCategory { Name = "Sub", CategoryId = 1 });
        await _db.SaveChangesAsync();

        var result = await _sut.GetSubCategoriesForCategory(1);

        result.Should().ContainSingle(s => s.Name == "Sub");
    }

    [Fact]
    public async Task GetSubCategoriesForCategory_ReturnsEmpty_WhenIdIsNull()
    {
        var result = await _sut.GetSubCategoriesForCategory(null);
        result.Should().BeEmpty();
    }

    // ── MapSubCategoriesToSelectList ───────────────────────────────────────────

    [Fact]
    public void MapSubCategoriesToSelectList_MapsCorrectly()
    {
        var subs = new List<TicketSubCategory>
        {
            new() { Id = 1, Name = "Sub A" },
            new() { Id = 2, Name = "Sub B" }
        };

        var result = _sut.MapSubCategoriesToSelectList(subs);

        result.Should().HaveCount(2);
        result.Should().Contain(item => item.Text == "Sub A" && item.Value == "1");
        result.Should().Contain(item => item.Text == "Sub B" && item.Value == "2");
    }
}

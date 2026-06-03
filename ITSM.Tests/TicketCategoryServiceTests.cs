using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Models;
using ITSM.Services.TicketCategory;
using ITSM.ViewModels.Create;

namespace ITSM.Tests;

public class TicketCategoryServiceTests
{
    private readonly DBaseContext _db;
    private readonly TicketCategoryService _sut;

    public TicketCategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();
        _sut = new TicketCategoryService(_db);
    }

    // ── CreateCategory ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCategory_Fails_WhenDuplicateNameExists()
    {
        await _sut.CreateCategory("Networking");
        var result = await _sut.CreateCategory("Networking");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateCategory_Fails_WhenDuplicateNameExistsCaseInsensitive()
    {
        await _sut.CreateCategory("Networking");
        var result = await _sut.CreateCategory("NETWORKING");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateCategory_Succeeds_AndPersistsRecord()
    {
        var before = _db.TicketCategories.Count();
        var result = await _sut.CreateCategory("New Unique Category");

        Assert.True(result.IsSuccess);
        Assert.Equal(before + 1, _db.TicketCategories.Count());
    }

    // ── AddSubCategoryAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task AddSubCategory_Fails_WhenNameIsWhitespace()
    {
        var model = new SubCategoryCreateViewModel { Name = "   ", CategoryId = 1 };
        var result = await _sut.AddSubCategoryAsync(model);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddSubCategory_Fails_WhenParentCategoryNotFound()
    {
        var model = new SubCategoryCreateViewModel { Name = "Valid Name", CategoryId = 9999 };
        var result = await _sut.AddSubCategoryAsync(model);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddSubCategory_Fails_WhenDuplicateNameInSameCategory()
    {
        var first = new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 };
        await _sut.AddSubCategoryAsync(first);

        var duplicate = new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 };
        var result = await _sut.AddSubCategoryAsync(duplicate);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddSubCategory_Fails_WhenDuplicateNameCaseInsensitive()
    {
        await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 });
        var result = await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "FRONTEND", CategoryId = 1 });
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddSubCategory_Succeeds_WhenSameNameInDifferentCategory()
    {
        await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 1 });
        var result = await _sut.AddSubCategoryAsync(new SubCategoryCreateViewModel { Name = "Frontend", CategoryId = 2 });
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddSubCategory_Succeeds_AndPersistsRecord()
    {
        var model = new SubCategoryCreateViewModel { Name = "Backend", CategoryId = 1 };
        var result = await _sut.AddSubCategoryAsync(model);

        Assert.True(result.IsSuccess);
        var sub = await _db.TicketSubCategories.FirstOrDefaultAsync(s => s.Name == "Backend");
        Assert.NotNull(sub);
        Assert.Equal(1, sub.CategoryId);
    }

    // ── SoftDeleteSubCategoryAsync ─────────────────────────────────────────────

    [Fact]
    public async Task SoftDeleteSubCategory_Fails_WhenNotFound()
    {
        var result = await _sut.SoftDeleteSubCategoryAsync(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SoftDeleteSubCategory_Succeeds_AndMarksDeleted()
    {
        var sub = new TicketSubCategory { Name = "Old Sub", CategoryId = 1 };
        _db.TicketSubCategories.Add(sub);
        await _db.SaveChangesAsync();

        var result = await _sut.SoftDeleteSubCategoryAsync(sub.Id);

        Assert.True(result.IsSuccess);
        var archived = await _db.TicketSubCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == sub.Id);
        Assert.True(archived!.IsDeleted);
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
        Assert.Null(found);
    }
}

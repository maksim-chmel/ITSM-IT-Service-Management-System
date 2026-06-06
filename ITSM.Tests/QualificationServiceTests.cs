using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Qualification;
using ITSM.Services.TicketCategory;
using ITSM.Services.UserManagement;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Tests;

public class QualificationServiceTests
{
    private readonly DBaseContext _db;
    private readonly QualificationService _sut;
    private readonly Mock<IUserManagementService> _userManagement;
    private readonly Mock<ITicketCategoryService> _categoryService;

    public QualificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        _userManagement = new Mock<IUserManagementService>();
        _categoryService = new Mock<ITicketCategoryService>();

        _categoryService
            .Setup(c => c.GetCategorySelectListAsync(It.IsAny<List<TicketCategory>?>()))
            .ReturnsAsync(new List<SelectListItem>());

        _sut = new QualificationService(_db, _userManagement.Object, _categoryService.Object);
    }

    private async Task<TicketCategory> SeedCategory()
    {
        var cat = new TicketCategory { Name = "IT" };
        _db.TicketCategories.Add(cat);
        await _db.SaveChangesAsync();
        return cat;
    }

    private User MakeUser(string id = "user-1") => new()
    {
        Id = id,
        UserName = "testuser",
        Email = "test@test.com"
    };

    // ── GetAssignCategoryViewModelAsync ────────────────────────────────────────

    [Fact]
    public async Task GetAssignCategoryViewModelAsync_ReturnsNull_WhenUserNotFound()
    {
        _userManagement.Setup(m => m.GetUserById("missing")).ReturnsAsync((User?)null);

        var result = await _sut.GetAssignCategoryViewModelAsync("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAssignCategoryViewModelAsync_ReturnsViewModel_WithUserData()
    {
        var user = MakeUser();
        _userManagement.Setup(m => m.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _sut.GetAssignCategoryViewModelAsync(user.Id);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.UserName.Should().Be(user.UserName);
    }

    // ── AssignCategoriesToUserAsync ────────────────────────────────────────────

    [Fact]
    public async Task AssignCategoriesToUserAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userManagement.Setup(m => m.GetUserById("missing")).ReturnsAsync((User?)null);

        var result = await _sut.AssignCategoriesToUserAsync(new AssignCategoryToUserViewModel
        {
            UserId = "missing",
            SelectedCategoryIds = new List<string>()
        });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AssignCategoriesToUserAsync_CreatesNewAssignment_WhenCategorySelected()
    {
        var user = MakeUser("user-99");
        _userManagement.Setup(m => m.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _sut.AssignCategoriesToUserAsync(new AssignCategoryToUserViewModel
        {
            UserId = user.Id,
            SelectedCategoryIds = new List<string> { "1" },
            SkillLevel = SkillLevel.Middle
        });

        result.IsSuccess.Should().BeTrue();
        var assignments = _db.UserCategoryAssignments.IgnoreQueryFilters().ToList();
        assignments.Should().Contain(a => a.UserId == user.Id && a.CategoryId == 1 && !a.IsDeleted);
    }

    [Fact]
    public async Task AssignCategoriesToUserAsync_MarksExistingAssignmentDeleted_WhenCategoryDeselected()
    {
        var user = MakeUser("user-99");
        _userManagement.Setup(m => m.GetUserById(user.Id)).ReturnsAsync(user);

        _db.UserCategoryAssignments.Add(new UserCategoryAssignment
        {
            UserId = user.Id, CategoryId = 1, IsDeleted = false
        });
        await _db.SaveChangesAsync();

        var result = await _sut.AssignCategoriesToUserAsync(new AssignCategoryToUserViewModel
        {
            UserId = user.Id,
            SelectedCategoryIds = new List<string>()
        });

        result.IsSuccess.Should().BeTrue();
        var assignment = _db.UserCategoryAssignments.IgnoreQueryFilters()
            .First(a => a.UserId == user.Id && a.CategoryId == 1);
        assignment.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task AssignCategoriesToUserAsync_UpdatesSkillLevel()
    {
        var user = MakeUser("user-99");
        user.SkillLevel = SkillLevel.Junior;
        _userManagement.Setup(m => m.GetUserById(user.Id)).ReturnsAsync(user);

        await _sut.AssignCategoriesToUserAsync(new AssignCategoryToUserViewModel
        {
            UserId = user.Id,
            SelectedCategoryIds = new List<string>(),
            SkillLevel = SkillLevel.Expert
        });

        user.SkillLevel.Should().Be(SkillLevel.Expert);
    }
}

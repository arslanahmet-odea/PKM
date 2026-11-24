using CausalCampaign.Domain.Common;
using CausalCampaign.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.WebTests;

[TestClass]
public class GlobalExceptionHandlerTests
{
    [TestMethod]
    public async Task TryHandleAsync_WhenConcurrencyExceptionThrown_Writes409ConflictProblemDetails()
    {
        // Arrange
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/transactions/authorize";
        httpContext.Response.Body = new MemoryStream();

        var concurrencyEx = new DbUpdateConcurrencyException("RowVersion concurrency conflict occurred.");

        // Act
        bool handled = await handler.TryHandleAsync(httpContext, concurrencyEx, CancellationToken.None);

        // Assert
        Assert.IsTrue(handled);
        Assert.AreEqual(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public void MapDomainErrorToProblemDetails_MapsAllErrorTypesToExpectedHttpStatusCodes()
    {
        // Act & Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest,
            GlobalExceptionHandler.MapDomainErrorToProblemDetails(DomainError.Validation("V", "Validation")).StatusCode);

        Assert.AreEqual(StatusCodes.Status404NotFound,
            GlobalExceptionHandler.MapDomainErrorToProblemDetails(DomainError.NotFound("N", "NotFound")).StatusCode);

        Assert.AreEqual(StatusCodes.Status409Conflict,
            GlobalExceptionHandler.MapDomainErrorToProblemDetails(DomainError.Conflict("C", "Conflict")).StatusCode);

        Assert.AreEqual(StatusCodes.Status422UnprocessableEntity,
            GlobalExceptionHandler.MapDomainErrorToProblemDetails(DomainError.AntiGaming("A", "AntiGaming")).StatusCode);
    }
}

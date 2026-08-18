using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Formatting;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Tests;

public class ByteFormatterTests
{
    [Theory]
    [InlineData(0, "0 Б")]
    [InlineData(512, "512 Б")]
    [InlineData(1024, "1 КБ")]
    [InlineData(1536, "1.5 КБ")]
    [InlineData(1048576, "1 МБ")]
    public void ToHuman_FormatsExpected(long bytes, string expected)
    {
        Assert.Equal(expected, ByteFormatter.ToHuman(bytes));
    }

    [Fact]
    public void ToHuman_Negative_ReturnsZero()
    {
        Assert.Equal("0 Б", ByteFormatter.ToHuman(-10));
    }
}

public class ScanResultTests
{
    [Fact]
    public void Totals_SumIssueSizes()
    {
        var result = new ScanResult
        {
            StartedAt = DateTimeOffset.Now,
            Duration = TimeSpan.FromSeconds(1),
            Issues =
            [
                CreateIssue(100),
                CreateIssue(250)
            ]
        };

        Assert.Equal(2, result.IssueCount);
        Assert.Equal(350, result.TotalSizeBytes);
    }

    private static IssueItem CreateIssue(long size) => new()
    {
        Id = Guid.NewGuid(),
        CheckId = "test",
        Title = "t",
        Description = "d",
        Category = OperationCategory.Cleaning,
        Severity = IssueSeverity.Low,
        Action = RepairAction.DeleteFile,
        SizeBytes = size
    };
}

public class OperationResultTests
{
    [Fact]
    public void Fail_SetsSuccessFalse()
    {
        var result = OperationResult.Fail("ошибка");
        Assert.False(result.Success);
        Assert.Contains("ошибка", result.Messages);
    }
}

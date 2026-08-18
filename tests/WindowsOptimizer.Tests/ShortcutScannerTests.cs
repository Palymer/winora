using WindowsOptimizer.Infrastructure.Cleaning;

namespace WindowsOptimizer.Tests;

public class ShortcutScannerTests
{
    [Theory]
    [InlineData(@"C:\this-path-does-not-exist-optimizer-test.exe", true)]
    [InlineData(@"C:\Windows", false)]
    [InlineData("https://example.com", false)]
    [InlineData("shell:AppsFolder", false)]
    [InlineData("", false)]
    [InlineData("relative\\file.exe", false)]
    public void IsBroken_DetectsMissingTargets(string target, bool expected)
    {
        Assert.Equal(expected, ShortcutScanner.IsBroken(target));
    }
}

using Microsoft.Playwright;
using Xunit;

public class LoginUITests
{
    [Fact]
    public async Task Login_With_Invalid_User_Shows_Error()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true });

        var page = await browser.NewPageAsync();

        await page.GotoAsync("https://localhost:5001/login");

        await page.FillAsync("#username", "wrong");
        await page.FillAsync("#password", "wrong");

        await page.ClickAsync("button");

        await page.WaitForSelectorAsync(".alert-danger");

        var content = await page.InnerTextAsync(".alert-danger");

        Assert.Contains("Invalid username", content);
    }
}
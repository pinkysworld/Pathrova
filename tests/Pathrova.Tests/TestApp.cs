using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Pathrova.Tests.TestAppBuilder))]

namespace Pathrova.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        Environment.SetEnvironmentVariable("PATHROVA_TEST_HOME", Directory.CreateTempSubdirectory("pathrova-home-").FullName);
        return AppBuilder.Configure<Pathrova.App.App>().UseSkia().WithInterFont()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}

namespace MauiApp3;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("HarmonyOS-Sans.ttf", "HarmonyOSSans");
				fonts.AddFont("HarmonyOS-Sans-Bold.ttf", "HarmonyOSSansBold");
			});

		return builder.Build();
	}
}

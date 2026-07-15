using Microsoft.Extensions.Logging;

using ZXing.Net.Maui.Controls;

namespace ItAssets.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

#if ANDROID
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("CustomUserAgent", (handler, view) =>
        {
            handler.PlatformView.Settings.UserAgentString += " MAUI-App";
        });
#endif

		return builder.Build();
	}
}

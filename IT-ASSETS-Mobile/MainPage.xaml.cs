using ZXing.Net.Maui;

namespace ItAssets.Mobile;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false
        };
	}

    private void AppWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("mauiscan://start"))
        {
            e.Cancel = true;
            StartScanner();
        }
    }

    private async void AppWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        await AppWebView.EvaluateJavaScriptAsync("window.isMauiApp = true;");
    }

    private async void StartScanner()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Camera permission is required to scan.", "OK");
                return;
            }
        }

        ScannerOverlay.IsVisible = true;
        BarcodeReader.IsDetecting = true;
    }

    private void CancelScanner_Clicked(object sender, EventArgs e)
    {
        StopScanner();
    }

    private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        var result = e.Results?.FirstOrDefault();
        if (result != null)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                StopScanner();
                var text = result.Value.Replace("'", "\\'"); // escape quotes
                await AppWebView.EvaluateJavaScriptAsync($"if(window.dotnetHelper) {{ window.dotnetHelper.invokeMethodAsync('OnQrCodeScanned', '{text}'); }}");
            });
        }
    }

    private void StopScanner()
    {
        BarcodeReader.IsDetecting = false;
        ScannerOverlay.IsVisible = false;
    }
}

using ZXing.Net.Maui;

namespace Frontend_Proyecto_Fridgeloop.Pages;

public partial class ScanPage : ContentPage
{
    bool _handledOnce;

    public ScanPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permiso requerido", "Necesitamos acceso a la cámara para escanear.", "OK");
            await Navigation.PopAsync();
            return;
        }
#endif
        _handledOnce = false;
        cameraView.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        cameraView.IsDetecting = false;
    }

    private void CameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_handledOnce) return;

        var code = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(code)) return;

        _handledOnce = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // devolver el código a la página anterior
            MessagingCenter.Send(this, "BarcodeScanned", code);
            await Navigation.PopAsync();
        });
    }
}

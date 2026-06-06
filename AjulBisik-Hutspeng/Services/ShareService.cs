using System.Threading.Tasks;
using Microsoft.Maui.Media;
using Microsoft.Maui.Graphics;
using System.IO;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.Services
{
    public class ShareService
    {
        public async Task ShareElementAsImageAsync(VisualElement element, string text)
        {
            if (element == null) return;

            try
            {
                // Capture element to image
                var result = await element.CaptureAsync();
                if (result != null)
                {
                    var file = Path.Combine(FileSystem.CacheDirectory, "share_image.png");
                    using var stream = await result.OpenReadAsync();
                    using var fileStream = File.OpenWrite(file);
                    await stream.CopyToAsync(fileStream);
                    fileStream.Close(); // Ensure it's fully written before sharing

                    // Share
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = "Bagikan ke IG Story",
                        File = new ShareFile(file),
                        PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet
                            ? new Rect(0, 20, 0, 0)
                            : Rect.Zero
                    });
                }
            }
            catch (System.Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Gagal membagikan: {ex.Message}", "OK");
            }
        }
    }
}
using JointWatermark.Class;
using MauiApp3.Classes;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.Maui.Graphics;
//using Microsoft.Maui.Graphics.Platform;
using System.Reflection;

namespace MauiApp3;

public partial class MainPage : ContentPage
{
    int count = 0;
    MemoryStream saveStream = new MemoryStream();
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        //count++;

        //if (count == 1)
        //	CounterBtn.Text = $"Clicked {count} time";
        //else
        //	CounterBtn.Text = $"Clicked {count} times";

        using var stream = FileSystem.OpenAppPackageFileAsync("ExifConfig.json").Result;

        using (var reader = new StreamReader(stream))
        {
            var c = reader.ReadToEnd();
            var result1 = JsonConvert.DeserializeObject<MainModel>(c);
        }

        var result = await PickAndShow(PickOptions.Images);


    }
    public async Task<FileResult> PickAndShow(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    var stream = await result.OpenReadAsync();
                    var stream2 = await result.OpenReadAsync();
                    var stream3 = await result.OpenReadAsync();
                    var i = ImagesHelper.Current.ReadImage(stream, result.FileName, "");
                    var rs = await ImagesHelper.Current.MergeWatermark(stream2, stream3, i);
                    var s1 = new MemoryStream();

                    rs.Save(s1, new JpegEncoder());
                    rs.Save(saveStream, new JpegEncoder());
                    //s1.CopyTo(saveStream);
                    //rs.Save(saveStream, new JpegEncoder());
                    s1.Position = 0;
                    pic.Source = ImageSource.FromStream(() => s1);

                }

            }

            return result;
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }

        return null;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
#if WINDOWS
        if (saveStream != null)
        {
            var bytes = saveStream.ToArray();
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "myImage.png");
            File.WriteAllBytes(filePath, bytes);
        }
#endif     


#if ANDROID
        if (saveStream == null) return;
        Microsoft.Maui.Graphics.IImage image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(saveStream);

        // Save image to a memory stream
        if (image != null)
        {
            Microsoft.Maui.Graphics.IImage newImage = image.Downsize(150, true);
            using (MemoryStream memStream = new MemoryStream())
            {
                newImage.Save(memStream);
            }

        }
#endif


    }
}


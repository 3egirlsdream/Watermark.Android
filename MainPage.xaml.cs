using ExifLib;
using JointWatermark.Class;
using MauiApp3.Classes;
using SkiaSharp;

namespace MauiApp3;

public partial class MainPage : ContentPage
{
    SKTypeface sKTypeface_B;
    SKTypeface sKTypeface;
    string currentLogoPath;
    List<string> ImagesFilePath;
    string selectedImagePath;
    Dictionary<string, ImageProperties> properties;
    public MainPage()
    {
        InitializeComponent();
        loading.IsVisible = false;
        ImagesFilePath= new List<string>();
        sKTypeface_B = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans-Bold.ttf").Result);
        sKTypeface = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans.ttf").Result);
        InitLogoes();
    }

    private void InitLogoes()
    {
        var file = new DirectoryInfo(FileSystem.Current.CacheDirectory);
        var f = file.GetFiles();
        logoes.Children.Clear();
        foreach (var f2 in f)
        {
            Microsoft.Maui.Controls.Image image = new Microsoft.Maui.Controls.Image();
            image.Margin = new Thickness(2);
            //image.Background = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Color.FromArgb("#fff"));
            image.HeightRequest = 30;
            image.WidthRequest = 30; 
            image.Source = f2.FullName;
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += Tap_Tapped;
            image.GestureRecognizers.Add(tap);
            logoes.Children.Add(image);
            currentLogoPath = f2.FullName;
        }
    }

    private async void Tap_Tapped(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Image im)
        {
            currentLogoPath = ((Microsoft.Maui.Controls.FileImageSource)im.Source).File;
            try
            {
                if(properties.TryGetValue(selectedImagePath, out var p))
                {
                    loading.IsVisible = true;
                    await CreateWatermark(p, sKTypeface, sKTypeface_B, true);
                    loading.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
               ShowErrorMsg(ex.Message);
            }
            finally
            {
                loading.IsVisible = false;
            }
        }
    }


    private void OnCounterClicked(object sender, EventArgs e)
    {
        SkiaSharpVersion(PickOptions.Images);
    }

    public async void SkiaSharpVersion(PickOptions options)
    {
        try
        {
            var imageFileResults = await FilePicker.Default.PickMultipleAsync(options);
            ImagesFilePath = new List<string>();
            properties = new Dictionary<string, ImageProperties>();
            pics.Children.Clear();
            loading.IsVisible = true;
            foreach (var imageFileResult in imageFileResults)
            {
                ImagesFilePath.Add(imageFileResult.FullPath);
                selectedImagePath = imageFileResult.FullPath;

                Microsoft.Maui.Controls.Image image = new Microsoft.Maui.Controls.Image();
                image.Margin = new Thickness(2);
                image.Background = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Color.FromArgb("#fff"));
                image.Source = ImageSource.FromFile(imageFileResult.FullPath);
                TapGestureRecognizer tap = new TapGestureRecognizer();
                tap.Tapped += Tap_Tapped1;
                image.GestureRecognizers.Add(tap);
                pics.Children.Add(image);

                var sm = await imageFileResult.OpenReadAsync();
                var p = LoadExifInfo(sm);
                properties[imageFileResult.FullPath] = p;
            }
            loading.IsVisible = false;
        }
        catch(Exception ex)
        {
            ShowErrorMsg(ex.Message);
        }
        finally
        {
            loading.IsVisible = false;
        }
    }

    private async void Tap_Tapped1(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Image im)
        {
            selectedImagePath = ((Microsoft.Maui.Controls.FileImageSource)im.Source).File;
            try
            {
                if(properties.TryGetValue(selectedImagePath, out ImageProperties p))
                {
                    loading.IsVisible = true;
                    await CreateWatermark(p, sKTypeface, sKTypeface_B, true);
                    loading.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsg(ex.Message);
            }
            finally
            {
                loading.IsVisible = false;
            }
        }
    }

    public SKBitmap Rotate(SKBitmap bitmap)
    {
        var rotated = new SKBitmap(bitmap.Height, bitmap.Width);

        using (var surface = new SKCanvas(rotated))
        {
            surface.Translate(rotated.Width, 0);
            surface.RotateDegrees(90);
            surface.DrawBitmap(bitmap, 0, 0);
        }

        return rotated;
    }


    private Task CreateWatermark(ImageProperties properties, SKTypeface sKTypeface, SKTypeface sKTypeface_B, bool isPreview = false)
    {
        return Task.Run(() =>
        {

            if (string.IsNullOrEmpty(currentLogoPath))
            {
                App.Current.Dispatcher.DispatchAsync(new Action(() =>
                {
                    ShowErrorMsg("请选选择Logo");
                }));
                return;
            }
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                App.Current.Dispatcher.DispatchAsync(new Action(() =>
                {
                    ShowErrorMsg("请选选择图片");
                }));
                return;
            }
            var logo = SKBitmap.Decode(currentLogoPath);
            using (var _img = SKBitmap.Decode(selectedImagePath))
            {
                var img = _img.Copy();
                //旋转图片
                for (int i = 0; i < properties.Config.RotateCount; i++)
                {
                    img = Rotate(img).Copy();
                }

                var w = img.Width;
                var h = img.Height * 0.13;
                if (img.Width < img.Height)
                {
                    h = img.Height * 0.13 * 0.8;
                }
                double xs = (double)(h / 2) / logo.Height;
                //字体比例系数
                float fontxs = ((float)h / 156);
                if (fontxs < 1) fontxs = 1;
                //下面定义一个矩形区域      
                var waterWidth = (int)(logo.Width * xs);
                var waterHeight = (int)(logo.Height * xs);
                SKBitmap logoResized = new SKBitmap(waterWidth, waterHeight);
                logo.Resize(logoResized, SKBitmapResizeMethod.Lanczos3);


                using (SKBitmap outputBitmap = new SKBitmap(w, img.Height + (int)h))
                {
                    using (SKCanvas canvas = new SKCanvas(outputBitmap))
                    {
                        canvas.DrawBitmap(img, new SKRect(0, 0, img.Width, img.Height));

                        using (SKPaint paint = new SKPaint() { Color = SKColors.White })
                        {
                            canvas.DrawRect(new SKRect(0, img.Height, img.Width, outputBitmap.Height), paint);
                        }

                        var skPaint = new SKPaint
                        {
                            IsAntialias = true
                        };

                        //右侧F ISO MM字体参数
                        float fontSize = 31 * fontxs;
                        var font20 = (24 * fontxs);
                        var TextSize = MeasureText(properties.Config.RightPosition1, sKTypeface_B, fontSize);
                        var oneSize = MeasureText("A", sKTypeface_B, fontSize);
                        var padding_right = MeasureText("23mmmm", sKTypeface_B, fontSize);

                        //计算水印2行文字的总体高度
                        var _fontSize = MeasureText("A", sKTypeface, font20);
                        var twoLineWordTotalHeight = 1.04 * fontSize + font20;

                        //绘制第右侧一行文字
                        var start = w - TextSize.Width - padding_right.Width;
                        var startHeight = (h - twoLineWordTotalHeight) / 2 + TextSize.Height;
                        var Params = new SKPoint(start, (int)startHeight);

                        using (SKPaint textPaint = new SKPaint() { Color = SKColors.Black, TextSize = fontSize, Typeface = sKTypeface_B })
                        {
                            canvas.DrawText(properties.Config.RightPosition1, new SKPoint(Params.X, Params.Y + img.Height), textPaint);
                        }
                        //绘制右侧第二行文字

                        var XY = new SKPoint(Params.X, (int)(Params.Y + 1.04 * fontSize));
                        //145, 145, 145
                        using (SKPaint textPaint = new SKPaint() { Color = SKColors.Gray, TextSize = font20, Typeface = sKTypeface })
                        {
                            canvas.DrawText(properties.Config.RightPosition2, new SKPoint(XY.X, (int)XY.Y + img.Height), textPaint);
                        }

                        //绘制竖线
                        var font20Size = MeasureText("A", sKTypeface, font20);
                        var lStart = new SKPoint(Params.X - (int)(oneSize.Width * 0.6), (int)(0.5*(h - logoResized.Height)));
                        var lEnd = new SKPoint(lStart.X, (int)(h - lStart.Y));

                        using (SKPaint textPaint = new SKPaint() { Color = SKColors.LightGray, TextSize = 2 * fontxs, Typeface = sKTypeface })
                        {
                            canvas.DrawLine(lStart.X, lStart.Y + img.Height, lEnd.X, lEnd.Y + img.Height, textPaint);
                            canvas.DrawLine(lStart.X+1, lStart.Y + img.Height, lEnd.X+1, lEnd.Y + img.Height, textPaint);
                        }

                        //绘制LOGO
                        var line = new SKPoint((int)(lStart.X - (int)(oneSize.Width * 0.6) - logoResized.Width), (int)(0.5*(h - logoResized.Height)));
                        using (SKPaint textPaint = new SKPaint() { Color = SKColors.LightGray, TextSize = 2 * fontxs, Typeface = sKTypeface })
                        {
                            SKImage image2 = SKImage.FromBitmap(logoResized);
                            canvas.DrawImage(image2, line.X, line.Y + img.Height);
                        }

                        //左边距系数
                        var leftWidth = (double)1 / 25 * w;// 100 * fontxs * 100 / 156;

                        //绘制设备信息
                        var font28 = (34 * fontxs);
                        var Producer = new SKPoint((int)(leftWidth), Params.Y);
                        using (SKPaint textPaint = new SKPaint() { Color = SKColors.Black, TextSize = font28, Typeface = sKTypeface_B })
                        {
                            canvas.DrawText(properties.Config.LeftPosition1, new SKPoint(Producer.X, (int)Producer.Y + img.Height), textPaint);
                        }

                        //绘制时间
                        var Date = new SKPoint(Producer.X, XY.Y);
                        using (SKPaint textPaint = new SKPaint() { Color = SKColors.Gray, TextSize = font20, Typeface = sKTypeface })
                        {
                            canvas.DrawText(properties.Config.LeftPosition2, new SKPoint(Date.X, (int)Date.Y + img.Height), textPaint);
                        }

                        SKImage image = SKImage.FromBitmap(outputBitmap);
                        SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

                        if (isPreview)
                        {
                            var s = data.AsStream(false);
                            App.Current.Dispatcher.DispatchAsync(new Action(() =>
                            {
                                preview.Source = ImageSource.FromStream(() => s);
                            }));
                            return;
                        }
                        string p1 = "";
#if WINDOWS
p1 = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
#endif
#if ANDROID
                        p1 = System.IO.Path.Combine("/storage/emulated/0/DCIM/Camera/", DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
#endif
                        using (FileStream rs = new FileStream(p1, FileMode.Create, FileAccess.Write))
                        {
                            data.SaveTo(rs);
                            App.Current.Dispatcher.DispatchAsync(new Action(() =>
                            {
                                ShowToast("结束");
                            }));
                        }

                    }
                }


            }
        });
    }

    private ImageProperties LoadExifInfo(Stream stream2 )
    {
        var properties = new ImageProperties("", "");
        ExifLib.ExifReader reader = new ExifLib.ExifReader(stream2);
        if (reader != null)
        {
            reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime date);
            reader.GetTagValue(ExifTags.Model, out string model);
            reader.GetTagValue(ExifTags.Make, out object make);

            reader.GetTagValue(ExifTags.FNumber, out object fnumber);

            reader.GetTagValue(ExifTags.ExposureTime, out double exposureTime);

            reader.GetTagValue(ExifTags.PhotographicSensitivity, out object iso2);
            reader.GetTagValue(ExifTags.FocalLengthIn35mmFilm, out object focal);

            reader.GetTagValue(ExifTags.LensModel, out string lensModel);
            properties.Config.LeftPosition1 = $"{make} {model}";
            properties.Config.LeftPosition2 = date.ToString("yyyy/MM/dd HH:mm:ss");
            int time = (int)(1/exposureTime);
            properties.Config.RightPosition1 = $"F{Convert.ToInt32(fnumber)} 1/{time}S ISO{iso2} {focal}mm";
            properties.Config.RightPosition2 = lensModel;
        }
        var config = Global.GetDefaultExifConfig(new Dictionary<string, object>());
        var right1 = config[2];
        var right2 = config[3];
        var left1 = config[0];
        var left2 = config[1];
        if (string.IsNullOrEmpty(properties.Config.LeftPosition1))
        {
            properties.Config.LeftPosition1 = left1;
        }
        if (string.IsNullOrEmpty(properties.Config.LeftPosition2))
        {
            properties.Config.LeftPosition2 = left2;
        }
        if (string.IsNullOrEmpty(properties.Config.RightPosition1))
        {
            properties.Config.RightPosition1 = right1;
        }
        if (string.IsNullOrEmpty(properties.Config.RightPosition2))
        {
            properties.Config.RightPosition2 = right2;
        }
        return properties;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            ShowToast("保存中...");
            loading.IsVisible = true;
            foreach(var i in ImagesFilePath)
            {
                selectedImagePath = i;
                if (properties.TryGetValue(i, out var p))
                {
                    await CreateWatermark(p, sKTypeface, sKTypeface_B);
                }
            }
            ShowToast("保存成功");
            loading.IsVisible = false;
        }
        catch (Exception ex)
        {
            ShowErrorMsg(ex.Message);
        }
        finally
        {
            loading.IsVisible = false;
        }

    }


    private async void ImportLogoClick(object sender, EventArgs e)
    {
        var logoFileResults = await FilePicker.Default.PickMultipleAsync(PickOptions.Images);
        foreach(var logoFileResult in logoFileResults)
        {
            if (logoFileResult == null)
                continue;
            if (
            !(logoFileResult.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                logoFileResult.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                ) continue;

            var cachePath = System.IO.Path.Combine(FileSystem.Current.CacheDirectory, logoFileResult.FileName);
            using FileStream fileStream = File.OpenWrite(cachePath);
            var ss = SKBitmap.Decode(logoFileResult.FullPath);
            SKImage image = SKImage.FromBitmap(ss);
            SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(fileStream);
        }
        InitLogoes();
    }

    public SKRect MeasureText(string text, SKTypeface sKTypeface, float fontSize)
    {
        SKPaint sKPaint = new SKPaint
        {
            IsAntialias = true
        };
        sKPaint.Typeface = sKTypeface;
        sKPaint.TextSize = fontSize;
        sKPaint.TextAlign = SKTextAlign.Center;
        sKPaint.Style = SKPaintStyle.Fill;
        SKRect sKRect = new SKRect();
        sKPaint.MeasureText(text, ref sKRect);
        return sKRect;
    }

    private async void RotateClick(object sender, EventArgs e)
    {
        try
        {
            if(properties.TryGetValue(selectedImagePath, out var p))
            {
                p.Config.RotateCount += 1;
                loading.IsVisible = true;
                await CreateWatermark(p, sKTypeface, sKTypeface_B, true);
                loading.IsVisible = false;
            }
        }
        catch(Exception ex)
        {
            ShowErrorMsg(ex.Message);
        }
        finally
        {
            loading.IsVisible = false;
        }
    }

    private async void ShowErrorMsg(string msg)
    {
        await DisplayAlert("出错了", msg, "关闭");
    }

    private  void ShowToast(string msg)
    {
        this.msg.Text = msg;
        toast.IsVisible = true;
        Task.Delay(3000).ContinueWith(t =>
        {
            Application.Current.Dispatcher.Dispatch(new Action(() =>
            {
                toast.IsVisible = false;
            }));
            
        });
    }
}


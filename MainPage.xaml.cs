using JointWatermark.Class;
using SixLabors.ImageSharp.Formats.Jpeg;
//using Microsoft.Maui.Graphics.Platform;
using SkiaSharp;
using SixLabors.Fonts;
using ExifLib;
using SixLabors.ImageSharp;
using Microsoft.Maui;

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
        msg.Text = "开始";
        SkiaSharpVersion(PickOptions.Images);


    }
    FileResult logoFileResult;
    public async void SkiaSharpVersion(PickOptions options)

    {
        var result = await FilePicker.Default.PickAsync(options);
        if (result == null)
            return;
        if (
        !(result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
            result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
            ) return;

        var stream1 = await result.OpenReadAsync();
        var stream2 = await result.OpenReadAsync();
        var stream3 = await result.OpenReadAsync();
        pic.Source = ImageSource.FromStream(() => stream3);
        var sKTypeface_B = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans-Bold.ttf").Result);
        var sKTypeface = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans.ttf").Result);
        if (logoFileResult == null) return;
        stream4 = await logoFileResult.OpenReadAsync();
        CreateWatermark(stream1, stream2, stream4, new ImageProperties("", ""), sKTypeface, sKTypeface_B);

       
    }


    private void CreateWatermark(System.IO.Stream stream, System.IO.Stream stream2, System.IO.Stream logoStream, ImageProperties properties, SKTypeface sKTypeface, SKTypeface sKTypeface_B)
    {
        var logo = SKBitmap.Decode(logoStream);
        using (var img = SKBitmap.Decode(stream))
        { 
            //旋转图片
            for (int i = 0; i < properties.Config.RotateCount; i++)
            {
                //img.r.Mutate(x => x.Rotate(RotateMode.Rotate90));
            }
            ExifLib.ExifReader reader = new ExifLib.ExifReader(stream2);
            if(reader != null)
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
                properties.Config.LeftPosition2 = date.ToString();
                var time = 1/exposureTime;
                properties.Config.RightPosition1 = $"F{fnumber} 1/{time}S ISO{iso2} {focal}mm";
                properties.Config.RightPosition2 = lensModel;
            }

            //var imageMetaData = img.GetExifData;
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
            using (SKBitmap temp = new SKBitmap(waterWidth, waterHeight))
            {
                logo.Resize(temp, SKBitmapResizeMethod.Lanczos3);
            }

            using (SKBitmap outputBitmap = new SKBitmap(w, img.Height + (int)h))
            {
                using (SKCanvas canvas = new SKCanvas(outputBitmap))
                {
                    // Draw the original image on the canvas
                    canvas.DrawBitmap(img, new SKRect(0, 0, img.Width, img.Height));

                    // Draw a white image at the bottom of the canvas
                    using (SKPaint paint = new SKPaint() { Color = SKColors.White })
                    {
                        canvas.DrawRect(new SKRect(0, img.Height, img.Width, outputBitmap.Height), paint);
                    }

                    var skPaint = new SKPaint
                    {
                        IsAntialias = true
                    };

                    // Add a text watermark at the bottom of the canvas
                    //右侧F ISO MM字体参数
                    FontFamily family = SystemFonts.Families.FirstOrDefault();
                    float fontSize = 31 * fontxs;
                    var font20 = (24 * fontxs);
                    var TextSize = MeasureText(properties.Config.RightPosition1, sKTypeface_B, fontSize);
                    var oneSize = MeasureText("A", sKTypeface_B, fontSize);
                    var padding_right = MeasureText("23mmmm", sKTypeface_B, fontSize);

                    //计算水印2行文字的总体高度
                    //var _font = family.CreateFont(font20, SixLabors.Fonts.FontStyle.Regular);
                    var _fontSize = MeasureText("A", sKTypeface, font20);
                    var twoLineWordTotalHeight = 1.04 * TextSize.Height + _fontSize.Height;

                    //绘制第右侧一行文字
                    var start = w - TextSize.Width - padding_right.Width;
                    var startHeight = (h - twoLineWordTotalHeight) / 2;
                    var Params = new SixLabors.ImageSharp.PointF(start, (int)startHeight);

                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.Black, TextSize = fontSize, Typeface = sKTypeface_B })
                    {
                        canvas.DrawText(properties.Config.RightPosition1, new SKPoint(Params.X, Params.Y + img.Height), textPaint);
                    }
                    //绘制右侧第二行文字

                    var XY = new SixLabors.ImageSharp.PointF(Params.X, (int)(Params.Y + 1.04 * TextSize.Height));
                    //145, 145, 145
                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.Gray, TextSize = font20, Typeface = sKTypeface })
                    {
                        canvas.DrawText(properties.Config.RightPosition2, new SKPoint(XY.X, (int)XY.Y + img.Height), textPaint);
                    }

                    //绘制竖线
                    var font20Size = MeasureText("A", sKTypeface, font20);
                    var lStart = new SixLabors.ImageSharp.PointF(Params.X - (int)(oneSize.Width * 0.6), (int)(0.5*(h - logo.Height)));
                    var lEnd = new SixLabors.ImageSharp.PointF(lStart.X, (int)(h - lStart.Y));

                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.LightGray, TextSize = 2 * fontxs, Typeface = sKTypeface })
                    {
                        canvas.DrawLine(lStart.X, lStart.Y + img.Height, lEnd.X, lEnd.Y + img.Height, textPaint);
                    }

                    //绘制LOGO
                    var line = new SixLabors.ImageSharp.Point((int)(lStart.X - (int)(oneSize.Width * 0.6) - logo.Width), (int)(0.5*(h - logo.Height)));
                    //wm.Mutate(x => x.DrawImage(logo, line, 1));
                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.LightGray, TextSize = 2 * fontxs, Typeface = sKTypeface })
                    {
                        SKImage image = SKImage.FromBitmap(logo);
                        canvas.DrawImage(image, line.X, line.Y + img.Height);
                    }

                    //左边距系数
                    var leftWidth = (double)1 / 25 * w;// 100 * fontxs * 100 / 156;

                    //绘制设备信息
                    var font28 = (34 * fontxs);
                    var Producer = new SixLabors.ImageSharp.PointF((int)(leftWidth), Params.Y);
                    //wm.Mutate(x => x.DrawText(properties.Config.LeftPosition1, font, SixLabors.ImageSharp.Color.ParseHex(properties.Config.Row1FontColor), Producer));
                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.Black, TextSize = font28, Typeface = sKTypeface_B })
                    {
                        canvas.DrawText(properties.Config.LeftPosition1, new SKPoint(Producer.X, (int)Producer.Y + img.Height), textPaint);
                    }

                    //绘制时间
                    var Date = new SixLabors.ImageSharp.PointF(Producer.X, XY.Y);
                    //wm.Mutate(x => x.DrawText(properties.Config.LeftPosition2, font, SixLabors.ImageSharp.Color.FromRgb(145, 145, 145), Date));
                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.Gray, TextSize = font20, Typeface = sKTypeface })
                    {
                        canvas.DrawText(properties.Config.LeftPosition2, new SKPoint(Date.X, (int)Date.Y + img.Height), textPaint);
                    }

                    // Save the output bitmap to a file
                    using (SKImage image = SKImage.FromBitmap(outputBitmap))
                    using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
                    {
                        string p1 = "";
#if WINDOWS
p1 = "C:\\Users\\kingdee\\Desktop\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
#endif
#if ANDROID
                        p1 = System.IO.Path.Combine("/storage/emulated/0/DCIM/Camera/", DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
#endif
                        using (FileStream rs = new FileStream(p1, FileMode.Create, FileAccess.Write))
                        {
                            data.SaveTo(rs);
                            msg.Text = "结束";
                        }
                    }
                }
            }


        }
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
            throw ex;// The user canceled or something went wrong
        }

        return null;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
#if WINDOWS
        if (saveStream != null)
        {
            var bytes = saveStream.ToArray();
            var filePath = "C:\\Users\\kingdee\\Desktop\\" + "output_image.jpg";
            File.WriteAllBytes(filePath, bytes);
        }
#endif


#if ANDROID
        try
        {
            //var mediaDir = FileSystem.;
            var p = System.IO.Path.Combine("/storage/emulated/0/DCIM/Camera/", DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
            var dest = File.OpenWrite(p);
            saveStream.CopyTo(dest);
            
        }
        catch (Exception ex)
        {

        }
        //if (saveStream == null) return;
        //Microsoft.Maui.Graphics.IImage image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(saveStream);

        //// Save image to a memory stream
        //if (image != null)
        //{
        //    Microsoft.Maui.Graphics.IImage newImage = image.Downsize(150, true);
        //    using (MemoryStream memStream = new MemoryStream())
        //    {
        //        newImage.Save(memStream);
        //    }

        //}
#endif


    }
    System.IO.Stream stream4 = new MemoryStream();
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        logoFileResult = await FilePicker.Default.PickAsync(PickOptions.Images);
        if (logoFileResult == null)
            return;
        if (
        !(logoFileResult.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
            logoFileResult.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
            ) return;

        stream4 = await logoFileResult.OpenReadAsync();
        var stream5 = await logoFileResult.OpenReadAsync();
        logo.Source = ImageSource.FromStream(() => stream5);
    }

    public SKRect MeasureText(string text, SKTypeface sKTypeface, float fontSize)
    {
        SKPaint sKPaint = new SKPaint
        {
            IsAntialias = true
        };
        sKPaint.Typeface = sKTypeface;
        sKPaint.TextSize = fontSize;
        SKRect sKRect = new SKRect();
        sKPaint.MeasureText(text, ref sKRect);
        return sKRect;
    }
}


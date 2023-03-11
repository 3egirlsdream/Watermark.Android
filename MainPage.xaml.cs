using JointWatermark.Class;
using SixLabors.ImageSharp.Formats.Jpeg;
//using Microsoft.Maui.Graphics.Platform;
using SkiaSharp;
using SixLabors.Fonts;
using ExifLib;
using SixLabors.ImageSharp;
using Microsoft.Maui;
using MauiApp3.Classes;

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
        SkiaSharpVersion(PickOptions.Images);


    }
    FileResult logoFileResult;
    FileResult imageFileResult;
    public async void SkiaSharpVersion(PickOptions options)

    {
        imageFileResult = await FilePicker.Default.PickAsync(options);
        if (imageFileResult == null)
            return;
        if (!imageFileResult.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase)) return;

        var stream3 = await imageFileResult.OpenReadAsync();
        pic.Source = ImageSource.FromStream(() => stream3);
        var stream1 = await imageFileResult.OpenReadAsync();
        var stream2 = await imageFileResult.OpenReadAsync();
        var sKTypeface_B = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans-Bold.ttf").Result);
        var sKTypeface = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans.ttf").Result);
        if (logoFileResult == null) return;
        stream4 = await logoFileResult.OpenReadAsync();
        CreateWatermark(stream1, stream2, stream4, new ImageProperties("", ""), sKTypeface, sKTypeface_B, true);

    }


    private void CreateWatermark(System.IO.Stream stream, System.IO.Stream stream2, System.IO.Stream logoStream, ImageProperties properties, SKTypeface sKTypeface, SKTypeface sKTypeface_B, bool isPreview = false)
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
                properties.Config.LeftPosition2 = date.ToString();
                var time = 1/exposureTime;
                properties.Config.RightPosition1 = $"F{fnumber} 1/{time}S ISO{iso2} {focal}mm";
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
            SKBitmap logoResized = new SKBitmap(waterWidth, waterHeight);
            logo.Resize(logoResized, SKBitmapResizeMethod.Lanczos3);


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
                    var twoLineWordTotalHeight = 1.04 * fontSize + font20;

                    //绘制第右侧一行文字
                    var start = w - TextSize.Width - padding_right.Width;
                    var startHeight = (h - twoLineWordTotalHeight) / 2 + TextSize.Height;
                    var Params = new SixLabors.ImageSharp.PointF(start, (int)startHeight);

                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.Black, TextSize = fontSize, Typeface = sKTypeface_B })
                    {
                        canvas.DrawText(properties.Config.RightPosition1, new SKPoint(Params.X, Params.Y + img.Height), textPaint);
                    }
                    //绘制右侧第二行文字

                    var XY = new SixLabors.ImageSharp.PointF(Params.X, (int)(Params.Y + 1.04 * fontSize));
                    //145, 145, 145
                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.Gray, TextSize = font20, Typeface = sKTypeface })
                    {
                        canvas.DrawText(properties.Config.RightPosition2, new SKPoint(XY.X, (int)XY.Y + img.Height), textPaint);
                    }

                    //绘制竖线
                    var font20Size = MeasureText("A", sKTypeface, font20);
                    var lStart = new SixLabors.ImageSharp.PointF(Params.X - (int)(oneSize.Width * 0.6), (int)(0.5*(h - logoResized.Height)));
                    var lEnd = new SixLabors.ImageSharp.PointF(lStart.X, (int)(h - lStart.Y));

                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.LightGray, TextSize = 2 * fontxs, Typeface = sKTypeface })
                    {
                        canvas.DrawLine(lStart.X, lStart.Y + img.Height, lEnd.X, lEnd.Y + img.Height, textPaint);
                        canvas.DrawLine(lStart.X+1, lStart.Y + img.Height, lEnd.X+1, lEnd.Y + img.Height, textPaint);
                    }

                    //绘制LOGO
                    var line = new SixLabors.ImageSharp.Point((int)(lStart.X - (int)(oneSize.Width * 0.6) - logoResized.Width), (int)(0.5*(h - logoResized.Height)));
                    //wm.Mutate(x => x.DrawImage(logo, line, 1));
                    using (SKPaint textPaint = new SKPaint() { Color = SKColors.LightGray, TextSize = 2 * fontxs, Typeface = sKTypeface })
                    {
                        SKImage image2 = SKImage.FromBitmap(logoResized);
                        canvas.DrawImage(image2, line.X, line.Y + img.Height);
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
                    SKImage image = SKImage.FromBitmap(outputBitmap);
                    SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

                    if (isPreview)
                    {
                        var s = data.AsStream(false);
                        preview.Source = ImageSource.FromStream(() => s);
                        return;
                    }
                    string p1 = "";
#if WINDOWS
p1 = "C:\\Users\\Jiang\\Desktop\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
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


    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            msg.Text  = "保存中...";
            var stream1 = await imageFileResult.OpenReadAsync();
            var stream2 = await imageFileResult.OpenReadAsync();
            var sKTypeface_B = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans-Bold.ttf").Result);
            var sKTypeface = SKTypeface.FromStream(FileSystem.OpenAppPackageFileAsync("HarmonyOS-Sans.ttf").Result);
            if (logoFileResult == null)
            {
                msg.Text  = "请选择LOGO";
                return;
            }
            stream4 = await logoFileResult.OpenReadAsync();
            CreateWatermark(stream1, stream2, stream4, new ImageProperties("", ""), sKTypeface, sKTypeface_B);
            msg.Text  ="保存成功";
        }
        catch (Exception ex)
        {

        }



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
        sKPaint.TextAlign = SKTextAlign.Center;
        sKPaint.Style = SKPaintStyle.Fill;
        SKRect sKRect = new SKRect();
        sKPaint.MeasureText(text, ref sKRect);
        return sKRect;
    }
}


using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

public class ImageBlender
{
    /// <summary>
    /// Blends an overlay image over a background image.
    /// </summary>
    /// <param name="background">The bottom image.</param>
    /// <param name="overlay">The top image.</param>
    /// <param name="opacity">Overlay opacity from 0.0 to 1.0.</param>
    public static Bitmap BlendImages(
        Bitmap background,
        Bitmap overlay,
        float opacity)
    {
        if (background == null)
            throw new ArgumentNullException(nameof(background));

        if (overlay == null)
            throw new ArgumentNullException(nameof(overlay));

        // Math.Clamp is not available in .NET Framework 4.7,
        // so clamp the value manually.
        if (opacity < 0.0f)
            opacity = 0.0f;
        else if (opacity > 1.0f)
            opacity = 1.0f;

        Bitmap result = new Bitmap(
            background.Width,
            background.Height,
            PixelFormat.Format32bppArgb);

        using (Graphics graphics = Graphics.FromImage(result))
        {
            // When a color is rendered, it overwrites the background color
            //graphics.CompositingMode = CompositingMode.SourceCopy; 
            // When a color is rendered, it is blended with the background color
            // The blend is determined by the alpha component of the color being rendered
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.CompositingQuality = CompositingQuality.GammaCorrected;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            graphics.DrawImage(
                background,
                new Rectangle(
                    0,
                    0,
                    result.Width,
                    result.Height));

            ColorMatrix colorMatrix = new ColorMatrix
            {
                Matrix33 = opacity
            };

            using (ImageAttributes attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(
                    colorMatrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);

                graphics.DrawImage(
                    overlay,
                    new Rectangle(
                        0,
                        0,
                        result.Width,
                        result.Height),
                    0,
                    0,
                    overlay.Width,
                    overlay.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }
        }

        return result;
    }
}

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.Error.WriteLine(
                "Usage: ImageBlend.exe " +
                "<background> <overlay> <opacity-percent> <output>");

            Console.Error.WriteLine();
            Console.Error.WriteLine(
                "Example: ImageBlend.exe " +
                "background.jpg overlay.png 50 blended.png");

            return 1;
        }

        string backgroundPath = args[0];
        string overlayPath = args[1];
        string outputPath = args[3];

        float opacityPercent;

        if (!float.TryParse(
                args[2],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out opacityPercent))
        {
            Console.Error.WriteLine(
                "Opacity must be a number between 0 and 100.");
            return 1;
        }

        if (opacityPercent < 0.0f || opacityPercent > 100.0f)
        {
            Console.Error.WriteLine(
                "Opacity must be between 0 and 100.");
            return 1;
        }

        float opacity = opacityPercent / 100.0f;

        try
        {
            using (Bitmap background = new Bitmap(backgroundPath))
            using (Bitmap overlay = new Bitmap(overlayPath))
            using (Bitmap result = ImageBlender.BlendImages(
                background,
                overlay,
                opacity))
            {
                result.Save(outputPath, GetImageFormat(outputPath));
            }

            Console.WriteLine("Blended image saved to: " + outputPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
            return 1;
        }
    }

    private static ImageFormat GetImageFormat(string filePath)
    {
        string extension =
            System.IO.Path.GetExtension(filePath)
                .ToLowerInvariant();

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return ImageFormat.Jpeg;

            case ".bmp":
                return ImageFormat.Bmp;

            case ".gif":
                return ImageFormat.Gif;

            case ".tif":
            case ".tiff":
                return ImageFormat.Tiff;

            case ".png":
            default:
                return ImageFormat.Png;
        }
    }
}

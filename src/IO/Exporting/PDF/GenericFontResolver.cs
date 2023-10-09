using System.Reflection;
using PdfSharpCore.Fonts;

namespace FarmOrganizer.IO.Exporting.PDF
{
    /// <summary>
    /// Original Author: <see href="https://github.com/icebeam7">Luis Beltran</see> | <see href="https://github.com/icebeam7/PDFDemo">Source GitHub repository - PDFDemo</see>
    /// </summary>
    public class GenericFontResolver : IFontResolver
    {
        public string DefaultFontName => "OpenSans";

        public byte[] GetFont(string faceName)
        {
            if (faceName.Contains(DefaultFontName))
            {
                //var assembly = typeof(ProductsReport).GetTypeInfo().Assembly;
                var assembly = typeof(PdfBuilder).GetTypeInfo().Assembly;
                //var stream = assembly.GetManifestResourceStream($"PDFDemo.Fonts.{faceName}.ttf");
                var stream = assembly.GetManifestResourceStream($"FarmOrganizer.Resources.Fonts.{faceName}.ttf");

                using (var reader = new StreamReader(stream))
                {
                    var bytes = default(byte[]);

                    using (var ms = new MemoryStream())
                    {
                        reader.BaseStream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }

                    return bytes;
                }
            }
            else
            {
                return GetFont(DefaultFontName);
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string fontName;
            switch (familyName)
            {
                case "Open Sans":
                case "OpenSans":
                default:
                    fontName = "OpenSans";
                    break;
            }

            if (isBold && isItalic)
                fontName = $"{fontName}-BoldItalic";
            else if (isBold)
                fontName = $"{fontName}-Bold";
            else if (isItalic)
                fontName = $"{fontName}-Italic";
            else
                fontName = $"{fontName}-Regular";
            
            return new FontResolverInfo(fontName);
        }
    }
}
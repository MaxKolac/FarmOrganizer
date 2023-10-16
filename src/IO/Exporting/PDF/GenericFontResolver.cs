using PdfSharpCore.Fonts;

namespace FarmOrganizer.IO.Exporting.PDF
{
    /// <summary>
    /// Generic implementation of <see cref="IFontResolver"/>, originally written by <see href="https://github.com/icebeam7">Luis Beltran</see>, modified to be compatible with .NET MAUI by <see href="https://github.com/MaxKolac">me</see>.
    /// <para>
    /// Original Author: <see href="https://github.com/icebeam7">Luis Beltran</see> | <see href="https://github.com/icebeam7/PDFDemo">Source GitHub repository - PDFDemo</see>
    /// </para>
    /// </summary>
    public class GenericFontResolver : IFontResolver
    {
        public string DefaultFontName => "OpenSans";

        public byte[] GetFont(string faceName)
        {
            if (faceName.Contains(DefaultFontName))
            {
                using var reader = new StreamReader(FileSystem.Current.OpenAppPackageFileAsync($"{faceName}.ttf").Result);
                using var memoryStream = new MemoryStream();
                reader.BaseStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            else
            {
                return GetFont(DefaultFontName);
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var fontName = familyName switch
            {
                "Open Sans" => "OpenSans",
                "OpenSans" => "OpenSans",
                _ => "OpenSans",
            };

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
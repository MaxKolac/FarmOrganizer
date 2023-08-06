namespace FarmOrganizer.Database
{
    /// <summary>
    /// A class containing methods for manipulation of the database's file.
    /// </summary>
    public static class DatabaseFile
    {
        /// <summary>
        /// The name of the database's file. It needs to be the same as the DB file in the <c>Resources</c> folder.
        /// </summary>
        public static string Filename => "database.sqlite3";
        /// <summary>
        /// The full qualified path to the database's file. Combination of its location and name.
        /// </summary>
        public static string FullPath => Path.Combine(FileSystem.Current.AppDataDirectory, Filename);

        /// <summary>
        /// Copy the read-only fresh database file bundled with the APK to app's local storage to be editable.
        /// <para>
        /// <see href="https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers?tabs=android"/>
        /// </para>
        /// </summary>
        public static async Task Create()
        {
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(Filename);
            using FileStream outputStream = File.Create(FullPath);
            await inputStream.CopyToAsync(outputStream);
        }

        /// <summary>
        /// Removes the database's file from the app's <c>AppDataDirectory, assuming it exists.</c>.
        /// </summary>
        public static void Delete()
        {
            if (Exists()) 
                File.Delete(FullPath);
        }

        /// <summary>
        /// Returns <c>true</c> if the database's file exists in the predetermined location and if the app has required permissions to it. Uses <c>File.Exists</c> method.
        /// </summary>
        public static bool Exists() =>
            File.Exists(FullPath);
    }
}

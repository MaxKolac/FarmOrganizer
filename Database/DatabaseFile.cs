using FarmOrganizer.Models;

namespace FarmOrganizer.Database
{
    /// <summary>
    /// A class containing static methods for manipulation of the database's file.
    /// </summary>
    public static class DatabaseFile
    {
        /// <summary>
        /// The name of the database's file. It needs to be the same as the DB file in the <c>Resources</c> folder.
        /// </summary>
        public static string Filename => "database.sqlite3";
        /// <summary>
        /// The name of a temporary copy of the database's file.
        /// </summary>
        public static string BackupFilename => Filename + ".temp";
        /// <summary>
        /// The path of the folder, in which the database file is stored.
        /// </summary>
        public static string Location => FileSystem.Current.AppDataDirectory;
        /// <summary>
        /// The full qualified path to the database's file. Combination of its location and name.
        /// </summary>
        public static string FullPath => Path.Combine(FileSystem.Current.AppDataDirectory, Filename);

        /// <summary>
        /// Copy the read-only fresh database file bundled with the APK to app's external storage to be editable.
        /// You can dispatch the creation of a new file with the following sample:
        /// <code>
        /// Application.Current.MainPage.Dispatcher.Dispatch(async () => await DatabaseFile.Create());
        /// </code>
        /// Alternatively, you can invoke it on the main thread using the following sample:
        /// <code>
        /// MainThread.InvokeOnMainThreadAsync(DatabaseFile.Create);
        /// </code>
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
        /// Removes the database's file from the app's <c>AppDataDirectory.</c>.
        /// </summary>
        public static async Task Delete()
        {
            using var context = new DatabaseContext();
            await context.Database.EnsureDeletedAsync();
        }

        /// <summary>
        /// Returns <c>true</c> if the database's file exists in the predetermined location and if the app has required permissions to it. Uses <c>File.Exists</c> method.
        /// </summary>
        public static bool Exists() =>
            File.Exists(FullPath);

        /// <summary>
        /// Exports a copy of the database's file to the specified folder.
        /// </summary>
        /// <param name="destinationFolderPath">The destination folder to copy the file to.</param>
        public static async Task ExportTo(string destinationFolderPath)
        {
            using Stream dbFile = File.OpenRead(FullPath);
            using FileStream outputStream = File.Create(Path.Combine(destinationFolderPath, Filename));
            await dbFile.CopyToAsync(outputStream);
        }

        /// <summary>
        /// <para>
        /// Creates or overwrites database's file with the one provided in the source path. It also creates a new DatabaseContext instance and runs basic read tests to check for consistency with the schema.
        /// </para>
        /// <para>
        /// It is highly recommended to first create an emergency back up of the database with <see cref="CreateBackup"/> method, and in case of an import failure, restore the contents with <see cref="RestoreBackup(bool)"/>.
        /// </para>
        /// </summary>
        /// <param name="sourceFilePath">The file which should be imported as the database.</param>
        /// <exception cref="IOException"/>
        /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException"/>
        public static async Task ImportFrom(string sourceFilePath)
        {
            using Stream sourceFile = File.OpenRead(sourceFilePath);
            using FileStream destinationFile = File.Create(FullPath);
            await sourceFile.CopyToAsync(destinationFile);

            using var context = new DatabaseContext();
            List<BalanceLedger> ledger = context.BalanceLedgers.ToList();
            List<CostType> costTypes = context.CostTypes.ToList();
            List<CropField> cropFields = context.CropFields.ToList();
            List<FieldEfficiency> fieldEfficiencies = context.FieldEfficiencies.ToList();
            List<Season> seasons = context.Seasons.ToList();
            context.SaveChanges();
        }

        /// <summary>
        /// Creates a backup copy of the database's file in the same directory and an additional ".temp" at the end of its name.
        /// </summary>
        public static async Task CreateBackup()
        {
            using FileStream dbBackupFile = File.Create(Path.Combine(Location, BackupFilename));
            using FileStream database = File.OpenRead(FullPath);
            await database.CopyToAsync(dbBackupFile);
        }

        /// <summary>
        /// Copies and overwrites the contents of database's file with contents of the backup file.
        /// </summary>
        /// <param name="deleteOnCompletion">Should the used backup file be deleted after restoring.</param>
        public static async Task RestoreBackup(bool deleteOnCompletion = true)
        {
            using (FileStream dbBackupFile = File.OpenRead(Path.Combine(Location, BackupFilename)))
            {
                using FileStream database = File.Create(FullPath);
                await dbBackupFile.CopyToAsync(database);
            }
            if (deleteOnCompletion)
                DeleteBackup();
        }

        /// <summary>
        /// Deletes the backup file of the database.
        /// </summary>
        public static void DeleteBackup() =>
            File.Delete(Path.Combine(Location, BackupFilename));
    }
}

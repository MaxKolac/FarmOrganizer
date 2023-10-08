using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.Database
{
    /// <summary>
    /// A class containing static methods for manipulation of the database's file.
    /// </summary>
    public static class DatabaseFile
    {
        public static string Filename => "database";
        public static string Extension => ".sqlite3";
        /// <summary>
        /// The name of the database's file. It needs to be the same as the DB file in the <c>Resources</c> folder.
        /// </summary>
        public static string FullFilename => Filename + Extension;
        /// <summary>
        /// The name of a temporary copy of the database's file.
        /// </summary>
        public static string BackupFilename => FullFilename + ".temp";
        /// <summary>
        /// The path of the folder, in which the database file is stored.
        /// </summary>
        public static string InternalStorage => FileSystem.Current.AppDataDirectory;
        /// <summary>
        /// The full qualified path to the database's file. Combination of its location and name.
        /// </summary>
        public static string FullPath => Path.Combine(FileSystem.Current.AppDataDirectory, FullFilename);

        /// <summary>
        /// Copy the read-only fresh database file bundled with the APK to app's external storage to be editable.
        /// <para>
        /// <see href="https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers?tabs=android"/>
        /// </para>
        /// </summary>
        public static async Task Create()
        {
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(FullFilename);
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
            //SqliteConnection.ClearAllPools();
            //File.Delete(FullPath);
            //context.SaveChanges();
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
            string actualFilename = FullFilename;
            int attempts = 0;
            while (File.Exists(Path.Combine(destinationFolderPath, actualFilename)))
            {
                attempts++;
                actualFilename = $"{Filename}({attempts})" + Extension;
            };
            using FileStream outputStream = File.Create(Path.Combine(destinationFolderPath, actualFilename));
            await File.OpenRead(FullPath).CopyToAsync(outputStream);
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
        /// <exception cref="TableValidationException"/>
        public static async Task ImportFrom(string sourceFilePath)
        {
            using Stream sourceFile = File.OpenRead(sourceFilePath);
            using FileStream destinationFile = File.Create(FullPath);
            await sourceFile.CopyToAsync(destinationFile);

            BalanceLedger.Validate(null);
            CostType.Validate(null);
            CropField.Validate(null);
            Season.Validate(null);
        }

        /// <summary>
        /// Creates a backup copy of the database's file in the same directory and an additional ".temp" at the end of its name.
        /// </summary>
        public static async Task CreateBackup()
        {
            using FileStream dbBackupFile = File.Create(Path.Combine(InternalStorage, BackupFilename));
            using FileStream database = File.OpenRead(FullPath);
            await database.CopyToAsync(dbBackupFile);
        }

        /// <summary>
        /// Copies and overwrites the contents of database's file with contents of the backup file.
        /// </summary>
        /// <param name="deleteOnCompletion">Should the used backup file be deleted after restoring.</param>
        public static async Task RestoreBackup(bool deleteOnCompletion = true)
        {
            using (FileStream dbBackupFile = File.OpenRead(Path.Combine(InternalStorage, BackupFilename)))
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
            File.Delete(Path.Combine(InternalStorage, BackupFilename));

        /// <summary>
        /// <para>
        /// Checks if the user granted <see cref="Permissions.StorageWrite"/> and <see cref="Permissions.StorageRead"/>. 
        /// If any of those permissions turns out to be <see cref="PermissionStatus.Denied"/>, attempts to request it from the user through a pop-up.
        /// </para>
        /// <para>
        /// <b>Important!</b> The pop-ups do not show on Android API level greater than 31. 
        /// According to the linked StackOverflow question, the <see cref="Permissions.StorageWrite"/> has been discontinued from Android API 33.
        /// The current workaround is purposefuly downgrading the app to API 31.
        /// </para>
        /// <para><see href="https://stackoverflow.com/a/75331176/21342746">Liyun Zhang's answer on StackOverflow</see></para>
        /// <para><see href="https://github.com/dotnet/maui/issues/11275">.NET MAUI GitHub Issue discussion regarding the workaround</see></para>
        /// </summary>
        /// <returns><c>true</c> if both permissions have been granted, <c>false</c> otherwise.</returns>
        [Obsolete("This method should no longer be required for the app to receive proper permissions.")]
        public static async Task<bool> RequestPermissions()
        {
            PermissionStatus storageWritePerm = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            PermissionStatus storageReadPerm = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            bool writePermissionGranted = storageWritePerm == PermissionStatus.Granted;
            bool readPermissionGranted = storageReadPerm == PermissionStatus.Granted;

            if (!writePermissionGranted)
            {
                Permissions.ShouldShowRationale<Permissions.StorageWrite>();
                storageWritePerm = await Permissions.RequestAsync<Permissions.StorageWrite>();
                writePermissionGranted = storageWritePerm == PermissionStatus.Granted;
            }
            if (!readPermissionGranted)
            {
                Permissions.ShouldShowRationale<Permissions.StorageRead>();
                storageReadPerm = await Permissions.RequestAsync<Permissions.StorageRead>();
                readPermissionGranted = storageReadPerm == PermissionStatus.Granted;
            }
            return writePermissionGranted && readPermissionGranted;
        }
    }
}

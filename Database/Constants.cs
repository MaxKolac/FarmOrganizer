using SQLite;

namespace FarmOrganizer.Database
{
    public static class Constants
    {
        public const string Filename = "farmOrganizerDB.db3";
        public static string Filepath = Path.Combine(FileSystem.AppDataDirectory, Filename);
        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |     //Open db in Read/Write mode
            SQLiteOpenFlags.Create |        //Create a db if it doesn't exist
            SQLiteOpenFlags.ProtectionNone |//The db isn't encrypted
            SQLiteOpenFlags.SharedCache;    //Enable multithreaded db access
    }
}

using SQLite;

namespace FarmOrganizer.Database
{
    internal class DatabaseAccess
    {
        public static readonly string Filename = "database.sqlite3";
        public static readonly string Filepath = Path.Combine(FileSystem.AppDataDirectory, Filename);
        public static readonly SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |     //Open db in Read/Write mode
            SQLiteOpenFlags.Create |        //Create a db if it doesn't exist
            SQLiteOpenFlags.ProtectionNone |//The db isn't encrypted
            SQLiteOpenFlags.SharedCache;    //Enable multithreaded db access

        public static DatabaseAccess Instance { get; private set; } = new DatabaseAccess();
        private readonly SQLiteAsyncConnection _database;

        public DatabaseAccess()
        {
            _database = new SQLiteAsyncConnection(Filepath, Flags);
            //_database.CreateTableAsync<BalanceLedgerEntry>();
            //_database.CreateTableAsync<CropField>();
            //_database.CreateTableAsync<CostType>();
        }
    }
}

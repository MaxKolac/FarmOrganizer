/*using SQLite;

namespace FarmOrganizer.Database
{
    internal class Database
    {
        private readonly SQLiteAsyncConnection _database;
        public static Database Instance { get; private set; } = new Database();

        public Database()
        {
            _database = new SQLiteAsyncConnection(Constants.Filepath, Constants.Flags);
            //_database.CreateTableAsync<BalanceLedgerEntry>();
            //_database.CreateTableAsync<CropField>();
            //_database.CreateTableAsync<CostType>();
        }
    }
}*/

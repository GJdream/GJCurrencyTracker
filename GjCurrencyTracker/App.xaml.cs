using GjCurrencyTracker.Data;

namespace GjCurrencyTracker
{
    public partial class App : Application
    {
        private static LocalDatabase _database;
        public static LocalDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "alerts.db3");
                    _database = new LocalDatabase(dbPath);
                }
                return _database;
            }
        }

        public App()
        {
            InitializeComponent();

            //var dbPath = Path.Combine(FileSystem.AppDataDirectory, "alerts.db3");
            //Database = new LocalDatabase(dbPath);

            MainPage = new AppShell();
        }
    }

}

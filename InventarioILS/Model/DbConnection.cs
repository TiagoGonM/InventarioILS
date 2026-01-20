using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace InventarioILS.Model
{
    public class DbConnection : SqliteConnection
    {
        readonly static string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");
        readonly static string resourcePath = "InventarioILS.Resources.DatabaseSchema.sql";

        private static void SetupDatabase(DbConnection conn)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath)
                ?? throw new FileNotFoundException("Database schema resource not found.");

            using var reader = new StreamReader(stream);

            string sqlScript = reader.ReadToEnd();

            conn.Execute("PRAGMA foreign_keys = ON"); // Force foreign_keys policy
            conn.Execute(sqlScript);
        }

        private static async Task SetupDatabaseAsync(DbConnection conn)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath)
                ?? throw new FileNotFoundException("Database schema resource not found.");

            using var reader = new StreamReader(stream);

            string sqlScript = await reader.ReadToEndAsync().ConfigureAwait(false);


            await conn.ExecuteAsync("PRAGMA foreign_keys = ON").ConfigureAwait(false); // Force foreign_keys policy
            await conn.ExecuteAsync(sqlScript).ConfigureAwait(false);
        }

        public static DbConnection CreateAndOpen()
        {
            var connection = new DbConnection();

            try
            {

                if (!File.Exists(dbPath))
                {
                    connection.Open();
                    try
                    {
                        SetupDatabase(connection);
                    }
                    catch (SqliteException)
                    {
                        File.Delete(dbPath);
                        throw;
                    }
                }
                else
                {
                    connection.Open();
                }

                return connection;
            }
            catch (SqliteException ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}");
                connection.Dispose();
                throw;
            }
        }

        public static async Task<DbConnection> CreateAndOpenAsync()
        {
            var connection = new DbConnection();

            try
            {
                if (!File.Exists(dbPath))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    try
                    {
                        await SetupDatabaseAsync(connection);

                    }
                    catch (SqliteException)
                    {
                        File.Delete(dbPath);
                        throw;
                    }
                }
                else
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                return connection;
            }
            catch (SqliteException ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}");
                connection.Dispose();
                throw;
            }
        }

        public uint LastRowIdInserted => this.ExecuteScalar<uint>("SELECT last_insert_rowid()");

        private DbConnection() : base($"Data Source={dbPath}") { }

        ~DbConnection()
        {
            if (State == System.Data.ConnectionState.Open)
            {
                Close();
            }
            Dispose();
        }
    }
}

using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace InventarioILS.Model
{
    public sealed class DbConnection : SqliteConnection, IDisposable, IAsyncDisposable
    {
        readonly static string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        readonly static string dbDirectory = Path.Combine(appDataDir, "InventarioILS");
        readonly static string dbPath = Path.Combine(dbDirectory, "inventory.db");

        readonly static string resourcePath = "InventarioILS.Resources.DatabaseSchema.sql";

        private bool _disposed;

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

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            try
            {
                if (!File.Exists(dbPath))   
                {
                    connection.Open();
                    try
                    {
                        SetupDatabase(connection);
                    }
                    catch (Exception)
                    {
                        try { File.Delete(dbPath); } catch (Exception deleteEx) { StatusManager.Instance.UpdateMessageStatus(deleteEx.ToString(), StatusManager.MessageType.ERROR); }
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
                MessageBox.Show($"Error inicializando la base de datos: {ex}", "Error", MessageBoxButton.OK);
                connection.Dispose();
                throw new InvalidOperationException("Error initializing database.", ex);
            }
        }

        public static async Task<DbConnection> CreateAndOpenAsync()
        {
            var connection = new DbConnection();

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            try
            {
                if (!File.Exists(dbPath))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    try
                    {
                        await SetupDatabaseAsync(connection).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        try { File.Delete(dbPath); } catch (Exception deleteEx) { StatusManager.Instance.UpdateMessageStatus(deleteEx.ToString(), StatusManager.MessageType.ERROR); }
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
                MessageBox.Show($"Error inicializando la base de datos: {ex}", "Error", MessageBoxButton.OK );
                await connection.DisposeAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Error initializing database.", ex);
            }
        }

        public long LastRowIdInserted => this.ExecuteScalar<long>("SELECT last_insert_rowid()");

        private DbConnection() : base($"Data Source={dbPath}") { }

        public async new ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            base.Dispose(false);
            GC.SuppressFinalize(this);
        }

        private new void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    if (State == System.Data.ConnectionState.Open)
                    {
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    StatusManager.Instance.UpdateMessageStatus($"Error al intentar cerrar la conexión con la base de datos: {ex}", StatusManager.MessageType.WARNING);
                }

                base.Dispose();
            }

            _disposed = true;
        }

        private async ValueTask DisposeAsyncCore()
        {
            try
            {
                if (State == System.Data.ConnectionState.Open)
                {
                    await CloseAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                StatusManager.Instance.UpdateMessageStatus($"Error al intentar cerrar la conexión con la base de datos: {ex}", StatusManager.MessageType.WARNING);
            }

            await base.DisposeAsync().ConfigureAwait(false);
        }
    }
}

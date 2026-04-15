using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace GymTracker.Data
{
    public static class DatabaseHelper
    {
        public static string ConnectionString { get; } =
            "Data Source=" + Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GymTracker", "gymtracker.db");

        /// <summary>
        /// Devuelve una conexión SQLite abierta.
        /// </summary>
        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Crea las tablas de la base de datos si no existen.
        /// Debe llamarse al arrancar la aplicación.
        /// </summary>
        public static void InitializeDatabase()
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GymTracker");
            Directory.CreateDirectory(dbPath);

            using var connection = GetConnection();

            // Activar claves foráneas
            Ejecutar(connection, "PRAGMA foreign_keys = ON;");

            Ejecutar(connection, @"
                CREATE TABLE IF NOT EXISTS Ejercicio (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre          TEXT    NOT NULL,
                    GrupoMuscular   TEXT    NOT NULL,
                    Descripcion     TEXT,
                    ImagenRuta      TEXT
                );");

            Ejecutar(connection, @"
                CREATE TABLE IF NOT EXISTS Rutina (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre          TEXT    NOT NULL,
                    Descripcion     TEXT,
                    FechaCreacion   TEXT    NOT NULL
                );");

            Ejecutar(connection, @"
                CREATE TABLE IF NOT EXISTS RutinaEjercicio (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    RutinaId    INTEGER NOT NULL,
                    EjercicioId INTEGER NOT NULL,
                    DiaSemana   TEXT    NOT NULL,
                    Orden       INTEGER NOT NULL,
                    FOREIGN KEY (RutinaId)    REFERENCES Rutina(Id)    ON DELETE CASCADE,
                    FOREIGN KEY (EjercicioId) REFERENCES Ejercicio(Id) ON DELETE CASCADE
                );");

            Ejecutar(connection, @"
                CREATE TABLE IF NOT EXISTS Sesion (
                    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
                    RutinaId         INTEGER NOT NULL,
                    Fecha            TEXT    NOT NULL,
                    DuracionMinutos  INTEGER NOT NULL,
                    Notas            TEXT,
                    FOREIGN KEY (RutinaId) REFERENCES Rutina(Id) ON DELETE CASCADE
                );");

            Ejecutar(connection, @"
                CREATE TABLE IF NOT EXISTS SerieRegistro (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    SesionId     INTEGER NOT NULL,
                    EjercicioId  INTEGER NOT NULL,
                    NumSerie     INTEGER NOT NULL,
                    Repeticiones INTEGER NOT NULL,
                    PesoKg       REAL    NOT NULL,
                    FOREIGN KEY (SesionId)    REFERENCES Sesion(Id)    ON DELETE CASCADE,
                    FOREIGN KEY (EjercicioId) REFERENCES Ejercicio(Id) ON DELETE CASCADE
                );");
        }

        private static void Ejecutar(SqliteConnection connection, string sql)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}

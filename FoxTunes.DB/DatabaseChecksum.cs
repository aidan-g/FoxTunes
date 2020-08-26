using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class DatabaseChecksum : BaseComponent
    {
        public string Calculate(string setup)
        {
            var hashCode = setup.GetHashCode();
            var components = ComponentRegistry.Instance.GetComponents<IDatabaseInitializer>();
            unchecked
            {
                foreach (var component in components)
                {
                    hashCode += component.Checksum.GetHashCode();
                }
            }
            return Convert.ToString(Math.Abs(hashCode));
        }

        public string Get(IDatabase database)
        {
            const string SQL = "SELECT Checksum FROM Main";
            try
            {
                using (var connection = database.Provider.CreateConnection(database))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = SQL;
                        return Convert.ToString(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to retrieve checksum from database: {0}", e.Message);
                return null;
            }
        }

        public void Set(IDatabase database, string setup)
        {
            const string SQL = "INSERT INTO Main (Checksum) VALUES ('{0}')";
            var checksum = this.Calculate(setup);
            using (var connection = database.Provider.CreateConnection(database))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(SQL, checksum);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool Validate(IDatabase database, string setup)
        {
            var expected = this.Calculate(setup);
            var actual = this.Get(database);
            if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Write(this, LogLevel.Warn, "Database checksum does not match. Expected \"{0}\" but found \"{1}\".", expected, actual);
                return false;
            }
            return true;
        }

        public static readonly DatabaseChecksum Instance = new DatabaseChecksum();
    }
}

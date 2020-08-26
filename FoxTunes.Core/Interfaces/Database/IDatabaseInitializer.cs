using System;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseInitializer
    {
        string Checksum { get; }

        void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type);
    }

    [Flags]
    public enum DatabaseInitializeType : byte
    {
        None = 0,
        Library = 1,
        Playlist = 2,
        All = Library | Playlist
    }
}

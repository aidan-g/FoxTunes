CREATE TABLE [Main](
	[Checksum] text COLLATE NOCASE);

CREATE TABLE [MetaDataItems](
	[Id] INTEGER PRIMARY KEY NOT NULL, 
	[Name] text NOT NULL COLLATE NOCASE, 
	[Type] bigint NOT NULL,
	[Value] text COLLATE NOCASE);

CREATE TABLE [LibraryRoots] (
	[Id] INTEGER PRIMARY KEY NOT NULL, 
	[DirectoryName] text NOT NULL COLLATE NOCASE);

CREATE TABLE [LibraryItems] (
	[Id] INTEGER PRIMARY KEY NOT NULL, 
	[DirectoryName] text NOT NULL COLLATE NOCASE, 
	[FileName] text NOT NULL COLLATE NOCASE, 
	[ImportDate] text NOT NULL COLLATE NOCASE,
	[Status] INTEGER NOT NULL,
	[Flags] INTEGER NOT NULL);

CREATE TABLE [Playlists](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
	[Sequence] INTEGER NOT NULL,
	[Name] TEXT NOT NULL COLLATE NOCASE,
	[Type] bigint NOT NULL,
	[Filter] TEXT NULL COLLATE NOCASE,
	[Enabled] bit NOT NULL);

CREATE TABLE [PlaylistItems](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
	[Playlist_Id] INTEGER NOT NULL,
	[LibraryItem_Id] INTEGER NULL,
    [Sequence] bigint NOT NULL, 
    [DirectoryName] text NOT NULL COLLATE NOCASE, 
    [FileName] text NOT NULL COLLATE NOCASE, 
    [Status] bigint NOT NULL,
	[Flags] INTEGER NOT NULL);

CREATE TABLE [LibraryHierarchies] ( 
	[Id] INTEGER PRIMARY KEY NOT NULL, 
	[Sequence] INTEGER NOT NULL, 
	[Name] TEXT NOT NULL COLLATE NOCASE,
	[Type] bigint NOT NULL,
	[Enabled] bit NOT NULL);

CREATE TABLE [LibraryHierarchyLevels] (
	[Id]	INTEGER PRIMARY KEY NOT NULL,
	[LibraryHierarchy_Id]  INTEGER NULL,
	[Sequence] INTEGER NOT NULL,
	[Script]	TEXT NOT NULL COLLATE NOCASE);

CREATE TABLE [LibraryHierarchyItems](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
	[Parent_Id] INTEGER NULL,
    [LibraryHierarchy_Id] bigint NOT NULL,
    [Value] text NOT NULL COLLATE NOCASE, 
    [IsLeaf] bit NOT NULL);

CREATE TABLE [PlaylistItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL,
    [MetaDataItem_Id] INTEGER NOT NULL);

CREATE TABLE [PlaylistColumns] ( 
  [Id] INTEGER PRIMARY KEY NOT NULL, 
  [Sequence] INTEGER NOT NULL, 
  [Name] text NOT NULL COLLATE NOCASE, 
  [Type] bigint NOT NULL,
  [Script] text NULL COLLATE NOCASE,
  [Plugin] text NULL COLLATE NOCASE,
  [Tag] text NULL COLLATE NOCASE,
  [Format] text NULL COLLATE NOCASE,
  [Width] numeric(53,0) NULL,
  [Enabled] bit NOT NULL);

CREATE TABLE [LibraryItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL,
    [MetaDataItem_Id] INTEGER NOT NULL);

CREATE TABLE [LibraryHierarchyItem_LibraryItem] (
	[Id]	INTEGER PRIMARY KEY NOT NULL,
	[LibraryHierarchyItem_Id] INTEGER NOT NULL,
	[LibraryItem_Id] INTEGER NOT NULL
);

CREATE INDEX [IDX_LibraryHierarchies_1] ON [LibraryHierarchies](
	[Enabled]
);

CREATE INDEX [IDX_LibraryHierarchyLevels_1] ON [LibraryHierarchyLevels](
	[LibraryHierarchy_Id]
);

CREATE INDEX [IDX_LibraryHierarchyItems_1] ON [LibraryHierarchyItems](
	[Parent_Id],
	[LibraryHierarchy_Id]
);

CREATE INDEX [IDX_LibraryHierarchyItems_2] ON [LibraryHierarchyItems](
	[Parent_Id],
	[LibraryHierarchy_Id],
	[Value],
	[IsLeaf]
);

CREATE INDEX [IDX_PlaylistItem_MetaDataItem_1] ON [PlaylistItem_MetaDataItem](
	[PlaylistItem_Id]
);

CREATE INDEX [IDX_PlaylistItem_MetaDataItem_2] ON [PlaylistItem_MetaDataItem](
	[MetaDataItem_Id]
);

CREATE INDEX [IDX_PlaylistItem_MetaDataItem_3] ON [PlaylistItem_MetaDataItem](
	[PlaylistItem_Id],
	[MetaDataItem_Id]
);

CREATE INDEX [IDX_LibraryItem_MetaDataItem_1] ON [LibraryItem_MetaDataItem](
	[LibraryItem_Id]
);

CREATE INDEX [IDX_LibraryItem_MetaDataItem_2] ON [LibraryItem_MetaDataItem](
	[MetaDataItem_Id]
);

CREATE INDEX [IDX_LibraryItem_MetaDataItem_3] ON [LibraryItem_MetaDataItem](
	[LibraryItem_Id],
	[MetaDataItem_Id]
);

CREATE INDEX [IDX_PlaylistColumns_1] ON [PlaylistColumns](
	[Enabled]
);

CREATE INDEX [IDX_LibraryItems_1] ON [LibraryItems](
	[FileName]
);

CREATE INDEX [IDX_LibraryItems_2] ON [LibraryItems](
	[Status]
);

CREATE INDEX [IDX_LibraryHierarchyItem_LibraryItem_1] ON [LibraryHierarchyItem_LibraryItem](
	[LibraryHierarchyItem_Id]
);

CREATE INDEX [IDX_LibraryHierarchyItem_LibraryItem_2] ON [LibraryHierarchyItem_LibraryItem](
	[LibraryItem_Id]
);

CREATE INDEX [IDX_LibraryHierarchyItem_LibraryItem_3] ON [LibraryHierarchyItem_LibraryItem](
	[LibraryHierarchyItem_Id],
	[LibraryItem_Id]
);

CREATE INDEX [IDX_PlaylistItems_1] ON [PlaylistItems](
	[LibraryItem_Id]
);

CREATE INDEX [IDX_PlaylistItems_2] ON [PlaylistItems](
	[Status]
);

CREATE INDEX [IDX_MetaDataItems_1] ON [MetaDataItems](
	[Name],
	[Type],
	[Value]
);
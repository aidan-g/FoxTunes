CREATE TABLE [Main](
	[Checksum] nvarchar(4000));

CREATE TABLE [MetaDataItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [Name] nvarchar(4000) NOT NULL, 
	[Type] INTEGER NOT NULL,
    [Value] nvarchar(4000));

CREATE TABLE [LibraryRoots] (
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[DirectoryName] nvarchar(4000) NOT NULL);

CREATE TABLE [LibraryItems] (
	Id INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	DirectoryName nvarchar(4000) NOT NULL, 
	FileName nvarchar(4000) NOT NULL, 
	ImportDate varchar(50) NOT NULL,
	Status INTEGER NOT NULL,
	Flags INTEGER NOT NULL);

CREATE TABLE [Playlists](
	[Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	[Sequence] INTEGER NOT NULL,
	[Name] nvarchar(4000) NOT NULL,
	[Type] bigint NOT NULL,
	[Filter] nvarchar(4000) NULL,
	[Enabled] bit NOT NULL);

CREATE TABLE [PlaylistItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	[Playlist_Id] INTEGER NOT NULL REFERENCES Playlists([Id]),
	[LibraryItem_Id] INTEGER NULL REFERENCES LibraryItems([Id]),
    [Sequence] INTEGER NOT NULL, 
    [DirectoryName] nvarchar(4000) NOT NULL, 
    [FileName] nvarchar(4000) NOT NULL, 
    [Status] INTEGER NOT NULL,
	[Flags] INTEGER NOT NULL);

CREATE TABLE [LibraryHierarchies] ( 
  [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
  [Sequence] INTEGER NOT NULL, 
  [Name] nvarchar(4000) NOT NULL,
  [Type] bigint NOT NULL,
  [Enabled] bit NOT NULL);

CREATE TABLE [LibraryHierarchyLevels] (
	[Id]	INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[LibraryHierarchy_Id] INTEGER NULL REFERENCES LibraryHierarchies([Id]),
	[Sequence] INTEGER NOT NULL,
	[Script]	nvarchar(max) NOT NULL
);

CREATE TABLE [LibraryHierarchyItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	[Parent_Id] INTEGER NULL REFERENCES LibraryHierarchyItems([Id]),
    [LibraryHierarchy_Id] INTEGER NOT NULL REFERENCES LibraryHierarchies([Id]), 
    [Value] nvarchar(4000) NOT NULL, 
    [IsLeaf] bit NOT NULL);

CREATE TABLE [PlaylistItem_MetaDataItem](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]), 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [PlaylistColumns] ( 
  [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [Sequence] INTEGER NOT NULL,
  [Name] nvarchar(4000) NOT NULL, 
  [Type] bigint NOT NULL,
  [Script] nvarchar(max) NULL, 
  [Plugin] nvarchar(4000) NULL, 
  [Tag] nvarchar(4000) NULL, 
  [Format] nvarchar(4000) NULL, 
  [Width] numeric(38,0) NULL,
  [Enabled] bit NOT NULL);

CREATE TABLE [LibraryItem_MetaDataItem](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]), 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [LibraryHierarchyItem_LibraryItem] (
	[Id]	INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[LibraryHierarchyItem_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]),
	[LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id])
);
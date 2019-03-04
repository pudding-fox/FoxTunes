CREATE TABLE [MetaDataItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [Name] nvarchar(250) NOT NULL, 
	[Type] INTEGER NOT NULL,
    [Value] nvarchar(250));

CREATE TABLE [LibraryItems] (
	Id INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	DirectoryName nvarchar(250) NOT NULL, 
	FileName nvarchar(250) NOT NULL, 
	Status INTEGER NOT NULL);

CREATE TABLE [PlaylistItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	[LibraryItem_Id] INTEGER NULL REFERENCES LibraryItems([Id]),
    [Sequence] INTEGER NOT NULL, 
    [DirectoryName] nvarchar(250) NOT NULL, 
    [FileName] nvarchar(250) NOT NULL, 
    [Status] INTEGER NOT NULL);

CREATE TABLE [LibraryHierarchies] ( 
  [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
  [Sequence] INTEGER NOT NULL, 
  [Name] nvarchar(250) NOT NULL);

CREATE TABLE [LibraryHierarchyLevels] (
	[Id]	INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[LibraryHierarchy_Id] INTEGER NULL REFERENCES LibraryHierarchies([Id]),
	[Sequence] INTEGER NOT NULL,
	[Name] nvarchar(250) NOT NULL,
	[Script]	nvarchar(max) NOT NULL
);

CREATE TABLE [LibraryHierarchyItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	[Parent_Id] INTEGER NULL REFERENCES LibraryHierarchyItems([Id]),
    [LibraryHierarchy_Id] INTEGER NOT NULL REFERENCES LibraryHierarchies([Id]), 
    [LibraryHierarchyLevel_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyLevels([Id]), 
    [Value] nvarchar(250) NOT NULL, 
    [IsLeaf] bit NOT NULL);

CREATE TABLE [PlaylistItem_MetaDataItem](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]), 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [PlaylistColumns] ( 
  [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [Sequence] INTEGER NOT NULL,
  [Name] nvarchar(250) NOT NULL, 
  [Script] nvarchar(max) NOT NULL, 
  [IsDynamic] INTEGER NOT NULL, 
  [Width] numeric(38,0) NULL);

CREATE TABLE [LibraryItem_MetaDataItem](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]), 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [LibraryHierarchyItem_LibraryItem] (
	[Id]	INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[LibraryHierarchyItem_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]),
	[LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id])
);

CREATE TABLE [PlaylistSort]
(
	[Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]),
	[Value1] nvarchar(255) NULL,
	[Value2] nvarchar(255) NULL,
	[Value3] nvarchar(255) NULL,
	[Value4] nvarchar(255) NULL,
	[Value5] nvarchar(255) NULL,
	[Value6] nvarchar(255) NULL,
	[Value7] nvarchar(255) NULL,
	[Value8] nvarchar(255) NULL,
	[Value9] nvarchar(255) NULL,
	[Value10] nvarchar(255) NULL
);

CREATE TABLE [PlaylistSequence]
(
	[Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]),
	[Sequence] INTEGER NULL
);
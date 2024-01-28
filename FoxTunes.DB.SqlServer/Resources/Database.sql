CREATE TABLE [MetaDataItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [Name] nvarchar(250) NOT NULL, 
	[Type] INTEGER NOT NULL,
    [NumericValue] INTEGER, 
    [TextValue] nvarchar(250), 
    [FileValue] nvarchar(250));

CREATE TABLE LibraryItems (
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

CREATE TABLE "LibraryHierarchyLevels" (
	"Id"	INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	"LibraryHierarchy_Id" INTEGER NOT NULL,
	"Sequence" INTEGER NOT NULL,
	"Name" nvarchar(250) NOT NULL,
	"Script"	nvarchar(max) NOT NULL
);

CREATE TABLE [LibraryHierarchyItems](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [Parent_Id] INTEGER REFERENCES LibraryHierarchyItems([Id]), 
    [LibraryHierarchy_Id] INTEGER NOT NULL REFERENCES LibraryHierarchies([Id]), 
    [LibraryHierarchyLevel_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyLevels([Id]), 
    [Value] nvarchar(250) NOT NULL, 
    [IsLeaf] bit NOT NULL);

CREATE TABLE [PlaylistItem_MetaDataItem](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]), 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [PlaylistColumns] ( 
  [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL 
, [Sequence] INTEGER NOT NULL
, [Name] nvarchar(250) NOT NULL 
, [Script] nvarchar(max) NOT NULL 
, [IsDynamic] INTEGER NOT NULL 
, [Width] numeric(38,0) NULL);

CREATE TABLE [LibraryItem_MetaDataItem](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]), 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [LibraryHierarchy_LibraryHierarchyItem](
    [Id] INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL, 
    [LibraryHierarchy_Id] INTEGER NOT NULL REFERENCES LibraryHierarchies([Id]), 
    [LibraryHierarchyItem_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]));

CREATE TABLE "LibraryHierarchyItem_LibraryItem" (
	"Id"	INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	"LibraryHierarchyItem_Id"INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]),
	"LibraryItem_Id"	INTEGER NOT NULL REFERENCES LibraryItems([Id])
);

CREATE TABLE PlaylistSort
(
	"PlaylistItem_Id" INTEGER NOT NULL, 
	"Value1" nvarchar(255) NULL,
	"Value2" nvarchar(255) NULL,
	"Value3" nvarchar(255) NULL,
	"Value4" nvarchar(255) NULL,
	"Value5" nvarchar(255) NULL,
	"Value6" nvarchar(255) NULL,
	"Value7" nvarchar(255) NULL,
	"Value8" nvarchar(255) NULL,
	"Value9" nvarchar(255) NULL,
	"Value10" nvarchar(255) NULL
);

CREATE TABLE PlaylistSequence
(
	"Id" INTEGER IDENTITY(1,1) PRIMARY KEY NOT NULL,
	"PlaylistItem_Id" INTEGER NOT NULL,
	"Sequence" INTEGER NULL
);

CREATE TABLE "LibraryHierarchyLevelLeaf"
(
	"LibraryHierarchy_Id" INTEGER NOT NULL, 
	"LibraryHierarchyLevel_Id" INTEGER NOT NULL
);

CREATE TABLE "LibraryHierarchyLevelParent"
(
	"Id" INTEGER NOT NULL, 
	"Parent_Id" INTEGER NOT NULL
);

CREATE TABLE "LibraryHierarchy"
(
	"LibraryHierarchy_Id" INTEGER NOT NULL, 
	"LibraryHierarchyLevel_Id" INTEGER NOT NULL, 
	"LibraryItem_Id" INTEGER NOT NULL, 
	"Value" nvarchar(255) NOT NULL,
	"IsLeaf" bit NOT NULL
);

CREATE INDEX [IDX_PlaylistItems]
ON [PlaylistItems](
    [Status]);

CREATE INDEX [IDX_LibraryHierarchyLevels]
ON [LibraryHierarchyLevels](
	[LibraryHierarchy_Id]);

CREATE INDEX [IDX_PlaylistItem_MetaDataItem_1]
ON [PlaylistItem_MetaDataItem](
    [PlaylistItem_Id]);

CREATE INDEX [IDX_PlaylistItem_MetaDataItem_2]
ON [PlaylistItem_MetaDataItem](
    [MetaDataItem_Id]);

CREATE UNIQUE INDEX [IDX_PlaylistItem_MetaDataItem_3]
ON [PlaylistItem_MetaDataItem](
    [PlaylistItem_Id], 
    [MetaDataItem_Id]);

CREATE UNIQUE INDEX [IDX_MetaDataItems_TextValue]
ON [MetaDataItems](
    [Name], 
    [TextValue])
WHERE
    [TextValue] IS NOT NULL;

CREATE UNIQUE INDEX [IDX_MetaDataItems_NumericValue]
ON [MetaDataItems](
    [Name], 
    [NumericValue])
WHERE
    [NumericValue] IS NOT NULL;

CREATE UNIQUE INDEX [IDX_MetaDataItems_FileValue]
ON [MetaDataItems](
    [Name], 
    [FileValue])
WHERE
    [FileValue] IS NOT NULL;

CREATE UNIQUE INDEX IDX_LibraryItems_Location 
ON LibraryItems (
	DirectoryName, 
	FileName);

CREATE INDEX [IDX_LibraryItem_MetaDataItem_1]
ON [LibraryItem_MetaDataItem](
    [LibraryItem_Id]);

CREATE INDEX [IDX_LibraryItem_MetaDataItem_2]
ON [LibraryItem_MetaDataItem](
    [MetaDataItem_Id]);

CREATE UNIQUE INDEX [IDX_LibraryItem_MetaDataItem_3]
ON [LibraryItem_MetaDataItem](
    [LibraryItem_Id], 
    [MetaDataItem_Id]);

CREATE UNIQUE INDEX [IDX_LibraryHierarchy_LibraryHierarchyItem]
ON [LibraryHierarchy_LibraryHierarchyItem](
    [LibraryHierarchy_Id], 
    [LibraryHierarchyItem_Id]);

CREATE INDEX [IDX_LibraryHierarchyItem_Values]
ON [LibraryHierarchyItems](
	[Value]);

CREATE UNIQUE INDEX "IDX_LibraryHierarchyItem_LibraryItem" 
ON "LibraryHierarchyItem_LibraryItem" (
	"LibraryHierarchyItem_Id" ,
	"LibraryItem_Id");

CREATE INDEX [IDX_LibraryHierarchyItem]
ON [LibraryHierarchyItems](
	[Parent_Id],
	[LibraryHierarchy_Id], 
	[LibraryHierarchyLevel_Id]);

CREATE UNIQUE INDEX "IDX_LibraryHierarchyLevelParent" 
ON "LibraryHierarchyLevelParent"
(
	"Id"
);

CREATE UNIQUE INDEX "IDX_LibraryHierarchyLevelLeaf" 
ON "LibraryHierarchyLevelLeaf"
(
	"LibraryHierarchy_Id"
);

CREATE UNIQUE INDEX "IDX_LibraryHierarchy" 
ON "LibraryHierarchy"
(
	"LibraryHierarchy_Id",
	"LibraryHierarchyLevel_Id",
	"LibraryItem_Id",
	"Value",
	"IsLeaf"
);

CREATE INDEX "IDX_LibraryHierarchy_LibraryItem" 
ON "LibraryHierarchy"
(
	"LibraryItem_Id"
);
-- Script Date: 30/07/2017 21:05  - ErikEJ.SqlCeScripting version 3.5.2.69
-- Database information:
-- Database: C:\Source\FoxTunes\FoxTunes.DB.EntityFramework.SQLite\Database.dat
-- ServerVersion: 3.18.0
-- DatabaseSize: 76 KB
-- Created: 19/07/2017 17:55

-- User Table information:
-- Number of tables: 16
-- LibraryHierarchies: -1 row(s)
-- LibraryHierarchy_LibraryHierarchyItem: -1 row(s)
-- LibraryHierarchy_LibraryHierarchyLevel: -1 row(s)
-- LibraryHierarchyItems: -1 row(s)
-- LibraryHierarchyLevels: -1 row(s)
-- LibraryItem_LibraryHierarchyItem: -1 row(s)
-- LibraryItem_MetaDataItem: -1 row(s)
-- LibraryItem_PropertyItem: -1 row(s)
-- LibraryItem_StatisticItem: -1 row(s)
-- LibraryItems: -1 row(s)
-- MetaDataItems: -1 row(s)
-- PlaylistItem_MetaDataItem: -1 row(s)
-- PlaylistItem_PropertyItem: -1 row(s)
-- PlaylistItems: -1 row(s)
-- PropertyItems: -1 row(s)
-- StatisticItems: -1 row(s)

SELECT 1;
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE [StatisticItems] (
  [Id] INTEGER NOT NULL
, [Name] text NOT NULL
, [NumericValue] bigint NULL
, [TextValue] text NULL
, CONSTRAINT [sqlite_master_PK_StatisticItems] PRIMARY KEY ([Id])
);
CREATE TABLE [PropertyItems] (
  [Id] INTEGER NOT NULL
, [Name] text NOT NULL
, [NumericValue] bigint NULL
, [TextValue] text NULL
, CONSTRAINT [sqlite_master_PK_PropertyItems] PRIMARY KEY ([Id])
);
CREATE TABLE [PlaylistItems] (
  [Id] INTEGER NOT NULL
, [FileName] text NOT NULL
, CONSTRAINT [sqlite_master_PK_PlaylistItems] PRIMARY KEY ([Id])
);
CREATE TABLE [PlaylistItem_PropertyItem] (
  [Id] INTEGER NOT NULL
, [PlaylistItem_Id] bigint NOT NULL
, [PropertyItem_Id] bigint NOT NULL
, CONSTRAINT [sqlite_master_PK_PlaylistItem_PropertyItem] PRIMARY KEY ([Id])
);
CREATE TABLE [PlaylistItem_MetaDataItem] (
  [Id] INTEGER NOT NULL
, [PlaylistItem_Id] bigint NOT NULL
, [MetaDataItem_Id] bigint NOT NULL
, CONSTRAINT [sqlite_master_PK_PlaylistItem_MetaDataItem] PRIMARY KEY ([Id])
);
CREATE TABLE [MetaDataItems] (
  [Id] INTEGER NOT NULL
, [Name] text NOT NULL
, [NumericValue] bigint NULL
, [TextValue] text NULL
, [FileValue] text NULL
, CONSTRAINT [sqlite_master_PK_MetaDataItems] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryItems] (
  [Id] INTEGER NOT NULL
, [FileName] text NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryItems] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryItem_StatisticItem] (
  [Id] INTEGER NOT NULL
, [LibraryItem_Id] bigint NOT NULL
, [StatisticItem_Id] bigint NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryItem_StatisticItem] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryItem_PropertyItem] (
  [Id] INTEGER NOT NULL
, [LibraryItem_Id] bigint NOT NULL
, [PropertyItem_Id] bigint NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryItem_PropertyItem] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryItem_MetaDataItem] (
  [Id] INTEGER NOT NULL
, [LibraryItem_Id] bigint NOT NULL
, [MetaDataItem_Id] bigint NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryItem_MetaDataItem] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryItem_LibraryHierarchyItem] (
  [Id] INTEGER NOT NULL
, [LibraryItem_Id] bigint NOT NULL
, [LibraryHierarchyItem_Id] bigint NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryItem_LibraryHierarchyItem] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryHierarchyLevels] (
  [Id] INTEGER NOT NULL
, [DisplayScript] text NOT NULL
, [SortScript] text NULL
, CONSTRAINT [sqlite_master_PK_LibraryHierarchyLevels] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryHierarchyItems] (
  [Id] INTEGER NOT NULL
, [Parent_Id] bigint NULL
, [DisplayValue] text NOT NULL
, [SortValue] text NULL
, [IsLeaf] bit NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryHierarchyItems] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryHierarchy_LibraryHierarchyLevel] (
  [Id] INTEGER NOT NULL
, [LibraryHierarchy_Id] bigint NULL
, [LibraryHierarchyLevel_Id] bigint NULL
, CONSTRAINT [sqlite_master_PK_LibraryHierarchy_LibraryHierarchyLevel] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryHierarchy_LibraryHierarchyItem] (
  [Id] INTEGER NOT NULL
, [LibraryHierarchy_Id] bigint NOT NULL
, [LibraryHierarchyItem_Id] bigint NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryHierarchy_LibraryHierarchyItem] PRIMARY KEY ([Id])
);
CREATE TABLE [LibraryHierarchies] (
  [Id] INTEGER NOT NULL
, [Name] text NOT NULL
, CONSTRAINT [sqlite_master_PK_LibraryHierarchies] PRIMARY KEY ([Id])
);
INSERT INTO [LibraryHierarchyLevels] ([Id],[DisplayScript],[SortScript]) VALUES (
1,'tag.firstalbumartist||tag.firstalbumartistsort||tag.firstartist||"No Artist"',NULL);
INSERT INTO [LibraryHierarchyLevels] ([Id],[DisplayScript],[SortScript]) VALUES (
2,'(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()',NULL);
INSERT INTO [LibraryHierarchyLevels] ([Id],[DisplayScript],[SortScript]) VALUES (
3,'(function(){if(tag.title){var parts=[];if(tag.disccount != 1 && tag.disc){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return item.FileName;})()',NULL);
INSERT INTO [LibraryHierarchyLevels] ([Id],[DisplayScript],[SortScript]) VALUES (
4,'ucfirst(tag.firstgenre)||"No Genre"',NULL);
INSERT INTO [LibraryHierarchyLevels] ([Id],[DisplayScript],[SortScript]) VALUES (
5,'(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()',NULL);
INSERT INTO [LibraryHierarchyLevels] ([Id],[DisplayScript],[SortScript]) VALUES (
6,'(function(){if(tag.title){var parts=[];if(tag.disccount != 1 && tag.disc){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return item.FileName;})()',NULL);
INSERT INTO [LibraryHierarchy_LibraryHierarchyLevel] ([Id],[LibraryHierarchy_Id],[LibraryHierarchyLevel_Id]) VALUES (
1,1,1);
INSERT INTO [LibraryHierarchy_LibraryHierarchyLevel] ([Id],[LibraryHierarchy_Id],[LibraryHierarchyLevel_Id]) VALUES (
2,1,2);
INSERT INTO [LibraryHierarchy_LibraryHierarchyLevel] ([Id],[LibraryHierarchy_Id],[LibraryHierarchyLevel_Id]) VALUES (
3,1,3);
INSERT INTO [LibraryHierarchy_LibraryHierarchyLevel] ([Id],[LibraryHierarchy_Id],[LibraryHierarchyLevel_Id]) VALUES (
4,2,4);
INSERT INTO [LibraryHierarchy_LibraryHierarchyLevel] ([Id],[LibraryHierarchy_Id],[LibraryHierarchyLevel_Id]) VALUES (
5,2,5);
INSERT INTO [LibraryHierarchy_LibraryHierarchyLevel] ([Id],[LibraryHierarchy_Id],[LibraryHierarchyLevel_Id]) VALUES (
6,2,6);
INSERT INTO [LibraryHierarchies] ([Id],[Name]) VALUES (
1,'Artist/Album/Title');
INSERT INTO [LibraryHierarchies] ([Id],[Name]) VALUES (
2,'Genre/Album/Title');
COMMIT;


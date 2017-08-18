BEGIN TRANSACTION;
CREATE TABLE "StatisticItems" (
	`Id`	INTEGER NOT NULL,
	`Name`	text NOT NULL,
	`NumericValue`	INTEGER,
	`TextValue`	text,
	PRIMARY KEY(`Id`)
);
CREATE TABLE "PropertyItems" (
	`Id`	INTEGER NOT NULL,
	`Name`	text NOT NULL,
	`NumericValue`	INTEGER,
	`TextValue`	text,
	PRIMARY KEY(`Id`)
);
CREATE TABLE [PlaylistItems] ( 
  [Id] INTEGER NOT NULL 
, [Sequence] INTEGER NOT NULL  
, [FileName] text NOT NULL 
, CONSTRAINT [sqlite_master_PK_PlaylistItems] PRIMARY KEY ([Id]) 
);
CREATE TABLE [PlaylistItem_PropertyItem] ( 
  [Id] INTEGER NOT NULL 
, [PlaylistItem_Id] INTEGER NOT NULL 
, [PropertyItem_Id] INTEGER NOT NULL 
, CONSTRAINT [PK_PlaylistItem_PropertyItem] PRIMARY KEY ([Id]) 
);
CREATE TABLE "PlaylistItem_MetaDataItem" (
	`Id`	INTEGER NOT NULL,
	`PlaylistItem_Id`	INTEGER NOT NULL,
	`MetaDataItem_Id`	INTEGER NOT NULL,
	PRIMARY KEY(`Id`)
);
CREATE TABLE [PlaylistItem_ImageItem] ( 
  [Id] INTEGER NOT NULL 
, [PlaylistItem_Id] bigint NOT NULL 
, [ImageItem_Id] bigint NOT NULL 
, CONSTRAINT [sqlite_master_PK_PlaylistItem_ImageItem] PRIMARY KEY ([Id]) 
);
CREATE TABLE "MetaDataItems" (
	`Id`	INTEGER NOT NULL,
	`Name`	text NOT NULL,
	`NumericValue`	INTEGER,
	`TextValue`	text,
	`FileValue`	text,
	PRIMARY KEY(`Id`)
);
CREATE TABLE [LibraryItems] ( 
  [Id] INTEGER NOT NULL 
  , [FileName] text NOT NULL 
, CONSTRAINT [PK_LibraryItems] PRIMARY KEY ([Id]) 
);
CREATE TABLE "LibraryItem_StatisticItem" (
	`Id`	INTEGER NOT NULL,
	`LibraryItem_Id`	INTEGER NOT NULL,
	`StatisticItem_Id`	INTEGER NOT NULL,
	PRIMARY KEY(`Id`)
);
CREATE TABLE "LibraryItem_PropertyItem" (
	`Id`	INTEGER NOT NULL,
	`LibraryItem_Id`	INTEGER NOT NULL,
	`PropertyItem_Id`	INTEGER NOT NULL,
	PRIMARY KEY(`Id`)
);
CREATE TABLE "LibraryItem_MetaDataItem" (
	`Id`	INTEGER NOT NULL,
	`LibraryItem_Id`	INTEGER NOT NULL,
	`MetaDataItem_Id`	INTEGER NOT NULL,
	PRIMARY KEY(`Id`)
);
CREATE TABLE [LibraryItem_LibraryHierarchyItem] ( 
  [Id] INTEGER NOT NULL 
, [LibraryItem_Id] INTEGER NOT NULL 
, [LibraryHierarchyItem_Id] INTEGER NOT NULL 
, CONSTRAINT [PK_LibraryItem_LibraryHierarchyItem] PRIMARY KEY ([Id]) 
);
CREATE TABLE [LibraryItem_ImageItem] ( 
  [Id] INTEGER NOT NULL 
, [LibraryItem_Id] bigint NOT NULL 
, [ImageItem_Id] bigint NOT NULL 
, CONSTRAINT [sqlite_master_PK_LibraryItem_ImageItem] PRIMARY KEY ([Id]) 
);
CREATE TABLE `LibraryHierarchy_LibraryHierarchyLevel` (
	`Id`	INTEGER,
	`LibraryHierarchy_Id`	INTEGER,
	`LibraryHierarchyLevel_Id`	INTEGER,
	PRIMARY KEY(`Id`)
);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (1,1,1);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (2,1,2);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (3,1,3);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (4,2,4);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (5,2,5);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (6,2,6);
CREATE TABLE [LibraryHierarchy_LibraryHierarchyItem] ( 
  [Id] INTEGER NOT NULL 
, [LibraryHierarchy_Id] INTEGER NOT NULL 
, [LibraryHierarchyItem_Id] INTEGER NOT NULL 
, CONSTRAINT [PK_LibraryHierarchy_LibraryHierarchyItem] PRIMARY KEY ([Id]) 
);
CREATE TABLE "LibraryHierarchyLevels" (
	`Id`	INTEGER NOT NULL,
	`DisplayScript`	TEXT NOT NULL,
	`SortScript`	TEXT,
	PRIMARY KEY(`Id`)
);
INSERT INTO `LibraryHierarchyLevels` VALUES (1,'tag.firstalbumartist||tag.firstalbumartistsort||tag.firstartist||"No Artist"',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (2,'(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (3,'(function(){if(tag.title){var parts=[];if(tag.disccount != 1 && tag.disc){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return item.FileName;})()',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (4,'ucfirst(tag.firstgenre)||"No Genre"',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (5,'(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (6,'(function(){if(tag.title){var parts=[];if(tag.disccount != 1 && tag.disc){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return item.FileName;})()',NULL);
CREATE TABLE [LibraryHierarchyItems] ( 
  [Id] INTEGER NOT NULL 
, [Parent_Id] bigint NULL 
, [DisplayValue] text NOT NULL 
, [SortValue] text NULL 
, [IsLeaf] bit NOT NULL 
, CONSTRAINT [sqlite_master_PK_LibraryHierarchyItems] PRIMARY KEY ([Id]) 
);
CREATE TABLE [LibraryHierarchies] ( 
  [Id] INTEGER NOT NULL 
, [Name] TEXT NOT NULL 
, CONSTRAINT [PK_LibraryHierarchies] PRIMARY KEY ([Id]) 
);
INSERT INTO `LibraryHierarchies` VALUES (1,'Artist/Album/Title');
INSERT INTO `LibraryHierarchies` VALUES (2,'Genre/Album/Title');
CREATE TABLE [ImageItems] ( 
  [Id] INTEGER NOT NULL 
, [FileName] TEXT NOT NULL 
, CONSTRAINT [PK_ImageItems] PRIMARY KEY ([Id]) 
);
CREATE TABLE [ImageItem_MetaDataItem] ( 
  [Id] INTEGER NOT NULL 
, [ImageItem_Id] bigint NOT NULL 
, [MetaDataItem_Id] bigint NOT NULL 
, CONSTRAINT [sqlite_master_PK_ImageItem_MetaDataItem] PRIMARY KEY ([Id]) 
);
COMMIT;

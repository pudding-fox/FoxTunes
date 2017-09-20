BEGIN TRANSACTION;
CREATE TABLE "StatisticItems" (
	`Id`	INTEGER NOT NULL,
	`Name`	text NOT NULL,
	`NumericValue`	INTEGER,
	`TextValue`	text,
	PRIMARY KEY(`Id`)
);
CREATE TABLE [PropertyItems](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [Name] text NOT NULL, 
    [NumericValue] INTEGER, 
    [TextValue] text);
CREATE TABLE [PlaylistItems](
    [Id] INTEGER CONSTRAINT [sqlite_master_PK_PlaylistItems] PRIMARY KEY NOT NULL, 
    [Sequence] bigint NOT NULL, 
    [DirectoryName] text NOT NULL, 
    [FileName] text NOT NULL, 
    [Status] bigint NOT NULL);
CREATE TABLE [PlaylistItem_PropertyItem](
    [Id] INTEGER CONSTRAINT [PK_PlaylistItem_PropertyItem] PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]) ON DELETE CASCADE, 
    [PropertyItem_Id] INTEGER NOT NULL REFERENCES PropertyItems([Id]) ON DELETE CASCADE);
CREATE TABLE [PlaylistItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]) ON DELETE CASCADE, 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]) ON DELETE CASCADE);
CREATE TABLE [PlaylistItem_ImageItem](
    [Id] INTEGER CONSTRAINT [sqlite_master_PK_PlaylistItem_ImageItem] PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] bigint NOT NULL REFERENCES PlaylistItems([Id]) ON DELETE CASCADE, 
    [ImageItem_Id] bigint NOT NULL REFERENCES ImageItems([Id]) ON DELETE CASCADE);
CREATE TABLE [PlaylistColumns] ( 
  [Id] INTEGER NOT NULL 
, [Name] text NOT NULL 
, [DisplayScript] text NOT NULL 
, [Width] numeric(53,0) NULL 
, CONSTRAINT [sqlite_master_PK_PlaylistColumns] PRIMARY KEY ([Id]) 
);
INSERT INTO `PlaylistColumns` VALUES (1,'Playing','playing != null && item.Id == playing.Id ? "\u2022" : ""',NULL);
INSERT INTO `PlaylistColumns` VALUES (2,'Artist / album','(function(){ var parts = [tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist]; if(tag.album) { parts.push(tag.album); } return parts.join(" - "); })()',NULL);
INSERT INTO `PlaylistColumns` VALUES (3,'Track no','(function(){ var parts = []; if (tag.disccount != 1 && tag.disc) { parts.push(tag.disc); } if (tag.track) { parts.push(zeropad(tag.track, 2)); } return parts.join(" - "); })()',NULL);
INSERT INTO `PlaylistColumns` VALUES (4,'Title / track artist','(function(){var parts= []; if (tag.title) { parts.push(tag.title); } if (tag.firstperformer && tag.firstperformer != (tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist)) { parts.push(tag.firstperformer); } return parts.join(" - "); })()',NULL);
INSERT INTO `PlaylistColumns` VALUES (5,'Duration','timestamp(stat.duration)',NULL);
CREATE TABLE [MetaDataItems](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [Name] text NOT NULL, 
    [NumericValue] INTEGER, 
    [TextValue] text, 
    [FileValue] text);
CREATE TABLE LibraryItems (Id INTEGER NOT NULL, DirectoryName text NOT NULL, FileName text NOT NULL, Status INTEGER NOT NULL, CONSTRAINT sqlite_master_PK_LibraryItems PRIMARY KEY (Id));
CREATE TABLE [LibraryItem_StatisticItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]) ON DELETE CASCADE, 
    [StatisticItem_Id] INTEGER NOT NULL REFERENCES StatisticItems([Id]) ON DELETE CASCADE);
CREATE TABLE [LibraryItem_PropertyItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]) ON DELETE CASCADE, 
    [PropertyItem_Id] INTEGER NOT NULL REFERENCES PropertyItems([Id]) ON DELETE CASCADE);
CREATE TABLE [LibraryItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]) ON DELETE CASCADE, 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]) ON DELETE CASCADE);
CREATE TABLE [LibraryItem_LibraryHierarchyItem](
    [Id] INTEGER CONSTRAINT [PK_LibraryItem_LibraryHierarchyItem] PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]) ON DELETE CASCADE, 
    [LibraryHierarchyItem_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]) ON DELETE CASCADE);
CREATE TABLE [LibraryItem_ImageItem](
    [Id] INTEGER CONSTRAINT [sqlite_master_PK_LibraryItem_ImageItem] PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] bigint NOT NULL REFERENCES LibraryItems([Id]) ON DELETE CASCADE, 
    [ImageItem_Id] bigint NOT NULL REFERENCES ImageItems([Id]) ON DELETE CASCADE);
CREATE TABLE [LibraryHierarchy_LibraryHierarchyLevel](
    [Id] INTEGER PRIMARY KEY, 
    [LibraryHierarchy_Id] INTEGER REFERENCES LibraryHierarchies([Id]) ON DELETE CASCADE, 
    [LibraryHierarchyLevel_Id] INTEGER REFERENCES LibraryHierarchyLevels([Id]) ON DELETE CASCADE);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (1,1,1);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (2,1,2);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (3,1,3);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (4,2,4);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (5,2,5);
INSERT INTO `LibraryHierarchy_LibraryHierarchyLevel` VALUES (6,2,6);
CREATE TABLE [LibraryHierarchy_LibraryHierarchyItem](
    [Id] INTEGER CONSTRAINT [PK_LibraryHierarchy_LibraryHierarchyItem] PRIMARY KEY NOT NULL, 
    [LibraryHierarchy_Id] INTEGER NOT NULL REFERENCES LibraryHierarchies([Id]) ON DELETE CASCADE, 
    [LibraryHierarchyItem_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]) ON DELETE CASCADE);
CREATE TABLE "LibraryHierarchyLevels" (
	`Id`	INTEGER NOT NULL,
	`DisplayScript`	TEXT NOT NULL,
	`SortScript`	TEXT,
	PRIMARY KEY(`Id`)
);
INSERT INTO `LibraryHierarchyLevels` VALUES (1,'(function(){if(tag.__ft_variousartists) { return "Various Artists"; } return  tag.firstalbumartist||tag.firstalbumartistsort||tag.firstartist||"No Artist";})()',NULL);
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
CREATE TABLE "ImageItems" (
	`Id`	INTEGER NOT NULL,
	`FileName`	TEXT NOT NULL,
	`ImageType`	TEXT,
	PRIMARY KEY(`Id`)
);
CREATE UNIQUE INDEX [IDX_PropertyValues_TextValue]
ON [PropertyItems](
    [Name], 
    [TextValue])
WHERE
    [TextValue] IS NOT NULL;
CREATE UNIQUE INDEX [IDX_PropertyItems_NumericValue]
ON [PropertyItems](
    [Name], 
    [NumericValue])
WHERE
    [NumericValue] IS NOT NULL;
CREATE UNIQUE INDEX [IDX_PlaylistItem_PropertyItem]
ON [PlaylistItem_PropertyItem](
    [PlaylistItem_Id], 
    [PropertyItem_Id]);
CREATE UNIQUE INDEX [IDX_PlaylistItem_MetaDataItem]
ON [PlaylistItem_MetaDataItem](
    [PlaylistItem_Id], 
    [MetaDataItem_Id]);
CREATE UNIQUE INDEX [IDX_PlaylistItem_ImageItem]
ON [PlaylistItem_ImageItem](
    [PlaylistItem_Id], 
    [ImageItem_Id]);
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
CREATE UNIQUE INDEX IDX_LibraryItems_Location ON LibraryItems (DirectoryName, FileName);
CREATE UNIQUE INDEX [IDX_LibraryItem_StatisticItem]
ON [LibraryItem_StatisticItem](
    [LibraryItem_Id], 
    [StatisticItem_Id]);
CREATE UNIQUE INDEX [IDX_LibraryItem_PropertyItem]
ON [LibraryItem_PropertyItem](
    [LibraryItem_Id], 
    [PropertyItem_Id]);
CREATE UNIQUE INDEX [IDX_LibraryItem_MetaDataItem]
ON [LibraryItem_MetaDataItem](
    [LibraryItem_Id], 
    [MetaDataItem_Id]);
CREATE UNIQUE INDEX [IDX_LibraryItem_LibraryHierarchyItem]
ON [LibraryItem_LibraryHierarchyItem](
    [LibraryItem_Id], 
    [LibraryHierarchyItem_Id]);
CREATE UNIQUE INDEX [IDX_LibraryItem_ImageItem]
ON [LibraryItem_ImageItem](
    [LibraryItem_Id], 
    [ImageItem_Id]);
CREATE UNIQUE INDEX [IDX_LibraryHierarchy_LibraryHierarchyLevel]
ON [LibraryHierarchy_LibraryHierarchyLevel](
    [LibraryHierarchy_Id], 
    [LibraryHierarchyLevel_Id]);
CREATE UNIQUE INDEX [IDX_LibraryHierarchy_LibraryHierarchyItem]
ON [LibraryHierarchy_LibraryHierarchyItem](
    [LibraryHierarchy_Id], 
    [LibraryHierarchyItem_Id]);
COMMIT;

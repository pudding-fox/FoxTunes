BEGIN TRANSACTION;
CREATE TABLE [PlaylistItems](
    [Id] INTEGER CONSTRAINT [sqlite_master_PK_PlaylistItems] PRIMARY KEY NOT NULL, 
    [Sequence] bigint NOT NULL, 
    [DirectoryName] text NOT NULL, 
    [FileName] text NOT NULL, 
    [Status] bigint NOT NULL);
CREATE INDEX [IDX_PlaylistItems]
ON [PlaylistItems](
    [Status]);
CREATE TABLE [PlaylistItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]) ON DELETE CASCADE, 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]) ON DELETE CASCADE);
CREATE TABLE [PlaylistColumns] ( 
  [Id] INTEGER NOT NULL 
, [Name] text NOT NULL 
, [DisplayScript] text NOT NULL 
, [IsDynamic] INTEGER NOT NULL 
, [Width] numeric(53,0) NULL 
, CONSTRAINT [sqlite_master_PK_PlaylistColumns] PRIMARY KEY ([Id]) 
);
INSERT INTO `PlaylistColumns` VALUES (1,'Playing','playing != null && item.Id == playing.Id && item.FileName == playing.FileName ? "\u2022" : ""',1,NULL);
INSERT INTO `PlaylistColumns` VALUES (2,'Artist / album','(function(){ var parts = [tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist]; if(tag.album) { parts.push(tag.album); } return parts.join(" - "); })()',0,NULL);
INSERT INTO `PlaylistColumns` VALUES (3,'Track no','(function(){ var parts = []; if (tag.disccount != 1 && tag.disc) { parts.push(tag.disc); } if (tag.track) { parts.push(zeropad(tag.track, 2)); } return parts.join(" - "); })()',0,NULL);
INSERT INTO `PlaylistColumns` VALUES (4,'Title / track artist','(function(){var parts= []; if (tag.title) { parts.push(tag.title); } if (tag.firstperformer && tag.firstperformer != (tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist)) { parts.push(tag.firstperformer); } return parts.join(" - "); })()',0,NULL);
INSERT INTO `PlaylistColumns` VALUES (5,'Duration','timestamp(property.duration)',0,NULL);
CREATE TABLE [MetaDataItems](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [Name] text NOT NULL, 
	[Type] bigint NOT NULL,
    [NumericValue] INTEGER, 
    [TextValue] text, 
    [FileValue] text);
CREATE TABLE LibraryItems (Id INTEGER NOT NULL, DirectoryName text NOT NULL, FileName text NOT NULL, Status INTEGER NOT NULL, CONSTRAINT sqlite_master_PK_LibraryItems PRIMARY KEY (Id));
CREATE TABLE [LibraryItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]) ON DELETE CASCADE, 
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]) ON DELETE CASCADE);
CREATE TABLE [LibraryHierarchy_LibraryHierarchyItem](
    [Id] INTEGER CONSTRAINT [PK_LibraryHierarchy_LibraryHierarchyItem] PRIMARY KEY NOT NULL, 
    [LibraryHierarchy_Id] INTEGER NOT NULL REFERENCES LibraryHierarchies([Id]) ON DELETE CASCADE, 
    [LibraryHierarchyItem_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]) ON DELETE CASCADE);
CREATE TABLE "LibraryHierarchyLevels" (
	`Id`	INTEGER NOT NULL,
	`LibraryHierarchy_Id` INTEGER NOT NULL,
	`DisplayScript`	TEXT NOT NULL,
	`SortScript`	TEXT,
	PRIMARY KEY(`Id`)
);
CREATE INDEX [IDX_LibraryHierarchyLevels]
ON [LibraryHierarchyLevels](
	[LibraryHierarchy_Id]);
INSERT INTO `LibraryHierarchyLevels` VALUES (1,1,'(function(){if(tag.__ft_variousartists) { return "Various Artists"; } return  tag.firstalbumartist||tag.firstalbumartistsort||tag.firstartist||"No Artist";})()',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (2,1,'(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (3,1,'(function(){if(tag.title){var parts=[];if(parseInt(tag.disccount) != 1 && parseInt(tag.disc)){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return fileName;})()',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (4,2,'ucfirst(tag.firstgenre)||"No Genre"',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (5,2,'(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()',NULL);
INSERT INTO `LibraryHierarchyLevels` VALUES (6,2,'(function(){if(tag.title){var parts=[];if(parseInt(tag.disccount) != 1 && parseInt(tag.disc)){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return fileName;})()',NULL);
CREATE TABLE [LibraryHierarchyItems](
    [Id] INTEGER CONSTRAINT [sqlite_master_PK_LibraryHierarchyItems] PRIMARY KEY NOT NULL, 
    [Parent_Id] INTEGER REFERENCES LibraryHierarchyItems([Id]) ON DELETE CASCADE, 
    [LibraryHierarchy_Id] bigint NOT NULL REFERENCES LibraryHierarchies([Id]) ON DELETE CASCADE, 
    [LibraryHierarchyLevel_Id] bigint NOT NULL REFERENCES LibraryHierarchyLevels([Id]) ON DELETE CASCADE, 
    [DisplayValue] text NOT NULL, 
    [SortValue] text, 
    [IsLeaf] bit NOT NULL);
CREATE TABLE `LibraryHierarchyItem_LibraryItem` (
	`Id`	INTEGER CONSTRAINT [sqlite_master_PK_LibraryHierarchyItem_LibraryItem] PRIMARY KEY NOT NULL,
	`LibraryHierarchyItem_Id`INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]) ON DELETE CASCADE,
	`LibraryItem_Id`	INTEGER NOT NULL REFERENCES LibraryItems([Id]) ON DELETE CASCADE
);
CREATE TABLE [LibraryHierarchies] ( 
  [Id] INTEGER NOT NULL 
, [Name] TEXT NOT NULL 
, CONSTRAINT [PK_LibraryHierarchies] PRIMARY KEY ([Id]) 
);
INSERT INTO `LibraryHierarchies` VALUES (1,'Artist/Album/Title');
INSERT INTO `LibraryHierarchies` VALUES (2,'Genre/Album/Title');
CREATE UNIQUE INDEX [IDX_PlaylistItem_MetaDataItem]
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
CREATE UNIQUE INDEX IDX_LibraryItems_Location ON LibraryItems (DirectoryName, FileName);
CREATE UNIQUE INDEX [IDX_LibraryItem_MetaDataItem]
ON [LibraryItem_MetaDataItem](
    [LibraryItem_Id], 
    [MetaDataItem_Id]);
CREATE UNIQUE INDEX [IDX_LibraryHierarchy_LibraryHierarchyItem]
ON [LibraryHierarchy_LibraryHierarchyItem](
    [LibraryHierarchy_Id], 
    [LibraryHierarchyItem_Id]);
CREATE INDEX [IDX_LibraryHierarchyItem_Values]
ON [LibraryHierarchyItems](
    [DisplayValue], 
    [SortValue]);
CREATE UNIQUE INDEX `IDX_LibraryHierarchyItem_LibraryItem` ON `LibraryHierarchyItem_LibraryItem` (`LibraryHierarchyItem_Id` ,`LibraryItem_Id` );
CREATE INDEX [IDX_LibraryHierarchyItem]
ON [LibraryHierarchyItems](
	[Parent_Id],
    [LibraryHierarchy_Id], 
    [LibraryHierarchyLevel_Id]);
COMMIT;

CREATE TABLE [MetaDataItems](
	[Id] INTEGER PRIMARY KEY NOT NULL, 
	[Name] text NOT NULL, 
	[Type] bigint NOT NULL,
	[NumericValue] INTEGER, 
	[TextValue] text, 
	[FileValue] text);

CREATE TABLE LibraryItems (
	Id INTEGER PRIMARY KEY NOT NULL, 
	DirectoryName text NOT NULL, 
	FileName text NOT NULL, 
	Status INTEGER NOT NULL);

CREATE TABLE [PlaylistItems](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
	[LibraryItem_Id] INTEGER NULL REFERENCES LibraryItems([Id]),
    [Sequence] bigint NOT NULL, 
    [DirectoryName] text NOT NULL, 
    [FileName] text NOT NULL, 
    [Status] bigint NOT NULL);

CREATE TABLE [LibraryHierarchies] ( 
	[Id] INTEGER PRIMARY KEY NOT NULL, 
	[Sequence] INTEGER NOT NULL, 
	[Name] TEXT NOT NULL);

CREATE TABLE "LibraryHierarchyLevels" (
	"Id"	INTEGER PRIMARY KEY NOT NULL,
	"LibraryHierarchy_Id" INTEGER NOT NULL,
	"Sequence" INTEGER NOT NULL,
	"Name" text NOT NULL,
	"Script"	TEXT NOT NULL);

CREATE TABLE [LibraryHierarchyItems](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [Parent_Id] INTEGER REFERENCES LibraryHierarchyItems([Id]),
    [LibraryHierarchy_Id] bigint NOT NULL REFERENCES LibraryHierarchies([Id]),
    [LibraryHierarchyLevel_Id] bigint NOT NULL REFERENCES LibraryHierarchyLevels([Id]),
    [Value] text NOT NULL, 
    [IsLeaf] bit NOT NULL);

CREATE TABLE [PlaylistItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [PlaylistItem_Id] INTEGER NOT NULL REFERENCES PlaylistItems([Id]),
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [PlaylistColumns] ( 
  [Id] INTEGER PRIMARY KEY NOT NULL, 
  [Sequence] INTEGER NOT NULL, 
  [Name] text NOT NULL, 
  [Script] text NOT NULL,
  [IsDynamic] INTEGER NOT NULL, 
  [Width] numeric(53,0) NULL);

CREATE TABLE [LibraryItem_MetaDataItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [LibraryItem_Id] INTEGER NOT NULL REFERENCES LibraryItems([Id]),
    [MetaDataItem_Id] INTEGER NOT NULL REFERENCES MetaDataItems([Id]));

CREATE TABLE [LibraryHierarchy_LibraryHierarchyItem](
    [Id] INTEGER PRIMARY KEY NOT NULL, 
    [LibraryHierarchy_Id] INTEGER NOT NULL REFERENCES LibraryHierarchies([Id]),
    [LibraryHierarchyItem_Id] INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]));

CREATE TABLE "LibraryHierarchyItem_LibraryItem" (
	"Id"	INTEGER PRIMARY KEY NOT NULL,
	"LibraryHierarchyItem_Id"INTEGER NOT NULL REFERENCES LibraryHierarchyItems([Id]),
	"LibraryItem_Id"	INTEGER NOT NULL REFERENCES LibraryItems([Id]) 
);

CREATE TABLE PlaylistSort
(
	"PlaylistItem_Id" INTEGER NOT NULL, 
	"Value1" text NULL,
	"Value2" text NULL,
	"Value3" text NULL,
	"Value4" text NULL,
	"Value5" text NULL,
	"Value6" text NULL,
	"Value7" text NULL,
	"Value8" text NULL,
	"Value9" text NULL,
	"Value10" text NULL
);

CREATE TABLE PlaylistSequence
(
	"Id" INTEGER PRIMARY KEY NOT NULL,
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
	"Value" text NOT NULL,
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

INSERT INTO "PlaylistColumns" (Id,Sequence,Name,Script,IsDynamic,Width) VALUES (1,0,'Playing','playing != null && item.Id == playing.Id && item.FileName == playing.FileName ? "\u2022" : ""',1,NULL);
INSERT INTO "PlaylistColumns" (Id,Sequence,Name,Script,IsDynamic,Width) VALUES (2,1,'Artist / album','(function(){ var parts = [tag.albumartist || tag.artist || "No Artist"]; if(tag.album) { parts.push(tag.album); } return parts.join(" - "); })()',0,NULL);
INSERT INTO "PlaylistColumns" (Id,Sequence,Name,Script,IsDynamic,Width) VALUES (3,2,'Track no','(function(){ var parts = []; if (tag.disccount != 1 && tag.disc) { parts.push(tag.disc); } if (tag.track) { parts.push(zeropad(tag.track, 2)); } return parts.join(" - "); })()',0,NULL);
INSERT INTO "PlaylistColumns" (Id,Sequence,Name,Script,IsDynamic,Width) VALUES (4,3,'Title / track artist','(function(){var parts= []; if (tag.title) { parts.push(tag.title); } if (tag.performer && tag.performer != (tag.albumartist || tag.artist)) { parts.push(tag.performer); } if (parts.length) { return parts.join(" - "); } else { return filename(item.FileName); } })()',0,NULL);
INSERT INTO "PlaylistColumns" (Id,Sequence,Name,Script,IsDynamic,Width) VALUES (5,4,'Duration','timestamp(property.duration)',0,NULL);

INSERT INTO "LibraryHierarchies" (Id,Sequence,Name) VALUES (1,0,'Artist/Album/Title');
INSERT INTO "LibraryHierarchies" (Id,Sequence,Name) VALUES (2,1,'Genre/Album/Title');

INSERT INTO "LibraryHierarchyLevels" (Id,LibraryHierarchy_Id,Sequence,Name,Script) VALUES (1,1,0,'Artist','(function(){if(tag.__ft_variousartists) { return "Various Artists"; } return  tag.albumartist||tag.artist||"No Artist";})()');
INSERT INTO "LibraryHierarchyLevels" (Id,LibraryHierarchy_Id,Sequence,Name,Script) VALUES (2,1,1,'Year - Album','(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()');
INSERT INTO "LibraryHierarchyLevels" (Id,LibraryHierarchy_Id,Sequence,Name,Script) VALUES (3,1,2,'Disk - Track - Title','(function(){if(tag.title){var parts=[];if(parseInt(tag.disccount) != 1 && parseInt(tag.disc)){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return fileName;})()');
INSERT INTO "LibraryHierarchyLevels" (Id,LibraryHierarchy_Id,Sequence,Name,Script) VALUES (4,2,0,'Genre','ucfirst(tag.genre)||"No Genre"');
INSERT INTO "LibraryHierarchyLevels" (Id,LibraryHierarchy_Id,Sequence,Name,Script) VALUES (5,2,1,'Year - Album','(function(){if(tag.album){var parts=[];if(tag.year){parts.push(tag.year);}parts.push(tag.album);return parts.join(" - ");}return "No Album";})()');
INSERT INTO "LibraryHierarchyLevels" (Id,LibraryHierarchy_Id,Sequence,Name,Script) VALUES (6,2,2,'Disk - Track - Title','(function(){if(tag.title){var parts=[];if(parseInt(tag.disccount) != 1 && parseInt(tag.disc)){parts.push(tag.disc);}if(tag.track){parts.push(zeropad(tag.track,2));}parts.push(tag.title);return parts.join(" - ");}return fileName;})()');
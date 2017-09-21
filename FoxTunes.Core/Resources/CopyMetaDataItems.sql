INSERT INTO "PlaylistItem_MetaDataItem" ("PlaylistItem_Id", "MetaDataItem_Id")
SELECT "PlaylistItems"."Id", "LibraryItem_MetaDataItem"."MetaDataItem_Id"
FROM "PlaylistItems"
	JOIN "LibraryItems" 
		ON "PlaylistItems"."FileName" = "LibraryItems"."FileName"
	JOIN "LibraryItem_MetaDataItem" 
		ON "LibraryItems"."Id" = "LibraryItem_MetaDataItem"."LibraryItem_Id"
WHERE "PlaylistItems"."Status" = @status;

INSERT INTO "PlaylistItem_PropertyItem" ("PlaylistItem_Id", "PropertyItem_Id")
SELECT "PlaylistItems"."Id", "LibraryItem_PropertyItem"."PropertyItem_Id"
FROM "PlaylistItems"
	JOIN "LibraryItems" 
		ON "PlaylistItems"."FileName" = "LibraryItems"."FileName"
	JOIN "LibraryItem_PropertyItem" 
		ON "LibraryItems"."Id" = "LibraryItem_PropertyItem"."LibraryItem_Id"
WHERE "PlaylistItems"."Status" = @status;

INSERT INTO "PlaylistItem_ImageItem" ("PlaylistItem_Id", "ImageItem_Id")
SELECT "PlaylistItems"."Id", "LibraryItem_ImageItem"."ImageItem_Id"
FROM "PlaylistItems"
	JOIN "LibraryItems" 
		ON "PlaylistItems"."FileName" = "LibraryItems"."FileName"
	JOIN "LibraryItem_ImageItem" 
		ON "LibraryItems"."Id" = "LibraryItem_ImageItem"."LibraryItem_Id"
WHERE "PlaylistItems"."Status" = @status;
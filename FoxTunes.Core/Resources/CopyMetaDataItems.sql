INSERT INTO "PlaylistItem_MetaDataItem" ("PlaylistItem_Id", "MetaDataItem_Id")
SELECT "PlaylistItems"."Id", "LibraryItem_MetaDataItem"."MetaDataItem_Id"
FROM "PlaylistItems"
	JOIN "LibraryItems" 
		ON "PlaylistItems"."FileName" = "LibraryItems"."FileName"
	JOIN "LibraryItem_MetaDataItem" 
		ON "LibraryItems"."Id" = "LibraryItem_MetaDataItem"."LibraryItem_Id"
WHERE "PlaylistItems"."Status" = @status;
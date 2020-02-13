INSERT INTO "PlaylistItems" ("LibraryItem_Id", "Sequence", "DirectoryName", "FileName", "Status") 
SELECT "LibraryItems"."Id", @sequence, "LibraryItems"."DirectoryName", "LibraryItems"."FileName", @status
FROM "LibraryHierarchyItems"
	JOIN "LibraryHierarchyItem_LibraryItem" 
		ON "LibraryHierarchyItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id"
	JOIN "LibraryItems"
		ON "LibraryItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id"
WHERE "LibraryHierarchyItems"."Id" = @libraryHierarchyItemId;

SELECT COUNT(*)
FROM "PlaylistItems"
WHERE "Status" = @status
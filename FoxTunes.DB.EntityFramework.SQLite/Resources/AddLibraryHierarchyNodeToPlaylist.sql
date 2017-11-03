WITH "OrderedLibraryItems"
AS
(
	SELECT  "LibraryItems"."Id", "LibraryItems"."DirectoryName", "LibraryItems"."FileName", "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."SortValue", "LibraryHierarchyItems"."DisplayValue"
	FROM  "LibraryItems"
		JOIN "LibraryHierarchyItem_LibraryItem" ON "LibraryItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id"
		JOIN "LibraryHierarchyItems" ON "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id" = "LibraryHierarchyItems"."Id"
	WHERE "LibraryHierarchyItems"."IsLeaf" = 1
)

INSERT INTO "PlaylistItems" ("Sequence", "DirectoryName", "FileName", "Status") 
SELECT @sequence, "OrderedLibraryItems"."DirectoryName", "OrderedLibraryItems"."FileName", @status
FROM "LibraryHierarchyItems"
	JOIN "LibraryHierarchyItem_LibraryItem" ON "LibraryHierarchyItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id"
	JOIN "OrderedLibraryItems" ON  "OrderedLibraryItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id"
		AND "OrderedLibraryItems"."LibraryHierarchy_Id" = "LibraryHierarchyItems"."LibraryHierarchy_Id"
WHERE "LibraryHierarchyItems"."Id" = @libraryHierarchyItemId
ORDER BY "OrderedLibraryItems"."SortValue", "OrderedLibraryItems"."DisplayValue";

UPDATE "PlaylistItems"
SET "Sequence" = "Sequence" +
(
	SELECT COUNT(*) 
	FROM "PlaylistItems" AS "PlaylistItems_Copy"
	WHERE "PlaylistItems_Copy"."Id" < "PlaylistItems"."Id" 
		AND "Status" = @status
)
WHERE "Status" = @status;

SELECT COUNT(*)
FROM "PlaylistItems"
WHERE "Status" = @status
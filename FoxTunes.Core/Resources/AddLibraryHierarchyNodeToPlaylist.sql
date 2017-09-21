INSERT INTO "PlaylistItems" ("Sequence", "DirectoryName", "FileName", "Status") 
SELECT @sequence, "LibraryItems"."DirectoryName", "LibraryItems"."FileName", @status
FROM "LibraryHierarchyItems"
JOIN "LibraryItems" ON "LibraryHierarchyItems"."LibraryItem_Id" = "LibraryItems"."Id"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId 
	AND "LibraryHierarchyLevel_Id" = @libraryHierarchyLevelId 
	AND "DisplayValue" = @displayValue;

UPDATE "PlaylistItems"
SET "Sequence" = "Sequence" +
(
	SELECT COUNT(*) 
	FROM "PlaylistItems" AS "PlaylistItems_Copy"
	WHERE "PlaylistItems_Copy"."Id" < "PlaylistItems"."Id" 
		AND "Status" = @status
);

SELECT COUNT(*)
FROM "PlaylistItems"
WHERE "Status" = @status
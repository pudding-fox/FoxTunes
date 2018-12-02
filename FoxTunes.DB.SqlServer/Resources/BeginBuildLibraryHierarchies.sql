DELETE FROM "LibraryHierarchyLevelLeaf";

INSERT INTO "LibraryHierarchyLevelLeaf" ("LibraryHierarchy_Id", "LibraryHierarchyLevel_Id")
SELECT "LibraryHierarchyLevels"."LibraryHierarchy_Id", "LibraryHierarchyLevels"."Id"
FROM "LibraryHierarchyLevels"
JOIN 
(
	SELECT "LibraryHierarchy_Id", MAX("Sequence") AS "Sequence"
	FROM "LibraryHierarchyLevels"
	GROUP BY "LibraryHierarchy_Id"
) AS "Leaves" 
	ON "LibraryHierarchyLevels"."LibraryHierarchy_Id" = "Leaves"."LibraryHierarchy_Id"
		AND "LibraryHierarchyLevels"."Sequence" = "Leaves"."Sequence";

DELETE FROM "LibraryHierarchyLevelParent";

INSERT INTO "LibraryHierarchyLevelParent" ("Id", "Parent_Id")
SELECT "Id", "Parent_Id"
FROM
(
	SELECT 
		"LibraryHierarchyLevels"."Id" AS "Id", 
		"LibraryHierarchyLevels_Copy"."Id" AS "Parent_Id", 
		ROW_NUMBER() OVER (PARTITION BY "LibraryHierarchyLevels_Copy"."Id" ORDER BY "LibraryHierarchyLevels_Copy"."Sequence" DESC) AS "RowNumber"
	FROM "LibraryHierarchyLevels"
	JOIN "LibraryHierarchyLevels" AS "LibraryHierarchyLevels_Copy"
		ON "LibraryHierarchyLevels"."LibraryHierarchy_Id"  = "LibraryHierarchyLevels_Copy"."LibraryHierarchy_Id"
			AND "LibraryHierarchyLevels_Copy"."Sequence" < "LibraryHierarchyLevels"."Sequence"
) "LibraryHierarchyLevels"
WHERE "RowNumber" = 1

DELETE FROM "LibraryHierarchy";
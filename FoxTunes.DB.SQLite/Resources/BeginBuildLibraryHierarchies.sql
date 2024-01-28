DELETE FROM "LibraryHierarchyLevelLeaf";

INSERT INTO "LibraryHierarchyLevelLeaf"
SELECT LibraryHierarchy_Id, "Id"
FROM "LibraryHierarchyLevels"
GROUP BY "LibraryHierarchy_Id"
HAVING MAX("Sequence")
ORDER BY "Sequence";

DELETE FROM "LibraryHierarchyLevelParent";

INSERT INTO "LibraryHierarchyLevelParent"
SELECT "LibraryHierarchyLevels"."Id" AS "Id", "LibraryHierarchyLevels_Copy"."Id" AS "Parent_Id"
FROM "LibraryHierarchyLevels"
JOIN "LibraryHierarchyLevels" AS "LibraryHierarchyLevels_Copy"
	ON "LibraryHierarchyLevels"."LibraryHierarchy_Id"  = "LibraryHierarchyLevels_Copy"."LibraryHierarchy_Id"
		AND "LibraryHierarchyLevels_Copy"."Sequence" < "LibraryHierarchyLevels"."Sequence"
GROUP BY "LibraryHierarchyLevels"."Id"
ORDER BY "LibraryHierarchyLevels_Copy"."Sequence";

DELETE FROM "LibraryHierarchy";
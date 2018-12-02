CREATE TEMPORARY TABLE IF NOT EXISTS "LibraryHierarchyLevelLeaf"
(
	"LibraryHierarchy_Id" INTEGER NOT NULL, 
	"LibraryHierarchyLevel_Id" INTEGER NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "IDX_LibraryHierarchyLevelLeaf" ON "LibraryHierarchyLevelLeaf"
(
	"LibraryHierarchy_Id"
);

DELETE FROM "LibraryHierarchyLevelLeaf";

INSERT INTO "LibraryHierarchyLevelLeaf"
SELECT LibraryHierarchy_Id, "Id"
FROM "LibraryHierarchyLevels"
GROUP BY "LibraryHierarchy_Id"
HAVING MAX("Sequence")
ORDER BY "Sequence";

CREATE TEMPORARY TABLE IF NOT EXISTS "LibraryHierarchyLevelParent"
(
	"Id" INTEGER NOT NULL, 
	"Parent_Id" INTEGER NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "IDX_LibraryHierarchyLevelParent" ON "LibraryHierarchyLevelParent"
(
	"Id"
);

DELETE FROM "LibraryHierarchyLevelParent";

INSERT INTO "LibraryHierarchyLevelParent"
SELECT "LibraryHierarchyLevels"."Id" AS "Id", "LibraryHierarchyLevels_Copy"."Id" AS "Parent_Id"
FROM "LibraryHierarchyLevels"
JOIN "LibraryHierarchyLevels" AS "LibraryHierarchyLevels_Copy"
	ON "LibraryHierarchyLevels"."LibraryHierarchy_Id"  = "LibraryHierarchyLevels_Copy"."LibraryHierarchy_Id"
		AND "LibraryHierarchyLevels_Copy"."Sequence" < "LibraryHierarchyLevels"."Sequence"
GROUP BY "LibraryHierarchyLevels"."Id"
ORDER BY "LibraryHierarchyLevels_Copy"."Sequence";

CREATE TEMPORARY TABLE IF NOT EXISTS "LibraryHierarchy"
(
	"LibraryHierarchy_Id" INTEGER NOT NULL, 
	"LibraryHierarchyLevel_Id" INTEGER NOT NULL, 
	"LibraryItem_Id" INTEGER NOT NULL, 
	"DisplayValue" text NOT NULL,
	"SortValue" text NOT NULL,
	"IsLeaf" bit NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "IDX_LibraryHierarchy" ON "LibraryHierarchy"
(
	"LibraryHierarchy_Id",
	"LibraryHierarchyLevel_Id",
	"LibraryItem_Id",
	"DisplayValue",
	"SortValue",
	"IsLeaf"
);
CREATE INDEX IF NOT EXISTS "IDX_LibraryHierarchy_LibraryItem" ON "LibraryHierarchy"
(
	"LibraryItem_Id"
);

DELETE FROM "LibraryHierarchy";
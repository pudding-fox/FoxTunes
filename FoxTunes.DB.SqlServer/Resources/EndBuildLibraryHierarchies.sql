INSERT INTO "LibraryHierarchyItems" ("LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "SortValue", "IsLeaf")
SELECT "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "SortValue", "IsLeaf"
FROM "LibraryHierarchy"
GROUP BY "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "SortValue", "IsLeaf";

UPDATE "LibraryHierarchyItems"
SET "Parent_Id" = 
(
	SELECT TOP 1 "LibraryHierarchyItems_Copy"."Id"
	FROM "LibraryHierarchyItems" AS "LibraryHierarchyItems_Copy"
		JOIN "LibraryHierarchy" 
			ON "LibraryHierarchy"."LibraryHierarchy_Id" = "LibraryHierarchyItems"."LibraryHierarchy_Id"
				AND "LibraryHierarchy"."LibraryHierarchyLevel_Id" = "LibraryHierarchyItems"."LibraryHierarchyLevel_Id"
				AND "LibraryHierarchy"."DisplayValue" = "LibraryHierarchyItems"."DisplayValue"
				AND "LibraryHierarchy"."SortValue" = "LibraryHierarchyItems"."SortValue"
				AND "LibraryHierarchy"."IsLeaf" = "LibraryHierarchyItems"."IsLeaf"
	JOIN "LibraryHierarchyLevelParent" 
		ON "LibraryHierarchyLevelParent"."Id" = "LibraryHierarchyItems"."LibraryHierarchyLevel_Id"
	JOIN "LibraryHierarchy" AS "LibraryHierarchy_Copy" 
		ON "LibraryHierarchy_Copy"."LibraryHierarchy_Id" = "LibraryHierarchyItems_Copy"."LibraryHierarchy_Id"
			AND "LibraryHierarchy_Copy"."LibraryHierarchyLevel_Id" = "LibraryHierarchyLevelParent"."Parent_Id"
			AND "LibraryHierarchy_Copy"."LibraryItem_Id" = "LibraryHierarchy"."LibraryItem_Id"
			AND "LibraryHierarchy_Copy"."DisplayValue" = "LibraryHierarchyItems_Copy"."DisplayValue"
			AND "LibraryHierarchy_Copy"."SortValue" = "LibraryHierarchyItems_Copy"."SortValue"
			AND "LibraryHierarchy_Copy"."IsLeaf" = "LibraryHierarchyItems_Copy"."IsLeaf"
)
WHERE "Parent_Id" IS NULL;

INSERT INTO "LibraryHierarchyItem_LibraryItem" ("LibraryHierarchyItem_Id", "LibraryItem_Id")
SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchy"."LibraryItem_Id"
FROM "LibraryHierarchyItems"
	JOIN "LibraryHierarchy" ON "LibraryHierarchy"."LibraryHierarchy_Id" ="LibraryHierarchyItems"."LibraryHierarchy_Id"
		AND "LibraryHierarchy"."LibraryHierarchyLevel_Id" = "LibraryHierarchyItems"."LibraryHierarchyLevel_Id"
		AND "LibraryHierarchy"."DisplayValue" = "LibraryHierarchyItems"."DisplayValue"
		AND "LibraryHierarchy"."SortValue" = "LibraryHierarchyItems"."SortValue"
		AND "LibraryHierarchy"."IsLeaf" = "LibraryHierarchyItems"."IsLeaf";
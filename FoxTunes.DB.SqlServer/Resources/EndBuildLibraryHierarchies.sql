INSERT INTO "LibraryHierarchyItems" ("LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "SortValue", "IsLeaf")
SELECT "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "SortValue", "IsLeaf"
FROM "LibraryHierarchy"
GROUP BY "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "SortValue", "IsLeaf";

UPDATE "LibraryHierarchyItems"
SET "LibraryHierarchyItems"."Parent_Id" = "LibraryHierarchyItems_Parent"."Id"
FROM "LibraryHierarchyItems" 
	JOIN "LibraryHierarchy" 
		ON "LibraryHierarchyItems"."LibraryHierarchyLevel_Id" = "LibraryHierarchy"."LibraryHierarchyLevel_Id" 
			AND "LibraryHierarchyItems"."LibraryHierarchy_Id" = "LibraryHierarchy"."LibraryHierarchy_Id"
			AND "LibraryHierarchyItems"."DisplayValue" = "LibraryHierarchy"."DisplayValue"
			AND "LibraryHierarchyItems"."SortValue" = "LibraryHierarchy"."SortValue" 
			AND "LibraryHierarchyItems"."IsLeaf" = "LibraryHierarchy"."IsLeaf" 
	JOIN "LibraryHierarchyLevelParent" 
		ON "LibraryHierarchyItems"."LibraryHierarchyLevel_Id" = "LibraryHierarchyLevelParent"."Id"
	JOIN "LibraryHierarchyItems" AS "LibraryHierarchyItems_Parent"
		ON "LibraryHierarchyLevelParent"."Parent_Id" = "LibraryHierarchyItems_Parent"."LibraryHierarchyLevel_Id"
			AND "LibraryHierarchy"."LibraryHierarchy_Id" = "LibraryHierarchyItems_Parent"."LibraryHierarchy_Id"
	JOIN "LibraryHierarchy" AS "LibraryHierarchy_Parent"
		ON "LibraryHierarchyItems_Parent"."LibraryHierarchy_Id" = "LibraryHierarchy_Parent"."LibraryHierarchy_Id"
			AND "LibraryHierarchyItems_Parent"."LibraryHierarchyLevel_Id" = "LibraryHierarchy_Parent"."LibraryHierarchyLevel_Id"
			AND "LibraryHierarchyItems_Parent"."DisplayValue" = "LibraryHierarchy_Parent"."DisplayValue"
			AND "LibraryHierarchyItems_Parent"."SortValue" = "LibraryHierarchy_Parent"."SortValue" 
			AND "LibraryHierarchyItems_Parent"."IsLeaf" = "LibraryHierarchy_Parent"."IsLeaf"
			AND "LibraryHierarchy"."LibraryItem_Id" = "LibraryHierarchy_Parent"."LibraryItem_Id"
WHERE "LibraryHierarchyItems"."Parent_Id" IS NULL;

INSERT INTO "LibraryHierarchyItem_LibraryItem" ("LibraryHierarchyItem_Id", "LibraryItem_Id")
SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchy"."LibraryItem_Id"
FROM "LibraryHierarchyItems"
	JOIN "LibraryHierarchy" ON "LibraryHierarchy"."LibraryHierarchy_Id" ="LibraryHierarchyItems"."LibraryHierarchy_Id"
		AND "LibraryHierarchy"."LibraryHierarchyLevel_Id" = "LibraryHierarchyItems"."LibraryHierarchyLevel_Id"
		AND "LibraryHierarchy"."DisplayValue" = "LibraryHierarchyItems"."DisplayValue"
		AND "LibraryHierarchy"."SortValue" = "LibraryHierarchyItems"."SortValue"
		AND "LibraryHierarchy"."IsLeaf" = "LibraryHierarchyItems"."IsLeaf";
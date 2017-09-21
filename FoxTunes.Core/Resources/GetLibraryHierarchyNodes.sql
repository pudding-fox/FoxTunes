SELECT "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."LibraryHierarchyLevel_Id", "LibraryHierarchyItems"."DisplayValue","LibraryHierarchyItems"."IsLeaf"
FROM "LibraryHierarchyItems"
JOIN  "LibraryHierarchyItems" AS "LibraryHierarchyItems_Copy" ON "LibraryHierarchyItems"."Parent_Id" = "LibraryHierarchyItems_Copy"."Id"
WHERE "LibraryHierarchyItems"."LibraryHierarchy_Id" = @libraryHierarchyId 
	AND "LibraryHierarchyItems_Copy"."LibraryHierarchy_Id" = @libraryHierarchyId 
	AND "LibraryHierarchyItems_Copy"."LibraryHierarchyLevel_Id" = @libraryHierarchyLevelId 
	AND "LibraryHierarchyItems_Copy"."DisplayValue" = @displayValue
GROUP BY "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."LibraryHierarchyLevel_Id", "LibraryHierarchyItems"."DisplayValue", "LibraryHierarchyItems"."IsLeaf"
SELECT "Id", "LibraryHierarchy_Id", "DisplayValue", "IsLeaf"
FROM "LibraryHierarchyItems"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
	AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
ORDER BY "SortValue", "DisplayValue"
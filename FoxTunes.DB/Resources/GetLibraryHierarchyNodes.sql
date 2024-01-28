SELECT "Id", "LibraryHierarchy_Id", "Value", "IsLeaf"
FROM "LibraryHierarchyItems"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
	AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
ORDER BY "Value"
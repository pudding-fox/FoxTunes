SELECT "LibraryItem_Id"
FROM "LibraryHierarchyItems"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId 
	AND "LibraryHierarchyLevel_Id" = @libraryHierarchyLevelId 
	AND "DisplayValue" = @displayValue
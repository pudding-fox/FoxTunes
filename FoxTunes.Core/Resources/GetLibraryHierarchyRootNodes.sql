SELECT "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "IsLeaf"
FROM "LibraryHierarchyItems"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId AND "Parent_Id" IS NULL
GROUP BY "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "IsLeaf"
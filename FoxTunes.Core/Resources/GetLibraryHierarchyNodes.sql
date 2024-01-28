WITH RECURSIVE LibraryHierarchy("Root", "Id", "Parent_Id", "DisplayValue")
AS
(
	SELECT "Id", "Id", "Parent_Id", "DisplayValue"
	FROM "LibraryHierarchyItems"
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."DisplayValue"
	FROM "LibraryHierarchyItems"
		JOIN LibraryHierarchy ON "LibraryHierarchyItems"."Parent_Id" = LibraryHierarchy."Id"
)

SELECT "Id", "DisplayValue", "IsLeaf"
FROM "LibraryHierarchyItems"
WHERE (@libraryHierarchyId IS NULL OR "LibraryHierarchy_Id" = @libraryHierarchyId)
	AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
	AND 
	(
		 "IsLeaf" = 1 OR @filter IS NULL OR EXISTS
		(
			SELECT * 
			FROM LibraryHierarchy 
			WHERE "Root" = "LibraryHierarchyItems"."Id" 
				AND "DisplayValue" LIKE @filter
		)
	)
ORDER BY "SortValue", "DisplayValue"
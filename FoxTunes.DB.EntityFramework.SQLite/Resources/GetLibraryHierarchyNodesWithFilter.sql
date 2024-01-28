WITH RECURSIVE 

LibraryHierarchyParents("Root", "Id", "Parent_Id", "DisplayValue")
AS
(
	SELECT "Id", "Id", "Parent_Id", "DisplayValue"
	FROM "LibraryHierarchyItems"
	WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
		AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."DisplayValue"
	FROM "LibraryHierarchyItems"
		JOIN LibraryHierarchyParents ON "LibraryHierarchyItems"."Id" = LibraryHierarchyParents."Parent_Id"
),

LibraryHierarchyChildren("Root", "Id", "Parent_Id", "DisplayValue")
AS
(
	SELECT "Id", "Id", "Parent_Id", "DisplayValue"
	FROM "LibraryHierarchyItems"
	WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
		AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."DisplayValue"
	FROM "LibraryHierarchyItems"
		JOIN LibraryHierarchyChildren ON "LibraryHierarchyItems"."Parent_Id" = LibraryHierarchyChildren."Id"
)

SELECT "Id", "LibraryHierarchy_Id", "DisplayValue", "IsLeaf"
FROM "LibraryHierarchyItems"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
	AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
	AND 
	(
		@filter IS NULL OR EXISTS
		(
			SELECT * 
			FROM LibraryHierarchyParents 
			WHERE "Root" = "LibraryHierarchyItems"."Id" 
				AND "DisplayValue" LIKE @filter
		) OR EXISTS
		(
			SELECT * 
			FROM LibraryHierarchyChildren 
			WHERE "Root" = "LibraryHierarchyItems"."Id" 
				AND "DisplayValue" LIKE @filter
		)
	)
ORDER BY "SortValue", "DisplayValue"
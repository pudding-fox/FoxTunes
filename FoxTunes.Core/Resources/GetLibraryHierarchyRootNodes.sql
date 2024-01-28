WITH RECURSIVE LibraryHierarchy("Root", "Id", "Parent_Id", "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "IsLeaf")
AS
(
	SELECT "Id", "Id", "Parent_Id", "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "IsLeaf"
	FROM "LibraryHierarchyItems"
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."LibraryHierarchyLevel_Id", "LibraryHierarchyItems"."DisplayValue", "LibraryHierarchyItems"."IsLeaf"
	FROM "LibraryHierarchyItems"
	JOIN LibraryHierarchy ON "LibraryHierarchyItems"."Parent_Id" = LibraryHierarchy."Id"
)

SELECT "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "IsLeaf"
FROM "LibraryHierarchyItems"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId AND "Parent_Id" IS NULL AND 
	(
		@filter IS NULL OR EXISTS
		(
			SELECT * 
			FROM LibraryHierarchy 
			WHERE "Root" = FIRST("LibraryHierarchyItems"."Id")
				AND "DisplayValue" LIKE @filter
		)
	)
GROUP BY "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "IsLeaf"
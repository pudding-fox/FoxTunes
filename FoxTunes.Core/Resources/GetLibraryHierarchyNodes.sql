WITH RECURSIVE LibraryHierarchy("Root", "Id", "Parent_Id", "LibraryHierarchy_Id", "LibraryHierarchyLevel_Id", "DisplayValue", "IsLeaf")
AS
(
	SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."LibraryHierarchyLevel_Id", "LibraryHierarchyItems"."DisplayValue","LibraryHierarchyItems"."IsLeaf"
	FROM "LibraryHierarchyItems"
		JOIN  "LibraryHierarchyItems" AS "LibraryHierarchyItems_Copy" ON "LibraryHierarchyItems"."Parent_Id" = "LibraryHierarchyItems_Copy"."Id"
	WHERE "LibraryHierarchyItems"."LibraryHierarchy_Id" = @libraryHierarchyId 
		AND "LibraryHierarchyItems_Copy"."LibraryHierarchy_Id" = @libraryHierarchyId 
		AND "LibraryHierarchyItems_Copy"."LibraryHierarchyLevel_Id" = @libraryHierarchyLevelId 
		AND "LibraryHierarchyItems_Copy"."DisplayValue" = @displayValue
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."LibraryHierarchyLevel_Id", "LibraryHierarchyItems"."DisplayValue", "LibraryHierarchyItems"."IsLeaf"
	FROM "LibraryHierarchyItems"
		JOIN LibraryHierarchy ON "LibraryHierarchyItems"."Parent_Id" = LibraryHierarchy."Id"
)

SELECT "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."LibraryHierarchyLevel_Id", "LibraryHierarchyItems"."DisplayValue","LibraryHierarchyItems"."IsLeaf"
FROM "LibraryHierarchyItems"
	JOIN  "LibraryHierarchyItems" AS "LibraryHierarchyItems_Copy" ON "LibraryHierarchyItems"."Parent_Id" = "LibraryHierarchyItems_Copy"."Id"
WHERE "LibraryHierarchyItems"."LibraryHierarchy_Id" = @libraryHierarchyId 
	AND "LibraryHierarchyItems_Copy"."LibraryHierarchy_Id" = @libraryHierarchyId 
	AND "LibraryHierarchyItems_Copy"."LibraryHierarchyLevel_Id" = @libraryHierarchyLevelId 
	AND "LibraryHierarchyItems_Copy"."DisplayValue" = @displayValue
	AND 
	(
		@filter IS NULL OR EXISTS
		(
			SELECT * 
			FROM LibraryHierarchy 
			WHERE "Root" = "LibraryHierarchyItems"."Id" 
				AND "DisplayValue" LIKE @filter
		)
	)
GROUP BY "LibraryHierarchyItems"."LibraryHierarchy_Id", "LibraryHierarchyItems"."LibraryHierarchyLevel_Id", "LibraryHierarchyItems"."DisplayValue", "LibraryHierarchyItems"."IsLeaf"
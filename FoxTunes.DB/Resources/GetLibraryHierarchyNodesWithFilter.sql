WITH

LibraryHierarchyParents("Root", "Id", "Parent_Id", "Value")
AS
(
	SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value"
	FROM "LibraryHierarchyItems"
	WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
		AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value"
	FROM "LibraryHierarchyItems" 
		JOIN LibraryHierarchyParents ON "LibraryHierarchyItems"."Id" = LibraryHierarchyParents."Parent_Id"
),

LibraryHierarchyChildren("Root", "Id", "Parent_Id", "Value")
AS
(
	SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value"
	FROM "LibraryHierarchyItems"
	WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
		AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value"
	FROM "LibraryHierarchyItems" 
		JOIN LibraryHierarchyChildren ON "LibraryHierarchyItems"."Parent_Id" = LibraryHierarchyChildren."Id"
)

SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchy_Id", "Value", "IsLeaf"
FROM "LibraryHierarchyItems"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
	AND ((@libraryHierarchyItemId IS NULL AND "Parent_Id" IS NULL) OR "Parent_Id" = @libraryHierarchyItemId)
	AND 
	(
		@filter IS NULL OR EXISTS
		(
			SELECT * 
			FROM LibraryHierarchyParents 
			WHERE "Root" = "LibraryHierarchyItems"."Id" 
				AND "Value" LIKE @filter
		) OR EXISTS
		(
			SELECT * 
			FROM LibraryHierarchyChildren 
			WHERE "Root" = "LibraryHierarchyItems"."Id" 
				AND "Value" LIKE @filter
		)
	)
ORDER BY "Value"
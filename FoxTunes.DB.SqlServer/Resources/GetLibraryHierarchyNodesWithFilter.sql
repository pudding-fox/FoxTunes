WITH 

LibraryHierarchyParent
AS
(
	SELECT "LibraryHierarchyItems"."Id", "Parent_Id", "Value"
	FROM "LibraryHierarchyItems"
	WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
),

LibraryHierarchyParents("Root", "Id", "Parent_Id", "Value")
AS
(
	SELECT LibraryHierarchyParent."Id", LibraryHierarchyParent."Id", LibraryHierarchyParent."Parent_Id", LibraryHierarchyParent."Value"
	FROM LibraryHierarchyParent
	WHERE ((@libraryHierarchyItemId IS NULL AND LibraryHierarchyParent."Parent_Id" IS NULL) OR LibraryHierarchyParent."Parent_Id" = @libraryHierarchyItemId)
	UNION ALL 
	SELECT "Root", LibraryHierarchyParent."Id", LibraryHierarchyParent."Parent_Id", LibraryHierarchyParent."Value"
	FROM LibraryHierarchyParent
		JOIN LibraryHierarchyParents ON LibraryHierarchyParent."Id" = LibraryHierarchyParents."Parent_Id"
),

LibraryHierarchyChildren("Root", "Id", "Parent_Id", "Value")
AS
(
	SELECT LibraryHierarchyParent."Id", LibraryHierarchyParent."Id", LibraryHierarchyParent."Parent_Id", LibraryHierarchyParent."Value"
	FROM LibraryHierarchyParent
	WHERE ((@libraryHierarchyItemId IS NULL AND LibraryHierarchyParent."Parent_Id" IS NULL) OR LibraryHierarchyParent."Parent_Id" = @libraryHierarchyItemId)
	UNION ALL 
	SELECT "Root", LibraryHierarchyParent."Id", LibraryHierarchyParent."Parent_Id", LibraryHierarchyParent."Value"
	FROM LibraryHierarchyParent
		JOIN LibraryHierarchyChildren ON LibraryHierarchyParent."Parent_Id" = LibraryHierarchyChildren."Id"
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
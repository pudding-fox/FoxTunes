WITH RECURSIVE 

LibraryHierarchyParent
AS
(
	SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Parent_Id" AS "Parent_Id", "Value"
	FROM "LibraryHierarchyItems"
		LEFT JOIN "LibraryHierarchyItem_Parent"
			ON "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Id" = "LibraryHierarchyItems"."Id"
),

LibraryHierarchyParents("Root", "Id", "Parent_Id", "Value")
AS
(
	SELECT LibraryHierarchyParent."Id", LibraryHierarchyParent."Id", LibraryHierarchyParent."Parent_Id", LibraryHierarchyParent."Value"
	FROM LibraryHierarchyParent
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
	UNION ALL 
	SELECT "Root", LibraryHierarchyParent."Id", LibraryHierarchyParent."Parent_Id", LibraryHierarchyParent."Value"
	FROM LibraryHierarchyParent
		JOIN LibraryHierarchyChildren ON LibraryHierarchyParent."Parent_Id" = LibraryHierarchyChildren."Id"
)

SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchy_Id", "Value", "IsLeaf"
FROM "LibraryHierarchyItems"
	LEFT JOIN "LibraryHierarchyItem_Parent"
		ON "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Id" = "LibraryHierarchyItems"."Id"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
	AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Parent_Id" IS NULL) OR "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Parent_Id" = @libraryHierarchyItemId)
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
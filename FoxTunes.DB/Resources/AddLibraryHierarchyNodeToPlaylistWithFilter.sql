WITH

LibraryHierarchyParents("Root", "Id", "Parent_Id", "Value")
AS
(
	SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value"
	FROM "LibraryHierarchyItems"
	WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value"
	FROM "LibraryHierarchyItems" 
		JOIN LibraryHierarchyParents ON "LibraryHierarchyItems"."Id" = LibraryHierarchyParents."Parent_Id"
),

LibraryHierarchyChildren("Root", "Id", "Parent_Id", "Value", "IsLeaf")
AS
(
	SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value", "LibraryHierarchyItems"."IsLeaf"
	FROM "LibraryHierarchyItems"
	WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
		AND "LibraryHierarchyItems"."Id" = @libraryHierarchyItemId
	UNION ALL 
	SELECT "Root", "LibraryHierarchyItems"."Id", "LibraryHierarchyItems"."Parent_Id", "LibraryHierarchyItems"."Value", "LibraryHierarchyItems"."IsLeaf"
	FROM "LibraryHierarchyItems" 
		JOIN LibraryHierarchyChildren ON "LibraryHierarchyItems"."Parent_Id" = LibraryHierarchyChildren."Id"
)

INSERT INTO "PlaylistItems" ("LibraryItem_Id", "Sequence", "DirectoryName", "FileName", "Status") 
SELECT "LibraryItems"."Id", @sequence, "LibraryItems"."DirectoryName", "LibraryItems"."FileName", @status
FROM LibraryHierarchyChildren
	JOIN "LibraryHierarchyItem_LibraryItem" 
		ON LibraryHierarchyChildren."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id"
	JOIN "LibraryItems"
		ON "LibraryItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id"
WHERE 
	(
		@filter IS NULL 
		OR 
		(
			LibraryHierarchyChildren."Value" LIKE @filter OR EXISTS
			(
				SELECT * 
				FROM LibraryHierarchyParents 
				WHERE LibraryHierarchyParents."Root" = LibraryHierarchyChildren."Id" 
					AND LibraryHierarchyParents."Value" LIKE @filter
			)
		)
	) 
	AND (@favorite IS NULL OR "LibraryItems"."Favorite" = @favorite)
	AND LibraryHierarchyChildren."IsLeaf" = 1;

SELECT COUNT(*)
FROM "PlaylistItems"
WHERE "Status" = @status
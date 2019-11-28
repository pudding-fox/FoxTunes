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

SELECT 
	"LibraryItems"."Id" AS "LibraryItems_Id",
	"LibraryItems"."DirectoryName" AS "LibraryItems_DirectoryName",
	"LibraryItems"."FileName" AS "LibraryItems_FileName",
	"LibraryItems"."ImportDate" AS "LibraryItems_ImportDate",
	"LibraryItems"."Favorite" AS "LibraryItems_Favorite",
	"LibraryItems"."Status" AS "LibraryItems_Status",
	"MetaDataItems"."Id" AS "MetaDataItems_Id",
	"MetaDataItems"."Name" AS "MetaDataItems_Name",
	"MetaDataItems"."Type" AS "MetaDataItems_Type",
	"MetaDataItems"."Value" AS "MetaDataItems_Value"
FROM LibraryHierarchyChildren
	JOIN "LibraryHierarchyItem_LibraryItem" 
		ON "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id" = LibraryHierarchyChildren."Id"
	JOIN "LibraryItems" 
		ON "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id" = "LibraryItems"."Id"
	LEFT OUTER JOIN "LibraryItem_MetaDataItem"
		ON @loadMetaData = 1 
			AND "LibraryItems"."Id" = "LibraryItem_MetaDataItem"."LibraryItem_Id"
	LEFT OUTER JOIN "MetaDataItems"
		ON @loadMetaData = 1 
			AND "MetaDataItems"."Id" = "LibraryItem_MetaDataItem"."MetaDataItem_Id"
			AND (@metaDataType & "MetaDataItems"."Type") =  "MetaDataItems"."Type"
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
	AND LibraryHierarchyChildren."IsLeaf" = 1
ORDER BY "LibraryItems"."FileName"
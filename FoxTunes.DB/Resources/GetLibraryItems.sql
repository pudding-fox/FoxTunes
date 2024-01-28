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
FROM "LibraryHierarchyItems"
	JOIN "LibraryHierarchyItem_LibraryItem" 
		ON "LibraryHierarchyItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id"
	JOIN "LibraryItems" 
		ON "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id" = "LibraryItems"."Id"
	LEFT OUTER JOIN "LibraryItem_MetaDataItem"
		ON @loadMetaData = 1 
			AND "LibraryItems"."Id" = "LibraryItem_MetaDataItem"."LibraryItem_Id"
	LEFT OUTER JOIN "MetaDataItems"
		ON @loadMetaData = 1 
			AND "MetaDataItems"."Id" = "LibraryItem_MetaDataItem"."MetaDataItem_Id"
			AND (@metaDataType & "MetaDataItems"."Type") =  "MetaDataItems"."Type"
WHERE "LibraryHierarchyItems"."LibraryHierarchy_Id" = @libraryHierarchyId
	AND "LibraryHierarchyItems"."Id" = @libraryHierarchyItemId
	AND (@favorite IS NULL OR EXISTS(
		SELECT * 
		FROM "LibraryItems" 
			JOIN "LibraryHierarchyItem_LibraryItem" 
				ON "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id" = "LibraryItems"."Id"
					AND "LibraryItems"."Favorite" = @favorite
		WHERE "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id" = "LibraryHierarchyItems"."Id"))
ORDER BY "LibraryItems"."FileName"
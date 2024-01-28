SELECT TOP 5 "MetaDataItems"."Value"
FROM "LibraryHierarchyItems"
	JOIN "LibraryHierarchyItem_LibraryItem" ON "LibraryHierarchyItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id"
	JOIN "LibraryItem_MetaDataItem" ON "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id" = "LibraryItem_MetaDataItem"."LibraryItem_Id"
	JOIN "MetaDataItems" ON "MetaDataItems"."Id" = "LibraryItem_MetaDataItem"."MetaDataItem_Id"
WHERE "LibraryHierarchyItems"."Id" = @libraryHierarchyItemId 
	AND (@type & "MetaDataItems"."Type") =  "MetaDataItems"."Type"
GROUP BY "MetaDataItems"."Value"
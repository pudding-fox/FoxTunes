SELECT "MetaDataItems"."Name", "MetaDataItems"."Type", "MetaDataItems"."NumericValue",  "MetaDataItems"."TextValue", "MetaDataItems"."FileValue"
FROM "LibraryHierarchyItems"
	JOIN "LibraryItem_MetaDataItem" ON "LibraryHierarchyItems"."LibraryItem_Id" = "LibraryItem_MetaDataItem"."LibraryItem_Id"
	JOIN "MetaDataItems" ON "MetaDataItems"."Id" = "LibraryItem_MetaDataItem"."MetaDataItem_Id"
WHERE "LibraryHierarchyItems"."LibraryHierarchy_Id" = @libraryHierarchyId 
	AND "LibraryHierarchyItems"."LibraryHierarchyLevel_Id" = @libraryHierarchyLevelId 
	AND "LibraryHierarchyItems"."DisplayValue" = @displayValue
	AND "MetaDataItems"."Type" = @type
GROUP BY "MetaDataItems"."Name", "MetaDataItems"."Type", "MetaDataItems"."NumericValue",  "MetaDataItems"."TextValue", "MetaDataItems"."FileValue"
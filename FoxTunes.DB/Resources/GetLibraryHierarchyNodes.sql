SELECT "LibraryHierarchyItems"."Id", "LibraryHierarchy_Id", "Value", "IsLeaf"
FROM "LibraryHierarchyItems"
	LEFT JOIN "LibraryHierarchyItem_Parent"
		ON "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Id" = "LibraryHierarchyItems"."Id"
WHERE "LibraryHierarchy_Id" = @libraryHierarchyId
	AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Parent_Id" IS NULL) OR "LibraryHierarchyItem_Parent"."LibraryHierarchyItem_Parent_Id" = @libraryHierarchyItemId)
ORDER BY "Value"
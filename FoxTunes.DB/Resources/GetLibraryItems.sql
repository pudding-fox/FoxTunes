SELECT "LibraryItems".*
FROM "LibraryHierarchyItems"
	JOIN "LibraryHierarchyItem_LibraryItem" 
		ON "LibraryHierarchyItems"."Id" = "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id"
	JOIN "LibraryItems" 
		ON "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id" = "LibraryItems"."Id"
WHERE "LibraryHierarchyItems"."LibraryHierarchy_Id" = @libraryHierarchyId
	AND ((@libraryHierarchyItemId IS NULL AND "LibraryHierarchyItems"."Parent_Id" IS NULL) 
		OR "LibraryHierarchyItems"."Parent_Id" = @libraryHierarchyItemId)
	AND (@favorite IS NULL OR EXISTS(
		SELECT * 
		FROM "LibraryItems" 
			JOIN "LibraryHierarchyItem_LibraryItem" 
				ON "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id" = "LibraryItems"."Id"
					AND "LibraryItems"."Favorite" = @favorite
		WHERE "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id" = "LibraryHierarchyItems"."Id"))
ORDER BY "LibraryItems"."FileName"
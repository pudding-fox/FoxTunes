UPDATE "LibraryItems"
SET "Favorite" = @isFavorite
WHERE "Id" IN
(
	SELECT "LibraryHierarchyItem_LibraryItem"."LibraryItem_Id"
	FROM "LibraryHierarchyItem_LibraryItem"
	WHERE "LibraryHierarchyItem_LibraryItem"."LibraryHierarchyItem_Id" = @libraryHierarchyItemId
)
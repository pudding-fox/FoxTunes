DELETE FROM [LibraryHierarchyItem_LibraryItem]
WHERE [LibraryItem_Id] IN
(
	SELECT [Id] 
	FROM [LibraryItems]
	WHERE @status IS NULL OR [Status] = @status
);

DELETE FROM [LibraryHierarchyItems]
WHERE NOT EXISTS
(
	SELECT *
	FROM [LibraryHierarchyItem_LibraryItem]
	WHERE [LibraryHierarchyItem_LibraryItem].[LibraryHierarchyItem_Id] = [LibraryHierarchyItems].[Id]
);
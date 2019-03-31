DELETE FROM [LibraryHierarchyItem_LibraryItem]
WHERE [LibraryItem_Id] IN
(
	SELECT [Id] 
	FROM [LibraryItems]
	WHERE @status IS NULL OR [Status] = @status
);

DELETE FROM [LibraryHierarchyItems]
WHERE [Id] IN
(
	SELECT [LibraryHierarchyItems].[Id]
	FROM [LibraryHierarchyItems]
		 LEFT JOIN [LibraryHierarchyItem_LibraryItem]
			ON [LibraryHierarchyItems].[Id] = [LibraryHierarchyItem_LibraryItem].[LibraryHierarchyItem_Id]
	WHERE [LibraryHierarchyItem_LibraryItem].[Id] IS NULL
);
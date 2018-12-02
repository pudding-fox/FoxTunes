DELETE FROM [LibraryHierarchy_LibraryHierarchyItem];

DELETE FROM [LibraryHierarchyItem_LibraryItem];

DELETE FROM [LibraryHierarchyItems];

DELETE FROM [LibraryItem_MetaDataItem];

DELETE FROM [PlaylistItem_MetaDataItem]
WHERE [PlaylistItem_Id] IN
(
	SELECT [PlaylistItems].[Id]
	FROM [PlaylistItems]
		JOIN [LibraryItems] ON [PlaylistItems].[LibraryItem_Id] = [LibraryItems].[Id]
);

DELETE FROM [PlaylistItems]
WHERE [Id] IN
(
	SELECT [PlaylistItems].[Id]
	FROM [PlaylistItems]
		JOIN [LibraryItems] ON [PlaylistItems].[LibraryItem_Id] = [LibraryItems].[Id]
);

DELETE FROM [LibraryItems];
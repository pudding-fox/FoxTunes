WITH 
PopulatedNodes AS
(
	SELECT [LibraryHierarchyItems].[Id], [LibraryHierarchyItems].[Value]
	FROM [LibraryHierarchyItems]
		JOIN [LibraryHierarchyItems] Children
			ON [LibraryHierarchyItems].[Id] = Children.[Parent_Id]
	GROUP BY [LibraryHierarchyItems].[Id], [LibraryHierarchyItems].[Value]
	HAVING COUNT(Children.[Id]) > 1
),

DefunctNodes(Id, Parent_Id, Value) AS
(
	SELECT [LibraryHierarchyItems].[Id], [LibraryHierarchyItems].[Parent_Id], [LibraryHierarchyItems].[Value]
	FROM [LibraryHierarchyItems]
	WHERE [LibraryHierarchyItems].[LibraryHierarchy_Id] = @libraryHierarchyId
		AND [LibraryHierarchyItems].[Parent_Id] IS NULL
		AND NOT EXISTS
		(
			SELECT * 
			FROM PopulatedNodes
			WHERE PopulatedNodes.Id = [LibraryHierarchyItems].[Id]
		)
	UNION ALL
	SELECT [LibraryHierarchyItems].[Id], [LibraryHierarchyItems].[Parent_Id], [LibraryHierarchyItems].[Value]
	FROM [LibraryHierarchyItems]
		JOIN DefunctNodes 
			ON [LibraryHierarchyItems].[Parent_Id] = DefunctNodes.[Id]
	WHERE NOT EXISTS
	(
		SELECT * 
		FROM PopulatedNodes
		WHERE PopulatedNodes.Id = [LibraryHierarchyItems].[Id]
	)		
)

DELETE FROM [LibraryHierarchyItem_LibraryItem]
WHERE [LibraryHierarchyItem_Id] IN
(
	SELECT [Id]
	FROM  DefunctNodes
);

UPDATE [LibraryHierarchyItems]
SET [Parent_Id] = NULL
WHERE NOT EXISTS
(
	SELECT *
	FROM [LibraryHierarchyItem_LibraryItem]
	WHERE [LibraryHierarchyItem_LibraryItem].[LibraryHierarchyItem_Id] = [LibraryHierarchyItems].[Parent_Id]
);

DELETE FROM [LibraryHierarchyItems]
WHERE NOT EXISTS
(
	SELECT *
	FROM [LibraryHierarchyItem_LibraryItem]
	WHERE [LibraryHierarchyItem_LibraryItem].[LibraryHierarchyItem_Id] = [LibraryHierarchyItems].[Id]
);
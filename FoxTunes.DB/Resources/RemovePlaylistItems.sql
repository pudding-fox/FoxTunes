DELETE FROM [PlaylistItem_MetaDataItem]
WHERE [PlaylistItem_Id] IN
(
	SELECT [Id]
	FROM [PlaylistItems]
	WHERE [Status] = @status
);

DELETE FROM [PlaylistItems]
WHERE [Status] = @status
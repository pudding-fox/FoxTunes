SELECT "MetaDataItems".*
FROM "MetaDataItems"
	JOIN "PlaylistItem_MetaDataItem" ON  "MetaDataItems"."Id" = "PlaylistItem_MetaDataItem"."MetaDataItem_Id"
WHERE "PlaylistItem_MetaDataItem"."PlaylistItem_Id" = @playlistItemId
	AND (@type & "MetaDataItems"."Type") =  "MetaDataItems"."Type"
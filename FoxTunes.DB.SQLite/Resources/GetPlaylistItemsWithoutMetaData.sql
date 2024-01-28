SELECT *
FROM "PlaylistItems"
WHERE "Status" = @status
    AND NOT EXISTS
    (
        SELECT *
        FROM "PlaylistItem_MetaDataItem"
        WHERE "PlaylistItem_MetaDataItem"."PlaylistItem_Id" = "PlaylistItems"."Id"
    )
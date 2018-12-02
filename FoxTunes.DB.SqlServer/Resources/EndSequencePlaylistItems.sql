UPDATE "PlaylistItems"
SET "Sequence" = "Sequence" + ("RowNumber" - 1)
FROM
(
	SELECT "PlaylistItems".*, ROW_NUMBER() OVER (ORDER BY "PlaylistSort"."Value1", "PlaylistSort"."Value2", "PlaylistSort"."Value3", "PlaylistSort"."Value4", "PlaylistSort"."Value5", "PlaylistSort"."Value6", "PlaylistSort"."Value7", "PlaylistSort"."Value8", "PlaylistSort"."Value9", "PlaylistSort"."Value10") AS "RowNumber"
	FROM "PlaylistItems"
		JOIN "PlaylistSort" ON "PlaylistItems"."Id" = "PlaylistSort"."PlaylistItem_Id"
	WHERE "Status" = @status
) AS "PlaylistItems"
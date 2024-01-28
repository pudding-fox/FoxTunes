INSERT INTO "PlaylistSequence" ("PlaylistItem_Id")
SELECT "PlaylistItem_Id"
FROM "PlaylistSort"
ORDER BY "Value1", "Value2", "Value3", "Value4", "Value5", "Value6", "Value7", "Value8", "Value9", "Value10";

UPDATE "PlaylistItems"
SET "Sequence" = "Sequence" +
(
	SELECT "Id" - 1
	FROM "PlaylistSequence"
	WHERE "PlaylistSequence"."PlaylistItem_Id" = "PlaylistItems"."Id"
)
WHERE "Status" = @status;
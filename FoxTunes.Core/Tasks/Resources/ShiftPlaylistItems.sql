UPDATE "PlaylistItems"
SET "Sequence" = "Sequence" + @offset
WHERE "Status" = @status AND "Sequence" >= @sequence
WITH "CurrentSequence"
AS
(	
		SELECT "Sequence" 
		FROM "PlaylistItems"
		WHERE "Id" = @id
)

UPDATE "PlaylistItems"
SET "Sequence" = 
(
	CASE WHEN (SELECT "Sequence" FROM "CurrentSequence") < @sequence THEN
		"Sequence" - 1
	ELSE
		"Sequence" + 1
	END
)
WHERE 
(
	(SELECT "Sequence" FROM "CurrentSequence") < @sequence 
		AND "Sequence" BETWEEN (SELECT "Sequence" FROM "CurrentSequence") 
			AND @sequence
) 
OR 
(
	"Sequence" BETWEEN @sequence 
		AND (SELECT "Sequence" FROM "CurrentSequence")
);

UPDATE "PlaylistItems"
SET "Sequence" = @sequence
WHERE "Id" = @id;
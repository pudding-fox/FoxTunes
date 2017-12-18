SELECT *
FROM "PlaylistItems"
WHERE "Sequence" < @sequence
ORDER BY  "Sequence" DESC
LIMIT 1
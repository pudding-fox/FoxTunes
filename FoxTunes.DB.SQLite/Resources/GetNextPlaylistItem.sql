SELECT *
FROM "PlaylistItems"
WHERE "Sequence" > @sequence
ORDER BY  "Sequence"
LIMIT 1
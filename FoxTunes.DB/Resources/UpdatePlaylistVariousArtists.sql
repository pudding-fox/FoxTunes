INSERT INTO "MetaDataItems" ("Name", "Type", "NumericValue")
SELECT @name, @type, @numericValue
WHERE NOT EXISTS(SELECT * FROM "MetaDataItems" WHERE "Name" = @name AND "Type" = @type AND "NumericValue" = @numericValue);

WITH "MetaData"
AS
(
	SELECT 
		"PlaylistItem_MetaDataItem"."PlaylistItem_Id" AS "Id",
		"MetaDataItems"."Name",
		"MetaDataItems"."NumericValue",
		"MetaDataItems"."TextValue"
	FROM "PlaylistItem_MetaDataItem" 
		JOIN "MetaDataItems" 
			ON "MetaDataItems"."Id" = "PlaylistItem_MetaDataItem"."MetaDataItem_Id"
),

"AlbumArtist"
AS
(
	SELECT "AlbumArtist"."Id", "AlbumArtist"."DirectoryName", "Album", CASE WHEN "AlbumArtist" IS NOT NULL THEN "AlbumArtist" ELSE "Artist" END AS "Artist"
	FROM
	(
		SELECT "PlaylistItems".*,
		(
			SELECT "TextValue"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "PlaylistItems"."Id" AND "MetaData"."Name" = 'Album'
		) AS "Album",
		(
			SELECT "TextValue"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "PlaylistItems"."Id" AND "MetaData"."Name" = 'AlbumArtist'
		) AS "AlbumArtist",
		(
			SELECT "TextValue"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "PlaylistItems"."Id" AND "MetaData"."Name" = 'Artist'
		) AS "Artist"
		FROM "PlaylistItems"
		WHERE "Status" = @status
	) AS "AlbumArtist"
),

"VariousArtists" AS
(
	SELECT "DirectoryName", "Album"
	FROM "AlbumArtist"
	GROUP BY "DirectoryName", "Album"
	HAVING COUNT(DISTINCT "Artist") > 1
),

"VA_MetaDataItem" AS
(
	SELECT "Id" 
	FROM "MetaDataItems" 
	WHERE "Name" = @name AND "Type" = @type AND "NumericValue" = @numericValue
)
	
INSERT INTO "PlaylistItem_MetaDataItem" ("PlaylistItem_Id", "MetaDataItem_Id")
SELECT "AlbumArtist"."Id", (SELECT "Id" FROM "VA_MetaDataItem")
FROM "AlbumArtist"
JOIN "VariousArtists" ON "AlbumArtist"."DirectoryName" = "VariousArtists"."DirectoryName" 
	AND "AlbumArtist"."Album" = "VariousArtists"."Album"
WHERE NOT EXISTS(
	SELECT * 
	FROM "PlaylistItem_MetaDataItem"
	JOIN "VA_MetaDataItem" ON "PlaylistItem_MetaDataItem"."MetaDataItem_Id" = "VA_MetaDataItem"."Id"
	WHERE "PlaylistItem_MetaDataItem"."PlaylistItem_Id" = "AlbumArtist"."Id"
)
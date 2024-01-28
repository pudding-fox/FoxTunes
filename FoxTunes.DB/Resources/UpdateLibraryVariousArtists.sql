INSERT INTO "MetaDataItems" ("Name", "Type", "NumericValue")
SELECT @name, @type, @numericValue
WHERE NOT EXISTS(SELECT * FROM "MetaDataItems" WHERE "Name" = @name AND "Type" = @type AND "NumericValue" = @numericValue);

WITH "MetaData"
AS
(
	SELECT 
		"LibraryItem_MetaDataItem"."LibraryItem_Id" AS "Id",
		"MetaDataItems"."Name",
		"MetaDataItems"."NumericValue",
		"MetaDataItems"."TextValue"
	FROM "LibraryItem_MetaDataItem" 
		JOIN "MetaDataItems" 
			ON "MetaDataItems"."Id" = "LibraryItem_MetaDataItem"."MetaDataItem_Id"
),

"AlbumArtist"
AS
(
	SELECT "AlbumArtist"."Id", "AlbumArtist"."DirectoryName", "Album", CASE WHEN "FirstAlbumArtist" IS NOT NULL THEN "FirstAlbumArtist" ELSE "FirstArtist" END AS "Artist"
	FROM
	(
		SELECT "LibraryItems".*,
		(
			SELECT "TextValue"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "LibraryItems"."Id" AND "MetaData"."Name" = 'Album'
		) AS "Album",
		(
			SELECT "TextValue"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "LibraryItems"."Id" AND "MetaData"."Name" = 'FirstAlbumArtist'
		) AS "FirstAlbumArtist",
		(
			SELECT "TextValue"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "LibraryItems"."Id" AND "MetaData"."Name" = 'FirstArtist'
		) AS "FirstArtist"
		FROM "LibraryItems"
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
	
INSERT INTO "LibraryItem_MetaDataItem" ("LibraryItem_Id", "MetaDataItem_Id")
SELECT "AlbumArtist"."Id", (SELECT "Id" FROM "VA_MetaDataItem")
FROM "AlbumArtist"
JOIN "VariousArtists" ON "AlbumArtist"."DirectoryName" = "VariousArtists"."DirectoryName"
	AND "AlbumArtist"."Album" = "VariousArtists"."Album"
WHERE NOT EXISTS(
	SELECT * 
	FROM "LibraryItem_MetaDataItem"
	JOIN "VA_MetaDataItem" ON "LibraryItem_MetaDataItem"."MetaDataItem_Id" = "VA_MetaDataItem"."Id"
	WHERE "LibraryItem_MetaDataItem"."LibraryItem_Id" = "AlbumArtist"."Id"
)
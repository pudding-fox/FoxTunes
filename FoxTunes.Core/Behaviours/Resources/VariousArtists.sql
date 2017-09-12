WITH "MetaData"
AS
(
	SELECT 
		"LibraryItem_MetaDataItem"."LibraryItem_Id" AS "Id",
		"MetaDataItems"."Name",
		"MetaDataItems"."TextValue" AS "Value"
	FROM "LibraryItem_MetaDataItem" 
		JOIN "MetaDataItems" 
			ON "MetaDataItems"."Id" = "LibraryItem_MetaDataItem"."MetaDataItem_Id"
),

"AlbumArtist"
AS
(
	SELECT 
		"LibraryItems".*, 
		(
			SELECT "Value"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "LibraryItems"."Id" AND "MetaData"."Name" = 'Album'
		) AS "Album",
		(
			SELECT "Value"
			FROM "MetaData"
			WHERE "MetaData"."Id" = "LibraryItems"."Id" AND "MetaData"."Name" = 'FirstArtist'
		) AS "FirstArtist"
	FROM "LibraryItems"
),

"VariousArtists" AS
(
	SELECT "DirectoryName", "Album", COUNT(DISTINCT "FirstArtist") AS "ArtistCount"
	FROM "AlbumArtist"
	GROUP BY "DirectoryName", "Album"
	HAVING "ArtistCount" > 1
)
	

SELECT "AlbumArtist".* 
FROM "AlbumArtist"
JOIN "VariousArtists" ON "AlbumArtist".DirectoryName = "VariousArtists".DirectoryName 
	AND "AlbumArtist".Album = "VariousArtists".Album 
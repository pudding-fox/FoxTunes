WITH 
"ImageItems_Lookup" AS
(
	SELECT *
	FROM "ImageItems" 
	WHERE "FileName" = @fileName AND "ImageType" = @imageType 
)

INSERT INTO "ImageItems" ("FileName", "ImageType") 
SELECT @fileName, @imageType
WHERE NOT EXISTS(SELECT * FROM "ImageItems_Lookup");

WITH 
"ImageItems_Lookup" AS
(
	SELECT *
	FROM "ImageItems" 
	WHERE "FileName" = @fileName AND "ImageType" = @imageType 
),

"{0}Item_ImageItem_Lookup" AS 
(
	SELECT "{0}Item_ImageItem".*
	FROM "{0}Item_ImageItem"
		JOIN "ImageItems_Lookup" 
			ON "{0}Item_ImageItem"."ImageItem_Id" = "ImageItems_Lookup"."Id"
	WHERE "{0}Item_ImageItem"."{0}Item_Id" = @itemId
)

INSERT INTO "{0}Item_ImageItem" ("{0}Item_Id", "ImageItem_Id")
SELECT @itemId, "Id"
FROM "ImageItems_Lookup"
WHERE NOT EXISTS(SELECT * FROM "{0}Item_ImageItem_Lookup");
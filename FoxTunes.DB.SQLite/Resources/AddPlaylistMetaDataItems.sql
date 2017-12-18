WITH 
"MetaDataItems_Lookup" AS
(
	SELECT *
	FROM "MetaDataItems" 
	WHERE "Name" = @name AND "Type" = @type
		AND (("NumericValue" IS NULL AND @numericValue IS NULL) OR "NumericValue" = @numericValue)
		AND (("TextValue" IS NULL AND @textValue IS NULL) OR "TextValue" = @textValue) 
		AND (("FileValue" IS NULL AND @fileValue IS NULL) OR "FileValue" = @fileValue)
)

INSERT INTO "MetaDataItems" ("Name", "Type", "NumericValue", "TextValue", "FileValue") 
SELECT @name, @type, @numericValue, @textValue, @fileValue
WHERE NOT EXISTS(SELECT * FROM "MetaDataItems_Lookup");

WITH 
"MetaDataItems_Lookup" AS
(
	SELECT *
	FROM "MetaDataItems" 
	WHERE "Name" = @name AND "Type" = @type
		AND (("NumericValue" IS NULL AND @numericValue IS NULL) OR "NumericValue" = @numericValue)
		AND (("TextValue" IS NULL AND @textValue IS NULL) OR "TextValue" = @textValue) 
		AND (("FileValue" IS NULL AND @fileValue IS NULL) OR "FileValue" = @fileValue)
),

"PlaylistItem_MetaDataItem_Lookup" AS 
(
	SELECT "PlaylistItem_MetaDataItem".*
	FROM "PlaylistItem_MetaDataItem"
		JOIN "MetaDataItems_Lookup" 
			ON "PlaylistItem_MetaDataItem"."MetaDataItem_Id" = "MetaDataItems_Lookup"."Id"
	WHERE "PlaylistItem_MetaDataItem"."PlaylistItem_Id" = @itemId
)

INSERT INTO "PlaylistItem_MetaDataItem" ("PlaylistItem_Id", "MetaDataItem_Id")
SELECT @itemId, "Id"
FROM "MetaDataItems_Lookup"
WHERE NOT EXISTS(SELECT * FROM "PlaylistItem_MetaDataItem_Lookup");
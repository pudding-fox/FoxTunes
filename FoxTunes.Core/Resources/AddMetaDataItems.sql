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

"{0}Item_MetaDataItem_Lookup" AS 
(
	SELECT "{0}Item_MetaDataItem".*
	FROM "{0}Item_MetaDataItem"
		JOIN "MetaDataItems_Lookup" 
			ON "{0}Item_MetaDataItem"."MetaDataItem_Id" = "MetaDataItems_Lookup"."Id"
	WHERE "{0}Item_MetaDataItem"."{0}Item_Id" = @itemId
)

INSERT INTO "{0}Item_MetaDataItem" ("{0}Item_Id", "MetaDataItem_Id")
SELECT @itemId, "Id"
FROM "MetaDataItems_Lookup"
WHERE NOT EXISTS(SELECT * FROM "{0}Item_MetaDataItem_Lookup");
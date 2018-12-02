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

"LibraryItem_MetaDataItem_Lookup" AS 
(
	SELECT "LibraryItem_MetaDataItem".*
	FROM "LibraryItem_MetaDataItem"
		JOIN "MetaDataItems_Lookup" 
			ON "LibraryItem_MetaDataItem"."MetaDataItem_Id" = "MetaDataItems_Lookup"."Id"
	WHERE "LibraryItem_MetaDataItem"."LibraryItem_Id" = @itemId
)

INSERT INTO "LibraryItem_MetaDataItem" ("LibraryItem_Id", "MetaDataItem_Id")
SELECT @itemId, "Id"
FROM "MetaDataItems_Lookup"
WHERE NOT EXISTS(SELECT * FROM "LibraryItem_MetaDataItem_Lookup");
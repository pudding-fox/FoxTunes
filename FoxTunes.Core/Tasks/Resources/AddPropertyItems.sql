WITH 
"PropertyItems_Lookup" AS
(
	SELECT *
	FROM "PropertyItems" 
	WHERE "Name" = @name 
		AND (("NumericValue" IS NULL AND @numericValue IS NULL) OR "NumericValue" = @numericValue)
		AND (("TextValue" IS NULL AND @textValue IS NULL) OR "TextValue" = @textValue) 
)

INSERT INTO "PropertyItems" ("Name", "NumericValue", "TextValue") 
SELECT @name, @numericValue, @textValue
WHERE NOT EXISTS(SELECT * FROM "PropertyItems_Lookup");

WITH 
"PropertyItems_Lookup" AS
(
	SELECT *
	FROM "PropertyItems" 
	WHERE "Name" = @name 
		AND (("NumericValue" IS NULL AND @numericValue IS NULL) OR "NumericValue" = @numericValue)
		AND (("TextValue" IS NULL AND @textValue IS NULL) OR "TextValue" = @textValue) 
),

"{0}Item_PropertyItem_Lookup" AS 
(
	SELECT "{0}Item_PropertyItem".*
	FROM "{0}Item_PropertyItem"
		JOIN "PropertyItems_Lookup" 
			ON "{0}Item_PropertyItem"."PropertyItem_Id" = "PropertyItems_Lookup"."Id"
	WHERE "{0}Item_PropertyItem"."{0}Item_Id" = @itemId
)

INSERT INTO "{0}Item_PropertyItem" ("{0}Item_Id", "PropertyItem_Id")
SELECT @itemId, "Id"
FROM "PropertyItems_Lookup"
WHERE NOT EXISTS(SELECT * FROM "{0}Item_PropertyItem_Lookup");
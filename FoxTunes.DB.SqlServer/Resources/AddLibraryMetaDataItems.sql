WITH 
"MetaDataItems_Lookup" AS
(
	SELECT *
	FROM "MetaDataItems" 
	WHERE "Name" = @name AND "Type" = @type AND "Value" = @value
)

INSERT INTO "MetaDataItems" ("Name", "Type", "Value") 
SELECT @name, @type, @value
WHERE NOT EXISTS(SELECT * FROM "MetaDataItems_Lookup");

WITH 
"MetaDataItems_Lookup" AS
(
	SELECT *
	FROM "MetaDataItems" 
	WHERE "Name" = @name AND "Type" = @type AND "Value" = @value
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
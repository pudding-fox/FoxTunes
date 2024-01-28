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
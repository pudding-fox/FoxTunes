INSERT OR IGNORE INTO "MetaDataItems" ("Name", "Type", "NumericValue", "TextValue", "FileValue") 
SELECT @name, @type, @numericValue, @textValue, @fileValue;

INSERT OR IGNORE INTO "LibraryItem_MetaDataItem" ("LibraryItem_Id", "MetaDataItem_Id")
SELECT @itemId, last_insert_rowid();
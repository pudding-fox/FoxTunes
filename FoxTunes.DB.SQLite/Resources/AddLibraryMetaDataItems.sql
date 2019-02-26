INSERT OR IGNORE INTO "MetaDataItems" ("Name", "Type", "Value") 
SELECT @name, @type, @value;

INSERT OR IGNORE INTO "LibraryItem_MetaDataItem" ("LibraryItem_Id", "MetaDataItem_Id")
SELECT @itemId, last_insert_rowid();
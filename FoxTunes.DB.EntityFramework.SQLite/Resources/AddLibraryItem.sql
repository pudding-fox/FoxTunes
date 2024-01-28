INSERT INTO "LibraryItems" ("DirectoryName", "FileName", "Status") 
SELECT @directoryName, @fileName, @status
WHERE NOT EXISTS(SELECT * FROM "LibraryItems" WHERE "FileName" = @fileName)
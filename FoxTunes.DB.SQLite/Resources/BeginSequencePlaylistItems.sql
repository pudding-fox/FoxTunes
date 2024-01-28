CREATE TEMPORARY TABLE IF NOT EXISTS "PlaylistSort"
(
	"PlaylistItem_Id" INTEGER NOT NULL, 
	"Value1" text NULL,
	"Value2" text NULL,
	"Value3" text NULL,
	"Value4" text NULL,
	"Value5" text NULL,
	"Value6" text NULL,
	"Value7" text NULL,
	"Value8" text NULL,
	"Value9" text NULL,
	"Value10" text NULL
);

CREATE TEMPORARY TABLE IF NOT EXISTS "PlaylistSequence"
(
	"Id" INTEGER PRIMARY KEY NOT NULL,
	"PlaylistItem_Id" INTEGER NOT NULL
);

DELETE FROM "PlaylistSort";
DELETE FROM "PlaylistSequence";
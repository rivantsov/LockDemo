CREATE SCHEMA lck
  AUTHORIZATION postgres;

CREATE TABLE lck."DocHeader" (
  "DocName" character varying(5) NOT NULL,
  "Total" integer,
  CONSTRAINT "PK_DocName" PRIMARY KEY ("DocName")
)
WITH (
  OIDS=FALSE
);
ALTER TABLE lck."DocHeader"
  OWNER TO postgres;

CREATE TABLE lck."DocDetail" (
  "DocName" character varying(5) NOT NULL,
  "Name" character varying(5) NOT NULL,
  "Value" integer,
  CONSTRAINT "PK_DocDetail" PRIMARY KEY ("DocName", "Name"),
  CONSTRAINT "FK_DocName" FOREIGN KEY ("DocName")
      REFERENCES lck."DocHeader" ("DocName") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE lck."DocDetail"
  OWNER TO postgres;


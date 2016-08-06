
CREATE DATABASE IF NOT EXISTS "lck";
USE "lck";


CREATE TABLE IF NOT EXISTS "docheader" (
  "DocName" varchar(5) NOT NULL,
  "Total" int(11) NOT NULL,
  PRIMARY KEY ("DocName")
) ENGINE=InnoDB DEFAULT CHARSET=latin1;


CREATE TABLE IF NOT EXISTS "docdetail" (
  "DocName" varchar(5) NOT NULL,
  "Name" varchar(5) NOT NULL,
  "Value" int(11) NOT NULL,
  PRIMARY KEY ("DocName","Name"),
  CONSTRAINT "FK_DocHeader" FOREIGN KEY ("DocName") REFERENCES "docheader" ("DocName") 
      ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=latin1;



﻿
--------------------------------------------------------
--  DDL for Table DOCHEADER
--------------------------------------------------------

CREATE TABLE "SYSTEM"."DOCHEADER" 
   (	"DOCNAME" VARCHAR2(5 BYTE), 
	"TOTAL" NUMBER
   ) PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 
 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM" ;
--------------------------------------------------------
--  DDL for Index DOCHEADER_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYSTEM"."DOCHEADER_PK" ON "SYSTEM"."DOCHEADER" ("DOCNAME") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM" ;
--------------------------------------------------------
--  Constraints for Table DOCHEADER
--------------------------------------------------------

  ALTER TABLE "SYSTEM"."DOCHEADER" ADD CONSTRAINT "DOCHEADER_PK" PRIMARY KEY ("DOCNAME")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM"  ENABLE;
  ALTER TABLE "SYSTEM"."DOCHEADER" MODIFY ("TOTAL" NOT NULL ENABLE);
  ALTER TABLE "SYSTEM"."DOCHEADER" MODIFY ("DOCNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Ref Constraints for Table DOCHEADER
--------------------------------------------------------

  ALTER TABLE "SYSTEM"."DOCHEADER" ADD CONSTRAINT "FK_DOCHEADER" FOREIGN KEY ("DOCNAME")
	  REFERENCES "SYSTEM"."DOCHEADER" ("DOCNAME") ENABLE;






--------------------------------------------------------
--  DDL for Table DOCDETAIL
--------------------------------------------------------

CREATE TABLE "SYSTEM"."DOCDETAIL" 
   (	"DOCNAME" VARCHAR2(5 BYTE), 
	"NAME" VARCHAR2(5 BYTE), 
	"VALUE" NUMBER
   ) PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 
 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM" ;
--------------------------------------------------------
--  DDL for Index DOCDETAIL_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYSTEM"."DOCDETAIL_PK" ON "SYSTEM"."DOCDETAIL" ("DOCNAME", "NAME") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM" ;
--------------------------------------------------------
--  Constraints for Table DOCDETAIL
--------------------------------------------------------

  ALTER TABLE "SYSTEM"."DOCDETAIL" ADD CONSTRAINT "DOCDETAIL_PK" PRIMARY KEY ("DOCNAME", "NAME")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM"  ENABLE;
  ALTER TABLE "SYSTEM"."DOCDETAIL" MODIFY ("VALUE" NOT NULL ENABLE);
  ALTER TABLE "SYSTEM"."DOCDETAIL" MODIFY ("NAME" NOT NULL ENABLE);
  ALTER TABLE "SYSTEM"."DOCDETAIL" MODIFY ("DOCNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Ref Constraints for Table DOCDETAIL
--------------------------------------------------------

  ALTER TABLE "SYSTEM"."DOCDETAIL" ADD CONSTRAINT "FK_DOCDETAIL_HEADER" FOREIGN KEY ("DOCNAME")
	  REFERENCES "SYSTEM"."DOCHEADER" ("DOCNAME") ENABLE;
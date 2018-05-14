CREATE TABLE "__BuildMaster_DbSchemaChanges" (
  "Numeric_Release_Number" NUMBER NOT NULL,
  "Script_Id" INTEGER NOT NULL,
  "Script_Sequence" INTEGER NOT NULL,

  "Script_Name" NVARCHAR2(50) NOT NULL,
  "Executed_Date" DATE NOT NULL,
  "Success_Indicator" CHAR(1) NOT NULL,

  CONSTRAINT "__BldMstr_DbSchemaChangesPK"
    PRIMARY KEY ("Numeric_Release_Number", "Script_Id", "Script_Sequence")
)

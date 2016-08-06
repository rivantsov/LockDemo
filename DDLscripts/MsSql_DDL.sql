-- Create database LockTest on your SQL Server and run the following script


ALTER DATABASE LockTest SET ALLOW_SNAPSHOT_ISOLATION ON;

-- To use on existing databases, you might need to switch to single user before setting this option
-- Otherwise the command might hang forever
--   ALTER DATABASE LockTest SET SINGLE_USER WITH ROLLBACK IMMEDIATE; -- switch to single-user and close all other connections
ALTER DATABASE LockTest SET READ_COMMITTED_SNAPSHOT ON;
--   ALTER DATABASE LockTest SET MULTI_USER;  -- back to multi-user

Use LockTest;

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[DocHeader](
	[DocName] [nvarchar](5) NOT NULL,
	[Total] [int] NOT NULL,
PRIMARY KEY NONCLUSTERED 
(
	[DocName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[DocDetail](
	[DocName] [nvarchar](5) NOT NULL,
	[Name] [nchar](5) NOT NULL,
	[Value] [int] NOT NULL,
 CONSTRAINT [PK_DocDetail] PRIMARY KEY NONCLUSTERED 
(
	[DocName] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[DocDetail]  WITH CHECK ADD  CONSTRAINT [FK_DocDetailHeader] FOREIGN KEY([DocName])
REFERENCES [dbo].[DocHeader] ([DocName])
GO

ALTER TABLE [dbo].[DocDetail] CHECK CONSTRAINT [FK_DocDetailHeader]
GO

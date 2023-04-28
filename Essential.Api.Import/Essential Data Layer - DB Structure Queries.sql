

--Information Representations (Databases)
SELECT	[Name] + ' Database' AS [Name],
		'SQL Database. ' + CHAR(10) + CHAR(13) +
		'Server Instance: ' + @@SERVERNAME + CHAR(10) + CHAR(13) +
		'SQL Version: ' + @@VERSION + CHAR(10) + CHAR(13) +
		'Compatibility level: ' + CAST([compatibility_level] AS VARCHAR) + CHAR(10) + CHAR(13) +
		'Collation: ' + collation_name + CHAR(10) + CHAR(13) +
		'State: ' + [state_desc] + CHAR(10) + CHAR(13) +
		'Recovery Mode: ' + recovery_model_desc COLLATE SQL_Latin1_General_CP1_CI_AS + CHAR(10) + CHAR(13)
		AS [Description]
FROM	SYS.databases
WHERE	[Name] NOT IN ('master', 'tempdb', 'model', 'msdb', 'ReportServer', 'ReportServerTempDB')
GO

--Data Representation (Tables)
SET NOCOUNT ON 
DECLARE @Tables TABLE
(
	[Name] NVARCHAR(100),
	[Technical Name] NVARCHAR(255),
	[Description] NVARCHAR(255),
	[Information Representation] NVARCHAR(100)
)
DECLARE @tableQuery NVARCHAR(400)
SET @tableQuery = 'SELECT	[TABLE_NAME] AS [Name], ' +
				  '[TABLE_SCHEMA] + ''.'' + [TABLE_NAME] AS [Technical Name], ' +
				  ''''' AS [Description], ' +
				  'TABLE_CATALOG + '' Database'' AS [Information Representation] ' +
				  'FROM	[?].INFORMATION_SCHEMA.TABLES ' +
				  'WHERE	[TABLE_CATALOG] NOT IN (''master'', ''tempdb'', ''model'', ''msdb'', ''ReportServer'', ''ReportServerTempDB'') ' +
				  'AND		[TABLE_TYPE] != ''VIEW''	 '

INSERT INTO @Tables (Name, [Technical Name], [Description], [Information Representation])
EXEC sp_msforeachdb @tableQuery
SET NOCOUNT OFF
SELECT * FROM @Tables ORDER BY 1
GO	

--Data Representation Attributes (Columns)
SET NOCOUNT ON 
DECLARE @Columns TABLE
(
	[Name] NVARCHAR(100),
	[Technical Name] NVARCHAR(255),
	[Description] NVARCHAR(255),
	[Data Representation] NVARCHAR(100),
	[Information Representation] NVARCHAR(100)
)
DECLARE @ColumnQuery NVARCHAR(1000)
SET @ColumnQuery = 'SELECT	C.[COLUMN_NAME] AS [Name], ' + 
							'C.[TABLE_SCHEMA] + ''.'' + C.[TABLE_NAME] + ''.'' + C.[COLUMN_NAME] AS [Technical Name], ' + 
							''''' AS [Description], ' + 
							'C.[TABLE_NAME] + '' Database'' AS [Information Representation], ' + 
							'C.TABLE_CATALOG + '' Database'' AS [Information Representation] ' +  
					'FROM	[?].INFORMATION_SCHEMA.COLUMNS C ' + 
							'INNER JOIN [?].INFORMATION_SCHEMA.TABLES T ON [C].[TABLE_NAME] = [T].[TABLE_NAME] ' +  
					'WHERE	T.[TABLE_TYPE] != ''VIEW'' ' + 
					'AND	T.[TABLE_CATALOG] NOT IN (''master'', ''tempdb'', ''model'', ''msdb'', ''ReportServer'', ''ReportServerTempDB'') '

INSERT INTO @Columns (Name, [Technical Name], [Description], [Data Representation], [Information Representation])
EXEC sp_msforeachdb @ColumnQuery
SET NOCOUNT OFF
SELECT * FROM @Columns ORDER BY 1
GO


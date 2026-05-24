/***********************************************************************************************************************
* Database
***********************************************************************************************************************/
-- Drop DB
--USE tempdb;
--go
--DECLARE @SQL nvarchar(1000);
--IF EXISTS (SELECT 1 FROM sys.databases WHERE [name] = N'CASCETracker')
--BEGIN
--    SET @SQL = N'USE [CASCETracker];

--                 ALTER DATABASE CASCETracker SET SINGLE_USER WITH ROLLBACK IMMEDIATE; -- disconnect all other users
--                 use [tempdb];

--                 DROP DATABASE CASCETracker;';
--    EXEC (@SQL);
--END;

---- Create DB
--create database CASCETracker;
--use CASCETracker;
--go

/***********************************************************************************************************************
 * Schemas
 **********************************************************************************************************************/

--if not exists (select * from sys.schemas where name = 'core')
--exec('create schema core')

/***********************************************************************************************************************
 * Tables
 **********************************************************************************************************************/

/*****************
* User
*****************/
if not exists (select * from dbo.sysobjects where ID=object_id(N'core.User') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
create table core.[User]
(
  -- primary key
  Id int not null identity(1,1)

  -- data
  ,FirstName nvarchar(128) not null default('')
  ,LastName nvarchar(128) not null default('')
  ,Email nvarchar(128) not null default('')
  ,UserName nvarchar(128) not null default('')
  ,[Password] nvarchar(256) not null default('')
  ,AccountStatus int not null default(0)
  ,IsDeleted bit not null default(0)

  ,Constraint PK_User_Id Primary Key Clustered (Id)
  ,Constraint AK_UserName UNIQUE(UserName)
)
GO

/*****************
* UserHistory
*****************/
if not exists (select * from dbo.sysobjects where ID=object_id(N'core.UserHistory') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
create table core.[UserHistory]
(
  -- primary key
  Uniqueifier int not null identity(1,1)

  -- data
  ,Id int not null default(0)
  ,FirstName nvarchar(128) not null default('')
  ,LastName nvarchar(128) not null default('')
  ,Email nvarchar(128) not null default('')
  ,UserName nvarchar(128) not null default('')
  ,[Password] nvarchar(256) not null default('')
  ,AccountStatus int not null default(0)
  ,UpdateDate DATETIMEOFFSET not null
  ,UpdateUserId int not null default(0)
  ,IsDeleted bit not null default(0)

  ,Constraint PK_Uniqueifier Primary Key Clustered (Uniqueifier)
)
GO

/*****************
* AccountStatus
*****************/
if not exists (select * from dbo.sysobjects where ID=object_id(N'core.AccountStatus') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
create table core.[AccountStatus]
(
    -- primary key
    AccountStatusId int not null identity(10,10)

    -- data
    ,[Description] nvarchar(20)

  ,Constraint PK_AccountStatus_Id Primary Key Clustered (AccountStatusId)
  ,Constraint AK_AccountStatus_Desc UNIQUE([Description])

)
GO

/****************
* RefreshToken
****************/
if not exists (select * from dbo.sysobjects where ID=object_id(N'core.RefreshToken') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
create table core.[RefreshToken]
(
    -- primary key
    Id int not null identity(1,1)

    -- data
    ,Token nvarchar(MAX)
    ,Expires DATETIMEOFFSET
    ,Revoked bit not null default(1)
    ,UserName nvarchar(128)

  ,Constraint PK_RefreshToken_Id Primary Key Clustered (Id)
)
GO

/****************
* Role
****************/
if not exists (select * from dbo.sysobjects where ID=object_id(N'core.Role') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
create table core.[Role]
(
    -- primary key
    RoleId int not null identity(1,1)

    -- data
    ,RoleName nvarchar(128)

  ,Constraint PK_Role_Id Primary Key Clustered (RoleId)
)
GO

/****************
* UserRole
****************/
if not exists (select * from dbo.sysobjects where ID=object_id(N'core.UserRole') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
create table core.[UserRole]
(
    UserId int
    ,RoleId int

    ,Constraint PK_UserRole Primary Key Clustered (UserId, RoleId)
    ,Constraint FK_UserRole_UserId Foreign Key (UserId) References core.[User](Id)
    ,Constraint FK_UserRole_RoleId Foreign Key (RoleId) References core.[Role](RoleId)
)
GO

/****************
* VerificationCode
****************/
if not exists (select * from dbo.sysobjects where ID=object_id(N'core.VerificationCode') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
create table core.[VerificationCode]
(
    -- primary key
    Id int not null identity(1,1)

    -- data
    ,Code nvarchar(20) not null
    ,UserId int not null
    ,ExpirationDate DATETIMEOFFSET not null

  ,Constraint PK_VerificationCode_Id Primary Key Clustered (Id)
)
GO

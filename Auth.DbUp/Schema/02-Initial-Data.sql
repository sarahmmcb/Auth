USE CASCETracker;
GO

/****
* AccountStatus
****/
if not exists (select 1 from core.AccountStatus where [Description]='Active') insert into core.AccountStatus values ('Active')
if not exists (select 1 from core.AccountStatus where [Description]='Closed') insert into core.AccountStatus values ('Closed')
if not exists (select 1 from core.AccountStatus where [Description]='Locked') insert into core.AccountStatus values ('Locked')
if not exists (select 1 from core.AccountStatus where [Description]='Blocked') insert into core.AccountStatus values ('Blocked')
if not exists (select 1 from core.AccountStatus where [Description]='Unknown') insert into core.AccountStatus values ('Unknown')
GO

/*****
* Test Users
*****/

if not exists (select 1	from core.[User] where UserName=N'sideshow.bob@simpsons.org')
insert into core.[User] values
(
	'Bob'
	,'Terwilliger'
	,'sideshow.bob@simpsons.org'
	,'sideshow.bob@simpsons.org'
	,'$2a$11$yIyAzG2qqVpGYr4vvW5Rhu0zAQBlo3GuxUsc/gyyvoqIOBC98A91W' -- BobsPassword
	,(Select AccountStatusId from core.AccountStatus where [Description]=N'Active')
	,0
)

if not exists (select 1	from core.[User] where UserName=N'lovejoy.helen@simpsons.org')
insert into core.[User] values
(
	'Helen'
	,'Lovejoy'
	,'lovejoy.helen@simpsons.org'
	,'lovejoy.helen@simpsons.org'
	,'$2a$11$jlxgg3huqAkKy9vN6enwred6wUvH.9B6LVTZWCLBZbcCmgvnXaCri' -- HelensPassword
	,(Select AccountStatusId from core.AccountStatus where [Description]=N'Active')
	,0
)
GO

/*****
* Roles
******/
if not exists (select 1 from core.[Role] where RoleName=N'User') insert into core.[Role] values ('User')
if not exists (select 1 from core.[Role] where RoleName=N'Admin') insert into core.[Role] values ('Admin')
GO

/*****
* User Roles
*****/
if not exists (select 1 from core.UserRole where UserId=1 and RoleId=2) insert into core.UserRole values (1,2) -- Give Bob Admin Privs
if not exists (select 1 from core.UserRole where UserId=1 and RoleId=1) insert into core.UserRole values (1,1)
if not exists (select 1 from core.UserRole where UserId=2 and RoleId=1) insert into core.UserRole values (2,1)
GO

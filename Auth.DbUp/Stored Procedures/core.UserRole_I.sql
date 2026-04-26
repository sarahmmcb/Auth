
IF OBJECT_ID('core.UserRole_I', 'P') IS NOT NULL  
   DROP PROCEDURE core.UserRole_I;  
GO 

create procedure core.UserRole_I
	@UserId int
	,@RoleId int
as

BEGIN
INSERT INTO
	core.UserRole
VALUES
(
	@UserId
	,@RoleId
)

END
GO

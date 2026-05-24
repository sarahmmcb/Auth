
IF OBJECT_ID('core.User_By_UserName_S', 'P') IS NOT NULL  
   DROP PROCEDURE core.User_By_UserName_S;  
GO 

create procedure core.User_By_UserName_S
	@UserName NVARCHAR(128)
as

begin
  select
	u.Id
	,u.FirstName
	,u.LastName
	,u.Email
	,u.UserName
	,u.[Password]
	,u.AccountStatus
	,r.RoleId
	,r.RoleName
  from core.[User] u
  left join core.UserRole ur ON u.Id = ur.UserId
  left join core.[Role] r on r.RoleId = ur.RoleId
  where
	UserName = @UserName
end
GO
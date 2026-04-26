
IF OBJECT_ID('core.User_D', 'P') IS NOT NULL  
   DROP PROCEDURE core.User_D;  
GO 

create procedure core.User_D
	@UserId int
	,@UpdateUserId INT
as

begin

-- Soft delete bc other APIs that use this may have columns dependent on UserId
update
	core.[User]
set
	IsDeleted = 1
output
	@UserId
	,inserted.FirstName
	,inserted.LastName
	,inserted.Email
	,inserted.UserName
	,inserted.[Password]
	,inserted.AccountStatus
	,GETDATE()
	,@UpdateUserId
	,1
into
	core.UserHistory
where
	Id = @UserId
end

GO;

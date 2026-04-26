IF OBJECT_ID('core.User_Password_U', 'P') IS NOT NULL  
   DROP PROCEDURE core.User_Password_U;  
GO 

create procedure core.User_Password_U
	@UserId int
	,@Password varchar(256)
	,@UpdateUserId int
as

begin
	update
		core.[User]
	set
		[Password] = @Password
	output
		@UserId
		,inserted.FirstName
		,inserted.LastName
		,inserted.Email
		,inserted.UserName
		,@Password
		,inserted.AccountStatus
		,GETDATE()
		,@UpdateUserId
		,inserted.IsDeleted
	into
		core.UserHistory
	where
		Id = @UserId	

end

GO;

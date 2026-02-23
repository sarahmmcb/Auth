create procedure core.User_By_UserName_S
	@UserName varchar
as

begin
	select
	  Id
	  ,FirstName
	  ,LastName
	  ,Email
	  ,UserName
	  ,[Password]
	  ,AccountStatus
	  ,CreateDate
	from core.[User]
	where 
	  UserName = @UserName
		
end

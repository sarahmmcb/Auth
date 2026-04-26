IF OBJECT_ID('core.User_By_UserId_U_I', 'P') IS NOT NULL  
   DROP PROCEDURE core.User_By_UserId_U_I;  
GO 

create procedure core.User_By_UserId_U_I
	@UserId INT
	,@FirstName NVARCHAR(128)
	,@LastName NVARCHAR(128)
	,@Email NVARCHAR(128)
	,@UserName NVARCHAR(128)
	,@Password NVARCHAR(256)
	,@AccountStatus INT
	,@UpdateUserId INT
	,@IsDeleted BIT
as

DECLARE @UpdateDate DATETIMEOFFSET = SYSDATETIMEOFFSET()

IF (@UserId IS NULL OR @UserId = 0)
begin
  DECLARE @user_output TABLE
  (
	 Id INT
	,FirstName NVARCHAR(128)
	,LastName NVARCHAR(128)
	,Email NVARCHAR(128)
	,UserName NVARCHAR(128)
	,[Password] NVARCHAR(256)
	,AccountStatus INT
	,UpdateDate DATETIMEOFFSET
	,UpdateUserId INT
	,IsDeleted BIT
  );

  INSERT INTO
	core.[User]
  OUTPUT
	 inserted.Id
	,inserted.FirstName
	,inserted.LastName
	,inserted.Email
	,inserted.UserName
	,inserted.[Password]
	,inserted.AccountStatus
	,@UpdateDate
	,@UpdateUserId
	,inserted.IsDeleted
  INTO
    @user_output
  VALUES
  (
	@FirstName
	,@LastName
	,@Email
	,@UserName
	,@Password
	,@AccountStatus
	,@UpdateDate
	,0
  )

  INSERT INTO
    core.UserHistory
	SELECT * FROM @user_output

  SELECT Id from @user_output

  DELETE @user_output

end
begin
  UPDATE
	core.[User]
  SET
	FirstName = @FirstName
	,LastName = @LastName
	,Email = @Email
	,UserName = @UserName
	,AccountStatus = @AccountStatus
  OUTPUT
	 @UserId
	,inserted.FirstName
	,inserted.LastName
	,inserted.Email
	,inserted.UserName
	,@Password
	,inserted.AccountStatus
	,@UpdateDate
	,@UpdateUserId
	,@IsDeleted
  INTO
	core.UserHistory
  WHERE
	Id = @UserId

  SELECT @UserId
end
GO;
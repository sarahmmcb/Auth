IF OBJECT_ID('core.RefreshToken_RevokeAll_ByUserName', 'P') IS NOT NULL
	DROP PROCEDURE core.RefreshToken_RevokeAll_ByUserName
GO;

CREATE PROCEDURE core.RefreshToken_RevokeAll_ByUserName
 @UserName NVARCHAR(128)
AS

Update
 core.RefreshToken
set
Revoked = 1
where
 Id in (
	Select Id
	From
	 core.RefreshToken
	 where
	 UserName = @UserName
)



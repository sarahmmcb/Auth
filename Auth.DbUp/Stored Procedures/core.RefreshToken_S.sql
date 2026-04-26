
IF OBJECT_ID('core.RefreshToken_S', 'P') IS NOT NULL
	DROP PROCEDURE core.RefreshToken_S
GO;

CREATE PROCEDURE core.RefreshToken_S
	@UserName NVARCHAR(128)
AS

select
	 Id
	,Token
	,Expires
	,Revoked
	,UserName
from
	core.RefreshToken
where
	UserName = @UserName

GO;


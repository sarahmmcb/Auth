
IF OBJECT_ID('core.RefreshToken_I', 'P') IS NOT NULL
	DROP PROCEDURE core.RefreshToken_I;
GO;

create procedure core.RefreshToken_I
	 @Token NVARCHAR(MAX)
	,@Expires DATETIMEOFFSET
	,@Revoked BIT
	,@UserName NVARCHAR(128)
AS

insert into
	core.RefreshToken
values
(
	 @Token
	,@Expires
	,@Revoked
	,@UserName
)
GO;

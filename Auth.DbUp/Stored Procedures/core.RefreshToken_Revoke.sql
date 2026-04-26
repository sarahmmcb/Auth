
IF OBJECT_ID('core.RefreshToken_Revoke', 'P') IS NOT NULL
	DROP PROCEDURE core.RefreshToken_Revoke;
GO;

CREATE PROCEDURE core.RefreshToken_Revoke
	@Id INT
AS

update
	core.RefreshToken
set
	Revoked = 1
where
	Id = @Id

GO;
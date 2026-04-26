IF OBJECT_ID('core.VerificationCode_S', 'P') IS NOT NULL  
   DROP PROCEDURE core.VerificationCode_S;  
GO

create procedure core.VerificationCode_S
	@UserId INT
	,@Code NVARCHAR(20)
AS

SELECT
	 Id
	,Code
	,UserId
	,ExpirationDate
FROM core.VerificationCode
WHERE
	UserId = @UserId
	AND
	Code = @Code
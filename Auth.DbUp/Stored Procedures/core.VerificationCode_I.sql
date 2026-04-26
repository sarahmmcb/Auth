IF OBJECT_ID('core.VerificationCode_I', 'P') IS NOT NULL  
   DROP PROCEDURE core.VerificationCode_I;  
GO

create procedure core.VerificationCode_I
	 @Code NVARCHAR(20)
	,@UserId INT
	,@ExpirationDate DATETIMEOFFSET
AS

INSERT INTO
	core.VerificationCode
VALUES
(
	 @Code
	,@UserId
	,@ExpirationDate
)

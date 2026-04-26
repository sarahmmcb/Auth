IF OBJECT_ID('core.VerificationCode_D', 'P') IS NOT NULL  
   DROP PROCEDURE core.VerificationCode_D;  
GO

create procedure core.VerificationCode_D
	@Id INT
AS

DELETE
FROM core.VerificationCode
WHERE
	Id = @Id

GO;
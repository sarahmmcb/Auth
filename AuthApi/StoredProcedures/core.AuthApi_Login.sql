-- AUTHAPI role for other domains to use to 
-- execute SPs in the Auth domain
IF NOT EXISTS
    (SELECT 1
     FROM sys.database_principals
     WHERE name='AUTHAPI_EXECROLE'
     and type_desc='DATABASE_ROLE')
BEGIN
    CREATE ROLE [AUTHAPI_EXECROLE];
END
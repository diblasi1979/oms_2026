IF DB_ID('oms_test') IS NULL
BEGIN
    CREATE DATABASE oms_test;
END;
GO

USE oms_test;
GO

IF SUSER_ID('api_oms') IS NULL
BEGIN
    PRINT 'El login api_oms no existe en la instancia. Debe crearse antes de continuar.';
END;
GO

IF SUSER_ID('api_oms') IS NOT NULL AND DATABASE_PRINCIPAL_ID('api_oms') IS NULL
BEGIN
    CREATE USER [api_oms] FOR LOGIN [api_oms];
END;
GO

IF DATABASE_PRINCIPAL_ID('api_oms') IS NOT NULL
BEGIN
    ALTER ROLE db_owner ADD MEMBER [api_oms];
END;
GO

USE [DespensaDB]

Go 
-- ========================================
-- STORED PROCEDURES FOR Despensa AI Tables
-- ========================================

-- USERS
GO
DROP PROCEDURE IF EXISTS InsertUser;
GO

CREATE PROCEDURE InsertUser
    @Nombre NVARCHAR(100),
    @CorreoElectronico NVARCHAR(100),
    @Password NVARCHAR(255),
    @CodigoVerificacion NVARCHAR(10),
    @ID_USUARIO INT OUTPUT,
    @ErrorId INT OUTPUT,
    @ErrorMensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @ID_USUARIO = 0;
    SET @ErrorId = 0;
    SET @ErrorMensaje = '';

    -- Verificar si el correo ya existe
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @CorreoElectronico)
    BEGIN
        SET @ErrorId = 201; -- Código para "Correo ya registrado"
        SET @ErrorMensaje = 'Ya existe un usuario registrado con este correo electrónico.';
        RETURN;
    END

    -- Insertar usuario
    BEGIN TRY
        INSERT INTO Users (FullName, Email, PasswordHash, VerificationCode, IsActive)
        VALUES (@Nombre, @CorreoElectronico, @Password, @CodigoVerificacion, 0);

        SET @ID_USUARIO = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        SET @ErrorId = 101; -- Código genérico de error de base de datos
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE PROCEDURE UpdateUser
    @UserID INT,
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100)
AS
BEGIN
    UPDATE Users
    SET FullName = @FullName, Email = @Email
    WHERE UserID = @UserID
END;
GO

CREATE PROCEDURE GetUsers
AS
BEGIN
    SELECT * FROM Users
END;
GO

-- PRODUCTS
CREATE PROCEDURE InsertProduct
    @Name NVARCHAR(100),
    @CategoryID INT,
    @Unit NVARCHAR(50),
    @UserID INT,
    @Quantity DECIMAL(10,2),
    @ExpirationDate DATE,
    @ProductID INT OUTPUT,
    @ErrorId INT OUTPUT,
    @ErrorMensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET @ErrorId = 0;
    SET @ErrorMensaje = '';
    SET @ProductID = 0;

    BEGIN TRY
        -- Buscar si el producto ya existe
        SELECT TOP 1 @ProductID = ProductID
        FROM Products
        WHERE Name = @Name AND CategoryID = @CategoryID AND Unit = @Unit;

        -- Si no existe, insertarlo
        IF @ProductID = 0
        BEGIN
            INSERT INTO Products (Name, CategoryID, Unit)
            VALUES (@Name, @CategoryID, @Unit);

            SET @ProductID = SCOPE_IDENTITY();
        END

        -- Llamar al SP que ya existe para insertar en el inventario
        EXEC InsertUserInventory
            @UserID = @UserID,
            @ProductID = @ProductID,
            @Quantity = @Quantity,
            @ExpirationDate = @ExpirationDate;
    END TRY
    BEGIN CATCH
        SET @ErrorId = 101;
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE PROCEDURE UpdateProduct
    @ProductID INT,
    @Name NVARCHAR(100),
    @CategoryID INT,
    @Unit NVARCHAR(50)
AS
BEGIN
    UPDATE Products
    SET Name = @Name, CategoryID = @CategoryID, Unit = @Unit
    WHERE ProductID = @ProductID
END;
GO

CREATE PROCEDURE GetProducts
AS
BEGIN
    SELECT * FROM Products
END;
GO

-- USER INVENTORY
CREATE PROCEDURE InsertUserInventory
    @UserID INT,
    @ProductID INT,
    @Quantity DECIMAL(10,2),
    @ExpirationDate DATE
AS
BEGIN
    INSERT INTO UserInventory (UserID, ProductID, Quantity, ExpirationDate)
    VALUES (@UserID, @ProductID, @Quantity, @ExpirationDate)
END;
GO

CREATE PROCEDURE UpdateUserInventory
    @InventoryID INT,
    @Quantity DECIMAL(10,2),
    @ExpirationDate DATE
AS
BEGIN
    UPDATE UserInventory
    SET Quantity = @Quantity, ExpirationDate = @ExpirationDate
    WHERE InventoryID = @InventoryID
END;
GO

CREATE PROCEDURE GetUserInventory
AS
BEGIN
    SELECT * FROM UserInventory
END;
GO

-- PURCHASES
CREATE PROCEDURE InsertPurchase
    @UserID INT,
    @PurchaseDate DATETIME,
    @TotalAmount DECIMAL(10,2)
AS
BEGIN
    INSERT INTO Purchases (UserID, PurchaseDate, TotalAmount)
    VALUES (@UserID, @PurchaseDate, @TotalAmount)
END;
GO

CREATE PROCEDURE UpdatePurchase
    @PurchaseID INT,
    @TotalAmount DECIMAL(10,2)
AS
BEGIN
    UPDATE Purchases
    SET TotalAmount = @TotalAmount
    WHERE PurchaseID = @PurchaseID
END;
GO

CREATE PROCEDURE GetPurchases
AS
BEGIN
    SELECT * FROM Purchases
END;
GO

-- Crear sesión
CREATE PROCEDURE CreateUserSession
    @UserID INT,
    @Token NVARCHAR(255),
    @ExpiresAt DATETIME,
    @Device NVARCHAR(100) = NULL,
    @IPAddress NVARCHAR(50) = NULL
AS
BEGIN
    INSERT INTO UserSessions (UserID, Token, ExpiresAt, Device, IPAddress)
    VALUES (@UserID, @Token, @ExpiresAt, @Device, @IPAddress)
END;
GO

-- Validar token
CREATE PROCEDURE GetActiveSession
    @Token NVARCHAR(255)
AS
BEGIN
    SELECT * FROM UserSessions
    WHERE Token = @Token AND IsActive = 1 AND ExpiresAt > GETDATE()
END;
GO

-- Cerrar sesión
CREATE PROCEDURE InvalidateSession
    @Token NVARCHAR(255)
AS
BEGIN
    UPDATE UserSessions SET IsActive = 0 WHERE Token = @Token
END;
GO


CREATE PROCEDURE CleanupExpiredSessions
AS
BEGIN
    DELETE FROM UserSessions
    WHERE IsActive = 0 AND ExpiresAt < DATEADD(DAY, -7, GETDATE());
END;
GO

CREATE PROCEDURE Login
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SELECT 
        UserID AS ID_USUARIO,
        FullName AS NOMBRE,
        Email AS CORREO_ELECTRONICO
    FROM Users
    WHERE Email = @Email AND PasswordHash = @PasswordHash
END;
GO
CREATE PROCEDURE GetUserById
    @UserID INT
AS
BEGIN
    SELECT 
        UserID AS ID_USUARIO,
        FullName AS NOMBRE,
        Email AS CORREO_ELECTRONICO
    FROM Users
    WHERE UserID = @UserID
END;
GO



CREATE PROCEDURE ActiveUser
    @Correo NVARCHAR(100),
    @Codigo NVARCHAR(10),
    @ID_USUARIO INT OUTPUT,
    @ErrorId INT OUTPUT,
    @ErrorMensaje NVARCHAR(255) OUTPUT,
    @FilasAfectadas INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @ErrorId = 0;
    SET @ErrorMensaje = '';
    SET @FilasAfectadas = 0;
    SET @ID_USUARIO = 0;

    BEGIN TRY
        UPDATE Users
        SET IsActive = 1
        WHERE Email = @Correo AND VerificationCode = @Codigo;

        SET @FilasAfectadas = @@ROWCOUNT;

        IF @FilasAfectadas = 0
        BEGIN
            SET @ErrorId = 302; -- UsuarioNoActivado
            SET @ErrorMensaje = 'Correo o código inválido.';
        END
        ELSE
        BEGIN
            SELECT @ID_USUARIO = UserID FROM Users WHERE Email = @Correo;
        END
    END TRY
    BEGIN CATCH
        SET @ErrorId = 101; -- Error de base de datos
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END;
GO


ALTER PROCEDURE InsertUser
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @VerificationCode NVARCHAR(10)
AS
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, VerificationCode)
    VALUES (@FullName, @Email, @PasswordHash, @VerificationCode)
END;
GO

CREATE PROCEDURE CloseUserSession
    @Token NVARCHAR(255),
    @ErrorId INT OUTPUT,
    @ErrorMensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET @ErrorId = 0;
    SET @ErrorMensaje = '';

    BEGIN TRY
        DELETE FROM UserSessions WHERE Token = @Token;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ErrorId = 301; -- Token no encontrado
            SET @ErrorMensaje = 'No se encontró una sesión con ese token.';
        END
    END TRY
    BEGIN CATCH
        SET @ErrorId = 101;
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END;
GO



DELETE  FROM Users

SELECT * FROM Users
SELECT* FROM UserSessions

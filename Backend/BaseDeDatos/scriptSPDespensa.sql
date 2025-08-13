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
        SET @ErrorId = 201; -- C�digo para "Correo ya registrado"
        SET @ErrorMensaje = 'Ya existe un usuario registrado con este correo electr�nico.';
        RETURN;
    END

    -- Insertar usuario
    BEGIN TRY
        INSERT INTO Users (FullName, Email, PasswordHash, VerificationCode, IsActive)
        VALUES (@Nombre, @CorreoElectronico, @Password, @CodigoVerificacion, 0);

        SET @ID_USUARIO = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        SET @ErrorId = 101; -- C�digo gen�rico de error de base de datos
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END;
GO
----------------------------------------------------------------------------------------------------
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
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE GetUsers
AS
BEGIN
    SELECT * FROM Users
END;
GO

-- PRODUCTS--------------------------------------------------------------
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
----------------------------------------------------------------------------------------------------

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
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE GetProducts
AS
BEGIN
    SELECT * FROM Products
END;
GO

-- USER INVENTORY------------------------------------------------------------------------------------

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
----------------------------------------------------------------------------------------------------

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
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE GetUserInventory
AS
BEGIN
    SELECT * FROM UserInventory
END;
GO

-- PURCHASES-----------------------------------------------------------------------------------------

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
----------------------------------------------------------------------------------------------------

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
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE GetPurchases
AS
BEGIN
    SELECT * FROM Purchases
END;
GO

-- Crear sesi�n------------------------------------------------------------------------------------

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

-- Validar token----------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE GetActiveSession
  @Token NVARCHAR(255)
AS
BEGIN
  SET NOCOUNT ON;
  SELECT *
  FROM UserSessions
  WHERE Token = @Token
    AND IsActive = 1
    AND ExpiresAt > GETUTCDATE();
END;
GO

-- Cerrar sesi�n----------------------------------------------------------------------------------

CREATE PROCEDURE InvalidateSession
    @Token NVARCHAR(255)
AS
BEGIN
    UPDATE UserSessions SET IsActive = 0 WHERE Token = @Token
END;
GO
----------------------------------------------------------------------------------------------------


CREATE PROCEDURE CleanupExpiredSessions
AS
BEGIN
    DELETE FROM UserSessions
    WHERE IsActive = 0 AND ExpiresAt < DATEADD(DAY, -7, GETDATE());
END;
GO
----------------------------------------------------------------------------------------------------

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
----------------------------------------------------------------------------------------------------

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
----------------------------------------------------------------------------------------------------



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
            SET @ErrorMensaje = 'Correo o c�digo inv�lido.';
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

----------------------------------------------------------------------------------------------------


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
            SET @ErrorMensaje = 'No se encontr� una sesi�n con ese token.';
        END
    END TRY
    BEGIN CATCH
        SET @ErrorId = 101;
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END;
GO
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE InsertNotification
    @UserID INT,
    @Message NVARCHAR(255),
    @Type NVARCHAR(50)
AS
BEGIN
    INSERT INTO Notifications (UserID, Message, Type)
    VALUES (@UserID, @Message, @Type)
END;
GO
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE GetUserNotifications
    @UserID INT
AS
BEGIN
    SELECT *
    FROM Notifications
    WHERE UserID = @UserID
    ORDER BY SentAt DESC
END;
GO
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE MarkNotificationAsRead
    @NotificationID INT
AS
BEGIN
    UPDATE Notifications
    SET IsRead = 1
    WHERE NotificationID = @NotificationID
END;
GO

----------------------------------------------------------------------------------------------------


IF OBJECT_ID('GetProductosInventarioUsuario', 'P') IS NOT NULL
    DROP PROCEDURE GetProductosInventarioUsuario;
GO
----------------------------------------------------------------------------------------------------

CREATE PROCEDURE GetProductosInventarioUsuario
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.ProductID,
        p.Name AS nombre,
        p.CategoryID AS idCategoria,
        p.Unit AS unidad,
        ui.UserID AS userID,
        ui.Quantity AS quantity,
        ui.ExpirationDate AS expirationDate
    FROM UserInventory ui
    INNER JOIN Products p ON ui.ProductID = p.ProductID
    WHERE ui.UserID = @UserID AND ui.Quantity > 0
END;
GO
----------------------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE SP_RegistrarRecetaYActualizarInventario
    @UserID INT,
    @Name NVARCHAR(100),
    @Description NVARCHAR(MAX),
    @PreparationTime INT,
    @Difficulty NVARCHAR(50),
    @Calories INT,
    @Style NVARCHAR(50),
    @IngredientesJson NVARCHAR(MAX),
    @RecipeID INT OUTPUT,
    @ErrorId INT OUTPUT,
    @ErrorMensaje NVARCHAR(500) OUTPUT
AS
BEGIN
    SET FMTONLY OFF;  -- <-- clave para que el diseñador pueda leer el SP
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF (@IngredientesJson IS NULL OR LTRIM(RTRIM(@IngredientesJson)) = '' OR ISJSON(@IngredientesJson) <> 1
            OR NOT EXISTS (SELECT 1 FROM OPENJSON(@IngredientesJson)))
        BEGIN
            ROLLBACK;
            SET @ErrorId = 208;
            SET @ErrorMensaje = 'No se proporcionaron ingredientes válidos.';
            RETURN;
        END

        INSERT INTO Recipes (Name, Description, PreparationTime, Difficulty, Calories, Style)
        VALUES (@Name, @Description, @PreparationTime, @Difficulty, @Calories, @Style);

        SET @RecipeID = SCOPE_IDENTITY();

        CREATE TABLE #Ingredientes (ProductID INT, Quantity DECIMAL(18,2));

        INSERT INTO #Ingredientes (ProductID, Quantity)
        SELECT ProductID, Quantity
        FROM OPENJSON(@IngredientesJson)
        WITH (ProductID INT '$.ProductID', Quantity DECIMAL(18,2) '$.Quantity');

        IF EXISTS (
            SELECT 1
            FROM #Ingredientes I
            JOIN UserInventory UI ON UI.ProductID = I.ProductID
            WHERE UI.UserID = @UserID
              AND (UI.Quantity IS NULL OR UI.Quantity < I.Quantity)
        )
        BEGIN
            ROLLBACK;
            SET @ErrorId = 404;
            SET @ErrorMensaje = 'Inventario insuficiente para uno o más ingredientes.';
            RETURN;
        END

        INSERT INTO RecipeIngredients (RecipeID, ProductID, Quantity)
        SELECT @RecipeID, ProductID, Quantity FROM #Ingredientes;

        UPDATE UI
        SET UI.Quantity = UI.Quantity - I.Quantity
        FROM UserInventory UI
        JOIN #Ingredientes I ON UI.ProductID = I.ProductID
        WHERE UI.UserID = @UserID;

        IF EXISTS (SELECT 1 FROM UserInventory WHERE UserID = @UserID AND Quantity < 0)
        BEGIN
            ROLLBACK;
            SET @ErrorId = 404;
            SET @ErrorMensaje = 'Uno o más ingredientes superan el inventario disponible.';
            RETURN;
        END

        DROP TABLE #Ingredientes;

        COMMIT;
        SET @ErrorId = 0;
        SET @ErrorMensaje = '';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        SET @ErrorId = 101;
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END


GO 
----------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE GetUserByEmail
  @Email NVARCHAR(100)
AS
BEGIN
  SET NOCOUNT ON;
  SELECT 
    UserID       AS ID_USUARIO,
    FullName     AS NOMBRE,
    Email        AS CORREO_ELECTRONICO,
    IsActive     AS IS_ACTIVE,
    PasswordHash AS PASSWORD_HASH
  FROM Users
  WHERE Email = @Email;
END;
GO

-- Lista por usuario + búsqueda + paginación------------------------------------------------------
CREATE OR ALTER PROCEDURE GetProductsByUser
 @UserID   INT,
 @Q        NVARCHAR(100) = NULL,
 @Page     INT = 1,
 @PageSize INT = 20
AS
BEGIN
  SET NOCOUNT ON;

  WITH base AS (
    SELECT
      ui.InventoryID,
      ui.UserID,
      ui.ProductID,
      p.Name,
      p.CategoryID,
      p.Unit,
      ui.Quantity,
      ui.ExpirationDate,
      ui.CreatedAt
    FROM UserInventory ui
    INNER JOIN Products p ON p.ProductID = ui.ProductID
    WHERE ui.UserID = @UserID
      AND (@Q IS NULL OR p.Name LIKE '%' + @Q + '%')
  )
  SELECT *
  FROM base
  ORDER BY Name
  OFFSET (@Page-1)*@PageSize ROWS
  FETCH NEXT @PageSize ROWS ONLY;
END
GO
-------------------------------------------------------

CREATE OR ALTER PROCEDURE UpdateUserInventoryForUser
 @UserID        INT,
 @ProductID     INT,
 @Quantity      DECIMAL(10,2),
 @ExpirationDate DATE = NULL
AS
BEGIN
  SET NOCOUNT ON;

  UPDATE ui
  SET Quantity = @Quantity,
      ExpirationDate = @ExpirationDate
  FROM UserInventory ui
  WHERE ui.UserID = @UserID
    AND ui.ProductID = @ProductID;

  IF @@ROWCOUNT = 0
    RAISERROR('No existe inventario para ese producto/usuario.', 16, 1);
END
GO
--------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE UpdateProductForUser
 @ProductID  INT,
 @UserID     INT,
 @Name       NVARCHAR(100),
 @CategoryID INT,
 @Unit       NVARCHAR(50)
AS
BEGIN
  SET NOCOUNT ON;

  UPDATE p
  SET Name = @Name,
      CategoryID = @CategoryID,
      Unit = @Unit
  FROM Products p
  WHERE p.ProductID = @ProductID
    AND EXISTS (
      SELECT 1 FROM UserInventory ui
      WHERE ui.ProductID = p.ProductID AND ui.UserID = @UserID
    );

  IF @@ROWCOUNT = 0
    RAISERROR('Producto no pertenece al usuario o no existe.', 16, 1);
END
GO




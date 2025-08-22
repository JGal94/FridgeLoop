--USE [DespensaDB]

Go 
-- ========================================
-- STORED PROCEDURES FOR Despensa AI Tables
-- ========================================

-- USERS


CREATE OR ALTER PROCEDURE InsertUser
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
CREATE OR ALTER PROCEDURE UpdateUser
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

CREATE OR ALTER PROCEDURE GetUsers
AS
BEGIN
    SELECT * FROM Users
END;
GO

-- PRODUCTS--------------------------------------------------------------
CREATE OR ALTER PROCEDURE InsertProduct
    @Name           NVARCHAR(100),
    @CategoryID     INT,
    @Unit           NVARCHAR(50),
    @UserID         INT,
    @Quantity       DECIMAL(10,2),
    @ExpirationDate DATE,
    @ProductID      INT OUTPUT,
    @ErrorId        INT OUTPUT,
    @ErrorMensaje   NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @ErrorId = 0;
    SET @ErrorMensaje = '';
    SET @ProductID = 0;

    BEGIN TRY
        /* 1) Resolver ProductID (crear si no existe) */
        SELECT TOP (1) @ProductID = p.ProductID
        FROM Products p
        WHERE p.Name = @Name
          AND p.CategoryID = @CategoryID
          AND p.Unit = @Unit;

        IF @ProductID = 0
        BEGIN
            INSERT INTO Products (Name, CategoryID, Unit)
            VALUES (@Name, @CategoryID, @Unit);

            SET @ProductID = SCOPE_IDENTITY();
        END

        /* 2) UPSERT en UserInventory:
              - si existe (UserID,ProductID) => sumo cantidad
              - si no existe => inserto fila nueva
           Regla de fecha: si llega @ExpirationDate:
              - si la actual es NULL, pongo la nueva
              - si ambas tienen valor, me quedo con la MÁS PRÓXIMA (MIN), para ser conservador
              (Cambia a MAX si prefieres conservar la más lejana) */
        IF EXISTS (
            SELECT 1
            FROM UserInventory
            WHERE UserID = @UserID
              AND ProductID = @ProductID
        )
        BEGIN
            UPDATE ui
            SET Quantity = ISNULL(ui.Quantity, 0) + ISNULL(@Quantity, 0),
                ExpirationDate =
                    CASE
                        WHEN @ExpirationDate IS NULL THEN ui.ExpirationDate
                        WHEN ui.ExpirationDate IS NULL THEN @ExpirationDate
                        WHEN @ExpirationDate < ui.ExpirationDate THEN @ExpirationDate  -- MIN (más próxima)
                        ELSE ui.ExpirationDate
                    END
            FROM UserInventory ui
            WHERE ui.UserID = @UserID
              AND ui.ProductID = @ProductID;
        END
        ELSE
        BEGIN
            INSERT INTO UserInventory (UserID, ProductID, Quantity, ExpirationDate)
            VALUES (@UserID, @ProductID, @Quantity, @ExpirationDate);
        END
    END TRY
    BEGIN CATCH
        SET @ErrorId = 101;
        SET @ErrorMensaje = ERROR_MESSAGE();
    END CATCH
END;
GO
----------------------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE UpdateProduct
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

CREATE OR ALTER PROCEDURE GetProducts
AS
BEGIN
    SELECT * FROM Products
END;
GO

-- USER INVENTORY------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE InsertUserInventory
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

CREATE OR ALTER PROCEDURE UpdateUserInventory
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

CREATE OR ALTER PROCEDURE GetUserInventory
AS
BEGIN
    SELECT * FROM UserInventory
END;
GO

-- PURCHASES-----------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE InsertPurchase
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

CREATE OR ALTER PROCEDURE UpdatePurchase
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

CREATE OR ALTER PROCEDURE GetPurchases
AS
BEGIN
    SELECT * FROM Purchases
END;
GO

-- Crear sesi�n------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE CreateUserSession
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

CREATE OR ALTER PROCEDURE InvalidateSession
    @Token NVARCHAR(255)
AS
BEGIN
    UPDATE UserSessions SET IsActive = 0 WHERE Token = @Token
END;
GO
----------------------------------------------------------------------------------------------------


CREATE OR ALTER PROCEDURE CleanupExpiredSessions
AS
BEGIN
    DELETE FROM UserSessions
    WHERE IsActive = 0 AND ExpiresAt < DATEADD(DAY, -7, GETDATE());
END;
GO
----------------------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE Login
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

CREATE OR ALTER PROCEDURE GetUserById
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



CREATE OR ALTER PROCEDURE ActiveUser
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


CREATE OR ALTER PROCEDURE CloseUserSession
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


----------------------------------------------------------------------------------------------------

----------------------------------------------------------------------------------------------------


----------------------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE GetProductosInventarioUsuario
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
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE SP_Compras_Registrar
 @UserID        INT,
 @FechaCompra   DATETIME = NULL,
 @ItemsJson     NVARCHAR(MAX),      -- JSON: [{ProductID, Quantity, UnitPrice?, ExpirationDate?}]
 @PurchaseID    INT OUTPUT,
 @ErrorId       INT OUTPUT,
 @ErrorMensaje  NVARCHAR(500) OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  SET @ErrorId = 0; 
  SET @ErrorMensaje = N''; 
  SET @PurchaseID = 0;

  BEGIN TRY
    BEGIN TRAN;

    -- Validación básica de JSON
    IF (@ItemsJson IS NULL OR LTRIM(RTRIM(@ItemsJson)) = N'' OR ISJSON(@ItemsJson) <> 1
        OR NOT EXISTS (SELECT 1 FROM OPENJSON(@ItemsJson)))
    BEGIN
      RAISERROR(N'No se proporcionaron items válidos.', 16, 1);
    END

    -- Usamos tabla variable en lugar de CTE
    DECLARE @Items TABLE
    (
      ProductID      INT           NOT NULL,
      Quantity       DECIMAL(10,2) NOT NULL,
      UnitPrice      DECIMAL(10,2) NULL,
      ExpirationDate DATE          NULL
    );

    INSERT INTO @Items (ProductID, Quantity, UnitPrice, ExpirationDate)
    SELECT 
      CONVERT(INT,             JSON_VALUE([value], '$.ProductID')),
      CONVERT(DECIMAL(10,2),   JSON_VALUE([value], '$.Quantity')),
      TRY_CONVERT(DECIMAL(10,2), JSON_VALUE([value], '$.UnitPrice')),
      TRY_CONVERT(DATE,        JSON_VALUE([value], '$.ExpirationDate'))
    FROM OPENJSON(@ItemsJson);

    -- Validación de cantidades > 0
    IF EXISTS (SELECT 1 FROM @Items WHERE ProductID IS NULL OR Quantity IS NULL OR Quantity <= 0)
    BEGIN
      RAISERROR(N'Items con ProductID/cantidad inválidos.', 16, 1);
    END

    DECLARE @Fecha DATETIME = ISNULL(@FechaCompra, GETUTCDATE());
    DECLARE @Total DECIMAL(12,2) = (SELECT SUM(ISNULL(UnitPrice,0) * Quantity) FROM @Items);

    -- 1) Header
    INSERT INTO Purchases(UserID, PurchaseDate, TotalAmount)
    VALUES(@UserID, @Fecha, ISNULL(@Total, 0));

    SET @PurchaseID = SCOPE_IDENTITY();

    -- 2) Detalles
    INSERT INTO PurchaseDetails(PurchaseID, ProductID, Quantity, UnitPrice)
    SELECT @PurchaseID, ProductID, Quantity, UnitPrice
    FROM @Items;

    -- 3) Upsert inventario del usuario
    MERGE UserInventory AS tgt
    USING @Items AS src
      ON (tgt.UserID = @UserID AND tgt.ProductID = src.ProductID)
    WHEN MATCHED THEN
      UPDATE SET 
        Quantity = ISNULL(tgt.Quantity,0) + ISNULL(src.Quantity,0),
        ExpirationDate =
          CASE
            WHEN src.ExpirationDate IS NULL THEN tgt.ExpirationDate
            WHEN tgt.ExpirationDate IS NULL THEN src.ExpirationDate
            WHEN src.ExpirationDate < tgt.ExpirationDate THEN src.ExpirationDate  -- más próxima
            ELSE tgt.ExpirationDate
          END
    WHEN NOT MATCHED THEN
      INSERT(UserID, ProductID, Quantity, ExpirationDate)
      VALUES(@UserID, src.ProductID, src.Quantity, src.ExpirationDate);

    COMMIT;
    SET @ErrorId = 0; 
    SET @ErrorMensaje = N'';
  END TRY
  BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    SET @ErrorId = 101;
    SET @ErrorMensaje = ERROR_MESSAGE();
  END CATCH
END
GO
-----------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE SP_Compras_ObtenerPorUsuario
 @UserID   INT,
 @Page     INT = 1,
 @PageSize INT = 20,
 @Desde    DATETIME = NULL,
 @Hasta    DATETIME = NULL
AS
BEGIN
  SET NOCOUNT ON;

  ;WITH base AS (
    SELECT p.PurchaseID, p.PurchaseDate, p.TotalAmount,
           Items = ISNULL(SUM(d.Quantity), 0)
    FROM Purchases p
    LEFT JOIN PurchaseDetails d ON d.PurchaseID = p.PurchaseID
    WHERE p.UserID = @UserID
      AND (@Desde IS NULL OR p.PurchaseDate >= @Desde)
      AND (@Hasta IS NULL OR p.PurchaseDate <  DATEADD(DAY, 1, @Hasta))
    GROUP BY p.PurchaseID, p.PurchaseDate, p.TotalAmount
  )
  SELECT *
  FROM base
  ORDER BY PurchaseDate DESC, PurchaseID DESC
  OFFSET (@Page-1)*@PageSize ROWS
  FETCH NEXT @PageSize ROWS ONLY;
END
GO
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE SP_Compras_ObtenerDetalle
  @UserID     INT,
  @PurchaseID INT
AS
BEGIN
  SET NOCOUNT ON;

  -- Encabezado
  SELECT 
      p.PurchaseID,
      p.PurchaseDate,
      p.TotalAmount,
      p.Notas
  FROM Purchases p
  WHERE p.PurchaseID = @PurchaseID
    AND p.UserID = @UserID;

  -- Items
  SELECT 
      d.PurchaseDetailID,
      d.ProductID,
      pr.Name       AS ProductName,
      pr.CategoryID,
      pr.Unit,
      d.Quantity,
      d.UnitPrice
  FROM PurchaseDetails d
  JOIN Products pr ON pr.ProductID = d.ProductID
  WHERE d.PurchaseID = @PurchaseID;
END;
GO

----------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE SP_Compras_Eliminar
 @UserID             INT,
 @PurchaseID         INT,
 @RevertirInventario BIT,
 @ErrorId            INT OUTPUT,
 @ErrorMensaje       NVARCHAR(500) OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @ErrorId = 0; SET @ErrorMensaje = '';

  BEGIN TRY
    BEGIN TRAN;

    -- Verifica pertenencia
    IF NOT EXISTS (SELECT 1 FROM Purchases WHERE PurchaseID=@PurchaseID AND UserID=@UserID)
    BEGIN
      ROLLBACK;
      SET @ErrorId = 400;   -- Producto/Entidad no encontrada (usa el que prefieras)
      SET @ErrorMensaje = 'Compra no encontrada para este usuario.';
      RETURN;
    END

    IF (@RevertirInventario = 1)
    BEGIN
      ;WITH det AS (
        SELECT d.ProductID, SUM(d.Quantity) AS Qty
        FROM PurchaseDetails d
        WHERE d.PurchaseID = @PurchaseID
        GROUP BY d.ProductID
      )
      UPDATE ui
      SET ui.Quantity = CASE 
                          WHEN ui.Quantity - det.Qty < 0 THEN 0 
                          ELSE ui.Quantity - det.Qty 
                        END
      FROM UserInventory ui
      JOIN det ON det.ProductID = ui.ProductID
      WHERE ui.UserID = @UserID;
    END

    DELETE FROM PurchaseDetails WHERE PurchaseID = @PurchaseID;
    DELETE FROM Purchases       WHERE PurchaseID = @PurchaseID;

    COMMIT;
    SET @ErrorId = 0; SET @ErrorMensaje = '';
  END TRY
  BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    SET @ErrorId = 101;
    SET @ErrorMensaje = ERROR_MESSAGE();
  END CATCH
END
GO
-----------------------------------------------------------
CREATE OR ALTER PROCEDURE SP_UserInventory_PorVencer
 @UserID             INT,
 @Dias               INT = 7,          -- ventana hacia adelante
 @IncluirVencidos    BIT = 0,          -- incluir vencidos (hacia atrás)
 @MaxDiasVencidos    INT = 7,          -- cuánto hacia atrás (si @IncluirVencidos=1)
 @Page               INT = 1,
 @PageSize           INT = 50
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @hoy DATE = CONVERT(date, GETUTCDATE());

  ;WITH base AS (
    SELECT 
      p.ProductID,
      p.Name,
      CategoryID    = ISNULL(p.CategoryID, 0),
      p.Unit,
      Quantity      = ISNULL(ui.Quantity, 0),
      ui.ExpirationDate,
      DiasRestantes = DATEDIFF(DAY, @hoy, ui.ExpirationDate)
    FROM UserInventory ui
    JOIN Products p ON p.ProductID = ui.ProductID
    WHERE ui.UserID = @UserID
      AND ISNULL(ui.Quantity,0) > 0
      AND ui.ExpirationDate IS NOT NULL
  )
  SELECT 
    ProductID, Name, CategoryID, Unit, Quantity, ExpirationDate, DiasRestantes
  FROM base
  WHERE
    (@IncluirVencidos = 0 AND DiasRestantes BETWEEN 0 AND @Dias)
    OR
    (@IncluirVencidos = 1 AND DiasRestantes BETWEEN -@MaxDiasVencidos AND @Dias)
  ORDER BY DiasRestantes ASC, ExpirationDate ASC, Name
  OFFSET (@Page-1)*@PageSize ROWS
  FETCH NEXT @PageSize ROWS ONLY;
END
GO

--------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_Compras_RegistrarDesdeItemsNominales
  @UserID         INT,
  @FechaCompra    DATETIME        = NULL,
  @ItemsJson      NVARCHAR(MAX),
  @PurchaseID     INT             OUTPUT,
  @ErrorId        INT             OUTPUT,
  @ErrorMensaje   NVARCHAR(500)   OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  SET @PurchaseID = 0;
  SET @ErrorId = 0;
  SET @ErrorMensaje = N'';

  BEGIN TRY
    IF @UserID IS NULL OR @UserID <= 0
      THROW 50001, 'UserID inválido.', 1;

    IF @ItemsJson IS NULL OR LTRIM(RTRIM(@ItemsJson)) = N'' OR ISJSON(@ItemsJson) <> 1
      THROW 50002, 'ItemsJson requerido.', 1;

    -- 1) Parseo JSON nominal
    DECLARE @ItemsNominal TABLE (
      Nombre           NVARCHAR(200)  NOT NULL,
      NombreKey        NVARCHAR(200)  NOT NULL,
      CategoryID       INT            NOT NULL,
      Unit             NVARCHAR(50)   NOT NULL,
      UnitKey          NVARCHAR(50)   NOT NULL,
      Quantity         DECIMAL(18,3)  NOT NULL,
      UnitPrice        DECIMAL(18,4)  NULL,
      ExpirationDate   DATE           NULL
    );

    INSERT INTO @ItemsNominal (Nombre, NombreKey, CategoryID, Unit, UnitKey, Quantity, UnitPrice, ExpirationDate)
    SELECT
      LTRIM(RTRIM(j.Nombre)),
      LOWER(LTRIM(RTRIM(j.Nombre))),
      j.idCategoria,
      LTRIM(RTRIM(j.unidad)),
      LOWER(LTRIM(RTRIM(j.unidad))),
      j.cantidad,
      j.precioUnitario,
      TRY_CONVERT(date, j.fechaExpiracion)
    FROM OPENJSON(@ItemsJson)
    WITH (
      Nombre           NVARCHAR(200)  '$.nombre',
      idCategoria      INT            '$.idCategoria',
      unidad           NVARCHAR(50)   '$.unidad',
      cantidad         DECIMAL(18,3)  '$.cantidad',
      precioUnitario   DECIMAL(18,4)  '$.precioUnitario',
      fechaExpiracion  NVARCHAR(30)   '$.fechaExpiracion'
    ) AS j;

    IF NOT EXISTS (SELECT 1 FROM @ItemsNominal)
      THROW 50003, 'ItemsJson vacío o inválido.', 1;

    IF EXISTS (SELECT 1 FROM @ItemsNominal WHERE Quantity <= 0 OR CategoryID <= 0 OR NombreKey = '' OR UnitKey = '')
      THROW 50004, 'Cada ítem debe traer nombre, idCategoria, unidad y cantidad > 0.', 1;

    -- 2) Resolver/crear Products
    DECLARE @DistinctNominal TABLE (NombreKey NVARCHAR(200), CategoryID INT, UnitKey NVARCHAR(50));
    INSERT INTO @DistinctNominal SELECT DISTINCT NombreKey, CategoryID, UnitKey FROM @ItemsNominal;

    DECLARE @NominalToProduct TABLE (NombreKey NVARCHAR(200), CategoryID INT, UnitKey NVARCHAR(50), ProductID INT);

    ;WITH ExistingProducts AS (
      SELECT p.ProductID,
             LOWER(LTRIM(RTRIM(p.Name))) AS NombreKey,
             p.CategoryID,
             LOWER(LTRIM(RTRIM(p.Unit))) AS UnitKey
      FROM dbo.Products p
    )
    MERGE dbo.Products AS tgt
    USING (
      SELECT dn.NombreKey, dn.CategoryID, dn.UnitKey
      FROM @DistinctNominal dn
      LEFT JOIN ExistingProducts ep
        ON ep.NombreKey = dn.NombreKey AND ep.CategoryID = dn.CategoryID AND ep.UnitKey = dn.UnitKey
      WHERE ep.ProductID IS NULL
    ) AS src
      ON 1=0
    WHEN NOT MATCHED THEN
      INSERT (Name, CategoryID, Unit)
      VALUES (src.NombreKey, src.CategoryID, src.UnitKey)
    OUTPUT inserted.Name, inserted.CategoryID, inserted.Unit, inserted.ProductID
      INTO @NominalToProduct(NombreKey, CategoryID, UnitKey, ProductID);

    INSERT INTO @NominalToProduct (NombreKey, CategoryID, UnitKey, ProductID)
    SELECT ep.NombreKey, ep.CategoryID, ep.UnitKey, ep.ProductID
    FROM @DistinctNominal dn
    JOIN (
      SELECT p.ProductID,
             LOWER(LTRIM(RTRIM(p.Name))) AS NombreKey,
             p.CategoryID,
             LOWER(LTRIM(RTRIM(p.Unit))) AS UnitKey
      FROM dbo.Products p
    ) ep
      ON ep.NombreKey = dn.NombreKey AND ep.CategoryID = dn.CategoryID AND ep.UnitKey = dn.UnitKey
    WHERE NOT EXISTS (
      SELECT 1 FROM @NominalToProduct m
      WHERE m.NombreKey = dn.NombreKey AND m.CategoryID = dn.CategoryID AND m.UnitKey = dn.UnitKey
    );

    -- 3) Consolidar por ProductID para inventario (no necesitamos precio aquí)
    DECLARE @ItemsByProduct TABLE (ProductID INT, Quantity DECIMAL(18,3), MinExpiration DATE NULL);

    INSERT INTO @ItemsByProduct(ProductID, Quantity, MinExpiration)
    SELECT m.ProductID,
           SUM(n.Quantity),
           MIN(n.ExpirationDate)
    FROM @ItemsNominal n
    JOIN @NominalToProduct m
      ON m.NombreKey = n.NombreKey AND m.CategoryID = n.CategoryID AND m.UnitKey = n.UnitKey
    GROUP BY m.ProductID;

    -- 4) Insertar Purchases (usa TotalAmount, no 'Total')
    DECLARE @Total DECIMAL(18,4) =
    ( SELECT SUM(ISNULL(n.UnitPrice,0) * n.Quantity) FROM @ItemsNominal n );

    INSERT INTO dbo.Purchases(UserID, PurchaseDate, TotalAmount)
    VALUES (@UserID, ISNULL(@FechaCompra, SYSUTCDATETIME()), ISNULL(@Total, 0));

    SET @PurchaseID = SCOPE_IDENTITY();

    -- 5) Insertar PurchaseDetails (SIN ExpirationDate porque no existe en la tabla)
    INSERT INTO dbo.PurchaseDetails (PurchaseID, ProductID, Quantity, UnitPrice)
    SELECT @PurchaseID,
           m.ProductID,
           n.Quantity,
           n.UnitPrice
    FROM @ItemsNominal n
    JOIN @NominalToProduct m
      ON m.NombreKey = n.NombreKey AND m.CategoryID = n.CategoryID AND m.UnitKey = n.UnitKey;

    -- 6) MERGE a UserInventory (usa MinExpiration)
    MERGE dbo.UserInventory AS tgt
    USING @ItemsByProduct AS src
      ON tgt.UserID = @UserID AND tgt.ProductID = src.ProductID
    WHEN MATCHED THEN
      UPDATE SET
        Quantity = ISNULL(tgt.Quantity, 0) + ISNULL(src.Quantity, 0),
        ExpirationDate =
          CASE
            WHEN src.MinExpiration IS NULL THEN tgt.ExpirationDate
            WHEN tgt.ExpirationDate IS NULL THEN src.MinExpiration
            WHEN src.MinExpiration <= tgt.ExpirationDate THEN src.MinExpiration
            ELSE tgt.ExpirationDate
          END
    WHEN NOT MATCHED THEN
      INSERT (UserID, ProductID, Quantity, ExpirationDate)
      VALUES (@UserID, src.ProductID, src.Quantity, src.MinExpiration);

    SET @ErrorId = 0;
    SET @ErrorMensaje = N'Compra registrada correctamente.';
    RETURN 0;

  END TRY
  BEGIN CATCH
    SET @ErrorId = ERROR_NUMBER();
    SET @ErrorMensaje = LEFT(ERROR_MESSAGE(), 500);
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    RETURN @ErrorId;
  END CATCH
END
GO
---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.GetUserSecurityById
  @UserID INT
AS
BEGIN
  SET NOCOUNT ON;

  SELECT 
      u.UserID,
      u.PasswordHash AS PASSWORD_HASH,
      u.IsActive     AS IS_ACTIVE
  FROM dbo.Users AS u WITH (NOLOCK)
  WHERE u.UserID = @UserID;
END
GO

-------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_CambiarPassword
  @UserID         INT,
  @NuevoHash      NVARCHAR(255),
  @ErrorId        INT           OUTPUT,
  @ErrorMensaje   NVARCHAR(500) OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  BEGIN TRY
    IF (@UserID IS NULL OR @UserID <= 0)
      THROW 50001, 'UserID inválido.', 1;

    IF (@NuevoHash IS NULL OR LTRIM(RTRIM(@NuevoHash)) = N'')
      THROW 50002, 'Hash de contraseña requerido.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @UserID AND IsActive = 1)
      THROW 50003, 'Usuario no encontrado o inactivo.', 1;

    UPDATE dbo.Users
      SET PasswordHash = @NuevoHash
    WHERE UserID = @UserID;

    SET @ErrorId = 0;
    SET @ErrorMensaje = N'Contraseña actualizada.';
    RETURN 0;
  END TRY
  BEGIN CATCH
    SET @ErrorId = ERROR_NUMBER();
    SET @ErrorMensaje = LEFT(ERROR_MESSAGE(), 500);
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    RETURN @ErrorId;
  END CATCH
END
GO

GO

------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_ActualizarNombre
  @UserID         INT,
  @NuevoNombre    NVARCHAR(100),
  @ErrorId        INT           OUTPUT,
  @ErrorMensaje   NVARCHAR(500) OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  BEGIN TRY
    IF (@UserID IS NULL OR @UserID <= 0)
      THROW 50011, 'UserID inválido.', 1;

    IF (@NuevoNombre IS NULL OR LTRIM(RTRIM(@NuevoNombre)) = N'')
      THROW 50012, 'El nombre no puede estar vacío.', 1;

    IF (LEN(@NuevoNombre) > 100)
      THROW 50013, 'El nombre supera el máximo permitido.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @UserID AND IsActive = 1)
      THROW 50014, 'Usuario no encontrado o inactivo.', 1;

    UPDATE dbo.Users
      SET FullName = @NuevoNombre
    WHERE UserID = @UserID;

    SET @ErrorId = 0;
    SET @ErrorMensaje = N'Nombre actualizado.';
    RETURN 0;
  END TRY
  BEGIN CATCH
    SET @ErrorId = ERROR_NUMBER();
    SET @ErrorMensaje = LEFT(ERROR_MESSAGE(), 500);
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    RETURN @ErrorId;
  END CATCH
END
GO










USE [DespensaDB]

Go 
-- ========================================
-- STORED PROCEDURES FOR Despensa AI Tables
-- ========================================

-- USERS
CREATE PROCEDURE InsertUser
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash)
    VALUES (@FullName, @Email, @PasswordHash)
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
    @Unit NVARCHAR(50)
AS
BEGIN
    INSERT INTO Products (Name, CategoryID, Unit)
    VALUES (@Name, @CategoryID, @Unit)
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


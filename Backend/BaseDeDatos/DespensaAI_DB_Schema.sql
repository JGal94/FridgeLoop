CREATE DATABASE [DespensaDB]
GO
USE [DespensaDB]

Go 
-- ================================
-- Despensa AI - SQL Server Schema
-- ================================

-- Users
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255),
    RegistrationDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE UserSessions (
    SessionID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    Token NVARCHAR(255), -- puede ser un JWT o token generado
    Device NVARCHAR(100), -- opcional: info del dispositivo o app
    IPAddress NVARCHAR(50), -- opcional: IP del usuario
    CreatedAt DATETIME DEFAULT GETDATE(),
    ExpiresAt DATETIME,
    IsActive BIT DEFAULT 1
);


-- Categories
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100)
);

-- Products
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100),
    CategoryID INT FOREIGN KEY REFERENCES Categories(CategoryID),
    Unit NVARCHAR(50) -- e.g., kg, units, liters
);

-- UserInventory
CREATE TABLE UserInventory (
    InventoryID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity DECIMAL(10,2),
    ExpirationDate DATE,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Recipes
CREATE TABLE Recipes (
    RecipeID INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100),
    Description NVARCHAR(MAX),
    PreparationTime INT,
    Difficulty NVARCHAR(50),
    Calories INT,
    Style NVARCHAR(50)
);

-- RecipeIngredients
CREATE TABLE RecipeIngredients (
    IngredientID INT PRIMARY KEY IDENTITY,
    RecipeID INT FOREIGN KEY REFERENCES Recipes(RecipeID),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity DECIMAL(10,2)
);

-- PreparedRecipes
CREATE TABLE PreparedRecipes (
    PreparedID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    RecipeID INT FOREIGN KEY REFERENCES Recipes(RecipeID),
    PreparedAt DATETIME DEFAULT GETDATE()
);

-- Purchases
CREATE TABLE Purchases (
    PurchaseID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    PurchaseDate DATETIME,
    TotalAmount DECIMAL(10,2)
);

-- PurchaseDetails
CREATE TABLE PurchaseDetails (
    DetailID INT PRIMARY KEY IDENTITY,
    PurchaseID INT FOREIGN KEY REFERENCES Purchases(PurchaseID),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity DECIMAL(10,2),
    UnitPrice DECIMAL(10,2)
);

-- Budgets
CREATE TABLE Budgets (
    BudgetID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    Month INT,
    Year INT,
    MaxAmount DECIMAL(10,2)
);

-- Notifications
CREATE TABLE Notifications (
    NotificationID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    Message NVARCHAR(255),
    Type NVARCHAR(50),
    IsRead BIT DEFAULT 0,
    SentAt DATETIME DEFAULT GETDATE()
);

-- AuditLog
CREATE TABLE AuditLog (
    AuditID INT PRIMARY KEY IDENTITY,
    TableName NVARCHAR(100),
    RecordID INT,
    Action NVARCHAR(50),
    ChangedBy INT FOREIGN KEY REFERENCES Users(UserID),
    ChangeDate DATETIME DEFAULT GETDATE(),
    PreviousData NVARCHAR(MAX),
    NewData NVARCHAR(MAX)
);

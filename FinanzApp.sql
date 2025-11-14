CREATE TABLE Personas (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(100) NOT NULL,
    Apellido        NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(200) NOT NULL UNIQUE,
    FechaAlta       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Activos (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Symbol          NVARCHAR(30) NOT NULL,    
    Nombre          NVARCHAR(200) NOT NULL,  
    Tipo            TINYINT NOT NULL,         -- 0 = AccionUSA,1 = Cedear,2 = Cripto,3 = Bono
    MonedaBase      CHAR(3) NOT NULL,         -- 'USD' o 'ARS'
    EsLocal         BIT NOT NULL DEFAULT 0    
);

CREATE UNIQUE INDEX UX_Activos_Symbol ON Activos(Symbol);

CREATE TABLE Operaciones (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PersonaId       INT NOT NULL,
    ActivoId        INT NOT NULL,

    Tipo            CHAR(10) NOT NULL,        
    Cantidad        DECIMAL(18,4) NOT NULL,   
    PrecioUnitario  DECIMAL(18,4) NOT NULL,   
    MonedaOperacion CHAR(3) NOT NULL,         
    FechaOperacion  DATETIME2 NOT NULL,       
    Comision        DECIMAL(18,4) NULL,      

    CONSTRAINT FK_Operaciones_Persona
        FOREIGN KEY (PersonaId) REFERENCES Personas(Id),

    CONSTRAINT FK_Operaciones_Activo
        FOREIGN KEY (ActivoId) REFERENCES Activos(Id)
);

CREATE TABLE Cotizaciones (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ActivoId        INT NOT NULL,
    Precio          DECIMAL(18,4) NOT NULL,
    Moneda          CHAR(3) NOT NULL,
    TimestampUtc    DATETIME2 NOT NULL,
    Source          NVARCHAR(100) NULL,

    CONSTRAINT FK_Cotizaciones_Activo
        FOREIGN KEY (ActivoId) REFERENCES Activos(Id)
);

CREATE TABLE CedearRatios (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    CedearId        INT NOT NULL,     
    UsAssetId       INT NOT NULL,     
    Ratio           DECIMAL(18,4) NOT NULL,  

    CONSTRAINT FK_CedearRatios_Cedear
        FOREIGN KEY (CedearId) REFERENCES Activos(Id),

    CONSTRAINT FK_CedearRatios_UsAsset
        FOREIGN KEY (UsAssetId) REFERENCES Activos(Id),
    
    CONSTRAINT UX_CedearRatios UNIQUE (CedearId, UsAssetId)
);

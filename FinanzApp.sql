CREATE TABLE Paises (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(200) NOT NULL,
    CodigoIso2      CHAR(2) NOT NULL,
    CodigoIso3      CHAR(3) NOT NULL,
    EsArgentina     BIT NOT NULL DEFAULT 0,

    CONSTRAINT UX_Paises_Iso2 UNIQUE (CodigoIso2),
    CONSTRAINT UX_Paises_Iso3 UNIQUE (CodigoIso3)
);

CREATE TABLE Provincias (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(200) NOT NULL,
    PaisId      INT NOT NULL,

    CONSTRAINT FK_Provincias_Pais
        FOREIGN KEY (PaisId) REFERENCES Paises(Id)
);

CREATE TABLE Localidades (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(200) NOT NULL,
    ProvinciaId INT NOT NULL,

    CONSTRAINT FK_Localidades_Provincia
        FOREIGN KEY (ProvinciaId) REFERENCES Provincias(Id)
);


CREATE TABLE Personas (
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    Nombre                  NVARCHAR(100) NOT NULL,
    Apellido                NVARCHAR(100) NOT NULL,
    Email                   NVARCHAR(200) NOT NULL UNIQUE,
    FechaAlta               DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FechaNacimiento         DATETIME2 NOT NULL,
    EsResidenteArgentina    BIT NOT NULL,

    NacionalidadId          INT NOT NULL,
    PaisResidenciaId        INT NULL,
    LocalidadResidenciaId   INT NULL,

    CONSTRAINT FK_Personas_PaisNacionalidad
        FOREIGN KEY (NacionalidadId) REFERENCES Paises(Id),

    CONSTRAINT FK_Personas_PaisResidencia
        FOREIGN KEY (PaisResidenciaId) REFERENCES Paises(Id),

    CONSTRAINT FK_Personas_LocalidadResidencia
        FOREIGN KEY (LocalidadResidenciaId) REFERENCES Localidades(Id)
);


CREATE UNIQUE INDEX IX_Personas_Email ON Personas(Email);

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

CREATE INDEX IX_Operaciones_PersonaId ON Operaciones(PersonaId);
CREATE INDEX IX_Operaciones_ActivoId   ON Operaciones(ActivoId);

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

CREATE INDEX IX_Cotizaciones_ActivoId ON Cotizaciones(ActivoId);

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

CREATE UNIQUE INDEX IX_CedearRatios_CedearId ON CedearRatios(CedearId);
CREATE UNIQUE INDEX IX_CedearRatios_UsAssetId ON CedearRatios(UsAssetId);

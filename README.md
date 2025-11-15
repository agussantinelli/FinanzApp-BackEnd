<h1>🛠️ FinanzApp - Backend</h1>

<div align="center">
    <a href="https://github.com/agussantinelli/FinanzApp-FrontEnd.git" target="_blank" style="text-decoration: none;">
        <img src="https://img.shields.io/badge/🚀%20Repo%20Frontend-Next.js-20232A?style=for-the-badge&logo=next.js&logoColor=white" alt="Frontend Repo Badge"/>
    </a>
    <a href="https://github.com/agussantinelli/FinanzApp-BackEnd.git" target="_blank" style="text-decoration: none;">
        <img src="https://img.shields.io/badge/⚙️%20Repo%20Backend%20(Estás%20Aquí)-ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="Backend Repo Badge"/>
    </a>
    <a href="https://github.com/agussantinelli" target="_blank" style="text-decoration: none;">
        <img src="https://img.shields.io/badge/👤%20Contacto-agussantinelli-000000?style=for-the-badge&logo=github&logoColor=white" alt="Contact Badge"/>
    </a>
</div>

<p align="center">
<img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET Badge"/>
<img src="https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=asp.net&logoColor=white" alt="ASP.NET Core Badge"/>
<img src="https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server Badge"/>
<img src="https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black" alt="Swagger Badge"/>
</p>

<hr>

<h2>🎯 Objetivo y Rol</h2>

<p>Este repositorio contiene la <strong>API REST</strong> de alto rendimiento construida en <strong>ASP.NET Core</strong> que actúa como el <em>motor de datos, lógica de negocio y persistencia</em> para toda la aplicación FinanzApp.</p>

<p>Su rol principal es:</p>
<ul>
    <li><strong>Agregación de Datos:</strong> Consumir, normalizar y cachear datos de múltiples APIs financieras externas (Binance, CoinGecko, CoinCap, DolarAPI, Yahoo Finance).</li>
    <li><strong>Lógica de Conversión:</strong> Aplicar la lógica compleja para la valuación y conversión de activos usando los tipos de cambio argentinos (MEP, CCL, Oficial, Blue).</li>
    <li><strong>Persistencia:</strong> Gestionar los portafolios de usuarios, históricos y cotizaciones en una base de datos relacional. Actualmente se utiliza <strong>SQL Server</strong>.</li>
    <li><strong>Seguridad:</strong> Implementar la autenticación y autorización (JWT, espacios personales).</li>
</ul>

<h2>⚙️ Stack Tecnológico</h2>

<table>
 <thead>
  <tr>
   <th>Componente</th>
   <th>Tecnología</th>
   <th>Notas</th>
  </tr>
 </thead>
 <tbody>
  <tr>
   <td><strong>Framework</strong></td>
   <td>ASP.NET Core</td>
   <td>Alto rendimiento para APIs REST.</td>
  </tr>
  <tr>
   <td><strong>Lenguaje</strong></td>
   <td>C#</td>
   <td>Foco en la arquitectura limpia y mantenible.</td>
  </tr>
  <tr>
   <td><strong>Base de Datos</strong></td>
   <td>SQL Server</td>
   <td>Base de datos relacional robusta. Actualmente el backend usa <strong>SQL Server</strong> con enfoque Code First.</td>
  </tr>
  <tr>
   <td><strong>ORM</strong></td>
   <td>Entity Framework Core</td>
   <td>
     Gestión de la base de datos a través de código (Code First).  
     Proveedor actual: <strong>SQL Server</strong>.  
     Contexto principal: <code>DBFinanzasContext</code> (proyecto <code>Data</code>).
   </td>
  </tr>
  <tr>
   <td><strong>Documentación</strong></td>
   <td>Swagger/OpenAPI</td>
   <td>Acceso a la documentación de los endpoints en <code>/swagger</code>.</td>
  </tr>
 </tbody>
</table>

<h2>🌐 Conexión con el Frontend</h2>

<p>Esta API es la fuente de datos para el <a href="https://github.com/agussantinelli/FinanzApp-FrontEnd.git">FinanzApp-FrontEnd</a>.</p>
<ul>
    <li><strong>Endpoint Base:</strong> Se expone en <code>https://localhost:7088</code> (por defecto) y es consumido por la variable <code>NEXT_PUBLIC_API_BASE</code> del frontend.</li>
    <li><strong>CORS:</strong> Debe estar configurado para aceptar peticiones desde el dominio del frontend (ej. <code>http://localhost:3000</code> para desarrollo local).</li>
</ul>

<hr>

<h2>🚀 Empezar (Setup Local)</h2>

<h3>1. Requisitos</h3>
<ul>
    <li><a href="https://dotnet.microsoft.com/download">.NET SDK</a> (recomendado: última versión LTS, por ejemplo .NET 8).</li>
    <li>Una instancia de base de datos relacional en ejecución:
        <ul>
            <li><strong>SQL Server</strong> (SQL Server Express, LocalDB o instancia local) – <em>configuración actualmente utilizada</em>.</li>
        </ul>
    </li>
</ul>

<hr/>

<h3>2. Configuración con SQL Server (implementación actual)</h3>

<p>Opciones típicas para desarrollo local:</p>
<ul>
    <li><strong>SQL Server Express</strong> (instancia <code>.\\SQLEXPRESS</code>).</li>
    <li><strong>LocalDB</strong> (instancia <code>(localdb)\\MSSQLLocalDB</code>).</li>
</ul>

<p>En el entorno actual se utiliza una base de datos llamada <code>FinanzAppDb</code>.</p>

<h4>Connection String (SQL Server)</h4>

<p>La <em>Connection String</em> se define en <code>appsettings.json</code> del proyecto WebAPI, por ejemplo:</p>

<pre><code>"ConnectionStrings": {
  "FinanzAppDb": "Server=.\\SQLEXPRESS;Database=FinanzAppDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Connect Timeout=60;"
}
</code></pre>

<p>Si usás LocalDB, podés utilizar algo como:</p>

<pre><code>"ConnectionStrings": {
  "FinanzAppDb": "Server=(localdb)\\MSSQLLocalDB;Database=FinanzAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
</code></pre>

<h4>Migraciones de Base de Datos (SQL Server)</h4>

<p>El proyecto utiliza <strong>Entity Framework Core</strong> con enfoque <em>Code First</em>. El contexto principal es <code>DBFinanzasContext</code> (proyecto <code>Data</code>), y el proyecto de inicio es <code>WebAPI</code>.</p>

<p>Desde la Consola del Administrador de Paquetes en Visual Studio:</p>

<pre><code># Proyecto de inicio: WebAPI
# Proyecto predeterminado: Data

Add-Migration InitialCreate
Update-Database
</code></pre>

<p>Con esto se creará la base <code>FinanzAppDb</code> y las tablas principales:</p>
<ul>
    <li><code>Personas</code></li>
    <li><code>Activos</code></li>
    <li><code>Operaciones</code></li>
    <li><code>Cotizaciones</code></li>
    <li><code>CedearRatios</code></li>
    <li><code>__EFMigrationsHistory</code> (tabla interna de EF Core)</li>
</ul>

<h4>Ejecutar el Servidor API (SQL Server)</h4>

<p>Desde el directorio del proyecto WebAPI:</p>

<pre><code>cd WebAPI
dotnet run
</code></pre>

<p>La API estará accesible típicamente en <code>https://localhost:7088</code> y <code>http://localhost:5195</code> (según <code>launchSettings.json</code>).</p>

<hr/>

<h2>🔌 Integraciones Clave (APIs Externas y Adaptadores)</h2>

<p>El diseño del Backend utiliza el Patrón Adapter para la abstracción de fuentes de datos. Actualmente se usan las siguientes APIs externas:</p>

<table>
 <thead>
  <tr>
   <th>Módulo / Cliente</th>
   <th>API / Proveedor</th>
   <th>Uso Principal</th>
  </tr>
 </thead>
 <tbody>
  <tr>
   <td><code>BinanceClient</code></td>
   <td><a href="https://api.binance.com" target="_blank">Binance Spot API</a><br/><code>/api/v3/ticker/price</code> (batch por <code>symbols</code>)</td>
   <td>
     Obtención de precios spot de pares cripto en USD.  
     Devuelve una lista de <code>QuoteDTO</code> con símbolo, precio, moneda <code>USD</code>, origen <code>Binance</code> y <code>TimestampUtc</code>.
   </td>
  </tr>
  <tr>
   <td><code>CoinCapClient</code></td>
   <td><a href="https://api.coincap.io" target="_blank">CoinCap API</a><br/><code>/v2/assets?limit=N</code></td>
   <td>
     Ranking de criptomonedas y precios en USD.  
     Devuelve una lista de <code>CryptoTopDTO</code> con <code>Rank</code>, <code>Name</code>, <code>Symbol</code>, <code>PriceUsd</code>, <code>Source = "CoinCap"</code> y <code>TimestampUtc</code>.
   </td>
  </tr>
  <tr>
   <td><code>CoinGeckoClient</code></td>
   <td><a href="https://api.coingecko.com" target="_blank">CoinGecko API</a><br/><code>/api/v3/coins/markets</code></td>
   <td>
     Ranking de criptomonedas por market cap y precio en USD.  
     Devuelve una lista de <code>CryptoTopDTO</code> con <code>Rank</code> (market_cap_rank), <code>Name</code>, <code>Symbol</code>, <code>PriceUsd</code>, <code>Source = "CoinGecko"</code> y <code>TimestampUtc</code>.
   </td>
  </tr>
  <tr>
   <td><code>DolarApiClient</code></td>
   <td><a href="https://dolarapi.com" target="_blank">DolarAPI</a><br/><code>/v1/dolares</code></td>
   <td>
     Obtención de cotizaciones de distintos tipos de dólar (Oficial, MEP, CCL, Blue, etc.) en ARS.  
     Devuelve una lista de <code>DolarDTO</code> usada para seleccionar el tipo de cambio que alimenta la lógica de conversión de activos.
   </td>
  </tr>
  <tr>
   <td><code>YahooFinanceClient</code></td>
   <td><a href="https://finance.yahoo.com" target="_blank">Yahoo Finance</a><br/>
       <code>/v7/finance/quote</code>, <code>/v8/finance/chart</code> y parsing de HTML quote.</td>
   <td>
     Obtención de precios de acciones/CEDEARs (símbolos locales <code>.BA</code> y USA).  
     Implementa estrategia de fallbacks:
     <ul>
       <li>Batch <code>v7/finance/quote</code> por múltiples símbolos.</li>
       <li>Fallback a <code>v8/finance/chart</code> por símbolo.</li>
       <li>Fallback final: scraping del HTML de la página de la cotización.</li>
     </ul>
     Devuelve un <code>Dictionary&lt;string, decimal&gt;</code> con precios por símbolo.
   </td>
  </tr>
 </tbody>
</table>

<hr/>

<h2>🗺️ Roadmap (Backend Focus)</h2>

<p>El Roadmap se centra en la infraestructura de negocio y la escalabilidad:</p>

<ul>
    <li><strong>MVP:</strong> Endpoints para gestión de portafolios (CRUD de personas, activos, operaciones) y endpoints para cotizaciones consolidadas.</li>
    <li><strong>Implementación de Autenticación</strong> (ej. Identity/IdentityServer/JWT para espacios personales de usuarios).</li>
    <li><strong>Servicio de Series Temporales</strong> para registrar la evolución histórica del patrimonio.</li>
    <li><strong>Módulos de Importación</strong> para la lógica de procesamiento de archivos (CSV/Excel) de movimientos.</li>
    <li><strong>Servicio de Alertas</strong> utilizando servicios en segundo plano (<code>BackgroundService</code>).</li>
</ul>

<h2>🤝 Contribuir</h2>

<ol>
    <li><strong>Fork</strong>, crea una rama con el formato <code>feature/...</code>, y envía un <strong>PR</strong>.</li>
    <li><strong>Incluir tests unitarios</strong> y de integración para la nueva lógica.</li>
    <li><strong>Explicá los cambios con claridad</strong> en la descripción del PR.</li>
</ol>

<h2>⚖️ Licencia</h2>

<p>MIT – ver archivo <code>LICENSE</code>.</p>

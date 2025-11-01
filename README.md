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
<img src="https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL Badge"/>
<img src="https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black" alt="Swagger Badge"/>
</p>

<hr>

<h2>🎯 Objetivo y Rol</h2>

<p>Este repositorio contiene la <strong>API REST</strong> de alto rendimiento construida en <strong>ASP.NET Core</strong> que actúa como el <em>motor de datos, lógica de negocio y persistencia</em> para toda la aplicación FinanzApp.</p>

<p>Su rol principal es:</p>
<ul>
    <li><strong>Agregación de Datos:</strong> Consumir, normalizar y cachear datos de múltiples APIs financieras externas.</li>
    <li><strong>Lógica de Conversión:</strong> Aplicar la lógica compleja para la valuación y conversión de activos usando los tipos de cambio argentinos (MEP, CCL, Oficial, Blue).</li>
    <li><strong>Persistencia:</strong> Gestionar los portafolios de usuarios, históricos y cotizaciones en <strong>PostgreSQL</strong>.</li>
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
   <td>PostgreSQL</td>
   <td>Base de datos relacional robusta.</td>
  </tr>
  <tr>
   <td><strong>ORM</strong></td>
   <td>Entity Framework Core</td>
   <td>Gestión de la base de datos a través de código (Code First).</td>
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
    <li><a href="https://dotnet.microsoft.com/download">.NET SDK</a> (Recomendado: última versión LTS)</li>
    <li>Una instancia de <strong>PostgreSQL</strong> en ejecución.</li>
</ul>

<h3>2. Instalación de PostgreSQL</h3>

<p>La forma más sencilla de ejecutar PostgreSQL es usando <strong>Docker</strong>:</p>

<pre><code># 1. Descarga y ejecuta la imagen de PostgreSQL
docker run --name finanzapp-postgres -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -d postgres:latest
</code></pre>

<h3>3. Configuración de Conexión</h3>

<p>Asegúrate de que la <em>Connection String</em> en <code>appsettings.json</code> o en tus variables de entorno apunte a esta instancia:</p>

<pre><code>// Ejemplo de Connection String
"ConnectionStrings": {
    "FinanzAppDb": "Host=localhost;Port=5432;Database=finanzapp;Username=postgres;Password=mysecretpassword"
}
</code></pre>

<h3>4. Migraciones de Base de Datos</h3>

<p>Utiliza Entity Framework Core para crear el esquema en la base de datos:</p>

<pre><code># Navega al directorio del proyecto principal de la API
cd src/FinanzApp.Api 

# Aplica las migraciones
dotnet ef database update
</code></pre>

<h3>5. Ejecutar el Servidor API</h3>

<p>El servidor debe correr en una terminal independiente a la del frontend (Next.js):</p>

<pre><code>dotnet run
</code></pre>

<p>La API estará accesible típicamente en <code>https://localhost:7088</code>.</p>

<h2>🔌 Integraciones Clave (Estrategia de Adaptadores)</h2>

<p>El diseño del Backend utiliza el Patrón Adapter para la abstracción de fuentes de datos:</p>

<table>
 <thead>
  <tr>
   <th>Módulo de Datos</th>
   <th>Proveedor (Ejemplo)</th>
   <th>Uso Principal</th>
  </tr>
 </thead>
 <tbody>
  <tr>
   <td><strong>CriptoAdapter</strong></td>
   <td>CoinGecko / Binance API</td>
   <td>Cotizaciones en USD.</td>
  </tr>
  <tr>
   <td><strong>AccionesAdapter</strong></td>
   <td>BYMA / Rava / MAV</td>
   <td>Cotizaciones en ARS.</td>
  </tr>
  <tr>
   <td><strong>ExchangeRateAdapter</strong></td>
   <td>DólarHoy / Ámbito / BCRA</td>
   <td>Tipos de cambio (MEP, CCL, Blue).</td>
  </tr>
 </tbody>
</table>

<h2>🗺️ Roadmap (Backend Focus)</h2>

<p>El Roadmap se centra en la infraestructura de negocio y la escalabilidad:</p>

<ul>
    <li><strong>MVP:</strong> Endpoints para gestión de portafolios (CRUD) y endpoints para cotizaciones consolidadas.</li>
    <li><strong>Implementación de Autenticación</strong> (ej. IdentityServer/JWT).</li>
    <li><strong>Servicio de Series Temporales</strong> para registrar la evolución histórica del patrimonio.</li>
    <li><strong>Módulos de Importación</strong> para la lógica de procesamiento de archivos (CSV/Excel).</li>
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

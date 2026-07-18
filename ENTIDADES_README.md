# Sistema de Entidades Dinámicas - PredatorWeb

## Descripción

Este sistema permite cargar y mostrar entidades de forma dinámica utilizando reflexión (Reflection) en .NET. Las entidades se configuran en la base de datos a través de la tabla `GrlMenu` y se resuelven automáticamente en tiempo de ejecución.

## Arquitectura

### 1. Tabla `GrlMenu` (Base de Datos)

La tabla `GrlMenu` contiene la configuración del menú dinámico con las siguientes columnas clave:

```sql
- MenuId: Identificador único del menú
- MenuGrupalId: ID del menú padre (NULL para módulos principales)
- Menu: Texto que se muestra en el menú
- NombreEntidad: Nombre de la clase entidad (ej: "ComprobanteVenta", "Cliente")
- CodigoMenu: Código único del menú
- Abierto: Si el menú está habilitado
```

**Importante:** El campo `NombreEntidad` debe contener el nombre exacto de la clase C# que hereda de `Entidad`.

### 2. Clase Base `Entidad`

Ubicación: `Entidades/Entidad.cs`

Define las propiedades base de todas las entidades:

```csharp
public class Entidad
{
	public string NombreEntidad { get; set; }      // Nombre interno
	public string NombreTabla { get; set; }        // Tabla en BD
	public string NombreVista { get; set; }        // Vista para consultas
	public string NombreProcedimiento { get; set; } // Stored procedure
	public string NombreClavePrimaria { get; set; } // Clave primaria
	public string NombrePlural { get; set; }       // Nombre para UI (usado en título de pestaña)
	public TipoEntidad Tipo { get; set; }          // Maestro o Documento
}
```

### 3. Entidades Concretas

Las entidades concretas heredan de `Entidad` y configuran sus propiedades en el constructor.

**Ejemplo: ComprobanteVenta**

Ubicación: `Entidades/Ventas/ComprobanteVenta.cs`

```csharp
public class ComprobanteVenta : Entidad
{
	public ComprobanteVenta()
	{
		NombreEntidad = "Comprobante";
		NombreTabla = "VenComprobante";
		NombreProcedimiento = "VenComprobantesMantenimimento";
		NombreVista = "VenComprobanteVistaLista";
		NombreClavePrimaria = "ComprobanteId";
		NombrePlural = "Comprobantes";  // <- Esto se usa como título de la pestaña
		Tipo = TipoEntidad.Maestro;
	}
}
```

### 4. Servicio de Resolución (`EntidadResolverService`)

Ubicación: `Services/EntidadResolverService.cs`

**Responsabilidades:**
- Escanear el ensamblado al inicio para encontrar todas las clases que heredan de `Entidad`
- Mantener un caché de tipos para resolución rápida
- Crear instancias de entidades dinámicamente basándose en el nombre

**Métodos principales:**
```csharp
// Obtiene una instancia de la entidad por nombre
Entidad? GetEntidadInstance(string nombreEntidad)

// Obtiene el tipo sin crear instancia
Type? GetEntidadType(string nombreEntidad)

// Lista todas las entidades registradas
IEnumerable<string> GetRegisteredEntidades()
```

### 5. Flujo de Trabajo

#### Paso 1: Carga del Menú
```
Usuario hace clic en "Iniciar Sesión"
  ↓
MenuService.GetMenuItemsAsync()
  ↓
SELECT MenuId, Menu, NombreEntidad, ... FROM GrlMenu WHERE MenuGrupalId IS NULL
  ↓
Carga columna "NombreEntidad" (ej: "ComprobanteVenta")
```

#### Paso 2: Clic en Opción de Menú
```
Usuario hace clic en "Comprobantes"
  ↓
MainPage.OnSubMenuItemClick(subMenuItem)
  ↓
EntidadService.GetEntidad(subMenuItem.NombreEntidad)
  ↓
EntidadResolverService.GetEntidadInstance("ComprobanteVenta")
  ↓
Reflexión: Busca tipo "ComprobanteVenta" en caché
  ↓
Activator.CreateInstance() → new ComprobanteVenta()
  ↓
Entidad con propiedades configuradas:
  - NombreEntidad: "Comprobante"
  - NombreVista: "VenComprobanteVistaLista"
  - NombrePlural: "Comprobantes"
```

#### Paso 3: Creación de Pestaña
```
Se crea ListaPage con:
  - Title = entidad.NombrePlural ("Comprobantes")
  - NombreEntidad = entidad.NombreEntidad ("Comprobante")
  - ComponentType = typeof(DynamicGridPage)
  ↓
DynamicGridPage recibe NombreEntidad
  ↓
EntidadService.GetEntidadDataAsync("Comprobante")
  ↓
Resuelve entidad → obtiene NombreVista
  ↓
Ejecuta: SELECT * FROM VenComprobanteVistaLista
  ↓
Muestra datos en grid
```

## Cómo Agregar una Nueva Entidad

### 1. Crear la Clase de Entidad

```csharp
// Ubicación: Entidades/[Módulo]/[NombreEntidad].cs
namespace PredatorWeb.Entidades.Ventas
{
	public class Producto : Entidad
	{
		public Producto()
		{
			NombreEntidad = "Producto";
			NombreTabla = "VenProducto";
			NombreProcedimiento = "VenProductoMantenimiento";
			NombreVista = "VenProductoVistaLista";
			NombreClavePrimaria = "ProductoId";
			NombrePlural = "Productos";
			Tipo = TipoEntidad.Maestro;
		}
	}
}
```

### 2. Configurar en la Base de Datos

```sql
INSERT INTO GrlMenu (MenuGrupalId, Menu, NombreEntidad, CodigoMenu, Abierto)
VALUES 
(
	1,                    -- MenuGrupalId (ID del módulo padre)
	'Productos',          -- Menu (texto del botón)
	'Producto',           -- NombreEntidad (nombre de la clase)
	'VEN-PROD',           -- CodigoMenu
	1                     -- Abierto
);
```

**Importante:** El valor de `NombreEntidad` debe coincidir exactamente con el nombre de la clase (en este caso `Producto`).

### 3. Crear la Vista en la Base de Datos

```sql
CREATE VIEW VenProductoVistaLista AS
SELECT 
	ProductoId,
	Codigo,
	Nombre,
	Precio,
	Stock,
	Activo
FROM VenProducto
WHERE EmpresaId = @EmpresaId
```

¡Listo! El sistema automáticamente:
- Detectará la nueva clase `Producto` al iniciar
- Resolverá la entidad cuando se haga clic en el menú
- Mostrará "Productos" como título de la pestaña
- Consultará la vista `VenProductoVistaLista`

## Ventajas del Sistema

✅ **Sin Código Repetitivo**: No necesitas modificar `EntidadService` para cada nueva entidad
✅ **Configuración Centralizada**: Todo se define en la clase de entidad
✅ **Base de Datos Dinámica**: El menú se carga desde BD
✅ **Tipado Fuerte**: Usa reflexión pero mantiene type safety
✅ **Fácil Mantenimiento**: Agregar entidades es trivial
✅ **Logging Automático**: El resolver registra todas las operaciones

## Estructura de Archivos

```
PredatorWeb/
├── Entidades/
│   ├── Entidad.cs                    # Clase base
│   └── Ventas/
│       ├── ComprobanteVenta.cs       # Entidad concreta
│       ├── Cliente.cs                # Entidad concreta
│       └── Producto.cs               # Nueva entidad...
├── Services/
│   ├── EntidadResolverService.cs     # Resuelve entidades por reflexión
│   ├── EntidadService.cs             # Carga datos de entidades
│   ├── MenuService.cs                # Carga menú desde BD
│   └── Datos.cs                      # Capa de acceso a datos
├── Models/
│   ├── GrlMenu.cs                    # Modelo del menú (incluye columna Entidad)
│   └── ListaPage.cs                  # Modelo de pestañas
└── Components/Pages/
	├── MainPage.razor                # Página principal (usa resolver)
	└── DynamicGridPage.razor         # Grid dinámico
```

## Debugging

Para ver las entidades registradas en los logs:
```
[Info] Entidad registrada: ComprobanteVenta (PredatorWeb.Entidades.Ventas.ComprobanteVenta)
[Info] Entidad registrada: Cliente (PredatorWeb.Entidades.Ventas.Cliente)
[Info] Total de entidades cargadas: 2
```

Para ver la resolución en tiempo real:
```
[Info] Instancia creada para entidad: ComprobanteVenta
[Info] Entidad encontrada. Vista a consultar: VenComprobanteVistaLista
```

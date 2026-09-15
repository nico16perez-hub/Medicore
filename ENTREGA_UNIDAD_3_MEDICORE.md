# Unidad 3 adaptada a Medicore

## Objetivo aplicado

En Medicore, la pantalla **Mis Turnos** deja de usar datos locales de `src/datos/turnos.js` y pasa a consumir datos desde el servidor. El flujo queda:

`turnos_page.vue -> turnos_store.js -> turnos_service.js -> ajax_service.js -> Medicore.Api /api/turnos -> MySQL`

## Backend

1. Crear base y usuario en MySQL:

```sql
source Medicore.Api/database.sql;
```

2. Revisar la cadena de conexion:

```json
"Medicore": "server=localhost;port=3306;database=medicore;user=medicore_app;password=medicore_app;SslMode=None;AllowPublicKeyRetrieval=True"
```

3. Aplicar la migracion:

```powershell
dotnet ef database update --project Medicore.Api/Medicore.Api.csproj
```

4. Levantar la API:

```powershell
dotnet run --project Medicore.Api/Medicore.Api.csproj --launch-profile http
```

La API queda en:

```text
http://IP_DE_TU_PC:5171/api/turnos
```

## Frontend

Para probar desde navegador en la PC, se puede usar el valor por defecto:

```text
http://localhost:5171/api
```

Para probar desde telefono fisico, crear `.env` en la raiz de la app:

```text
VITE_API_URL=http://IP_DE_TU_PC:5171/api
```

Luego levantar Ionic/Vite:

```powershell
npm run dev -- --host 0.0.0.0
```

## Estados pedidos

La pantalla **Mis Turnos** muestra:

- cargando: esqueleto mientras espera la API;
- error: mensaje y boton **Reintentar** cuando la API esta apagada o la URL no responde;
- vacio: mensaje cuando la API responde sin turnos.

## Tomas del video

1. Abrir Medicore desde el icono en el telefono y mostrar **Mis Turnos** con datos.
2. Apagar la API, volver a **Mis Turnos** o tocar actualizar, mostrar el error y tocar **Reintentar**.
3. Abrir `chrome://inspect`, entrar al WebView de Medicore y mostrar el request a `/api/turnos` con la URL de la API visible.

Orden recomendado para grabar: levantar MySQL, aplicar migracion, levantar API, configurar `.env` con la IP de la PC, levantar la app, abrir en telefono, inspeccionar WebView, apagar API, reintentar.

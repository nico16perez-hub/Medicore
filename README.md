## Iniciar la API en desarrollo

En una terminal, desde la carpeta del repositorio:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$bytes = New-Object byte[] 48
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$jwtKey = [Convert]::ToBase64String($bytes)
$rng.Dispose()
dotnet user-secrets set "Authentication:Jwt:SigningKey" $jwtKey --project Medicore.Api\Medicore.Api.csproj
dotnet run --project .\Medicore.Api\Medicore.Api.csproj --urls "http://0.0.0.0:5171"
```

Deja esta terminal abierta mientras usas la aplicación.

Iniciar la API no configura la URL del frontend. Si `.env` apunta a una IP de
la red local para Android, no cambies esa configuración para el navegador:
sobrescríbela en la terminal donde iniciarás Vite como se muestra abajo.

## Ejecutar en navegador con `ionic serve`

En otra terminal, apuntá el frontend al loopback de la misma PC. La
configuración HTTP para una IP de la red local se permite únicamente en Android
nativo; no se debe habilitar para el navegador.

```powershell
$env:VITE_API_URL = "http://localhost:5171/api"
npm run dev
```

Ejecutá ambos comandos en la misma terminal del frontend. Si `ionic serve` ya
estaba ejecutándose, detenelo y volvelo a iniciar para que tome la variable.
Esta configuración solo afecta esa terminal y no modifica `.env`; la variable
de proceso prevalece sobre `.env`. Si el frontend se abre en otro dispositivo o
en un dominio que no sea `localhost`, configurá una API HTTPS en lugar de usar
HTTP hacia una IP privada.

## Agenda y asignación de turnos

Antes de iniciar la API con una base existente, aplica la migración de agenda:

```powershell
dotnet ef database update --project .\Medicore.Api\Medicore.Api.csproj
```

Por defecto, cada doctor atiende de lunes a viernes de 08:00 a 13:00, en turnos
de 30 minutos. La migración completa solo los días faltantes y conserva los
rangos existentes. Luego, la secretaría puede ajustar la agenda semanal de cada
doctor y cargar varios rangos por día. La secretaría configura las áreas de atención de cada profesional desde
**Horarios de atención**; un doctor puede pertenecer a varias especialidades.
Al solicitar un turno, el paciente elige primero el área y ve únicamente los
profesionales que la atienden. La solicitud guarda esa preferencia, queda
pendiente y notifica a secretaría; no se muestra como turno asignado en la agenda
del doctor. Desde **Asignar horario**, secretaría confirma un horario libre de
ese doctor en las próximas cuatro semanas (o elige otro profesional de la misma
área).
También puede crear un turno confirmado directamente con un paciente, doctor y
horario disponible. La API valida la agenda y evita superposiciones; al asignar,
notifica al paciente y al doctor. Los horarios que ya tienen turnos vigentes no
se pueden quitar de la agenda.

Los doctores consultan la historia clínica completa por paciente desde su agenda
o desde **Pacientes e historias**. El acceso clínico está restringido al
paciente y a los doctores que lo atienden; secretaría gestiona turnos, no
historias clínicas.

Las fotos de perfil se guardan por cuenta autenticada, no por rol. La carga y
consulta usan el usuario de la sesión; la respuesta no se almacena en caché y
la pantalla vuelve a cargar la foto al cambiar de cuenta.

## Resumen de roles

- **Paciente:** consulta sus turnos e historia clínica, solicita turnos y ve el plan nutricional.
- **Doctor:** consulta sus turnos y registros clínicos; agrega, modifica o elimina registros de los pacientes que atiende y administra el plan nutricional.
- **Secretaria:** revisa solicitudes, crea, modifica y cancela turnos, y gestiona las agendas médicas. No tiene acceso a historias clínicas.

## Ejecutar en el emulador Android

En otra terminal, configura la URL antes de compilar. `10.0.2.2` es la dirección
que usa el emulador Android para acceder al equipo anfitrión; no sirve en un
celular físico.

```powershell
$env:VITE_API_URL = "http://10.0.2.2:5171/api"
$env:VITE_ALLOW_INSECURE_LOCAL_API = "true"
npm run build
npx cap sync android
npx cap run android
```

## Ejecutar en un celular físico en la red local

Conecta el celular a la misma red Wi-Fi que la PC. Antes de compilar, configura
la IP local de la PC y activa la opción de HTTP de desarrollo para Android:

```powershell
$env:VITE_API_URL = "http://192.168.1.2:5171/api"
$env:VITE_ALLOW_INSECURE_LOCAL_API = "true"
npm run build
npx cap sync android
npx cap run android --target R5CY73D1RDT
```

Reemplaza `192.168.1.2` por la IPv4 de la PC si cambia. `localhost` en el
celular apunta al propio teléfono y `0.0.0.0` es una dirección para enlazar el
servidor, no para conectarse a él. La opción permite HTTP únicamente desde
Android hacia IPs privadas; úsala solo durante el desarrollo en una red
confiable. Para publicar la app, configura una URL HTTPS con un certificado
confiable y deja esta opción desactivada.

## Unidad 6: cámara, QR, comprobantes y vibración

La app requiere Capacitor 8. Los complementos agregados son:

- `@capacitor/camera` 8.2.5
- `@capacitor/filesystem` 8.1.4
- `@capacitor/share` 8.0.3
- `@capacitor-mlkit/barcode-scanning` 8.2.1
- `qrcode` 1.5.4 y `@types/qrcode` 1.5.6
- `@capacitor/haptics` 8.0.2 (ya estaba instalado)

El perfil permite elegir Cámara o Galería en Android, revisar una vista previa
y confirmar o cancelar. En navegador se abre el selector de archivos. La API
guarda la foto autenticada bajo `Medicore.Api\App_Data\profile-images`, fuera
de los archivos públicos, valida JPEG/PNG/WebP y limita el tamaño a 5 MB. El
nombre interno es un GUID. Se agregó la migración `Unidad6FotoPerfil`; antes de
usar la foto con una base existente, aplicala con:

```powershell
dotnet ef database update --project .\Medicore.Api\Medicore.Api.csproj
```

Los QR usan únicamente el formato `medicore://turno/{id}`. Escanear requiere
que el dispositivo/navegador soporte Barcode Detection y que la API esté
disponible: la aplicación valida el identificador y consulta el detalle con la
sesión vigente. El PDF se genera en la API y solo se entrega si el usuario
autenticado puede acceder al turno y su estado es `Confirmado`. En Android, el
comprobante se escribe temporalmente en `Directory.Cache`, se comparte con el
diálogo nativo y se elimina al terminar. En navegador se ofrece descarga.

La vibración se puede activar o desactivar desde Mi cuenta; la preferencia se
persiste localmente. Haptics solo se invoca en plataformas nativas compatibles.

El modelo existente no contiene nombre completo del paciente ni consultorio:
el comprobante identifica al paciente mediante el email de su cuenta (o su ID
si no hay cuenta asociada) y omite el consultorio. No se inventan datos que no
estén disponibles. El QR nunca incluye información personal o clínica.

Después de instalar dependencias, sincroniza plugins y compila:

```powershell
npm run build
npx cap sync android
```

Para generar el APK de depuración en Windows:

```powershell
.\android\gradlew.bat -p .\android assembleDebug
```

El archivo se genera en
`android\app\build\outputs\apk\debug\app-debug.apk`. Instalarlo en un teléfono
requiere Android Studio/SDK y verificar allí cámara, galería, escáner ML Kit,
compartir y háptica; `ionic serve` no puede certificar esos comportamientos de
hardware. `QuestPDF` usa la licencia Community, que debe revisarse antes de
cualquier distribución comercial.

## Unidad 7: ubicación y notificaciones

Se agregaron los complementos Capacitor 8 para geolocalización, navegador
externo, notificaciones locales y push. Después de cambiar dependencias,
sincroniza Android y compila:

```powershell
npm run build
npx cap sync android
.\android\gradlew.bat -p .\android assembleDebug
```

### Ubicación y mapas

En **Mi cuenta**, un paciente puede elegir **Registrar mi ubicación**. Solo esa
acción solicita el permiso; la ubicación es optativa, se asocia al usuario del
JWT y se guarda como referencia, no como domicilio. El paciente puede abrirla
en Google Maps o eliminarla. La API no expone estas coordenadas a otros roles.
Si se conoce la dirección oficial del sanatorio, configúrala opcionalmente como
`VITE_SANATORIO_DIRECCION` para mostrar **Cómo llegar**. La URL del mapa codifica
esa dirección. No había una dirección del sanatorio en los datos existentes.

### Bandeja interna

La migración `Unidad7UbicacionNotificacionesPush` agrega campos opcionales de
ubicación a `usuarios` y crea `notificaciones` y `dispositivos_push`; no elimina
ni recrea tablas existentes. Con la conexión local configurada, aplica las
migraciones pendientes con:

```powershell
dotnet ef database update --project .\Medicore.Api\Medicore.Api.csproj
```

La bandeja autenticada está disponible en **Notificaciones**. La API deriva
siempre el destinatario del JWT:

- `GET /api/notificaciones?pagina=1&tamano=20`
- `GET /api/notificaciones/no-leidas`
- `PUT /api/notificaciones/{id}/leida`
- `PUT /api/notificaciones/leidas`

La creación, confirmación, reprogramación y cancelación de turnos notifican solo
a pacientes, doctores o secretarias interesados; el cambio del turno y sus
notificaciones se guardan en el mismo `SaveChanges`. Los mensajes no incluyen
datos clínicos. No se implementó seguimiento de entregas ni reparto: no tiene
una equivalencia apropiada para MediCore.

### Notificaciones locales

Al solicitar un turno desde la app móvil, MediCore intenta programar un aviso
local de confirmación después de que la API registra el turno. El permiso se
solicita en ese momento. Si se deniega o el dispositivo/navegador no ofrece el
plugin, la solicitud del turno permanece guardada y la interfaz informa que el
aviso no pudo programarse.

### Push Android con Firebase

La integración obtiene/renueva el token FCM, solicita permiso, registra el
dispositivo autenticado en `POST /api/push/dispositivos` y escucha los avisos
entrantes. El token propio se muestra en **Mi cuenta** para pruebas y no se
incluye en los mensajes de la bandeja. No se implementó envío push desde la
API; se puede probar desde Firebase Console.

Para habilitar FCM, crea en Firebase una aplicación Android con el identificador
`io.ionic.starter`, habilita Cloud Messaging y coloca el archivo
`google-services.json` en `android/app/`. El archivo está ignorado por Git:
no lo agregues al repositorio. Luego ejecuta `npx cap sync android` y vuelve a
compilar. Sin esa configuración, el registro push mostrará un error, pero la
API, el login, los turnos, la ubicación y la bandeja interna siguen
disponibles. No se pudo probar la entrega real desde Firebase Console sin el
proyecto y sus credenciales.

La aplicación solicita ubicación solo desde el botón del perfil. Para verificar
las funciones de GPS, permisos/notificaciones y apertura de mapas se necesita
un dispositivo/emulador Android; no es posible certificar permisos de hardware
desde `ionic serve`.

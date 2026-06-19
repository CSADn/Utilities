# NETBash — Guía de uso

## Requisitos

- Servidor **Ubuntu 24.04** (x86_64)
- Acceso **root** (`sudo -i`)
- **Dominio público** con DNS apuntando al servidor (solo si usas SSL)
- **Email válido** para Let's Encrypt (solo si usas SSL)

## Instalación

Copia NETBash al servidor y ejecuta como root:

```bash
cd /opt
git clone <repo> netbash
cd netbash
chmod +x netbash
```

### Instalación completa (con dominio y SSL)

```bash
./netbash install \
    --app mi-api \
    --domain api.ejemplo.com \
    --email admin@ejemplo.com
```

Esto instala y configura: runtime .NET 10, Nginx con HTTPS, certificado Let's Encrypt, firewall UFW, logrotate.

### Instalación solo HTTP (sin dominio)

```bash
./netbash install --app mi-api
```

Omitir `--domain` salta SSL y Nginx. Ideal para desarrollo interno o detrás de otro proxy.

### Incluir el SDK de .NET

```bash
./netbash install --app mi-api --domain api.ejemplo.com --email admin@ejemplo.com --sdk
```

## Despliegue

Una vez que tienes tu aplicación .NET publicada (por ejemplo en `/tmp/mi-api-publish`):

```bash
./netbash deploy --app mi-api --port 5000
```

NETBash copia los archivos a `releases/mi-api/<timestamp>`, crea el symlink `current/mi-api`, inicia el servicio y verifica `/health`.

### Redeploy

Si ya hay un release desplegado, necesitas `--force`:

```bash
./netbash deploy --app mi-api --force
```

### Health check

Si el health check falla tras 12 intentos (5s cada uno), NETBash revierte automáticamente al release anterior.

## Rollback

### Al release anterior

```bash
./netbash rollback --app mi-api
```

### A una versión específica

```bash
NETBASH_VERSION=20250101120000 ./netbash rollback --app mi-api
```

Las versiones son timestamps (p.ej. `20250101120000`). Listalas con `netbash status`.

## Estado

```bash
./netbash status
```

Muestra los módulos instalados y los releases de cada aplicación, marcando con `*` el activo.

## Dry-run

```bash
./netbash install --app mi-api --dry-run
```

Muestra lo que se ejecutaría sin hacer cambios. Válido para `install` y `deploy`.

## Ejemplos rápidos

| Escenario | Comando |
|-----------|---------|
| Instalar app interna sin SSL | `./netbash install --app interna` |
| Instalar app pública con SSL | `./netbash install --app web --domain web.midominio.com --email yo@midominio.com` |
| Desplegar primera vez | `./netbash deploy --app web` |
| Desplegar de nuevo | `./netbash deploy --app web --force` |
| Volver atrás | `./netbash rollback --app web` |
| Ver qué hay instalado | `./netbash status` |

## Solución de problemas

| Problema | Causa probable | Solución |
|----------|---------------|----------|
| `--app is required` | Falta el flag `--app` | Agrega `--app nombre` |
| `--force required` | Ya hay un release o instalación previa | Agrega `--force` |
| Health check falla | La app no responde en `/health` | Verifica que el puerto y la ruta `/health` existan |
| Certificado SSL no se emite | El DNS no apunta al servidor | Verifica el registro A del dominio |
| Puerto ocupado | Otro servicio usa ese puerto | Usa `--port` con un puerto diferente |

# oneSIMPLY.AI.Hub

SaaS B2B Marketing AI — copywriting, VietQR billing, multi-channel push.

## Cấu trúc

```
Backend/oneSIMPLY.AIHub.Api   — ASP.NET Core 8 API
Frontend/oneSIMPLY.AIHub.Web  — React + Vite
Doc/                          — SQL scripts, tài liệu
```

## Bảo mật cấu hình

**Không commit secret lên Git.** Chỉ dùng file mẫu (`.example`):

| File mẫu (commit) | File thật (local, gitignore) |
|---|---|
| `Backend/.../appsettings.example.json` | `appsettings.json` + `appsettings.Development.json` |
| `Frontend/.../.env.example` | `.env` |
| `.env.docker.example` | `.env.docker` |

### Setup lần đầu (Backend)

```powershell
cd Backend\oneSIMPLY.AIHub.Api
copy appsettings.example.json appsettings.json
# Chỉnh appsettings.Development.json (đã có sẵn local) hoặc copy secret vào đó
dotnet run --urls http://localhost:5000
```

ASP.NET Core tự merge `appsettings.json` + `appsettings.Development.json`.  
Production/Docker: dùng **biến môi trường** (`Jwt__Secret`, `ConnectionStrings__DefaultConnection`, …).

### Setup Frontend

```powershell
cd Frontend\oneSIMPLY.AIHub.Web
copy .env.example .env
npm install
npm run dev
```

### Docker

```powershell
copy .env.docker.example .env.docker
# Sửa password/secret trong .env.docker
docker compose up --build
```

> **Lưu ý:** JWT secret cũ từng push lên GitHub — nên **đổi secret mới** trong `appsettings.Development.json` / `.env.docker`.

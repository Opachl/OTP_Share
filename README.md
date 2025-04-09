# OTP_Share
OTP_Share is a Razor Pages project designed to manage and share OTP (One-Time Password) accounts securely. This project is built on .NET 8 and can be deployed using Docker.

### NOTE:
This project is in early development and may not be fully functional. Use at your own risk.

## Features
- Secure OTP sharing
- Admin dashboard (optional)
- Integration with Vaultwarden
- Database support

## Prerequisites
- Docker
- Docker Compose

## Environment Variables
The following environment variables can be configured to customize the application:

| Variable              | Default Value       | Description                                      |
|-----------------------|---------------------|--------------------------------------------------|
| `ENABLE_ADMIN`        | `false`            | Enable or disable the admin page.               |
| `ADMIN_USER`          | `admin`            | Admin username.                                 |
| `ADMIN_PASS`          | `admin`            | Admin password.                                 |
| `VW_URL`              | (empty)            | Vaultwarden URL.                                |
| `VW_USERPW`           | (empty)            | Vaultwarden user password.                      |
| `VW_CLIENTID`         | (empty)            | Vaultwarden client ID.                          |
| `VW_CLIENTSECRET`     | (empty)            | Vaultwarden client secret.                      |
| `DB_SERVER`           | (empty)            | Database server address.                        |
| `DB_SERVERPORT`       | `3306`             | Database server port.                           |
| `DB_USER`             | (empty)            | Database username.                              |
| `DB_PASSWORD`         | (empty)            | Database password.                              |
| `DB_DATABASE`         | `optShare`         | Database name.                                  |
| `NTP_DEFAULTPool`     | `de.pool.ntp.org`  | Default NTP pool for time synchronization.      |
| `NTP_SYNCWithSystem`  | (empty)            | Sync with system time (true/false).  (not implemented yet)          |


Here is the `docker-compose.yml` that powers the whole setup.
```yaml

services:
  otp_share:
    image: ghcr.io/opachl/otp_share:master
    container_name: otp_share
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      ENABLE_ADMIN: "true"
      ADMIN_USER: "admin"
      ADMIN_PASS: "admin"
      VW_URL: "https://vaultwarden.example.com"
      VW_USERPW: "vaultwarden-password"
      VW_CLIENTID: "vaultwarden-client-id"
      VW_CLIENTSECRET: "vaultwarden-client-secret"
      DB_SERVER: "db"
      DB_SERVERPORT: "3306"
      DB_USER: "root"
      DB_PASSWORD: "password"
      DB_DATABASE: "optShare"
      NTP_DEFAULTPool: "de.pool.ntp.org"
      NTP_SYNCWithSystem: "true"
    depends_on:
      - db

  db:
    image: mariadb:10.5
    container_name: otp_share_db
    environment:
      MYSQL_ROOT_PASSWORD: "password"
      MYSQL_DATABASE: "optShare"
      MYSQL_USER: "root"
      MYSQL_PASSWORD: "password"
    ports:
      - "3306:3306"
    volumes:
      - db_data:/var/lib/mysql

volumes:
  db_data:
```

## TODO / Pending Steps
The following improvements are planned for future development:

### 1. Improve Performance
- **Current Issue**: The site reacts slowly, especially under load.
- **Planned Actions**:
  - Optimize database queries to reduce latency.
  - Implement caching for frequently accessed data.
  - Profile and optimize server-side code to identify bottlenecks.
  - Consider using asynchronous programming where applicable.

### 2. Enhance Security
- **Current Issue**: Some code may expose too much information or lack proper security configurations.
- **Planned Actions**:
  - Review and sanitize all user inputs to prevent injection attacks.
  - Implement stricter access controls for sensitive endpoints.
  - Mask sensitive information in logs and error messages.
  - Add secure headers (e.g., Content Security Policy, X-Frame-Options).
  - Conduct a security audit to identify and fix vulnerabilities.

### 3. Documentation
- Improve documentation for developers and users to make the project easier to understand and contribute to.

---

Let me know if you need further updates or additional sections!
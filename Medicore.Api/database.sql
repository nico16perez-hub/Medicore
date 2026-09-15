CREATE DATABASE IF NOT EXISTS medicore
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'medicore_app'@'localhost'
  IDENTIFIED BY 'medicore_app';

CREATE USER IF NOT EXISTS 'medicore_app'@'%'
  IDENTIFIED BY 'medicore_app';

ALTER USER 'medicore_app'@'localhost'
  IDENTIFIED BY 'medicore_app';

ALTER USER 'medicore_app'@'%'
  IDENTIFIED BY 'medicore_app';

GRANT ALL PRIVILEGES ON medicore.* TO 'medicore_app'@'localhost';
GRANT ALL PRIVILEGES ON medicore.* TO 'medicore_app'@'%';

FLUSH PRIVILEGES;

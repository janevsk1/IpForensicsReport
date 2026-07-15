CREATE DATABASE IF NOT EXISTS IpForensicsReport
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;


USE IpForensicsReport;


CREATE TABLE IF NOT EXISTS Users
(
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    PasswordHash VARCHAR(512) NOT NULL,
    CreatedOn DATETIME(6) NOT NULL,

    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
) ENGINE = InnoDB;


CREATE TABLE IF NOT EXISTS IpForensicsReports
(
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    UserId BIGINT UNSIGNED NOT NULL,

    EncryptedPayload LONGBLOB NOT NULL,
    EncryptionNonce VARBINARY(12) NOT NULL,
    AuthenticationTag VARBINARY(16) NOT NULL,

    CreatedOn DATETIME(6) NOT NULL,

    CONSTRAINT PK_IpForensicsReports PRIMARY KEY (Id),

    CONSTRAINT FK_IpForensicsReports_Users
        FOREIGN KEY (UserId)
        REFERENCES Users (Id)
        ON DELETE CASCADE,

    INDEX IX_IpForensicsReports_UserId_CreatedOn
        (UserId, CreatedOn DESC)
) ENGINE = InnoDB;
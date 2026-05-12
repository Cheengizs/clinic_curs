-- Создание логической базы данных (выполнять под админом, затем переключиться на неё)
-- CREATE DATABASE profiles_db;

-- 1. Создание типа ENUM для ролей [cite: 134-140]
CREATE TYPE user_role AS ENUM ('Patient', 'Doctor', 'Receptionist', 'Admin');

-- 2. Таблица авторизации [cite: 152-158]
CREATE TABLE Accounts (
    Id UUID PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role user_role NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- 3. Профили пациентов [cite: 159-168]
CREATE TABLE Patients (
    Id UUID PRIMARY KEY,
    AccountId UUID NOT NULL REFERENCES Accounts(Id),
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    DateOfBirth DATE,
    PhoneNumber VARCHAR(255), -- Длина увеличена для хранения AES-256 хэша [cite: 51, 52]
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    AnonymizedAt TIMESTAMP
);

-- 4. Профили врачей [cite: 169-177]
CREATE TABLE Doctors (
    Id UUID PRIMARY KEY,
    AccountId UUID NOT NULL REFERENCES Accounts(Id),
    -- Логическая связь с микросервисом Facilities (без FOREIGN KEY) [cite: 54, 401]
    SpecializationId UUID NOT NULL, 
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    CareerStartYear INT,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);

-- 5. Профили регистратуры [cite: 179-185]
CREATE TABLE Receptionists (
    Id UUID PRIMARY KEY,
    AccountId UUID NOT NULL REFERENCES Accounts(Id),
    -- Логическая связь с микросервисом Facilities (без FOREIGN KEY) [cite: 54, 402]
    OfficeId UUID NOT NULL, 
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL
);

-- 6. Справочник политик конфиденциальности (GDPR) [cite: 186-192]
CREATE TABLE Consents (
    Id UUID PRIMARY KEY,
    PolicyName VARCHAR(255) NOT NULL,
    Description TEXT,
    IsMandatory BOOLEAN NOT NULL,
    Version VARCHAR(50) NOT NULL
);

-- 7. Подписанные политики (GDPR) [cite: 193-200]
CREATE TABLE PatientConsents (
    Id UUID PRIMARY KEY,
    PatientId UUID NOT NULL REFERENCES Patients(Id),
    ConsentId UUID NOT NULL REFERENCES Consents(Id),
    IsGranted BOOLEAN NOT NULL,
    GrantedAt TIMESTAMP NOT NULL,
    IpAddress VARCHAR(45)
);

-- ==========================================
-- БИЗНЕС-ЛОГИКА НА УРОВНЕ БД (GDPR)
-- ==========================================

-- Триггер BEFORE DELETE на таблице Patients [cite: 535]
-- Защита от случайного или умышленного удаления медицинских данных [cite: 536]
CREATE OR REPLACE FUNCTION prevent_patient_hard_delete()
RETURNS TRIGGER AS $$
BEGIN
    -- Вместо физического удаления делаем UPDATE флага IsDeleted [cite: 537]
    UPDATE Patients SET IsDeleted = TRUE WHERE Id = OLD.Id;
    RETURN NULL; -- Отменяет операцию DELETE [cite: 537]
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_PreventHardDelete_Patients
BEFORE DELETE ON Patients
FOR EACH ROW
EXECUTE FUNCTION prevent_patient_hard_delete();
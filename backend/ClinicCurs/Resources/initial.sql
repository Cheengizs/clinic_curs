-- 1. Безопасное создание типов (ENUM) через динамический SQL
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'role_type') THEN
            EXECUTE 'CREATE TYPE role_type AS ENUM (''patient'', ''doctor'', ''registrar'', ''admin'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'gender') THEN
            EXECUTE 'CREATE TYPE gender AS ENUM (''male'', ''female'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'verification_statuses') THEN
            EXECUTE 'CREATE TYPE verification_statuses AS ENUM (''wait'', ''verified'', ''declined'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'appointment_statuses') THEN
            EXECUTE 'CREATE TYPE appointment_statuses AS ENUM (''planned'', ''confirmed'', ''completed'', ''cancelled'', ''no_show'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'appointment_category') THEN
            EXECUTE 'CREATE TYPE appointment_category AS ENUM (''initial_consultation'', ''follow_up'', ''diagnostic'', ''procedure'', ''vaccination'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'lab_status') THEN
            EXECUTE 'CREATE TYPE lab_status AS ENUM (''pending'', ''ready'', ''cancelled'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'blood_type_enum') THEN
            EXECUTE 'CREATE TYPE blood_type_enum AS ENUM (''O_first'', ''A_second'', ''B_third'', ''AB_fourth'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'rhesus_factor_enum') THEN
            EXECUTE 'CREATE TYPE rhesus_factor_enum AS ENUM (''positive'', ''negative'', ''neutral'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'diagnosis_type') THEN
            EXECUTE 'CREATE TYPE diagnosis_type AS ENUM (''main'', ''concomitant'', ''complication'')';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'recommendation_type') THEN
            EXECUTE 'CREATE TYPE recommendation_type AS ENUM (''medication'', ''procedure'', ''lifestyle'', ''analysis'')';
        END IF;
    END $$;

-- 2. Таблицы (с использованием IF NOT EXISTS)

CREATE TABLE IF NOT EXISTS tbl_accounts (
                                            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                            email TEXT UNIQUE NOT NULL,
                                            password_hash TEXT NOT NULL,
                                            phone TEXT,
                                            email_verified BOOLEAN DEFAULT FALSE,
                                            phone_verified BOOLEAN DEFAULT FALSE,
                                            identity_verified BOOLEAN DEFAULT FALSE,
                                            role role_type NOT NULL,
                                            is_deleted BOOLEAN DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS tbl_password_resets (
                                                   id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                   account_id UUID REFERENCES tbl_accounts(id),
                                                   token_hash TEXT NOT NULL,
                                                   expires_at TIMESTAMP NOT NULL,
                                                   is_used BOOLEAN DEFAULT FALSE,
                                                   created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_offices (
                                           id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                           name TEXT NOT NULL,
                                           address TEXT NOT NULL,
                                           phone TEXT,
                                           is_active BOOLEAN DEFAULT TRUE,
                                           photo_url TEXT
);

CREATE TABLE IF NOT EXISTS tbl_registrars (
                                              id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                              account_id UUID UNIQUE REFERENCES tbl_accounts(id),
                                              office_id UUID REFERENCES tbl_offices(id),
                                              first_name TEXT NOT NULL,
                                              last_name TEXT NOT NULL,
                                              middle_name TEXT,
                                              hired_at DATE,
                                              avatar_url TEXT,
                                              is_active BOOLEAN DEFAULT TRUE,
                                              is_deleted BOOLEAN DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS tbl_verification_requests (
                                                         id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                         account_id UUID REFERENCES tbl_accounts(id),
                                                         last_name TEXT NOT NULL,
                                                         first_name TEXT NOT NULL,
                                                         middle_name TEXT,
                                                         birth_date DATE,
                                                         passport_series_number TEXT,
                                                         personal_number TEXT,
                                                         document_scan_url TEXT,
                                                         status verification_statuses DEFAULT 'wait',
                                                         registrar_id UUID REFERENCES tbl_registrars(id),
                                                         processed_at TIMESTAMP,
                                                         created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_patients (
                                            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                            account_id UUID UNIQUE REFERENCES tbl_accounts(id),
                                            first_name TEXT NOT NULL,
                                            last_name TEXT NOT NULL,
                                            middle_name TEXT,
                                            birth_date DATE,
                                            gender gender,
                                            passport_series_number TEXT,
                                            personal_number TEXT UNIQUE,
                                            residential_address TEXT,
                                            avatar_url TEXT,
                                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_specializations (
                                                   id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                   name TEXT NOT NULL,
                                                   description TEXT
);

CREATE TABLE IF NOT EXISTS tbl_doctors (
                                           id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                           account_id UUID UNIQUE REFERENCES tbl_accounts(id),
                                           office_id UUID REFERENCES tbl_offices(id),
                                           first_name TEXT NOT NULL,
                                           last_name TEXT NOT NULL,
                                           middle_name TEXT,
                                           bio TEXT,
                                           hired_at DATE,
                                           avatar_url TEXT,
                                           rating_avg DECIMAL(3, 2) DEFAULT 0,
                                           is_active BOOLEAN DEFAULT TRUE,
                                           is_deleted BOOLEAN DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS m2m_doctor_specialization (
                                                         doctor_id UUID REFERENCES tbl_doctors(id),
                                                         specialization_id UUID REFERENCES tbl_specializations(id),
                                                         is_primary BOOLEAN DEFAULT FALSE,
                                                         career_start_date DATE,
                                                         PRIMARY KEY (doctor_id, specialization_id)
);

CREATE TABLE IF NOT EXISTS tbl_schedules (
                                             id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                             doctor_id UUID REFERENCES tbl_doctors(id),
                                             office_id UUID REFERENCES tbl_offices(id),
                                             work_date DATE NOT NULL,
                                             start_time TIME NOT NULL,
                                             end_time TIME NOT NULL,
                                             is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS tbl_appointment_types (
                                                     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                     category appointment_category NOT NULL,
                                                     default_duration_minutes INTEGER DEFAULT 30
);

CREATE TABLE IF NOT EXISTS tbl_appointments (
                                                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                account_id UUID REFERENCES tbl_accounts(id),
                                                office_id UUID REFERENCES tbl_offices(id),
                                                type_id UUID REFERENCES tbl_appointment_types(id),
                                                doctor_id UUID REFERENCES tbl_doctors(id),
                                                registrar_id UUID REFERENCES tbl_registrars(id),
                                                scheduled_start TIMESTAMP NOT NULL,
                                                scheduled_end TIMESTAMP NOT NULL,
                                                status appointment_statuses DEFAULT 'planned',
                                                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_reviews (
                                           id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                           doctor_id UUID REFERENCES tbl_doctors(id),
                                           patient_id UUID REFERENCES tbl_patients(id),
                                           appointment_id UUID UNIQUE REFERENCES tbl_appointments(id),
                                           rating INTEGER CHECK (rating >= 1 AND rating <= 5),
                                           comment TEXT,
                                           created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_medical_cards (
                                                 id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                 patient_id UUID UNIQUE REFERENCES tbl_patients(id),
                                                 card_number TEXT UNIQUE NOT NULL,
                                                 blood_type blood_type_enum,
                                                 rhesus_factor rhesus_factor_enum,
                                                 chronic_diseases TEXT,
                                                 allergies TEXT,
                                                 created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                                 updated_at TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_medical_records (
                                                   id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                   card_id UUID REFERENCES tbl_medical_cards(id),
                                                   appointment_id UUID UNIQUE REFERENCES tbl_appointments(id),
                                                   doctor_id UUID REFERENCES tbl_doctors(id),
                                                   complaints TEXT,
                                                   objective_data TEXT,
                                                   assessment TEXT,
                                                   plan TEXT,
                                                   created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_icd10_dictionary (
                                                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                    code TEXT UNIQUE NOT NULL,
                                                    name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS m2m_record_diagnosis (
                                                    record_id UUID REFERENCES tbl_medical_records(id),
                                                    diagnosis_id UUID REFERENCES tbl_icd10_dictionary(id),
                                                    type diagnosis_type DEFAULT 'main',
                                                    PRIMARY KEY (record_id, diagnosis_id)
);

CREATE TABLE IF NOT EXISTS tbl_recommendations (
                                                   id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                   record_id UUID REFERENCES tbl_medical_records(id),
                                                   type recommendation_type NOT NULL,
                                                   title TEXT NOT NULL,
                                                   details TEXT,
                                                   duration_days INTEGER,
                                                   is_active BOOLEAN DEFAULT TRUE,
                                                   created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tbl_lab_tests_dictionary (
                                                        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                                        name TEXT NOT NULL,
                                                        description TEXT
);

CREATE TABLE IF NOT EXISTS tbl_lab_results (
                                               id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                               card_id UUID REFERENCES tbl_medical_cards(id),
                                               test_id UUID REFERENCES tbl_lab_tests_dictionary(id),
                                               recommendation_id UUID REFERENCES tbl_recommendations(id),
                                               office_id UUID REFERENCES tbl_offices(id),
                                               result_file_url TEXT,
                                               status lab_status DEFAULT 'pending',
                                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Индексы
CREATE INDEX IF NOT EXISTS idx_accounts_email ON tbl_accounts(email);
CREATE INDEX IF NOT EXISTS idx_appointments_start ON tbl_appointments(scheduled_start);
CREATE INDEX IF NOT EXISTS idx_schedules_date ON tbl_schedules(work_date);

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:appointment_category", "initial_consultation,follow_up,diagnostic,procedure,vaccination")
                .Annotation("Npgsql:Enum:appointment_statuses", "planned,confirmed,completed,cancelled,no_show")
                .Annotation("Npgsql:Enum:blood_type_enum", "o_first,a_second,b_third,ab_fourth")
                .Annotation("Npgsql:Enum:diagnosis_type", "main,concomitant,complication")
                .Annotation("Npgsql:Enum:gender", "male,female")
                .Annotation("Npgsql:Enum:lab_status", "pending,ready,cancelled")
                .Annotation("Npgsql:Enum:recommendation_type", "medication,procedure,lifestyle,analysis")
                .Annotation("Npgsql:Enum:rhesus_factor_enum", "positive,negative,neutral")
                .Annotation("Npgsql:Enum:role_type", "patient,doctor,registrar,admin")
                .Annotation("Npgsql:Enum:verification_statuses", "wait,verified,declined");

            migrationBuilder.CreateTable(
                name: "tbl_account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: true),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    phone_verified = table.Column<bool>(type: "boolean", nullable: false),
                    last_phone_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    identity_verified = table.Column<bool>(type: "boolean", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_appointment_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category = table.Column<int>(type: "integer", nullable: false),
                    default_duration_minutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_appointment_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_icd10_dictionary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_icd10_dictionary", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_lab_tests_dictionary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_lab_tests_dictionary", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_office",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_office", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_specialization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_specialization", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_admin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_admin", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_admin_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_password_reset",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_password_reset", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_password_reset_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_patient",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    passport_series_number = table.Column<string>(type: "text", nullable: false),
                    personal_number = table.Column<string>(type: "text", nullable: false),
                    residential_address = table.Column<string>(type: "text", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_patient", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_patient_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_refresh_token_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_doctor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: false),
                    bio = table.Column<string>(type: "text", nullable: false),
                    hired_at = table.Column<DateOnly>(type: "date", nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    rating_avg = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_doctor", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_doctor_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_doctor_tbl_office_office_id",
                        column: x => x.office_id,
                        principalTable: "tbl_office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_registrar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: false),
                    hired_at = table.Column<DateOnly>(type: "date", nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_registrar", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_registrar_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_registrar_tbl_office_office_id",
                        column: x => x.office_id,
                        principalTable: "tbl_office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_medical_card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number = table.Column<string>(type: "text", nullable: false),
                    blood_type = table.Column<int>(type: "integer", nullable: false),
                    rhesus_factor = table.Column<int>(type: "integer", nullable: false),
                    chronic_diseases = table.Column<string>(type: "text", nullable: false),
                    allergies = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_medical_card", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_medical_card_tbl_patient_patient_id",
                        column: x => x.patient_id,
                        principalTable: "tbl_patient",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "m2m_doctor_specialization",
                columns: table => new
                {
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    specialization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    career_start_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_m2m_doctor_specialization", x => new { x.doctor_id, x.specialization_id });
                    table.ForeignKey(
                        name: "FK_m2m_doctor_specialization_tbl_doctor_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "tbl_doctor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_m2m_doctor_specialization_tbl_specialization_specialization~",
                        column: x => x.specialization_id,
                        principalTable: "tbl_specialization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_schedule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_schedule", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_schedule_tbl_doctor_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "tbl_doctor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_schedule_tbl_office_office_id",
                        column: x => x.office_id,
                        principalTable: "tbl_office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_appointment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    registrar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scheduled_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduled_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_appointment", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_appointment_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_appointment_tbl_appointment_type_type_id",
                        column: x => x.type_id,
                        principalTable: "tbl_appointment_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_appointment_tbl_doctor_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "tbl_doctor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tbl_appointment_tbl_office_office_id",
                        column: x => x.office_id,
                        principalTable: "tbl_office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_appointment_tbl_registrar_registrar_id",
                        column: x => x.registrar_id,
                        principalTable: "tbl_registrar",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_verification_request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    passport_series_number = table.Column<string>(type: "text", nullable: false),
                    personal_number = table.Column<string>(type: "text", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    registrar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_verification_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_verification_request_tbl_account_account_id",
                        column: x => x.account_id,
                        principalTable: "tbl_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_verification_request_tbl_registrar_registrar_id",
                        column: x => x.registrar_id,
                        principalTable: "tbl_registrar",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_medical_record",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    complaints = table.Column<string>(type: "text", nullable: false),
                    objective_data = table.Column<string>(type: "text", nullable: false),
                    assessment = table.Column<string>(type: "text", nullable: false),
                    plan = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    medical_card_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_medical_record", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_medical_record_tbl_appointment_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "tbl_appointment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_medical_record_tbl_doctor_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "tbl_doctor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_medical_record_tbl_medical_card_medical_card_id",
                        column: x => x.medical_card_id,
                        principalTable: "tbl_medical_card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_review",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_review", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_review_tbl_appointment_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "tbl_appointment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_review_tbl_doctor_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "tbl_doctor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_review_tbl_patient_patient_id",
                        column: x => x.patient_id,
                        principalTable: "tbl_patient",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "m2m_record_diagnosis",
                columns: table => new
                {
                    record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    diagnosis_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_m2m_record_diagnosis", x => new { x.record_id, x.diagnosis_id });
                    table.ForeignKey(
                        name: "FK_m2m_record_diagnosis_tbl_icd10_dictionary_diagnosis_id",
                        column: x => x.diagnosis_id,
                        principalTable: "tbl_icd10_dictionary",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_m2m_record_diagnosis_tbl_medical_record_record_id",
                        column: x => x.record_id,
                        principalTable: "tbl_medical_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_recommendation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    duration_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_recommendation", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_recommendation_tbl_medical_record_record_id",
                        column: x => x.record_id,
                        principalTable: "tbl_medical_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_lab_result",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recommendation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    office_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_file_url = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    medical_card_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_lab_result", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_lab_result_tbl_lab_tests_dictionary_test_id",
                        column: x => x.test_id,
                        principalTable: "tbl_lab_tests_dictionary",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_lab_result_tbl_medical_card_medical_card_id",
                        column: x => x.medical_card_id,
                        principalTable: "tbl_medical_card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_lab_result_tbl_office_office_id",
                        column: x => x.office_id,
                        principalTable: "tbl_office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_lab_result_tbl_recommendation_recommendation_id",
                        column: x => x.recommendation_id,
                        principalTable: "tbl_recommendation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_m2m_doctor_specialization_specialization_id",
                table: "m2m_doctor_specialization",
                column: "specialization_id");

            migrationBuilder.CreateIndex(
                name: "IX_m2m_record_diagnosis_diagnosis_id",
                table: "m2m_record_diagnosis",
                column: "diagnosis_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_admin_account_id",
                table: "tbl_admin",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_appointment_account_id",
                table: "tbl_appointment",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_appointment_doctor_id",
                table: "tbl_appointment",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_appointment_office_id",
                table: "tbl_appointment",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_appointment_registrar_id",
                table: "tbl_appointment",
                column: "registrar_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_appointment_type_id",
                table: "tbl_appointment",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_doctor_account_id",
                table: "tbl_doctor",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_doctor_office_id",
                table: "tbl_doctor",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_lab_result_medical_card_id",
                table: "tbl_lab_result",
                column: "medical_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_lab_result_office_id",
                table: "tbl_lab_result",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_lab_result_recommendation_id",
                table: "tbl_lab_result",
                column: "recommendation_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_lab_result_test_id",
                table: "tbl_lab_result",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_medical_card_patient_id",
                table: "tbl_medical_card",
                column: "patient_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_medical_record_appointment_id",
                table: "tbl_medical_record",
                column: "appointment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_medical_record_doctor_id",
                table: "tbl_medical_record",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_medical_record_medical_card_id",
                table: "tbl_medical_record",
                column: "medical_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_password_reset_account_id",
                table: "tbl_password_reset",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_patient_account_id",
                table: "tbl_patient",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_recommendation_record_id",
                table: "tbl_recommendation",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_refresh_token_account_id",
                table: "tbl_refresh_token",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_registrar_account_id",
                table: "tbl_registrar",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_registrar_office_id",
                table: "tbl_registrar",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_appointment_id",
                table: "tbl_review",
                column: "appointment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_doctor_id",
                table: "tbl_review",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_patient_id",
                table: "tbl_review",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_schedule_doctor_id",
                table: "tbl_schedule",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_schedule_office_id",
                table: "tbl_schedule",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_verification_request_account_id",
                table: "tbl_verification_request",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_verification_request_registrar_id",
                table: "tbl_verification_request",
                column: "registrar_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "m2m_doctor_specialization");

            migrationBuilder.DropTable(
                name: "m2m_record_diagnosis");

            migrationBuilder.DropTable(
                name: "tbl_admin");

            migrationBuilder.DropTable(
                name: "tbl_lab_result");

            migrationBuilder.DropTable(
                name: "tbl_password_reset");

            migrationBuilder.DropTable(
                name: "tbl_refresh_token");

            migrationBuilder.DropTable(
                name: "tbl_review");

            migrationBuilder.DropTable(
                name: "tbl_schedule");

            migrationBuilder.DropTable(
                name: "tbl_verification_request");

            migrationBuilder.DropTable(
                name: "tbl_specialization");

            migrationBuilder.DropTable(
                name: "tbl_icd10_dictionary");

            migrationBuilder.DropTable(
                name: "tbl_lab_tests_dictionary");

            migrationBuilder.DropTable(
                name: "tbl_recommendation");

            migrationBuilder.DropTable(
                name: "tbl_medical_record");

            migrationBuilder.DropTable(
                name: "tbl_appointment");

            migrationBuilder.DropTable(
                name: "tbl_medical_card");

            migrationBuilder.DropTable(
                name: "tbl_appointment_type");

            migrationBuilder.DropTable(
                name: "tbl_doctor");

            migrationBuilder.DropTable(
                name: "tbl_registrar");

            migrationBuilder.DropTable(
                name: "tbl_patient");

            migrationBuilder.DropTable(
                name: "tbl_office");

            migrationBuilder.DropTable(
                name: "tbl_account");
        }
    }
}

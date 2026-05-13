import { useState, useEffect } from "react";
import {
  adminApi,
  type RegisterRegistrarDto,
  type RegisterDoctorDto,
} from "../../api/adminApi";
import { clinicApi } from "../../api/clinicApi";
import type { OfficeDto } from "../../types/clinic";
import { getErrorMessage } from "../../utils/errorHandler";

type Tab = "offices" | "specializations" | "staff" | "schedules";
type StaffRole = "registrar" | "doctor";

export default function AdminView() {
  const [activeTab, setActiveTab] = useState<Tab>("offices");
  const [loading, setLoading] = useState(false);

  const [errorMsg, setErrorMsg] = useState("");
  const [successMsg, setSuccessMsg] = useState("");

  const [offices, setOffices] = useState<OfficeDto[]>([]);
  const [specializations, setSpecializations] = useState<any[]>([]);
  const [doctors, setDoctors] = useState<any[]>([]);

  const [officeForm, setOfficeForm] = useState({
    name: "",
    address: "",
    phone: "",
  });
  const [specForm, setSpecForm] = useState({ name: "", description: "" });

  const [scheduleForm, setScheduleForm] = useState({
    doctorId: "",
    workDate: "",
    startTime: "09:00",
    endTime: "17:00",
  });

  const [staffRole, setStaffRole] = useState<StaffRole>("registrar");
  const [staffForm, setStaffForm] = useState({
    firstName: "",
    lastName: "",
    middleName: "",
    phone: "",
    officeId: "",
    bio: "",
  });
  const [doctorSpecs, setDoctorSpecs] = useState([
    { specializationId: "", isPrimary: true, careerStartDate: "" },
  ]);

  const clearMessages = () => {
    setErrorMsg("");
    setSuccessMsg("");
  };

  useEffect(() => {
    clearMessages();
    if (activeTab === "staff") {
      clinicApi.getOffices().then(setOffices);
      fetch("http://localhost:5133/api/clinic/specializations")
        .then((res) => res.json())
        .then(setSpecializations);
    }
    if (activeTab === "schedules") {
      clinicApi.getDoctors(1, 50).then((data) => setDoctors(data.items));
    }
  }, [activeTab]);

  const handleCreateOffice = async (e: React.FormEvent) => {
    e.preventDefault();
    clearMessages();
    setLoading(true);
    try {
      await adminApi.createOffice(officeForm);
      setSuccessMsg("Офис успешно создан!");
      setOfficeForm({ name: "", address: "", phone: "" });
    } catch (err: any) {
      setErrorMsg(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleCreateSpec = async (e: React.FormEvent) => {
    e.preventDefault();
    clearMessages();
    setLoading(true);
    try {
      await adminApi.createSpecialization(specForm);
      setSuccessMsg("Специализация успешно добавлена!");
      setSpecForm({ name: "", description: "" });
    } catch (err: any) {
      setErrorMsg(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleCreateSchedule = async (e: React.FormEvent) => {
    e.preventDefault();
    clearMessages();
    setLoading(true);
    const doc = doctors.find((d) => d.id === scheduleForm.doctorId);
    if (!doc) {
      setErrorMsg("Выберите врача");
      setLoading(false);
      return;
    }

    try {
      await adminApi.createSchedule({
        doctorId: doc.id,
        officeId: doc.officeId,
        workDate: scheduleForm.workDate,
        startTime: scheduleForm.startTime + ":00",
        endTime: scheduleForm.endTime + ":00",
      });
      setSuccessMsg("Смена успешно добавлена в расписание!");
      setScheduleForm({ ...scheduleForm, workDate: "" });
    } catch (err: any) {
      setErrorMsg(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const addSpec = () =>
    setDoctorSpecs([
      ...doctorSpecs,
      { specializationId: "", isPrimary: false, careerStartDate: "" },
    ]);
  const removeSpec = (index: number) =>
    setDoctorSpecs(doctorSpecs.filter((_, i) => i !== index));
  const updateSpec = (index: number, field: string, value: any) => {
    const newSpecs = [...doctorSpecs];
    (newSpecs[index] as any)[field] = value;
    setDoctorSpecs(newSpecs);
  };

  const handleRegisterStaff = async (e: React.FormEvent) => {
    e.preventDefault();
    clearMessages();
    if (staffRole === "doctor" && doctorSpecs.length === 0) {
      setErrorMsg("Необходимо указать хотя бы одну специализацию для врача.");
      return;
    }
    setLoading(true);
    try {
      if (staffRole === "registrar") {
        const res = await adminApi.registerRegistrar({ ...staffForm });
        setSuccessMsg(
          `Регистратор добавлен!\nEmail: ${res.email}\nПароль: ${res.password}`,
        );
      } else {
        const res = await adminApi.registerDoctor({
          ...staffForm,
          specializations: doctorSpecs.map((s) => ({
            specializationId: s.specializationId,
            isPrimary: s.isPrimary,
            careerStartDate: s.careerStartDate || null,
          })),
        });
        setSuccessMsg(
          `Врач добавлен!\nEmail: ${res.email}\nПароль: ${res.password}`,
        );
      }
      setStaffForm({
        firstName: "",
        lastName: "",
        middleName: "",
        phone: "",
        officeId: "",
        bio: "",
      });
      setDoctorSpecs([
        { specializationId: "", isPrimary: true, careerStartDate: "" },
      ]);
    } catch (err: any) {
      setErrorMsg(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-8">
      <div className="flex gap-4 border-b border-slate-200 pb-4 overflow-x-auto">
        <button
          onClick={() => setActiveTab("offices")}
          className={`px-6 py-2.5 rounded-full font-bold transition-all whitespace-nowrap ${activeTab === "offices" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500 hover:bg-slate-100"}`}
        >
          🏥 Офисы
        </button>
        <button
          onClick={() => setActiveTab("specializations")}
          className={`px-6 py-2.5 rounded-full font-bold transition-all whitespace-nowrap ${activeTab === "specializations" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500 hover:bg-slate-100"}`}
        >
          ⚕️ Специализации
        </button>
        <button
          onClick={() => setActiveTab("staff")}
          className={`px-6 py-2.5 rounded-full font-bold transition-all whitespace-nowrap ${activeTab === "staff" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500 hover:bg-slate-100"}`}
        >
          👥 Сотрудники
        </button>
        <button
          onClick={() => setActiveTab("schedules")}
          className={`px-6 py-2.5 rounded-full font-bold transition-all whitespace-nowrap ${activeTab === "schedules" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500 hover:bg-slate-100"}`}
        >
          📅 Расписание
        </button>
      </div>

      {errorMsg && (
        <div className="bg-red-50 text-red-600 p-4 rounded-2xl border border-red-100 font-medium flex items-start gap-3">
          <svg
            className="w-6 h-6 shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            ></path>
          </svg>
          <p>{errorMsg}</p>
        </div>
      )}
      {successMsg && (
        <div className="bg-green-50 text-green-700 p-4 rounded-2xl border border-green-200 font-medium flex items-start gap-3">
          <svg
            className="w-6 h-6 shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
            ></path>
          </svg>
          <p className="whitespace-pre-line leading-relaxed">{successMsg}</p>
        </div>
      )}

      {activeTab === "offices" && (
        <div className="bg-white p-8 rounded-[2rem] border border-slate-200 shadow-sm max-w-xl">
          <h2 className="text-2xl font-bold text-slate-900 mb-6">
            Добавить новый филиал
          </h2>
          <form onSubmit={handleCreateOffice} className="space-y-4">
            <input
              required
              type="text"
              placeholder="Название (например: Главный корпус)"
              value={officeForm.name}
              onChange={(e) =>
                setOfficeForm({ ...officeForm, name: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              required
              type="text"
              placeholder="Адрес"
              value={officeForm.address}
              onChange={(e) =>
                setOfficeForm({ ...officeForm, address: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              required
              type="text"
              placeholder="Контактный телефон"
              value={officeForm.phone}
              onChange={(e) =>
                setOfficeForm({ ...officeForm, phone: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              disabled={loading}
              type="submit"
              className="mt-4 bg-blue-600 text-white px-6 py-3 rounded-xl font-bold hover:bg-blue-700 w-full shadow-lg shadow-blue-200 transition-all"
            >
              Добавить офис
            </button>
          </form>
        </div>
      )}

      {activeTab === "specializations" && (
        <div className="bg-white p-8 rounded-[2rem] border border-slate-200 shadow-sm max-w-xl">
          <h2 className="text-2xl font-bold text-slate-900 mb-6">
            Добавить специализацию
          </h2>
          <form onSubmit={handleCreateSpec} className="space-y-4">
            <input
              required
              type="text"
              placeholder="Название (например: Кардиолог)"
              value={specForm.name}
              onChange={(e) =>
                setSpecForm({ ...specForm, name: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <textarea
              required
              placeholder="Краткое описание"
              value={specForm.description}
              onChange={(e) =>
                setSpecForm({ ...specForm, description: e.target.value })
              }
              rows={3}
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 resize-none"
            />
            <button
              disabled={loading}
              type="submit"
              className="mt-4 bg-blue-600 text-white px-6 py-3 rounded-xl font-bold hover:bg-blue-700 w-full shadow-lg shadow-blue-200 transition-all"
            >
              Добавить специализацию
            </button>
          </form>
        </div>
      )}

      {activeTab === "staff" && (
        <div className="bg-white p-8 rounded-[2rem] border border-slate-200 shadow-sm max-w-3xl">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-2xl font-bold text-slate-900">
              Регистрация сотрудника
            </h2>
            <select
              value={staffRole}
              onChange={(e) => {
                setStaffRole(e.target.value as StaffRole);
                clearMessages();
              }}
              className="px-4 py-2 bg-slate-100 rounded-lg font-bold text-slate-700 outline-none cursor-pointer"
            >
              <option value="registrar">Регистратор</option>
              <option value="doctor">Врач</option>
            </select>
          </div>

          <form
            onSubmit={handleRegisterStaff}
            className="grid grid-cols-1 md:grid-cols-2 gap-4"
          >
            <input
              required
              type="text"
              placeholder="Фамилия"
              value={staffForm.lastName}
              onChange={(e) =>
                setStaffForm({ ...staffForm, lastName: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              required
              type="text"
              placeholder="Имя"
              value={staffForm.firstName}
              onChange={(e) =>
                setStaffForm({ ...staffForm, firstName: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              placeholder="Отчество"
              value={staffForm.middleName}
              onChange={(e) =>
                setStaffForm({ ...staffForm, middleName: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              required
              type="text"
              placeholder="Телефон"
              value={staffForm.phone}
              onChange={(e) =>
                setStaffForm({ ...staffForm, phone: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
            />
            <select
              required
              value={staffForm.officeId}
              onChange={(e) =>
                setStaffForm({ ...staffForm, officeId: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 md:col-span-2 cursor-pointer"
            >
              <option value="">-- Выберите офис --</option>
              {offices.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name} ({o.address})
                </option>
              ))}
            </select>

            {staffRole === "doctor" && (
              <div className="md:col-span-2 space-y-3 bg-slate-50 p-6 rounded-2xl border border-slate-200">
                <div className="flex justify-between items-center mb-2">
                  <h3 className="font-bold text-slate-700">
                    Специализации врача
                  </h3>
                  <button
                    type="button"
                    onClick={addSpec}
                    className="text-blue-600 font-bold text-sm bg-blue-50 px-3 py-1.5 rounded-lg hover:bg-blue-100 transition-colors flex items-center gap-1"
                  >
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth="2"
                        d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                      ></path>
                    </svg>{" "}
                    Добавить
                  </button>
                </div>
                {doctorSpecs.map((spec, index) => (
                  <div
                    key={index}
                    className="flex flex-col sm:flex-row gap-3 items-center bg-white p-3 rounded-xl border border-slate-200 shadow-sm relative"
                  >
                    <select
                      required
                      value={spec.specializationId}
                      onChange={(e) =>
                        updateSpec(index, "specializationId", e.target.value)
                      }
                      className="w-full sm:w-1/2 px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
                    >
                      <option value="">-- Выберите специальность --</option>
                      {specializations.map((s) => (
                        <option key={s.id} value={s.id}>
                          {s.name}
                        </option>
                      ))}
                    </select>
                    <input
                      required
                      type="date"
                      value={spec.careerStartDate}
                      onChange={(e) =>
                        updateSpec(index, "careerStartDate", e.target.value)
                      }
                      title="Дата начала карьеры"
                      className="w-full sm:w-auto px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
                    />
                    <label className="flex items-center gap-2 text-sm font-medium text-slate-600 cursor-pointer whitespace-nowrap">
                      <input
                        type="checkbox"
                        checked={spec.isPrimary}
                        onChange={(e) =>
                          updateSpec(index, "isPrimary", e.target.checked)
                        }
                        className="w-4 h-4 text-blue-600 rounded border-slate-300 focus:ring-blue-500 cursor-pointer"
                      />{" "}
                      Основная
                    </label>
                    {index > 0 && (
                      <button
                        type="button"
                        onClick={() => removeSpec(index)}
                        className="w-8 h-8 flex items-center justify-center bg-red-50 text-red-500 rounded-lg hover:bg-red-100 sm:ml-auto transition-colors"
                        title="Удалить"
                      >
                        <svg
                          className="w-5 h-5"
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth="2"
                            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                          ></path>
                        </svg>
                      </button>
                    )}
                  </div>
                ))}
              </div>
            )}
            {staffRole === "doctor" && (
              <textarea
                required
                placeholder="Биография (описание опыта для пациентов)"
                value={staffForm.bio}
                onChange={(e) =>
                  setStaffForm({ ...staffForm, bio: e.target.value })
                }
                rows={3}
                className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 resize-none md:col-span-2"
              />
            )}
            <button
              disabled={loading}
              type="submit"
              className="mt-4 md:col-span-2 bg-blue-600 text-white px-6 py-4 rounded-xl font-bold hover:bg-blue-700 transition-all text-lg shadow-lg shadow-blue-200"
            >
              {loading
                ? "Регистрация..."
                : `Зарегистрировать ${staffRole === "doctor" ? "врача" : "регистратора"}`}
            </button>
          </form>
        </div>
      )}

      {/* ВКЛАДКА 4: РАСПИСАНИЕ */}
      {activeTab === "schedules" && (
        <div className="bg-white p-8 rounded-[2rem] border border-slate-200 shadow-sm max-w-xl">
          <h2 className="text-2xl font-bold text-slate-900 mb-6">
            Назначить смену
          </h2>
          <form onSubmit={handleCreateSchedule} className="space-y-4">
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Врач
              </label>
              <select
                required
                value={scheduleForm.doctorId}
                onChange={(e) =>
                  setScheduleForm({ ...scheduleForm, doctorId: e.target.value })
                }
                className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
              >
                <option value="">-- Выберите врача --</option>
                {doctors.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.lastName} {d.firstName} ({d.specializations[0]?.name})
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Дата смены
              </label>
              <input
                required
                type="date"
                value={scheduleForm.workDate}
                onChange={(e) =>
                  setScheduleForm({ ...scheduleForm, workDate: e.target.value })
                }
                className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="flex flex-col gap-1.5">
                <label className="text-sm font-semibold text-slate-700 ml-1">
                  Начало
                </label>
                <input
                  required
                  type="time"
                  value={scheduleForm.startTime}
                  onChange={(e) =>
                    setScheduleForm({
                      ...scheduleForm,
                      startTime: e.target.value,
                    })
                  }
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-sm font-semibold text-slate-700 ml-1">
                  Конец
                </label>
                <input
                  required
                  type="time"
                  value={scheduleForm.endTime}
                  onChange={(e) =>
                    setScheduleForm({
                      ...scheduleForm,
                      endTime: e.target.value,
                    })
                  }
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
            <button
              disabled={loading}
              type="submit"
              className="mt-6 bg-blue-600 text-white px-6 py-4 rounded-xl font-bold hover:bg-blue-700 w-full shadow-lg shadow-blue-200 transition-all text-lg"
            >
              {loading ? "Сохранение..." : "Назначить смену"}
            </button>
          </form>
        </div>
      )}
    </div>
  );
}

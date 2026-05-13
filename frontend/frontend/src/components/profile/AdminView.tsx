import { useState, useEffect } from "react";
import { adminApi } from "../../api/adminApi";
import { clinicApi } from "../../api/clinicApi";
import type { OfficeDto } from "../../types/clinic";
import { getErrorMessage } from "../../utils/errorHandler";

type Tab = "offices" | "specializations" | "staff" | "schedules" | "patients";
type StaffRole = "registrar" | "doctor";

export default function AdminView() {
  const [activeTab, setActiveTab] = useState<Tab>("offices");
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");
  const [successMsg, setSuccessMsg] = useState("");

  const [offices, setOffices] = useState<OfficeDto[]>([]);
  const [specializations, setSpecializations] = useState<any[]>([]);
  const [doctors, setDoctors] = useState<any[]>([]);
  const [patients, setPatients] = useState<any[]>([]);

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
      clinicApi.getSpecializations().then(setSpecializations);
    }
    if (activeTab === "schedules") {
      clinicApi.getDoctors(1, 50).then((data) => setDoctors(data.items));
    }
    if (activeTab === "patients") {
      setLoading(true);
      adminApi
        .getPatients()
        .then(setPatients)
        .catch(() => setPatients([]))
        .finally(() => setLoading(false));
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

  const handleRegisterStaff = async (e: React.FormEvent) => {
    e.preventDefault();
    clearMessages();
    if (staffRole === "doctor" && doctorSpecs.length === 0) {
      setErrorMsg("Укажите специализацию");
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

  const handleDeletePatient = async (accountId: string) => {
    if (
      window.confirm(
        "Удалить пациента? Данные будут анонимизированы, а записи отменены.",
      )
    ) {
      try {
        await adminApi.deletePatient(accountId);
        setSuccessMsg("Пациент успешно удален (анонимизирован).");
        adminApi.getPatients().then(setPatients);
      } catch (err: any) {
        setErrorMsg(getErrorMessage(err));
      }
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

  return (
    <div className="space-y-8">
      <div className="flex gap-4 border-b border-slate-200 pb-4 overflow-x-auto">
        <button
          onClick={() => setActiveTab("offices")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "offices" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500"}`}
        >
          🏥 Офисы
        </button>
        <button
          onClick={() => setActiveTab("specializations")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "specializations" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500"}`}
        >
          ⚕️ Специализации
        </button>
        <button
          onClick={() => setActiveTab("staff")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "staff" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500"}`}
        >
          👥 Сотрудники
        </button>
        <button
          onClick={() => setActiveTab("schedules")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "schedules" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500"}`}
        >
          📅 Расписание
        </button>
        <button
          onClick={() => setActiveTab("patients")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "patients" ? "bg-slate-900 text-white shadow-md" : "bg-white text-slate-500"}`}
        >
          👥 База пациентов
        </button>
      </div>

      {errorMsg && (
        <div className="bg-red-50 text-red-600 p-4 rounded-2xl border font-medium">
          {errorMsg}
        </div>
      )}
      {successMsg && (
        <div className="bg-green-50 text-green-700 p-4 rounded-2xl border font-medium whitespace-pre-line">
          {successMsg}
        </div>
      )}

      {activeTab === "offices" && (
        <div className="bg-white p-8 rounded-[2rem] border shadow-sm max-w-xl">
          <h2 className="text-2xl font-bold mb-6">Добавить новый филиал</h2>
          <form onSubmit={handleCreateOffice} className="space-y-4">
            <input
              required
              type="text"
              placeholder="Название"
              value={officeForm.name}
              onChange={(e) =>
                setOfficeForm({ ...officeForm, name: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <input
              required
              type="text"
              placeholder="Адрес"
              value={officeForm.address}
              onChange={(e) =>
                setOfficeForm({ ...officeForm, address: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <input
              required
              type="text"
              placeholder="Телефон"
              value={officeForm.phone}
              onChange={(e) =>
                setOfficeForm({ ...officeForm, phone: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <button
              disabled={loading}
              type="submit"
              className="w-full bg-blue-600 text-white py-3 rounded-xl font-bold"
            >
              Добавить офис
            </button>
          </form>
        </div>
      )}

      {activeTab === "specializations" && (
        <div className="bg-white p-8 rounded-[2rem] border shadow-sm max-w-xl">
          <h2 className="text-2xl font-bold mb-6">Добавить специализацию</h2>
          <form onSubmit={handleCreateSpec} className="space-y-4">
            <input
              required
              type="text"
              placeholder="Название"
              value={specForm.name}
              onChange={(e) =>
                setSpecForm({ ...specForm, name: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <textarea
              required
              placeholder="Описание"
              value={specForm.description}
              onChange={(e) =>
                setSpecForm({ ...specForm, description: e.target.value })
              }
              rows={3}
              className="w-full px-4 py-3 bg-slate-50 border rounded-xl resize-none"
            />
            <button
              disabled={loading}
              type="submit"
              className="w-full bg-blue-600 text-white py-3 rounded-xl font-bold"
            >
              Добавить специализацию
            </button>
          </form>
        </div>
      )}

      {activeTab === "staff" && (
        <div className="bg-white p-8 rounded-[2rem] border shadow-sm max-w-3xl">
          <div className="flex justify-between mb-6">
            <h2 className="text-2xl font-bold">Регистрация сотрудника</h2>
            <select
              value={staffRole}
              onChange={(e) => {
                setStaffRole(e.target.value as StaffRole);
                clearMessages();
              }}
              className="bg-slate-100 px-4 py-2 rounded-lg font-bold"
            >
              <option value="registrar">Регистратор</option>
              <option value="doctor">Врач</option>
            </select>
          </div>
          <form
            onSubmit={handleRegisterStaff}
            className="grid grid-cols-2 gap-4"
          >
            <input
              required
              type="text"
              placeholder="Фамилия"
              value={staffForm.lastName}
              onChange={(e) =>
                setStaffForm({ ...staffForm, lastName: e.target.value })
              }
              className="px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <input
              required
              type="text"
              placeholder="Имя"
              value={staffForm.firstName}
              onChange={(e) =>
                setStaffForm({ ...staffForm, firstName: e.target.value })
              }
              className="px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <input
              type="text"
              placeholder="Отчество"
              value={staffForm.middleName}
              onChange={(e) =>
                setStaffForm({ ...staffForm, middleName: e.target.value })
              }
              className="px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <input
              required
              type="text"
              placeholder="Телефон"
              value={staffForm.phone}
              onChange={(e) =>
                setStaffForm({ ...staffForm, phone: e.target.value })
              }
              className="px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <select
              required
              value={staffForm.officeId}
              onChange={(e) =>
                setStaffForm({ ...staffForm, officeId: e.target.value })
              }
              className="col-span-2 px-4 py-3 bg-slate-50 border rounded-xl"
            >
              <option value="">-- Выберите офис --</option>
              {offices.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </select>
            {staffRole === "doctor" && (
              <div className="col-span-2 space-y-3 bg-slate-50 p-6 rounded-2xl border">
                <div className="flex justify-between">
                  <h3 className="font-bold">Специализации</h3>
                  <button
                    type="button"
                    onClick={addSpec}
                    className="text-blue-600 font-bold"
                  >
                    Добавить
                  </button>
                </div>
                {doctorSpecs.map((spec, index) => (
                  <div
                    key={index}
                    className="flex gap-3 items-center bg-white p-3 rounded-xl border"
                  >
                    <select
                      required
                      value={spec.specializationId}
                      onChange={(e) =>
                        updateSpec(index, "specializationId", e.target.value)
                      }
                      className="w-1/2 p-2 border rounded-lg"
                    >
                      <option value="">Специальность</option>
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
                      className="p-2 border rounded-lg"
                    />
                    <label className="flex items-center gap-1 text-sm">
                      <input
                        type="checkbox"
                        checked={spec.isPrimary}
                        onChange={(e) =>
                          updateSpec(index, "isPrimary", e.target.checked)
                        }
                      />{" "}
                      Осн.
                    </label>
                    {index > 0 && (
                      <button
                        type="button"
                        onClick={() => removeSpec(index)}
                        className="text-red-500 font-bold ml-auto"
                      >
                        X
                      </button>
                    )}
                  </div>
                ))}
                <textarea
                  required
                  placeholder="Биография"
                  value={staffForm.bio}
                  onChange={(e) =>
                    setStaffForm({ ...staffForm, bio: e.target.value })
                  }
                  rows={3}
                  className="w-full px-4 py-3 border rounded-xl mt-3"
                />
              </div>
            )}
            <button
              disabled={loading}
              type="submit"
              className="col-span-2 bg-blue-600 text-white py-4 rounded-xl font-bold mt-4"
            >
              Зарегистрировать
            </button>
          </form>
        </div>
      )}

      {activeTab === "schedules" && (
        <div className="bg-white p-8 rounded-[2rem] border shadow-sm max-w-xl">
          <h2 className="text-2xl font-bold mb-6">Назначить смену</h2>
          <form onSubmit={handleCreateSchedule} className="space-y-4">
            <select
              required
              value={scheduleForm.doctorId}
              onChange={(e) =>
                setScheduleForm({ ...scheduleForm, doctorId: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border rounded-xl"
            >
              <option value="">-- Врач --</option>
              {doctors.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.lastName} {d.firstName}
                </option>
              ))}
            </select>
            <input
              required
              type="date"
              value={scheduleForm.workDate}
              onChange={(e) =>
                setScheduleForm({ ...scheduleForm, workDate: e.target.value })
              }
              className="w-full px-4 py-3 bg-slate-50 border rounded-xl"
            />
            <div className="grid grid-cols-2 gap-4">
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
                className="px-4 py-3 bg-slate-50 border rounded-xl"
              />
              <input
                required
                type="time"
                value={scheduleForm.endTime}
                onChange={(e) =>
                  setScheduleForm({ ...scheduleForm, endTime: e.target.value })
                }
                className="px-4 py-3 bg-slate-50 border rounded-xl"
              />
            </div>
            <button
              disabled={loading}
              type="submit"
              className="w-full bg-blue-600 text-white py-4 rounded-xl font-bold mt-2"
            >
              Назначить смену
            </button>
          </form>
        </div>
      )}

      {activeTab === "patients" && (
        <div className="bg-white rounded-[2rem] border shadow-sm overflow-hidden">
          <div className="p-8 border-b bg-slate-50/50">
            <h2 className="text-2xl font-bold">Управление пациентами</h2>
          </div>
          {loading ? (
            <div className="p-12 text-center text-slate-400">
              Загрузка базы...
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead className="bg-slate-50 text-slate-400 text-[10px] uppercase font-bold tracking-widest">
                  <tr>
                    <th className="p-6">Пациент</th>
                    <th className="p-6">Email</th>
                    <th className="p-6">Паспорт</th>
                    <th className="p-6 text-right">Управление</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {patients.map((p) => (
                    <tr key={p.accountId} className="hover:bg-slate-50/50">
                      <td className="p-6 font-bold">
                        {p.lastName} {p.firstName}
                      </td>
                      <td className="p-6 text-sm text-slate-500">{p.email}</td>
                      <td className="p-6 font-mono text-sm">
                        {p.passportSeriesNumber}
                      </td>
                      <td className="p-6 text-right">
                        <button
                          onClick={() => handleDeletePatient(p.accountId)}
                          className="bg-red-50 text-red-600 px-4 py-2 rounded-xl font-bold text-xs hover:bg-red-100"
                        >
                          Удалить
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {patients.length === 0 && (
                <div className="p-12 text-center text-slate-400 italic">
                  Пусто
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

import { useEffect, useState } from "react";
import {
  verificationApi,
  type VerificationRequestDto,
} from "../../api/verificationApi";
import { adminApi } from "../../api/adminApi";
import {
  appointmentApi,
  type OfficeAppointmentDto,
} from "../../api/appointmentApi";
import VerificationModal from "../VerificationModal";
import UploadLabModal from "./UploadLabModal"; // <-- Добавлен импорт модалки анализов

export default function RegistrarView() {
  const [activeTab, setActiveTab] = useState<"requests" | "patients" | "today">(
    "requests",
  );

  const [requests, setRequests] = useState<VerificationRequestDto[]>([]);
  const [patients, setPatients] = useState<any[]>([]);
  const [todayAppointments, setTodayAppointments] = useState<
    OfficeAppointmentDto[]
  >([]);

  const [loading, setLoading] = useState(true);
  const [editingRequest, setEditingRequest] =
    useState<VerificationRequestDto | null>(null);

  // <-- Добавлен стейт для управления модалкой анализов
  const [uploadLabPatient, setUploadLabPatient] = useState<{
    id: string;
    name: string;
  } | null>(null);

  const loadData = () => {
    setLoading(true);
    if (activeTab === "requests") {
      verificationApi
        .getPendingRequests()
        .then(setRequests)
        .catch(() => setRequests([]))
        .finally(() => setLoading(false));
    } else if (activeTab === "patients") {
      adminApi
        .getPatients()
        .then(setPatients)
        .catch(() => setPatients([]))
        .finally(() => setLoading(false));
    } else if (activeTab === "today") {
      appointmentApi
        .getOfficeAppointmentsToday()
        .then(setTodayAppointments)
        .catch(() => setTodayAppointments([]))
        .finally(() => setLoading(false));
    }
  };

  useEffect(() => {
    loadData();
  }, [activeTab]);

  const handleAction = async (id: string, approve: boolean) => {
    try {
      await verificationApi.processRequest(id, approve);
      alert(approve ? "Заявка одобрена!" : "Заявка отклонена");
      loadData();
    } catch (err) {
      alert("Ошибка при обработке");
    }
  };

  const handleDeletePatient = async (accountId: string) => {
    if (window.confirm("Удалить пациента? Данные будут анонимизированы.")) {
      try {
        await adminApi.deletePatient(accountId);
        loadData();
      } catch (err) {
        alert("Ошибка удаления");
      }
    }
  };

  const handleConfirmArrival = async (appointmentId: string) => {
    try {
      await appointmentApi.confirmAppointment(appointmentId);
      loadData();
    } catch (err) {
      alert("Ошибка при подтверждении");
    }
  };

  return (
    <div className="space-y-6">
      <VerificationModal
        isOpen={!!editingRequest}
        onClose={() => setEditingRequest(null)}
        onSuccess={loadData}
        initialData={editingRequest}
        isStaffMode={true}
      />

      {/* <-- Добавлен рендер модалки для загрузки анализов --> */}
      <UploadLabModal
        isOpen={!!uploadLabPatient}
        onClose={() => setUploadLabPatient(null)}
        patientAccountId={uploadLabPatient?.id || null}
        patientName={uploadLabPatient?.name || ""}
      />

      <div className="flex gap-4 border-b border-slate-200 pb-4 overflow-x-auto">
        <button
          onClick={() => setActiveTab("requests")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "requests" ? "bg-blue-600 text-white shadow-md" : "bg-white text-slate-500 hover:bg-slate-100"}`}
        >
          📥 Новые заявки
        </button>
        <button
          onClick={() => setActiveTab("patients")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "patients" ? "bg-blue-600 text-white shadow-md" : "bg-white text-slate-500 hover:bg-slate-100"}`}
        >
          👥 База пациентов
        </button>
        <button
          onClick={() => setActiveTab("today")}
          className={`px-6 py-2.5 rounded-full font-bold whitespace-nowrap ${activeTab === "today" ? "bg-emerald-600 text-white shadow-md" : "bg-white text-slate-500 hover:bg-slate-100"}`}
        >
          🛎️ Ресепшен (Сегодня)
        </button>
      </div>

      <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm overflow-hidden">
        {loading ? (
          <div className="p-10 text-center text-slate-400">
            Загрузка данных...
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead className="bg-slate-50 text-slate-400 text-[10px] uppercase font-bold tracking-widest">
                {activeTab === "requests" && (
                  <tr>
                    <th className="p-6">Пациент</th>
                    <th className="p-6">Данные паспорта</th>
                    <th className="p-6">Дата визита</th>
                    <th className="p-6 text-right">Действия</th>
                  </tr>
                )}
                {activeTab === "patients" && (
                  <tr>
                    <th className="p-6">Пациент</th>
                    <th className="p-6">Email</th>
                    <th className="p-6">Паспорт</th>
                    <th className="p-6 text-right">Управление</th>
                  </tr>
                )}
                {activeTab === "today" && (
                  <tr>
                    <th className="p-6">Время</th>
                    <th className="p-6">Пациент</th>
                    <th className="p-6">Врач</th>
                    <th className="p-6">Статус</th>
                    <th className="p-6 text-right">Действие</th>
                  </tr>
                )}
              </thead>
              <tbody className="divide-y divide-slate-100">
                {activeTab === "requests" &&
                  requests.map((req) => (
                    <tr key={req.id} className="hover:bg-slate-50/50">
                      <td className="p-6 text-slate-900 font-bold">
                        {req.lastName} {req.firstName}
                      </td>
                      <td className="p-6 font-mono text-sm">
                        {req.passportSeriesNumber}
                      </td>
                      <td className="p-6 text-sm">
                        {new Date(req.scheduledAt).toLocaleString()}
                      </td>
                      <td className="p-6 text-right space-x-2 whitespace-nowrap">
                        <button
                          onClick={() => setEditingRequest(req)}
                          className="bg-blue-50 text-blue-600 px-4 py-2 rounded-xl font-bold text-xs hover:bg-blue-100"
                        >
                          Изменить
                        </button>
                        <button
                          onClick={() => handleAction(req.id, true)}
                          className="bg-green-100 text-green-700 px-4 py-2 rounded-xl font-bold text-xs hover:bg-green-200"
                        >
                          Одобрить
                        </button>
                        <button
                          onClick={() => handleAction(req.id, false)}
                          className="bg-red-50 text-red-600 px-4 py-2 rounded-xl font-bold text-xs hover:bg-red-100"
                        >
                          Отклонить
                        </button>
                      </td>
                    </tr>
                  ))}

                {activeTab === "patients" &&
                  patients.map((p) => (
                    <tr key={p.accountId} className="hover:bg-slate-50/50">
                      <td className="p-6 text-slate-900 font-bold">
                        {p.lastName} {p.firstName}
                      </td>
                      <td className="p-6 text-sm text-slate-500">{p.email}</td>
                      <td className="p-6 font-mono text-sm">
                        {p.passportSeriesNumber}
                      </td>
                      <td className="p-6 text-right space-x-2 whitespace-nowrap">
                        {/* <-- Добавлена кнопка "+ Анализ" --> */}
                        <button
                          onClick={() =>
                            setUploadLabPatient({
                              id: p.accountId,
                              name: `${p.lastName} ${p.firstName}`,
                            })
                          }
                          className="bg-blue-50 text-blue-600 px-4 py-2 rounded-xl font-bold text-xs hover:bg-blue-100"
                        >
                          + Анализ
                        </button>
                        <button
                          onClick={() => handleDeletePatient(p.accountId)}
                          className="bg-red-50 text-red-600 px-4 py-2 rounded-xl font-bold text-xs hover:bg-red-100"
                        >
                          Удалить
                        </button>
                      </td>
                    </tr>
                  ))}

                {activeTab === "today" &&
                  todayAppointments.map((app) => (
                    <tr key={app.id} className="hover:bg-slate-50/50">
                      <td className="p-6 font-bold">
                        {new Date(app.scheduledStart).toLocaleTimeString([], {
                          hour: "2-digit",
                          minute: "2-digit",
                        })}
                      </td>
                      <td className="p-6 text-slate-900 font-medium">
                        {app.patientName}
                      </td>
                      <td className="p-6 text-sm text-slate-500">
                        {app.doctorName}
                      </td>
                      <td className="p-6">
                        <span
                          className={`px-3 py-1 text-xs font-bold rounded-full ${app.status === "confirmed" ? "bg-amber-100 text-amber-700" : app.status === "completed" ? "bg-slate-100 text-slate-500" : "bg-blue-100 text-blue-700"}`}
                        >
                          {app.status}
                        </span>
                      </td>
                      <td className="p-6 text-right">
                        {app.status === "planned" && (
                          <button
                            onClick={() => handleConfirmArrival(app.id)}
                            className="bg-emerald-100 text-emerald-700 px-4 py-2 rounded-xl font-bold text-xs hover:bg-emerald-200"
                          >
                            Подтвердить прибытие
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
            {((activeTab === "requests" && requests.length === 0) ||
              (activeTab === "patients" && patients.length === 0) ||
              (activeTab === "today" && todayAppointments.length === 0)) && (
              <div className="p-10 text-center text-slate-400 italic">
                Данных пока нет
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

import { useEffect, useState } from "react";
import { doctorApi, type DoctorAppointmentDto } from "../../api/doctorApi";
import MedicalRecordModal from "./MedicalRecordModal";

const getStatusStyle = (status: string) => {
  const map: Record<string, { text: string; color: string }> = {
    planned: { text: "Запланирован", color: "bg-blue-100 text-blue-700" },
    confirmed: {
      text: "Ожидает",
      color: "bg-amber-100 text-amber-700 animate-pulse",
    },
    completed: { text: "Завершен", color: "bg-slate-100 text-slate-500" },
    cancelled: { text: "Отменен", color: "bg-red-50 text-red-500" },
    no_show: { text: "Неявка", color: "bg-slate-100 text-slate-400" },
  };
  return map[status] || { text: status, color: "bg-slate-100 text-slate-700" };
};

export default function DoctorView() {
  const [date, setDate] = useState(new Date().toISOString().split("T")[0]);
  const [appointments, setAppointments] = useState<DoctorAppointmentDto[]>([]);
  const [loading, setLoading] = useState(false);

  // Стейт для активного приема (открытие модалки)
  const [activeAppointment, setActiveAppointment] =
    useState<DoctorAppointmentDto | null>(null);

  const loadAppointments = () => {
    setLoading(true);
    doctorApi
      .getMyAppointments(date)
      .then(setAppointments)
      .catch(() => setAppointments([]))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadAppointments();
  }, [date]);

  return (
    <div className="space-y-6">
      {/* Рендерим модалку заполнения медкарты */}
      <MedicalRecordModal
        isOpen={!!activeAppointment}
        onClose={() => setActiveAppointment(null)}
        onSuccess={() => {
          setActiveAppointment(null);
          loadAppointments(); // Перезагружаем список, чтобы статус стал "Завершен"
        }}
        appointment={activeAppointment}
      />

      <div className="bg-white p-8 rounded-[2rem] border border-slate-200 shadow-sm flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold text-slate-900">Мое расписание</h2>
          <p className="text-slate-500 mt-1">
            Просмотр записей пациентов на выбранный день
          </p>
        </div>
        <input
          type="date"
          value={date}
          onChange={(e) => setDate(e.target.value)}
          className="px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 font-bold text-slate-700 cursor-pointer"
        />
      </div>

      <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm overflow-hidden">
        {loading ? (
          <div className="p-12 text-center text-slate-400 font-medium">
            Загрузка расписания...
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead className="bg-slate-50 text-slate-400 text-[10px] uppercase font-bold tracking-widest">
                <tr>
                  <th className="p-6">Время</th>
                  <th className="p-6">Пациент</th>
                  <th className="p-6">Услуга</th>
                  <th className="p-6">Статус</th>
                  <th className="p-6 text-right">Действия</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {appointments.map((app) => {
                  const style = getStatusStyle(app.status);
                  const startTime = new Date(
                    app.scheduledStart,
                  ).toLocaleTimeString([], {
                    hour: "2-digit",
                    minute: "2-digit",
                  });
                  const endTime = new Date(app.scheduledEnd).toLocaleTimeString(
                    [],
                    { hour: "2-digit", minute: "2-digit" },
                  );

                  return (
                    <tr
                      key={app.id}
                      className="hover:bg-slate-50/50 transition-colors"
                    >
                      <td className="p-6 whitespace-nowrap">
                        <span className="font-bold text-slate-900 block">
                          {startTime}
                        </span>
                        <span className="text-xs text-slate-400 block mt-0.5">
                          до {endTime}
                        </span>
                      </td>
                      <td className="p-6">
                        <span className="font-bold text-slate-900 block">
                          {app.patientName}
                        </span>
                        <span className="text-xs text-slate-500 block mt-0.5">
                          {app.patientBirthDate} • {app.patientGender}
                        </span>
                      </td>
                      <td className="p-6 text-sm font-medium text-slate-700">
                        {app.typeName}
                      </td>
                      <td className="p-6">
                        <span
                          className={`px-3 py-1 text-xs font-bold rounded-full ${style.color}`}
                        >
                          {style.text}
                        </span>
                      </td>
                      <td className="p-6 text-right">
                        <button
                          onClick={() => setActiveAppointment(app)}
                          disabled={
                            app.status === "completed" ||
                            app.status === "cancelled"
                          }
                          className="bg-blue-50 text-blue-600 px-4 py-2 rounded-xl font-bold text-sm hover:bg-blue-100 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          Начать прием
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
            {appointments.length === 0 && (
              <div className="p-12 text-center text-slate-400 italic">
                На эту дату записей нет.
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

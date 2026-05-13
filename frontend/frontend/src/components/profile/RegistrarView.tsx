// src/components/profile/RegistrarView.tsx
import { useEffect, useState } from "react";
import {
  verificationApi,
  type VerificationRequestDto,
} from "../../api/verificationApi";

export default function RegistrarView() {
  const [requests, setRequests] = useState<VerificationRequestDto[]>([]);
  const [loading, setLoading] = useState(true);

  const loadRequests = () => {
    setLoading(true);
    verificationApi
      .getPendingRequests()
      .then(setRequests)
      .catch(() => setRequests([]))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadRequests();
  }, []);

  const handleAction = async (id: string, approve: boolean) => {
    try {
      await verificationApi.processRequest(id, approve);
      alert(approve ? "Заявка одобрена!" : "Заявка отклонена");
      loadRequests();
    } catch (err) {
      alert("Ошибка при обработке");
    }
  };

  if (loading)
    return (
      <div className="p-10 text-center text-slate-400">Загрузка заявок...</div>
    );

  return (
    <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm overflow-hidden">
      <div className="p-8 border-b border-slate-100 bg-slate-50/50">
        <h2 className="text-xl font-bold text-slate-800 flex items-center gap-2">
          <span className="w-2 h-6 bg-blue-600 rounded-full"></span>
          Заявки на верификацию ({requests.length})
        </h2>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead className="bg-slate-50 text-slate-400 text-[10px] uppercase font-bold tracking-widest">
            <tr>
              <th className="p-6">Пациент</th>
              <th className="p-6">Данные паспорта</th>
              <th className="p-6">Дата визита</th>
              <th className="p-6 text-right">Действия</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {requests.map((req) => (
              <tr
                key={req.id}
                className="hover:bg-slate-50/50 transition-colors"
              >
                <td className="p-6 text-slate-900 font-bold">
                  {req.lastName} {req.firstName}
                </td>
                <td className="p-6 font-mono text-sm">
                  {req.passportSeriesNumber} / {req.personalNumber}
                </td>
                <td className="p-6 text-sm">
                  {new Date(req.scheduledAt).toLocaleString()}
                </td>
                <td className="p-6 text-right space-x-2">
                  <button
                    onClick={() => handleAction(req.id, true)}
                    className="bg-green-100 text-green-700 px-4 py-2 rounded-xl font-bold text-xs hover:bg-green-200 transition-colors"
                  >
                    Одобрить
                  </button>
                  <button
                    onClick={() => handleAction(req.id, false)}
                    className="bg-red-50 text-red-600 px-4 py-2 rounded-xl font-bold text-xs hover:bg-red-100 transition-colors"
                  >
                    Отклонить
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {requests.length === 0 && (
          <div className="p-10 text-center text-slate-400 italic">
            Новых заявок пока нет
          </div>
        )}
      </div>
    </div>
  );
}

import { useEffect, useState } from "react";
import { patientApi, type MedicalRecordDto } from "../../api/patientApi";

export default function MedicalHistory() {
  const [history, setHistory] = useState<MedicalRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  useEffect(() => {
    patientApi
      .getMedicalHistory()
      .then(setHistory)
      .finally(() => setLoading(false));
  }, []);

  if (loading)
    return (
      <div className="text-center p-10 text-slate-400">
        Загрузка медкарты...
      </div>
    );

  return (
    <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm overflow-hidden">
      <div className="p-8 border-b border-slate-100 bg-slate-50/50">
        <h2 className="text-xl font-bold text-slate-800 flex items-center gap-2">
          <span className="w-2 h-6 bg-emerald-500 rounded-full"></span>
          Моя электронная медкарта
        </h2>
      </div>

      <div className="p-8 space-y-4">
        {history.length === 0 ? (
          <div className="text-center py-12 text-slate-400">
            В вашей медкарте пока нет записей.
          </div>
        ) : (
          history.map((record) => (
            <div
              key={record.id}
              className="border border-slate-100 rounded-2xl overflow-hidden transition-all hover:border-blue-200"
            >
              <button
                onClick={() =>
                  setExpandedId(expandedId === record.id ? null : record.id)
                }
                className="w-full flex items-center justify-between p-6 text-left hover:bg-slate-50/50 transition-colors"
              >
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 bg-blue-50 text-blue-600 rounded-xl flex items-center justify-center font-black">
                    {new Date(record.date).getDate()}
                  </div>
                  <div>
                    <p className="text-xs font-bold text-slate-400 uppercase tracking-widest">
                      {new Date(record.date).toLocaleDateString("ru-RU", {
                        month: "long",
                        year: "numeric",
                      })}
                    </p>
                    <h3 className="font-bold text-slate-900">
                      {record.doctorName}
                    </h3>
                    <p className="text-sm text-blue-600 font-medium">
                      {record.specialization}
                    </p>
                  </div>
                </div>
                <div
                  className={`transform transition-transform ${expandedId === record.id ? "rotate-180" : ""}`}
                >
                  <svg
                    className="w-6 h-6 text-slate-300"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      d="M19 9l-7 7-7-7"
                    />
                  </svg>
                </div>
              </button>

              {expandedId === record.id && (
                <div className="p-8 bg-slate-50/30 border-t border-slate-100 grid grid-cols-1 md:grid-cols-2 gap-8 animate-in fade-in slide-in-from-top-2">
                  <div className="space-y-6">
                    <div>
                      <h4 className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">
                        🗣️ Жалобы
                      </h4>
                      <p className="text-slate-700 leading-relaxed whitespace-pre-wrap">
                        {record.complaints}
                      </p>
                    </div>
                    <div>
                      <h4 className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">
                        🩺 Объективный статус
                      </h4>
                      <p className="text-slate-700 leading-relaxed whitespace-pre-wrap">
                        {record.objectiveData}
                      </p>
                    </div>
                  </div>
                  <div className="space-y-6">
                    <div className="bg-amber-50/50 p-4 rounded-xl border border-amber-100">
                      <h4 className="text-[10px] font-black text-amber-600 uppercase tracking-widest mb-2">
                        📋 Диагноз
                      </h4>
                      <p className="text-slate-900 font-bold leading-relaxed">
                        {record.assessment}
                      </p>
                    </div>
                    <div className="bg-green-50/50 p-4 rounded-xl border border-green-100">
                      <h4 className="text-[10px] font-black text-green-600 uppercase tracking-widest mb-2">
                        📝 План лечения
                      </h4>
                      <p className="text-slate-700 leading-relaxed whitespace-pre-wrap">
                        {record.plan}
                      </p>
                    </div>
                  </div>
                </div>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
}

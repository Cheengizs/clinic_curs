import { useState } from "react";
import { doctorApi, type DoctorAppointmentDto } from "../../api/doctorApi";
import { getErrorMessage } from "../../utils/errorHandler";

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  appointment: DoctorAppointmentDto | null;
}

export default function MedicalRecordModal({
  isOpen,
  onClose,
  onSuccess,
  appointment,
}: Props) {
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");

  const [form, setForm] = useState({
    complaints: "",
    objectiveData: "",
    assessment: "",
    plan: "",
  });

  if (!isOpen || !appointment) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setErrorMsg("");

    try {
      await doctorApi.completeAppointment(appointment.id, form);
      alert("Прием успешно завершен! Медкарта пациента обновлена.");

      // Очищаем форму
      setForm({ complaints: "", objectiveData: "", assessment: "", plan: "" });
      onSuccess();
    } catch (err: any) {
      setErrorMsg(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm">
      <div className="bg-white rounded-[2rem] w-full max-w-4xl max-h-[95vh] overflow-y-auto shadow-2xl">
        <div className="p-8 md:p-10">
          <div className="flex justify-between items-center mb-8 border-b border-slate-100 pb-6">
            <div>
              <h2 className="text-3xl font-black text-slate-900">
                Медицинская запись
              </h2>
              <p className="text-slate-500 mt-2 font-medium">
                Пациент:{" "}
                <span className="text-slate-900 font-bold">
                  {appointment.patientName}
                </span>
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-slate-400 hover:text-slate-600 text-3xl"
            >
              &times;
            </button>
          </div>

          {errorMsg && (
            <div className="mb-6 p-4 bg-red-50 text-red-600 rounded-xl text-sm font-medium border border-red-100">
              {errorMsg}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Левая колонка */}
              <div className="space-y-6">
                <div className="flex flex-col gap-2">
                  <label className="text-sm font-bold text-slate-700 uppercase tracking-wider flex items-center gap-2">
                    🗣️ Жалобы
                  </label>
                  <textarea
                    required
                    rows={5}
                    placeholder="На что жалуется пациент..."
                    className="p-4 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500 resize-none text-slate-700"
                    value={form.complaints}
                    onChange={(e) =>
                      setForm({ ...form, complaints: e.target.value })
                    }
                  />
                </div>

                <div className="flex flex-col gap-2">
                  <label className="text-sm font-bold text-slate-700 uppercase tracking-wider flex items-center gap-2">
                    🩺 Объективный статус
                  </label>
                  <textarea
                    required
                    rows={5}
                    placeholder="АД, ЧСС, температура, результаты осмотра..."
                    className="p-4 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500 resize-none text-slate-700"
                    value={form.objectiveData}
                    onChange={(e) =>
                      setForm({ ...form, objectiveData: e.target.value })
                    }
                  />
                </div>
              </div>

              {/* Правая колонка */}
              <div className="space-y-6">
                <div className="flex flex-col gap-2">
                  <label className="text-sm font-bold text-slate-700 uppercase tracking-wider flex items-center gap-2">
                    📋 Оценка / Диагноз
                  </label>
                  <textarea
                    required
                    rows={5}
                    placeholder="Предварительный или окончательный диагноз..."
                    className="p-4 bg-amber-50/50 border border-amber-200/50 rounded-2xl outline-none focus:ring-2 focus:ring-amber-500 resize-none text-slate-700"
                    value={form.assessment}
                    onChange={(e) =>
                      setForm({ ...form, assessment: e.target.value })
                    }
                  />
                </div>

                <div className="flex flex-col gap-2">
                  <label className="text-sm font-bold text-slate-700 uppercase tracking-wider flex items-center gap-2">
                    📝 План лечения и рекомендации
                  </label>
                  <textarea
                    required
                    rows={5}
                    placeholder="Назначенные препараты, анализы, процедуры..."
                    className="p-4 bg-green-50/50 border border-green-200/50 rounded-2xl outline-none focus:ring-2 focus:ring-green-500 resize-none text-slate-700"
                    value={form.plan}
                    onChange={(e) => setForm({ ...form, plan: e.target.value })}
                  />
                </div>
              </div>
            </div>

            <button
              disabled={loading}
              type="submit"
              className="mt-8 w-full bg-slate-900 text-white py-4 rounded-2xl font-bold text-lg hover:bg-slate-800 transition-all shadow-lg hover:shadow-xl hover:-translate-y-0.5"
            >
              {loading
                ? "Сохранение..."
                : "Завершить прием и сохранить в медкарту"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

import { useState, useEffect } from "react";
import { clinicApi } from "../api/clinicApi";
import {
  verificationApi,
  type SubmitVerificationDto,
  type VerificationRequestDto,
} from "../api/verificationApi";
import type { OfficeDto } from "../types/clinic";

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  initialData?: VerificationRequestDto | null; // Данные для редактирования
}

export default function VerificationModal({
  isOpen,
  onClose,
  onSuccess,
  initialData,
}: Props) {
  const [offices, setOffices] = useState<OfficeDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [form, setForm] = useState<SubmitVerificationDto>({
    firstName: "",
    lastName: "",
    middleName: "",
    birthDate: "",
    passportSeriesNumber: "",
    personalNumber: "",
    officeId: "",
    scheduledAt: "",
  });

  useEffect(() => {
    if (isOpen) {
      clinicApi.getOffices().then(setOffices);

      if (initialData) {
        // Предзаполняем форму данными из существующей заявки
        setForm({
          firstName: initialData.firstName,
          lastName: initialData.lastName,
          middleName: initialData.middleName || "",
          birthDate: initialData.birthDate,
          passportSeriesNumber: initialData.passportSeriesNumber,
          personalNumber: initialData.personalNumber,
          officeId: initialData.officeId,
          // Форматируем дату для инпута datetime-local (YYYY-MM-DDTHH:mm)
          scheduledAt: new Date(initialData.scheduledAt)
            .toLocaleString("sv-SE")
            .replace(" ", "T")
            .slice(0, 16),
        });
      } else {
        // Сбрасываем форму для новой записи
        setForm({
          firstName: "",
          lastName: "",
          middleName: "",
          birthDate: "",
          passportSeriesNumber: "",
          personalNumber: "",
          officeId: "",
          scheduledAt: "",
        });
      }
    }
  }, [isOpen, initialData]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    const dataToSend = {
      ...form,
      scheduledAt: new Date(form.scheduledAt).toISOString(),
    };

    try {
      if (initialData) {
        await verificationApi.updateRequest(dataToSend);
        alert("Заявка успешно обновлена!");
      } else {
        await verificationApi.submitRequest(dataToSend);
        alert("Заявка успешно создана!");
      }
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.error || "Ошибка при сохранении данных");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm">
      <div className="bg-white rounded-[2.5rem] w-full max-w-2xl max-h-[90vh] overflow-y-auto shadow-2xl">
        <div className="p-8 md:p-12">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-black text-slate-900">
              {initialData ? "Редактирование записи" : "Запись на верификацию"}
            </h2>
            <button
              onClick={onClose}
              className="text-slate-400 hover:text-slate-600 transition-colors text-2xl"
            >
              &times;
            </button>
          </div>

          {error && (
            <div className="mb-6 p-4 bg-red-50 text-red-600 rounded-2xl border border-red-100 text-sm font-medium">
              {error}
            </div>
          )}

          <form
            onSubmit={handleSubmit}
            className="grid grid-cols-1 md:grid-cols-2 gap-6"
          >
            <div className="space-y-4 md:col-span-2">
              <h3 className="text-sm font-bold uppercase tracking-widest text-blue-600">
                Личные данные
              </h3>
            </div>

            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Фамилия
              </label>
              <input
                required
                type="text"
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Имя
              </label>
              <input
                required
                type="text"
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.firstName}
                onChange={(e) =>
                  setForm({ ...form, firstName: e.target.value })
                }
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Отчество
              </label>
              <input
                required
                type="text"
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.middleName}
                onChange={(e) =>
                  setForm({ ...form, middleName: e.target.value })
                }
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Дата рождения
              </label>
              <input
                required
                type="date"
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.birthDate}
                onChange={(e) =>
                  setForm({ ...form, birthDate: e.target.value })
                }
              />
            </div>

            <div className="space-y-4 md:col-span-2 mt-4">
              <h3 className="text-sm font-bold uppercase tracking-widest text-blue-600">
                Документы
              </h3>
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Серия и номер паспорта
              </label>
              <input
                required
                type="text"
                placeholder="MP1234567"
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.passportSeriesNumber}
                onChange={(e) =>
                  setForm({ ...form, passportSeriesNumber: e.target.value })
                }
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Идентификационный номер
              </label>
              <input
                required
                type="text"
                placeholder="14 цифр"
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.personalNumber}
                onChange={(e) =>
                  setForm({ ...form, personalNumber: e.target.value })
                }
              />
            </div>

            <div className="space-y-4 md:col-span-2 mt-4">
              <h3 className="text-sm font-bold uppercase tracking-widest text-blue-600">
                Место и время
              </h3>
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Выберите офис
              </label>
              <select
                required
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.officeId}
                onChange={(e) => setForm({ ...form, officeId: e.target.value })}
              >
                <option value="">Не выбран</option>
                {offices.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name} ({o.address})
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-semibold text-slate-700 ml-1">
                Дата и время визита
              </label>
              <input
                required
                type="datetime-local"
                className="px-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl outline-none focus:ring-2 focus:ring-blue-500"
                value={form.scheduledAt}
                onChange={(e) =>
                  setForm({ ...form, scheduledAt: e.target.value })
                }
              />
            </div>

            <button
              disabled={loading}
              type="submit"
              className="md:col-span-2 mt-8 w-full bg-blue-600 text-white py-4 rounded-2xl font-bold text-lg hover:bg-blue-700 transition-all shadow-xl shadow-blue-200"
            >
              {loading
                ? "Сохранение..."
                : initialData
                  ? "Обновить данные"
                  : "Отправить заявку"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

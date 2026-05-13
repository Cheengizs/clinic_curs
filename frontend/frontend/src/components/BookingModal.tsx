import { useState, useEffect } from "react";
import { appointmentApi } from "../api/appointmentApi";
import { clinicApi } from "../api/clinicApi";
import type { DoctorDto, AppointmentTypeDto } from "../types/clinic";
import { getErrorMessage } from "../utils/errorHandler";

interface Props {
  isOpen: boolean;
  onClose: () => void;
  doctor: DoctorDto | null;
}

const getCategoryName = (category: string | number) => {
  const dict: Record<string, string> = {
    "0": "Первичный прием",
    initial_consultation: "Первичный прием",
    Initial_consultation: "Первичный прием",
    "1": "Повторный прием",
    follow_up: "Повторный прием",
    Follow_up: "Повторный прием",
    "2": "Диагностика",
    diagnostic: "Диагностика",
    Diagnostic: "Диагностика",
    "3": "Процедура",
    procedure: "Процедура",
    Procedure: "Процедура",
    "4": "Вакцинация",
    vaccination: "Вакцинация",
    Vaccination: "Вакцинация",
  };
  return dict[category.toString()] || "Прием врача";
};

export default function BookingModal({ isOpen, onClose, doctor }: Props) {
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const defaultDate = tomorrow.toISOString().split("T")[0];

  const [selectedDate, setSelectedDate] = useState(defaultDate);
  const [slots, setSlots] = useState<string[]>([]);
  const [selectedSlot, setSelectedSlot] = useState("");

  const [types, setTypes] = useState<AppointmentTypeDto[]>([]);
  const [selectedType, setSelectedType] = useState("");

  const [loading, setLoading] = useState(false);
  const [slotsLoading, setSlotsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");
  const [successMsg, setSuccessMsg] = useState("");

  // 1. При открытии модалки загружаем только типы (один раз)
  useEffect(() => {
    if (isOpen) {
      clinicApi.getAppointmentTypes().then((data) => {
        setTypes(data);
        if (data.length > 0) setSelectedType(data[0].id);
      });
    } else {
      // Сброс состояний при закрытии
      setSuccessMsg("");
      setErrorMsg("");
      setSelectedSlot("");
    }
  }, [isOpen]);

  // 2. Как только есть врач, дата и ТИП услуги — грузим слоты
  useEffect(() => {
    if (isOpen && doctor && selectedType) {
      setErrorMsg("");
      setSelectedSlot(""); // Сбрасываем выбранный слот, если поменялась услуга или дата
      loadSlots(selectedDate, selectedType);
    }
  }, [isOpen, selectedDate, doctor, selectedType]);

  const loadSlots = async (date: string, typeId: string) => {
    if (!doctor) return;
    setSlotsLoading(true);
    try {
      const available = await appointmentApi.getAvailableSlots(
        doctor.id,
        date,
        typeId,
      );
      setSlots(available);
    } catch (err: any) {
      setErrorMsg(getErrorMessage(err));
      setSlots([]);
    } finally {
      setSlotsLoading(false);
    }
  };

  const handleBook = async () => {
    if (!doctor || !selectedSlot || !selectedType) return;
    setLoading(true);
    setErrorMsg("");

    const scheduledStart = `${selectedDate}T${selectedSlot}:00`;

    try {
      await appointmentApi.bookAppointment(
        doctor.id,
        selectedType,
        scheduledStart,
      );
      setSuccessMsg("Вы успешно записаны на прием!");
      loadSlots(selectedDate, selectedType);
      setSelectedSlot("");
    } catch (err: any) {
      setErrorMsg(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen || !doctor) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm">
      <div className="bg-white rounded-[2rem] w-full max-w-lg shadow-2xl overflow-hidden">
        <div className="p-8">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-2xl font-black text-slate-900">
              Запись к врачу
            </h2>
            <button
              onClick={onClose}
              className="text-slate-400 hover:text-slate-600 text-2xl"
            >
              &times;
            </button>
          </div>

          <div className="flex items-center gap-4 mb-6 p-4 bg-blue-50 rounded-2xl">
            <img
              src={`http://localhost:5133/api/files/${doctor.avatarUrl}`}
              alt="Doctor"
              className="w-12 h-12 rounded-full object-cover border-2 border-white shadow-sm"
              onError={(e) =>
                ((e.target as HTMLImageElement).src =
                  "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?w=100")
              }
            />
            <div>
              <p className="font-bold text-slate-900">
                {doctor.lastName} {doctor.firstName}
              </p>
              <p className="text-xs font-medium text-blue-600 uppercase tracking-wider">
                {doctor.specializations[0]?.name}
              </p>
            </div>
          </div>

          {errorMsg && (
            <div className="mb-4 p-4 bg-red-50 text-red-600 rounded-xl text-sm font-medium border border-red-100">
              {errorMsg}
            </div>
          )}
          {successMsg && (
            <div className="mb-4 p-4 bg-green-50 text-green-700 rounded-xl text-sm font-medium border border-green-200">
              {successMsg}
            </div>
          )}

          {!successMsg && (
            <>
              <div className="mb-4">
                <label className="block text-sm font-bold text-slate-700 mb-2">
                  Вид записи
                </label>
                <select
                  value={selectedType}
                  onChange={(e) => setSelectedType(e.target.value)}
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 font-medium text-slate-700 cursor-pointer"
                >
                  {types.map((t) => (
                    <option key={t.id} value={t.id}>
                      {getCategoryName(t.category)} ({t.defaultDurationMinutes}{" "}
                      мин)
                    </option>
                  ))}
                </select>
              </div>

              <div className="mb-6">
                <label className="block text-sm font-bold text-slate-700 mb-2">
                  Выберите дату
                </label>
                <input
                  type="date"
                  min={defaultDate}
                  value={selectedDate}
                  onChange={(e) => setSelectedDate(e.target.value)}
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 font-medium text-slate-700 cursor-pointer"
                />
              </div>

              <div className="mb-8">
                <label className="block text-sm font-bold text-slate-700 mb-2">
                  Доступное время
                </label>
                {slotsLoading ? (
                  <div className="text-slate-400 text-sm animate-pulse">
                    Поиск свободных окошек...
                  </div>
                ) : slots.length > 0 ? (
                  <div className="grid grid-cols-4 gap-2 max-h-48 overflow-y-auto pr-2">
                    {slots.map((slot) => (
                      <button
                        key={slot}
                        onClick={() => setSelectedSlot(slot)}
                        className={`py-2 rounded-lg font-bold text-sm transition-all border ${selectedSlot === slot ? "bg-blue-600 text-white border-blue-600 shadow-md shadow-blue-200" : "bg-white text-slate-600 border-slate-200 hover:border-blue-300 hover:text-blue-600"}`}
                      >
                        {slot}
                      </button>
                    ))}
                  </div>
                ) : (
                  <div className="p-4 bg-slate-50 rounded-xl text-center text-slate-500 text-sm border border-slate-200">
                    На эту дату нет расписания или свободных окошек для данной
                    услуги.
                  </div>
                )}
              </div>

              <button
                onClick={handleBook}
                disabled={loading || !selectedSlot || !selectedType}
                className="w-full bg-slate-900 text-white py-4 rounded-xl font-bold text-lg hover:bg-slate-800 transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg"
              >
                {loading ? "Бронируем..." : "Подтвердить запись"}
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

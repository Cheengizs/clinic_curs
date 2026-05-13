import { useEffect, useState } from "react";
import {
  appointmentApi,
  type PatientAppointmentDto,
} from "../../api/appointmentApi";
import { getErrorMessage } from "../../utils/errorHandler";

const getStatusStyle = (status: string) => {
  const map: Record<string, { text: string; color: string }> = {
    planned: {
      text: "Запланирован",
      color: "bg-blue-100 text-blue-700 border-blue-200",
    },
    confirmed: {
      text: "Подтвержден",
      color: "bg-emerald-100 text-emerald-700 border-emerald-200",
    },
    completed: {
      text: "Завершен",
      color: "bg-slate-100 text-slate-700 border-slate-200",
    },
    cancelled: {
      text: "Отменен",
      color: "bg-red-100 text-red-700 border-red-200",
    },
    no_show: {
      text: "Неявка",
      color: "bg-amber-100 text-amber-700 border-amber-200",
    },
  };
  return (
    map[status] || {
      text: status,
      color: "bg-slate-100 text-slate-700 border-slate-200",
    }
  );
};

export default function PatientAppointments() {
  const [appointments, setAppointments] = useState<PatientAppointmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"upcoming" | "past">("upcoming");

  // Стейты для модалки отзыва
  const [reviewAppId, setReviewAppId] = useState<string | null>(null);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [reviewLoading, setReviewLoading] = useState(false);

  const loadAppointments = () => {
    setLoading(true);
    appointmentApi
      .getMyAppointments()
      .then(setAppointments)
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadAppointments();
  }, []);

  const handleCancel = async (id: string) => {
    if (!window.confirm("Вы уверены, что хотите отменить эту запись?")) return;
    try {
      await appointmentApi.cancelAppointment(id);
      loadAppointments();
    } catch (e) {
      alert("Ошибка при отмене записи");
    }
  };

  const submitReview = async () => {
    if (!reviewAppId) return;
    setReviewLoading(true);
    try {
      await appointmentApi.leaveReview(reviewAppId, rating, comment);
      alert("Спасибо за ваш отзыв! Рейтинг врача обновлен.");
      setReviewAppId(null);
      setRating(5);
      setComment("");
      loadAppointments(); // Чтобы пропала кнопка "Оценить"
    } catch (err: any) {
      alert(getErrorMessage(err));
    } finally {
      setReviewLoading(false);
    }
  };

  const upcoming = appointments.filter((a) =>
    ["planned", "confirmed"].includes(a.status),
  );
  const past = appointments.filter(
    (a) => !["planned", "confirmed"].includes(a.status),
  );
  const displayList = activeTab === "upcoming" ? upcoming : past;

  if (loading)
    return (
      <div className="text-center p-10 text-slate-400">Загрузка записей...</div>
    );

  return (
    <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm overflow-hidden relative">
      {/* МОДАЛКА ОТЗЫВА */}
      {reviewAppId && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm">
          <div className="bg-white rounded-[2rem] w-full max-w-md shadow-2xl p-8 transform transition-all scale-100 opacity-100">
            <h3 className="text-2xl font-bold text-slate-900 mb-2 text-center">
              Оцените прием
            </h3>
            <p className="text-slate-500 text-sm text-center mb-6">
              Ваш отзыв поможет другим пациентам
            </p>

            <div className="flex gap-2 justify-center mb-8 flex-row-reverse">
              {/* CSS хитрость для красивого закрашивания звезд справа налево */}
              {[5, 4, 3, 2, 1].map((star) => (
                <button
                  key={star}
                  onClick={() => setRating(star)}
                  className={`text-5xl transition-all peer peer-hover:text-amber-400 hover:text-amber-400 hover:scale-110 ${rating >= star ? "text-amber-400" : "text-slate-200"}`}
                >
                  ★
                </button>
              ))}
            </div>

            <textarea
              rows={4}
              placeholder="Что вам понравилось, а что можно улучшить?"
              className="w-full p-4 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 resize-none mb-6 text-sm text-slate-700"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
            />
            <div className="flex gap-3">
              <button
                onClick={() => setReviewAppId(null)}
                className="flex-1 py-3.5 bg-slate-100 text-slate-600 font-bold rounded-xl hover:bg-slate-200 transition-colors"
              >
                Отмена
              </button>
              <button
                onClick={submitReview}
                disabled={reviewLoading}
                className="flex-1 py-3.5 bg-blue-600 text-white font-bold rounded-xl hover:bg-blue-700 transition-colors shadow-lg shadow-blue-200 disabled:opacity-50"
              >
                {reviewLoading ? "Отправка..." : "Отправить"}
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="p-8 border-b border-slate-100 bg-slate-50/50 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <h2 className="text-xl font-bold text-slate-800 flex items-center gap-2">
          <span className="w-2 h-6 bg-blue-600 rounded-full"></span>
          Мои записи
        </h2>
        <div className="flex bg-slate-200/50 p-1 rounded-xl">
          <button
            onClick={() => setActiveTab("upcoming")}
            className={`px-4 py-2 rounded-lg text-sm font-bold transition-all ${activeTab === "upcoming" ? "bg-white text-slate-900 shadow-sm" : "text-slate-500 hover:text-slate-700"}`}
          >
            Предстоящие ({upcoming.length})
          </button>
          <button
            onClick={() => setActiveTab("past")}
            className={`px-4 py-2 rounded-lg text-sm font-bold transition-all ${activeTab === "past" ? "bg-white text-slate-900 shadow-sm" : "text-slate-500 hover:text-slate-700"}`}
          >
            Прошлые ({past.length})
          </button>
        </div>
      </div>

      <div className="p-8">
        {displayList.length === 0 ? (
          <div className="text-center py-12 text-slate-400 font-medium">
            Нет записей в этой категории.
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {displayList.map((app) => {
              const style = getStatusStyle(app.status);
              const startDate = new Date(app.scheduledStart);
              const endDate = new Date(app.scheduledEnd);

              return (
                <div
                  key={app.id}
                  className={`border rounded-2xl p-6 shadow-sm transition-all relative overflow-hidden group flex flex-col h-full ${app.status === "completed" && !app.hasReview ? "border-amber-200 bg-amber-50/30" : "border-slate-100"}`}
                >
                  <div
                    className={`absolute top-4 right-4 px-3 py-1 text-xs font-bold rounded-full border ${style.color}`}
                  >
                    {style.text}
                  </div>

                  <div className="flex items-center gap-4 mb-4 mt-2">
                    <img
                      src={`http://localhost:5133/api/files/${app.avatarUrl}`}
                      alt="Doctor"
                      className="w-14 h-14 rounded-full object-cover shadow-sm"
                      onError={(e) =>
                        ((e.target as HTMLImageElement).src =
                          "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?w=100")
                      }
                    />
                    <div>
                      <h3 className="font-bold text-slate-900 text-lg leading-tight">
                        {app.doctorName}
                      </h3>
                      <p className="text-blue-600 text-xs font-bold uppercase tracking-wider">
                        {app.doctorSpecialization}
                      </p>
                    </div>
                  </div>

                  <div className="space-y-2 text-sm text-slate-600 mb-6 bg-slate-50 p-4 rounded-xl border border-slate-100 flex-grow">
                    <p className="flex items-center gap-2">
                      <span>📅</span>{" "}
                      <span className="font-semibold text-slate-900">
                        {startDate.toLocaleDateString()}
                      </span>{" "}
                      (
                      {startDate.toLocaleTimeString([], {
                        hour: "2-digit",
                        minute: "2-digit",
                      })}{" "}
                      -{" "}
                      {endDate.toLocaleTimeString([], {
                        hour: "2-digit",
                        minute: "2-digit",
                      })}
                      )
                    </p>
                    <p className="flex items-center gap-2">
                      <span>🏥</span> {app.officeName} ({app.officeAddress})
                    </p>
                    <p className="flex items-center gap-2">
                      <span>🩺</span> {app.typeName}
                    </p>
                  </div>

                  {activeTab === "upcoming" && (
                    <button
                      onClick={() => handleCancel(app.id)}
                      className="w-full py-3 bg-red-50 text-red-600 font-bold rounded-xl hover:bg-red-100 transition-colors text-sm mt-auto"
                    >
                      Отменить запись
                    </button>
                  )}

                  {/* КНОПКА ОТЗЫВА */}
                  {activeTab === "past" &&
                    app.status === "completed" &&
                    !app.hasReview && (
                      <button
                        onClick={() => setReviewAppId(app.id)}
                        className="w-full py-3 bg-amber-400 text-white font-black rounded-xl hover:bg-amber-500 transition-all text-sm mt-auto shadow-md shadow-amber-200"
                      >
                        ⭐ Оценить прием
                      </button>
                    )}
                  {activeTab === "past" &&
                    app.status === "completed" &&
                    app.hasReview && (
                      <div className="w-full py-3 bg-slate-50 text-slate-400 font-bold rounded-xl text-center text-sm mt-auto border border-slate-100">
                        Отзыв оставлен ✓
                      </div>
                    )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}

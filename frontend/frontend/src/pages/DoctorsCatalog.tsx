import { useEffect, useState } from "react";
import { clinicApi } from "../api/clinicApi";
import type { DoctorDto, OfficeDto, SpecializationDto } from "../types/clinic";
import BookingModal from "../components/BookingModal";

export default function DoctorsCatalog() {
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Стейты для фильтров
  const [offices, setOffices] = useState<OfficeDto[]>([]);
  const [specializations, setSpecializations] = useState<SpecializationDto[]>(
    [],
  );

  const [selectedOffice, setSelectedOffice] = useState("");
  const [selectedSpec, setSelectedSpec] = useState("");

  // Стейты для пагинации
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 9; // По 9 карточек на страницу

  // Модалка записи
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedDoctor, setSelectedDoctor] = useState<DoctorDto | null>(null);

  // 1. Загружаем списки для фильтров при первом рендере
  useEffect(() => {
    clinicApi.getOffices().then(setOffices);
    clinicApi.getSpecializations().then(setSpecializations);
  }, []);

  // 2. Загружаем врачей при изменении страницы или фильтров
  useEffect(() => {
    setLoading(true);
    clinicApi
      .getDoctors(pageNumber, pageSize, selectedOffice, selectedSpec)
      .then((data) => {
        setDoctors(data.items);
        setTotalCount(data.totalCount);
      })
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, selectedOffice, selectedSpec]);

  // Обработчик изменения фильтра (при смене фильтра возвращаемся на 1 страницу)
  const handleFilterChange = (
    setter: React.Dispatch<React.SetStateAction<string>>,
    value: string,
  ) => {
    setter(value);
    setPageNumber(1);
  };

  const openBooking = (doc: DoctorDto) => {
    setSelectedDoctor(doc);
    setIsModalOpen(true);
  };

  // Вычисляем общее количество страниц
  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="max-w-6xl mx-auto p-6">
      <BookingModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        doctor={selectedDoctor}
      />

      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-800 mb-6">
          Наши специалисты
        </h1>

        {/* БЛОК ФИЛЬТРАЦИИ */}
        <div className="flex flex-col sm:flex-row gap-4 bg-white p-4 rounded-2xl shadow-sm border border-slate-200">
          <div className="flex-1">
            <label className="block text-xs font-bold text-slate-500 uppercase tracking-widest mb-1.5 ml-1">
              Филиал
            </label>
            <select
              value={selectedOffice}
              onChange={(e) =>
                handleFilterChange(setSelectedOffice, e.target.value)
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 font-medium text-slate-700 cursor-pointer"
            >
              <option value="">Все клиники</option>
              {offices.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </select>
          </div>

          <div className="flex-1">
            <label className="block text-xs font-bold text-slate-500 uppercase tracking-widest mb-1.5 ml-1">
              Специализация
            </label>
            <select
              value={selectedSpec}
              onChange={(e) =>
                handleFilterChange(setSelectedSpec, e.target.value)
              }
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 font-medium text-slate-700 cursor-pointer"
            >
              <option value="">Все специализации</option>
              {specializations.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {loading ? (
        <div className="p-20 text-center text-xl text-slate-400 font-medium">
          Поиск врачей...
        </div>
      ) : doctors.length === 0 ? (
        <div className="p-20 text-center bg-white rounded-3xl border border-slate-200 shadow-sm">
          <div className="text-5xl mb-4">🔍</div>
          <h2 className="text-xl font-bold text-slate-800">Врачи не найдены</h2>
          <p className="text-slate-500 mt-2">
            Попробуйте изменить параметры фильтрации.
          </p>
          <button
            onClick={() => {
              setSelectedOffice("");
              setSelectedSpec("");
            }}
            className="mt-6 text-blue-600 font-bold hover:underline"
          >
            Сбросить фильтры
          </button>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {doctors.map((doc) => (
              <div
                key={doc.id}
                className="bg-white rounded-2xl shadow-sm border border-slate-200 p-6 flex flex-col items-center text-center transition-all hover:shadow-lg hover:-translate-y-1 relative"
              >
                {/* Бейдж с рейтингом */}
                <div className="absolute top-4 right-4 bg-amber-50 border border-amber-200 text-amber-600 font-bold text-xs px-2.5 py-1 rounded-lg flex items-center gap-1">
                  ⭐{" "}
                  {doc.ratingAvg > 0 ? doc.ratingAvg.toFixed(1) : "Нет оценок"}
                </div>

                <div className="w-24 h-24 rounded-full bg-slate-100 flex items-center justify-center mb-4 mt-2 overflow-hidden border-4 border-white shadow-md z-10">
                  <img
                    src={`http://localhost:5133/api/files/${doc.avatarUrl}`}
                    alt={doc.lastName}
                    className="w-full h-full object-cover"
                    onError={(e) =>
                      ((e.target as HTMLImageElement).src =
                        "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?w=300")
                    }
                  />
                </div>

                <h2 className="text-xl font-bold text-gray-900">
                  {doc.lastName} {doc.firstName} {doc.middleName}
                </h2>

                <div className="flex flex-wrap justify-center gap-2 mt-3 mb-4">
                  {doc.specializations.map((spec) => (
                    <span
                      key={spec.id}
                      className="bg-blue-50 text-blue-700 text-xs px-3 py-1.5 rounded-lg font-bold"
                    >
                      {spec.name} ({spec.experienceYears} лет стажа)
                    </span>
                  ))}
                </div>

                <p className="text-gray-600 text-sm line-clamp-3 mb-6">
                  {doc.bio}
                </p>

                <button
                  onClick={() => openBooking(doc)}
                  className="mt-auto w-full bg-slate-900 hover:bg-slate-800 text-white font-bold py-3.5 px-4 rounded-xl transition-all shadow-md"
                >
                  Записаться на прием
                </button>
              </div>
            ))}
          </div>

          {/* БЛОК ПАГИНАЦИИ */}
          {totalPages > 1 && (
            <div className="flex justify-center items-center gap-4 mt-12">
              <button
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1}
                className="px-6 py-3 bg-white border border-slate-200 rounded-xl font-bold text-slate-700 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors shadow-sm"
              >
                Назад
              </button>

              <span className="font-bold text-slate-500 bg-white px-6 py-3 rounded-xl border border-slate-200 shadow-sm">
                Страница <span className="text-blue-600">{pageNumber}</span> из{" "}
                {totalPages}
              </span>

              <button
                onClick={() =>
                  setPageNumber((p) => Math.min(totalPages, p + 1))
                }
                disabled={pageNumber === totalPages}
                className="px-6 py-3 bg-white border border-slate-200 rounded-xl font-bold text-slate-700 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors shadow-sm"
              >
                Вперед
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

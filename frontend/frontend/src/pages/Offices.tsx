import { useEffect, useState } from "react";
import { clinicApi } from "../api/clinicApi";
import type { OfficeDto } from "../types/clinic";

export default function Offices() {
  const [offices, setOffices] = useState<OfficeDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    clinicApi
      .getOffices()
      .then(setOffices)
      .catch((err) => console.error("Ошибка при загрузке офисов:", err))
      .finally(() => setLoading(false));
  }, []);

  if (loading)
    return (
      <div className="p-20 text-center text-slate-500">
        Загрузка филиалов...
      </div>
    );

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="mb-12">
        <h1 className="text-4xl font-black text-slate-900 mb-4 text-center">
          Наши клиники
        </h1>
        <p className="text-slate-500 text-center text-lg max-w-2xl mx-auto">
          Выберите удобный для вас филиал. Все наши центры оснащены современным
          оборудованием и готовы к приему.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {offices.map((office) => (
          <div
            key={office.id}
            className="bg-white rounded-3xl overflow-hidden border border-slate-200 shadow-sm hover:shadow-xl transition-all group flex flex-col sm:flex-row"
          >
            <div className="sm:w-1/3 h-48 sm:h-auto relative overflow-hidden">
              <img
                src={`http://localhost:5133/api/files/${office.photoUrl}`}
                alt={office.name}
                className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                onError={(e) => {
                  // Запасной вариант только на случай, если файл удалят из Azurite
                  (e.target as HTMLImageElement).src =
                    "https://via.placeholder.com/800x600?text=Нет+фото";
                }}
              />
            </div>
            <div className="p-8 flex flex-col justify-center sm:w-2/3">
              <h2 className="text-2xl font-bold text-slate-900 mb-2">
                {office.name}
              </h2>
              <div className="space-y-3 mb-6">
                <p className="flex items-center text-slate-600 gap-2">
                  <svg
                    className="w-5 h-5 text-blue-500"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                    ></path>
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                    ></path>
                  </svg>
                  {office.address}
                </p>
                <p className="flex items-center text-slate-600 gap-2">
                  <svg
                    className="w-5 h-5 text-blue-500"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      d="M3 5a2 2 0 012-2h3.28a1 1 0 011.94.55l-.74 4.08a1 1 0 01-1.01.88L5.3 8.3c-.3-.33-.79-.14-1 .31A13.06 13.06 0 0014.7 18.7c.45-.21.64-.7.31-1l-1.08-1.07a1 1 0 01-.88-1.01l.41-2.23a1 1 0 011.1-.88l4.08.74a1 1 0 01.55 1.94V19a2 2 0 01-2 2h-1C9.71 21 5 16.29 5 10.3V5z"
                    ></path>
                  </svg>
                  {office.phone}
                </p>
              </div>
              <button className="text-blue-600 font-bold hover:text-blue-700 transition-colors flex items-center gap-1">
                Посмотреть на карте
                <svg
                  className="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="2"
                    d="M9 5l7 7-7 7"
                  ></path>
                </svg>
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

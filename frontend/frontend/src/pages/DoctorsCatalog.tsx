import { useEffect, useState } from "react";
import { clinicApi } from "../api/clinicApi";
import type { DoctorDto } from "../types/clinic";

export default function DoctorsCatalog() {
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    clinicApi
      .getDoctors(1, 10)
      .then((data) => {
        setDoctors(data.items);
      })
      .catch((err) => console.error("Ошибка загрузки врачей:", err))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="p-10 text-center text-xl text-gray-500">
        Загрузка врачей...
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto p-6">
      <h1 className="text-3xl font-bold text-gray-800 mb-8">
        Наши специалисты
      </h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {doctors.map((doc) => (
          <div
            key={doc.id}
            className="bg-white rounded-2xl shadow-md border border-gray-100 p-6 flex flex-col items-center text-center transition-transform hover:-translate-y-1 hover:shadow-lg"
          >
            {/* Аватарка врача */}
            <div className="w-24 h-24 rounded-full bg-slate-100 flex items-center justify-center mb-4 overflow-hidden border-4 border-white shadow-md z-10">
              <img
                src={`http://localhost:5133/api/files/${doc.avatarUrl}`}
                alt={doc.lastName}
                className="w-full h-full object-cover"
                onError={(e) => {
                  // Если фото нет на бэкенде, ставим красивую картинку доктора с Unsplash
                  (e.target as HTMLImageElement).src =
                    "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?auto=format&fit=crop&q=80&w=300";
                }}
              />
            </div>

            <h2 className="text-xl font-bold text-gray-900">
              {doc.lastName} {doc.firstName} {doc.middleName}
            </h2>

            <div className="flex flex-wrap justify-center gap-2 mt-3 mb-4">
              {doc.specializations.map((spec) => (
                <span
                  key={spec.id}
                  className="bg-blue-50 text-blue-700 text-xs px-3 py-1 rounded-full font-medium"
                >
                  {spec.name} ({spec.experienceYears} лет)
                </span>
              ))}
            </div>

            <p className="text-gray-600 text-sm line-clamp-3 mb-4">{doc.bio}</p>

            <button className="mt-auto w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors">
              Записаться на прием
            </button>
          </div>
        ))}
      </div>

      {doctors.length === 0 && (
        <div className="text-center text-gray-500 py-10">Врачи не найдены</div>
      )}
    </div>
  );
}

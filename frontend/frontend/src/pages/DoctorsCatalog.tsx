// src/pages/DoctorsCatalog.tsx
import { useEffect, useState } from "react";
import { clinicApi } from "../api/clinicApi";
import type { DoctorDto } from "../types/clinic";
import BookingModal from "../components/BookingModal";

export default function DoctorsCatalog() {
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Стейты для управления модальным окном
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedDoctor, setSelectedDoctor] = useState<DoctorDto | null>(null);

  useEffect(() => {
    clinicApi
      .getDoctors(1, 10)
      .then((data) => setDoctors(data.items))
      .finally(() => setLoading(false));
  }, []);

  const openBooking = (doc: DoctorDto) => {
    setSelectedDoctor(doc);
    setIsModalOpen(true);
  };

  if (loading)
    return (
      <div className="p-10 text-center text-xl text-gray-500">
        Загрузка врачей...
      </div>
    );

  return (
    <div className="max-w-6xl mx-auto p-6">
      {/* МОДАЛЬНОЕ ОКНО ЗАПИСИ */}
      <BookingModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        doctor={selectedDoctor}
      />

      <h1 className="text-3xl font-bold text-gray-800 mb-8">
        Наши специалисты
      </h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {doctors.map((doc) => (
          <div
            key={doc.id}
            className="bg-white rounded-2xl shadow-md border border-gray-100 p-6 flex flex-col items-center text-center transition-transform hover:-translate-y-1 hover:shadow-lg"
          >
            <div className="w-24 h-24 rounded-full bg-slate-100 flex items-center justify-center mb-4 overflow-hidden border-4 border-white shadow-md z-10">
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
                  className="bg-blue-50 text-blue-700 text-xs px-3 py-1 rounded-full font-medium"
                >
                  {spec.name} ({spec.experienceYears} лет)
                </span>
              ))}
            </div>

            <p className="text-gray-600 text-sm line-clamp-3 mb-4">{doc.bio}</p>

            <button
              onClick={() => openBooking(doc)}
              className="mt-auto w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors"
            >
              Записаться на прием
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}

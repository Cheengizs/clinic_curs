import { useEffect, useState } from "react";
import { labApi, type PatientLabResultDto } from "../../api/labApi";

export default function PatientLabs() {
  const [labs, setLabs] = useState<PatientLabResultDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    labApi
      .getMyLabs()
      .then(setLabs)
      .finally(() => setLoading(false));
  }, []);

  if (loading)
    return (
      <div className="text-center p-10 text-slate-400">
        Загрузка анализов...
      </div>
    );

  return (
    <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm overflow-hidden">
      <div className="p-8 border-b border-slate-100 bg-slate-50/50">
        <h2 className="text-xl font-bold text-slate-800 flex items-center gap-2">
          <span className="w-2 h-6 bg-purple-500 rounded-full"></span>{" "}
          Лабораторные исследования
        </h2>
      </div>
      <div className="p-8">
        {labs.length === 0 ? (
          <div className="text-center py-12 text-slate-400">
            В вашей медкарте пока нет результатов анализов.
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {labs.map((lab) => (
              <div
                key={lab.id}
                className="border border-slate-100 rounded-2xl p-6 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group flex flex-col h-full"
              >
                <div className="absolute top-4 right-4 px-3 py-1 text-xs font-bold rounded-full border bg-green-100 text-green-700 border-green-200">
                  Готово
                </div>
                <div className="w-12 h-12 bg-purple-50 text-purple-600 rounded-xl flex items-center justify-center font-black text-2xl mb-4 mt-2">
                  🔬
                </div>
                <h3 className="font-bold text-slate-900 text-lg leading-tight mb-2">
                  {lab.testName}
                </h3>
                <p className="text-sm text-slate-500 mb-6 flex-grow">
                  Сдано: {lab.date}
                  <br />
                  Филиал: {lab.officeName}
                </p>
                <a
                  href={`http://localhost:5133/api/files/${lab.fileUrl}`}
                  target="_blank"
                  rel="noreferrer"
                  className="w-full text-center py-3 bg-slate-900 text-white font-bold rounded-xl hover:bg-slate-800 transition-colors text-sm mt-auto shadow-md"
                >
                  Скачать / Посмотреть результат
                </a>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

import { useState, useEffect } from "react";
import { labApi, type LabTestDto } from "../../api/labApi";
import { fileApi } from "../../api/fileApi";

interface Props {
  isOpen: boolean;
  onClose: () => void;
  patientAccountId: string | null;
  patientName: string;
}

export default function UploadLabModal({
  isOpen,
  onClose,
  patientAccountId,
  patientName,
}: Props) {
  const [tests, setTests] = useState<LabTestDto[]>([]);
  const [selectedTest, setSelectedTest] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isOpen) labApi.getTests().then(setTests);
  }, [isOpen]);

  if (!isOpen || !patientAccountId) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file || !selectedTest) return alert("Заполните все поля");
    setLoading(true);
    try {
      // 1. Грузим файл в облако
      const fileId = await fileApi.upload(file);
      // 2. Привязываем к медкарте пациента
      await labApi.addLabResult(patientAccountId, selectedTest, fileId);
      alert("Анализ успешно загружен в медкарту пациента!");
      onClose();
    } catch (err) {
      alert("Ошибка при загрузке анализа");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm">
      <div className="bg-white rounded-[2rem] w-full max-w-md shadow-2xl p-8">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold text-slate-900">
            Загрузка анализа
          </h2>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-slate-600 text-2xl"
          >
            &times;
          </button>
        </div>
        <p className="text-sm text-slate-500 mb-6">
          Пациент:{" "}
          <span className="font-bold text-slate-900">{patientName}</span>
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <select
            required
            value={selectedTest}
            onChange={(e) => setSelectedTest(e.target.value)}
            className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">-- Выберите тип исследования --</option>
            {tests.map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </select>
          <input
            required
            type="file"
            accept=".pdf,image/*"
            onChange={(e) => setFile(e.target.files?.[0] || null)}
            className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-bold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
          />
          <button
            disabled={loading}
            type="submit"
            className="w-full bg-blue-600 text-white py-3 rounded-xl font-bold mt-4 shadow-lg disabled:opacity-50"
          >
            {loading ? "Загрузка..." : "Загрузить в ЭМК"}
          </button>
        </form>
      </div>
    </div>
  );
}

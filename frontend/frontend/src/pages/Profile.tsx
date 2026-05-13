import { useEffect, useState } from "react";
import { authApi, type AccountMeDto } from "../api/authApi";
import { useNavigate } from "react-router-dom";
import AccountHeader from "../components/profile/AccountHeader";
import PatientVerificationSteps from "../components/profile/PatientVerificationSteps";
import RegistrarView from "../components/profile/RegistrarView";
import AdminView from "../components/profile/AdminView";
import DoctorView from "../components/profile/DoctorView";
import PatientAppointments from "../components/profile/PatientAppointments";
import MedicalHistory from "../components/profile/MedicalHistory";
import PatientLabs from "../components/profile/PatientLabs"; // <-- Добавлен импорт анализов

export default function Profile({ onLogout }: { onLogout?: () => void }) {
  const [account, setAccount] = useState<AccountMeDto | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  // <-- Добавлен 'labs' в тип состояния вкладок
  const [patientTab, setPatientTab] = useState<
    "appointments" | "health" | "labs"
  >("appointments");

  const fetchMe = () => {
    authApi
      .getMe()
      .then(setAccount)
      .catch(() => {
        localStorage.removeItem("accessToken");
        navigate("/login");
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchMe();
  }, [navigate]);

  const handleLogout = async () => {
    const refresh = localStorage.getItem("refreshToken");
    if (refresh)
      try {
        await authApi.logout(refresh);
      } catch (e) {}
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    if (onLogout) onLogout();
    navigate("/login");
  };

  if (loading)
    return (
      <div className="p-20 text-center text-slate-500 font-medium text-xl">
        Загрузка системы...
      </div>
    );
  if (!account) return null;

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-12 space-y-10">
      <AccountHeader
        email={account.email}
        role={account.role}
        avatarUrl={account.avatarUrl}
        identityVerified={account.identityVerified}
        onLogout={handleLogout}
        onAvatarUpdate={fetchMe}
      />

      {account.role === "patient" && (
        <div className="space-y-10">
          <PatientVerificationSteps account={account} fetchMe={fetchMe} />

          {account.identityVerified && (
            <>
              {/* Навигация внутри кабинета пациента */}
              <div className="flex gap-4 border-b border-slate-200 pb-4 overflow-x-auto">
                <button
                  onClick={() => setPatientTab("appointments")}
                  className={`px-6 py-2 rounded-full font-bold transition-all whitespace-nowrap ${patientTab === "appointments" ? "bg-slate-900 text-white shadow-md" : "text-slate-500 hover:bg-slate-100"}`}
                >
                  📅 Мои записи
                </button>
                <button
                  onClick={() => setPatientTab("health")}
                  className={`px-6 py-2 rounded-full font-bold transition-all whitespace-nowrap ${patientTab === "health" ? "bg-slate-900 text-white shadow-md" : "text-slate-500 hover:bg-slate-100"}`}
                >
                  📖 Медкарта
                </button>
                {/* <-- Добавлена кнопка переключения на Анализы --> */}
                <button
                  onClick={() => setPatientTab("labs")}
                  className={`px-6 py-2 rounded-full font-bold transition-all whitespace-nowrap ${patientTab === "labs" ? "bg-slate-900 text-white shadow-md" : "text-slate-500 hover:bg-slate-100"}`}
                >
                  🔬 Анализы
                </button>
              </div>
              {patientTab === "appointments" && <PatientAppointments />}
              {patientTab === "health" && <MedicalHistory />}
              {patientTab === "labs" && <PatientLabs />}{" "}
              {/* <-- Вызов компонента */}
            </>
          )}
        </div>
      )}

      {account.role === "registrar" && <RegistrarView />}

      {account.role === "admin" && <AdminView />}

      {account.role === "doctor" && <DoctorView />}
    </div>
  );
}

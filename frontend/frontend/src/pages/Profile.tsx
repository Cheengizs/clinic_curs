// src/pages/Profile.tsx
import { useEffect, useState } from "react";
import { authApi, type AccountMeDto } from "../api/authApi";
import { useNavigate } from "react-router-dom";
import AccountHeader from "../components/profile/AccountHeader";
import PatientVerificationSteps from "../components/profile/PatientVerificationSteps";
import RegistrarView from "../components/profile/RegistrarView";
import AdminView from "../components/profile/AdminView";

export default function Profile({ onLogout }: { onLogout?: () => void }) {
  const [account, setAccount] = useState<AccountMeDto | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

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
        onLogout={handleLogout}
      />

      {/* Переключаем контент в зависимости от роли */}
      {account.role === "patient" && (
        <PatientVerificationSteps account={account} fetchMe={fetchMe} />
      )}

      {account.role === "registrar" && <RegistrarView />}

      {/* Заглушки для других ролей */}

      {account.role === "admin" && <AdminView />}

      {account.role === "doctor" && (
        <div className="p-20 text-center bg-white rounded-3xl border border-dashed border-slate-300 text-slate-400">
          Интерфейс для роли {account.role} находится в разработке
        </div>
      )}
    </div>
  );
}

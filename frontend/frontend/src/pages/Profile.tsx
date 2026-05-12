import { useEffect, useState } from "react";
import { authApi, type AccountMeDto } from "../api/authApi";
import { useNavigate } from "react-router-dom";
import VerificationModal from "../components/VerificationModal";

export default function Profile({ onLogout }: { onLogout?: () => void }) {
  const [account, setAccount] = useState<AccountMeDto | null>(null);
  const [loading, setLoading] = useState(true);

  const [phoneInput, setPhoneInput] = useState("");
  const [smsCode, setSmsCode] = useState("");
  const [codeSent, setCodeSent] = useState(false);

  // НОВОЕ: состояние для режима редактирования номера
  const [isEditingPhone, setIsEditingPhone] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const navigate = useNavigate();

  const fetchMe = () => {
    authApi
      .getMe()
      .then((data) => {
        setAccount(data);
        if (data.phone) setPhoneInput(data.phone);
      })
      .catch(() => {
        localStorage.removeItem("accessToken");
        navigate("/login");
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchMe();
  }, [navigate]);

  const handleLogoutClick = async () => {
    const refresh = localStorage.getItem("refreshToken");
    if (refresh) {
      try {
        await authApi.logout(refresh);
      } catch (e) {}
    }
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    if (onLogout) onLogout();
    navigate("/login");
  };

  const handleSendCode = async () => {
    if (!phoneInput.trim()) {
      alert("Пожалуйста, введите номер телефона");
      return;
    }
    try {
      // Если номер отличается от того, что в базе — запрашиваем смену
      if (phoneInput !== account?.phone || !account?.phone) {
        await authApi.requestPhoneChange(phoneInput);
      } else {
        await authApi.sendPhoneCode();
      }
      setCodeSent(true);
      alert("Код отправлен! Посмотри в консоль бэкенда (MockSmsService)");
    } catch (err: any) {
      alert(err.response?.data?.error || "Ошибка отправки кода");
    }
  };

  const handleVerifyCode = async () => {
    try {
      await authApi.verifyPhoneCode(smsCode);
      alert("Телефон успешно подтвержден!");
      setIsEditingPhone(false); // Выходим из режима редактирования
      setCodeSent(false);
      fetchMe();
    } catch (err: any) {
      alert(err.response?.data?.error || "Неверный код");
    }
  };

  if (loading)
    return (
      <div className="p-20 text-center text-slate-500 font-medium">
        Загрузка профиля...
      </div>
    );
  if (!account) return null;

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 py-12 space-y-8">
      <VerificationModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={() => fetchMe()}
      />

      {!account.emailVerified && (
        <div className="bg-white border-2 border-amber-200 rounded-[2rem] p-10 text-center shadow-xl shadow-amber-100/50 animate-pulse">
          <div className="w-20 h-20 bg-amber-100 text-amber-600 rounded-full flex items-center justify-center mx-auto mb-6 text-4xl shadow-inner">
            ✉️
          </div>
          <h1 className="text-4xl md:text-5xl font-black text-slate-900 mb-6 tracking-tight leading-tight">
            ПОДТВЕРДИТЕ <br /> ЭЛЕКТРОННУЮ ПОЧТУ
          </h1>
          <div className="bg-amber-50 rounded-2xl p-6 max-w-lg mx-auto border border-amber-100">
            <p className="text-slate-700 text-lg leading-relaxed">
              Мы отправили инструкции на{" "}
              <span className="font-bold text-blue-600">{account.email}</span>.
              Пожалуйста, перейдите по ссылке в письме, чтобы подтвердить ваш
              аккаунт.
            </p>
          </div>
        </div>
      )}

      <div className="flex justify-between items-end border-b border-slate-100 pb-6">
        <div>
          <h1 className="text-4xl font-black text-slate-900 tracking-tight">
            {" "}
            Личный кабинет{" "}
          </h1>
          <p className="text-slate-500 mt-2 text-lg">
            Добро пожаловать,{" "}
            <span className="text-slate-900 font-semibold">
              {account.email}
            </span>
          </p>
        </div>

        <div className="flex flex-col items-end gap-3">
          <div className="px-4 py-1.5 bg-blue-100 text-blue-700 rounded-full text-sm font-bold uppercase tracking-wider">
            Роль: {account.role}
          </div>
          <button
            onClick={handleLogoutClick}
            className="text-red-500 hover:text-red-700 font-semibold text-sm transition-colors flex items-center gap-1"
          >
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
                d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
              ></path>
            </svg>
            Выйти из системы
          </button>
        </div>
      </div>

      <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm overflow-hidden">
        <div className="p-8 border-b border-slate-100 bg-slate-50/50">
          <h2 className="text-xl font-bold text-slate-800 flex items-center gap-2">
            <span className="w-2 h-6 bg-blue-600 rounded-full"></span>
            Статус верификации
          </h2>
        </div>

        <div className="p-8 space-y-10">
          {/* ЭТАП 1: EMAIL */}
          <div className="flex items-start gap-6">
            <div
              className={`w-12 h-12 rounded-2xl flex items-center justify-center shrink-0 shadow-sm ${account.emailVerified ? "bg-green-100 text-green-600" : "bg-amber-100 text-amber-600"}`}
            >
              {account.emailVerified ? (
                <svg
                  className="w-7 h-7"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="3"
                    d="M5 13l4 4L19 7"
                  ></path>
                </svg>
              ) : (
                "1"
              )}
            </div>
            <div>
              <h3 className="font-bold text-xl text-slate-900">
                Email подтвержден
              </h3>
              <p className="text-slate-500 mt-1 leading-relaxed">
                Необходимо для активации аккаунта.
              </p>
            </div>
          </div>

          {/* ЭТАП 2: ТЕЛЕФОН */}
          <div
            className={`flex items-start gap-6 transition-opacity duration-300 ${!account.emailVerified ? "opacity-30 grayscale" : ""}`}
          >
            <div
              className={`w-12 h-12 rounded-2xl flex items-center justify-center shrink-0 shadow-sm ${account.phoneVerified && !isEditingPhone ? "bg-green-100 text-green-600" : "bg-blue-100 text-blue-600"}`}
            >
              {account.phoneVerified && !isEditingPhone ? (
                <svg
                  className="w-7 h-7"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="3"
                    d="M5 13l4 4L19 7"
                  ></path>
                </svg>
              ) : (
                "2"
              )}
            </div>

            <div className="flex-grow">
              <div className="flex items-center justify-between">
                <h3 className="font-bold text-xl text-slate-900">
                  Номер телефона
                </h3>
                {account.phoneVerified && !isEditingPhone && (
                  <button
                    onClick={() => setIsEditingPhone(true)}
                    className="text-blue-600 hover:text-blue-800 text-sm font-bold flex items-center gap-1 transition-colors"
                  >
                    Изменить
                  </button>
                )}
              </div>

              {account.phoneVerified && !isEditingPhone ? (
                <p className="text-slate-500 mt-1 leading-relaxed">
                  Ваш номер: {account.phone}
                </p>
              ) : (
                <>
                  <p className="text-slate-500 mt-1 leading-relaxed">
                    Укажите актуальный номер для получения SMS-кода.
                  </p>
                  <div className="mt-6 bg-slate-50 p-6 rounded-2xl border border-slate-200 max-w-md">
                    {!codeSent ? (
                      <div className="flex flex-col gap-3">
                        <input
                          type="text"
                          value={phoneInput}
                          onChange={(e) => setPhoneInput(e.target.value)}
                          placeholder="+375..."
                          className="w-full px-5 py-3 border border-slate-300 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                        />
                        <div className="flex gap-2">
                          <button
                            onClick={handleSendCode}
                            className="flex-grow bg-blue-600 text-white px-6 py-3 rounded-xl font-bold hover:bg-blue-700"
                          >
                            {isEditingPhone
                              ? "Обновить номер"
                              : "Получить SMS-код"}
                          </button>
                          {isEditingPhone && (
                            <button
                              onClick={() => setIsEditingPhone(false)}
                              className="px-4 py-3 bg-slate-200 text-slate-700 rounded-xl font-bold"
                            >
                              Отмена
                            </button>
                          )}
                        </div>
                      </div>
                    ) : (
                      <div className="flex flex-col gap-3">
                        <input
                          type="text"
                          value={smsCode}
                          onChange={(e) => setSmsCode(e.target.value)}
                          placeholder="Код из SMS"
                          className="w-full px-5 py-3 border border-slate-300 rounded-xl text-center font-mono tracking-widest bg-white"
                        />
                        <button
                          onClick={handleVerifyCode}
                          className="w-full bg-green-600 text-white px-6 py-3 rounded-xl font-bold hover:bg-green-700"
                        >
                          Подтвердить
                        </button>
                      </div>
                    )}
                  </div>
                </>
              )}
            </div>
          </div>

          {/* ЭТАП 3: ДОКУМЕНТЫ */}
          <div
            className={`flex items-start gap-6 transition-opacity duration-300 ${!account.phoneVerified || isEditingPhone ? "opacity-30 grayscale pointer-events-none" : ""}`}
          >
            <div
              className={`w-12 h-12 rounded-2xl flex items-center justify-center shrink-0 shadow-sm ${account.identityVerified ? "bg-green-100 text-green-600" : "bg-slate-100 text-slate-400"}`}
            >
              {account.identityVerified ? (
                <svg
                  className="w-7 h-7"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="3"
                    d="M5 13l4 4L19 7"
                  ></path>
                </svg>
              ) : (
                "3"
              )}
            </div>
            <div>
              <h3 className="font-bold text-xl text-slate-900">
                Личные данные
              </h3>
              <p className="text-slate-500 mt-1 leading-relaxed">
                Верификация паспорта необходима для записи на приемы.
              </p>
              {!account.identityVerified &&
                account.phoneVerified &&
                !isEditingPhone && (
                  <button
                    onClick={() => setIsModalOpen(true)}
                    className="mt-6 bg-slate-900 text-white px-8 py-3.5 rounded-xl font-bold hover:bg-slate-800 transition-all shadow-lg hover:-translate-y-0.5"
                  >
                    Записаться на верификацию
                  </button>
                )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

import { useState, useEffect } from "react";
import { authApi, type AccountMeDto } from "../../api/authApi";
import {
  verificationApi,
  type VerificationRequestDto,
} from "../../api/verificationApi";
import { clinicApi } from "../../api/clinicApi";
import VerificationModal from "../VerificationModal";
import type { OfficeDto } from "../../types/clinic";

interface Props {
  account: AccountMeDto;
  fetchMe: () => void;
}

export default function PatientVerificationSteps({ account, fetchMe }: Props) {
  const [offices, setOffices] = useState<OfficeDto[]>([]);
  const [myRequest, setMyRequest] = useState<VerificationRequestDto | null>(
    null,
  );

  const [phoneInput, setPhoneInput] = useState(account.phone || "");
  const [smsCode, setSmsCode] = useState("");
  const [codeSent, setCodeSent] = useState(false);

  const [isEditingPhone, setIsEditingPhone] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // При загрузке получаем список офисов и текущую заявку (если есть)
  useEffect(() => {
    clinicApi.getOffices().then(setOffices);

    if (account.phoneVerified && !account.identityVerified) {
      verificationApi
        .getMyRequest()
        .then(setMyRequest)
        .catch(() => setMyRequest(null));
    }
  }, [account.phoneVerified, account.identityVerified]);

  const handleSendCode = async () => {
    if (!phoneInput.trim()) {
      alert("Пожалуйста, введите номер телефона");
      return;
    }
    try {
      if (phoneInput !== account.phone || !account.phone) {
        await authApi.requestPhoneChange(phoneInput);
      } else {
        await authApi.sendPhoneCode();
      }
      setCodeSent(true);
      alert("Код отправлен! Посмотри в консоль бэкенда");
    } catch (err: any) {
      alert(err.response?.data?.error || "Ошибка отправки кода");
    }
  };

  const handleVerifyCode = async () => {
    try {
      await authApi.verifyPhoneCode(smsCode);
      setIsEditingPhone(false);
      setCodeSent(false);
      fetchMe(); // Обновляем глобальный профиль
      alert("Телефон успешно подтвержден!");
    } catch (err: any) {
      alert(err.response?.data?.error || "Неверный код");
    }
  };

  return (
    <div className="space-y-8">
      {/* Модалка записи */}
      <VerificationModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={fetchMe}
        initialData={myRequest}
      />

      {/* Если почта не подтверждена */}
      {!account.emailVerified && (
        <div className="bg-white border-2 border-amber-200 rounded-[2rem] p-10 text-center animate-pulse shadow-xl shadow-amber-100/50">
          <div className="w-20 h-20 bg-amber-100 text-amber-600 rounded-full flex items-center justify-center mx-auto mb-6 text-4xl shadow-inner">
            ✉️
          </div>
          <h1 className="text-3xl font-black text-slate-900 mb-4">
            ПОДТВЕРДИТЕ ПОЧТУ
          </h1>
          <p className="text-slate-600">
            Мы отправили инструкции на{" "}
            <span className="font-bold text-blue-600">{account.email}</span>
          </p>
        </div>
      )}

      <div className="bg-white rounded-[2rem] border border-slate-200 shadow-sm p-8 space-y-10">
        <h2 className="text-xl font-bold text-slate-800 flex items-center gap-2">
          <span className="w-2 h-6 bg-blue-600 rounded-full"></span>
          Статус верификации
        </h2>

        {/* ШАГ 1: EMAIL */}
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

        {/* ШАГ 2: ТЕЛЕФОН */}
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
            <div className="flex justify-between items-center">
              <h3 className="font-bold text-xl text-slate-900">
                Номер телефона
              </h3>
              {account.phoneVerified && !isEditingPhone && (
                <button
                  onClick={() => setIsEditingPhone(true)}
                  className="text-blue-600 text-sm font-bold hover:underline"
                >
                  Изменить
                </button>
              )}
            </div>

            {account.phoneVerified && !isEditingPhone ? (
              <p className="mt-1 text-slate-500">
                Ваш номер:{" "}
                <span className="font-semibold text-slate-900">
                  {account.phone}
                </span>
              </p>
            ) : (
              <div className="mt-4 bg-slate-50 border border-slate-200 p-6 rounded-2xl max-w-sm">
                {!codeSent ? (
                  <div className="flex flex-col gap-3">
                    <input
                      type="text"
                      value={phoneInput}
                      onChange={(e) => setPhoneInput(e.target.value)}
                      placeholder="+375 (__) ___-__-__"
                      className="p-3 border border-slate-300 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                    />
                    <div className="flex gap-2">
                      <button
                        onClick={handleSendCode}
                        className="flex-grow bg-blue-600 text-white p-3 rounded-xl font-bold hover:bg-blue-700 transition-all shadow-md"
                      >
                        {isEditingPhone ? "Обновить" : "Получить код"}
                      </button>
                      {isEditingPhone && (
                        <button
                          onClick={() => setIsEditingPhone(false)}
                          className="px-4 py-3 bg-slate-200 text-slate-700 rounded-xl font-bold hover:bg-slate-300 transition-colors"
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
                      placeholder="000000"
                      className="p-3 border border-slate-300 rounded-xl text-center tracking-[0.5em] font-mono text-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                    />
                    <button
                      onClick={handleVerifyCode}
                      className="w-full bg-green-600 text-white p-3 rounded-xl font-bold hover:bg-green-700 transition-all shadow-md"
                    >
                      Подтвердить
                    </button>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>

        {/* ШАГ 3: ДОКУМЕНТЫ */}
        <div
          className={`flex items-start gap-6 transition-opacity duration-300 ${!account.phoneVerified || isEditingPhone ? "opacity-30 grayscale pointer-events-none" : ""}`}
        >
          <div
            className={`w-12 h-12 rounded-2xl flex items-center justify-center shrink-0 shadow-sm ${account.identityVerified ? "bg-green-100 text-green-600" : myRequest ? "bg-amber-100 text-amber-600" : "bg-slate-100 text-slate-400"}`}
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
          <div className="flex-grow">
            <h3 className="font-bold text-xl text-slate-900">Личные данные</h3>
            <p className="text-slate-500 mt-1 leading-relaxed mb-4">
              Верификация паспорта необходима для записи на приемы.
            </p>

            {/* Если есть висящая заявка */}
            {myRequest && !account.identityVerified && (
              <div className="p-5 bg-white border border-amber-200 rounded-2xl flex justify-between items-center shadow-sm">
                <div>
                  <p className="text-[10px] font-black text-amber-600 uppercase tracking-widest flex items-center gap-2 mb-1">
                    <span className="w-2 h-2 bg-amber-500 rounded-full animate-ping"></span>{" "}
                    На проверке
                  </p>
                  <p className="text-sm text-slate-700 font-medium">
                    {offices.find((o) => o.id === myRequest.officeId)?.name} —{" "}
                    {new Date(myRequest.scheduledAt).toLocaleString([], {
                      day: "numeric",
                      month: "long",
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </p>
                </div>
                <button
                  onClick={() => setIsModalOpen(true)}
                  className="text-blue-600 font-bold text-sm hover:underline ml-4"
                >
                  Редактировать
                </button>
              </div>
            )}

            {/* Если заявки нет */}
            {!myRequest &&
              !account.identityVerified &&
              account.phoneVerified &&
              !isEditingPhone && (
                <button
                  onClick={() => setIsModalOpen(true)}
                  className="bg-slate-900 text-white px-8 py-3.5 rounded-xl font-bold hover:bg-slate-800 transition-all shadow-lg hover:-translate-y-0.5"
                >
                  Записаться на верификацию
                </button>
              )}

            {/* Если уже подтвержден */}
            {account.identityVerified && (
              <p className="text-green-600 font-medium text-sm italic">
                Личность успешно подтверждена. Доступ к медкарте открыт.
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

import { fileApi } from "../../api/fileApi";
import { authApi } from "../../api/authApi"; // <-- ДОБАВИЛИ
import { useState } from "react";

interface Props {
  email: string;
  role: string;
  avatarUrl: string | null;
  identityVerified: boolean;
  onLogout: () => void;
  onAvatarUpdate: () => void;
}

export default function AccountHeader({
  email,
  role,
  avatarUrl,
  identityVerified,
  onLogout,
  onAvatarUpdate,
}: Props) {
  const [uploading, setUploading] = useState(false);
  const canUpload =
    role !== "admin" && (role !== "patient" || identityVerified);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    try {
      const fileId = await fileApi.upload(file);
      await fileApi.updateAvatar(fileId);
      onAvatarUpdate();
    } catch (err) {
      alert("Ошибка при загрузке фото");
    } finally {
      setUploading(false);
    }
  };

  // ФУНКЦИЯ САМОУДАЛЕНИЯ
  const handleDeleteAccount = async () => {
    const confirmed = window.confirm(
      "Вы уверены, что хотите навсегда удалить свой аккаунт? Ваша медкарта будет анонимизирована, а все будущие записи отменены. Это действие необратимо!",
    );
    if (confirmed) {
      try {
        await authApi.deleteMe();
        alert("Ваш аккаунт успешно удален.");
        onLogout(); // Выкидываем на страницу логина
      } catch (err) {
        alert("Произошла ошибка при удалении аккаунта.");
      }
    }
  };

  return (
    <div className="flex flex-col md:flex-row justify-between items-center md:items-end border-b border-slate-100 pb-8 gap-6">
      <div className="flex items-center gap-6">
        <div className={`relative ${canUpload ? "group cursor-pointer" : ""}`}>
          <div className="w-24 h-24 rounded-full overflow-hidden border-4 border-white shadow-xl bg-slate-100">
            <img
              src={
                avatarUrl
                  ? `http://localhost:5133/api/files/${avatarUrl}`
                  : `https://ui-avatars.com/api/?name=${email}&background=random`
              }
              className={`w-full h-full object-cover ${uploading ? "opacity-50" : ""}`}
              alt="Profile"
            />
          </div>
          {canUpload && (
            <label className="absolute inset-0 flex items-center justify-center bg-black/40 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer text-xs font-bold text-center p-2">
              {uploading ? "..." : "Сменить фото"}
              <input
                type="file"
                className="hidden"
                accept="image/*"
                onChange={handleFileChange}
                disabled={uploading}
              />
            </label>
          )}
        </div>
        <div>
          <h1 className="text-4xl font-black text-slate-900 tracking-tight">
            Личный кабинет
          </h1>
          <p className="text-slate-500 mt-1 text-lg">
            Пользователь:{" "}
            <span className="text-slate-900 font-semibold">{email}</span>
          </p>
        </div>
      </div>

      <div className="flex flex-col items-end gap-3 w-full md:w-auto">
        <div className="px-4 py-1.5 bg-blue-100 text-blue-700 rounded-full text-sm font-bold uppercase tracking-wider">
          Роль: {role}
        </div>
        <div className="flex items-center gap-4 mt-2">
          {/* КНОПКА ПОКАЗЫВАЕТСЯ ТОЛЬКО ПАЦИЕНТАМ */}
          {role === "patient" && (
            <button
              onClick={handleDeleteAccount}
              className="text-slate-400 hover:text-red-600 font-semibold text-sm transition-colors"
            >
              Удалить аккаунт
            </button>
          )}
          <button
            onClick={onLogout}
            className="text-red-500 hover:text-red-700 font-semibold text-sm transition-colors flex items-center gap-1"
          >
            Выйти из системы
          </button>
        </div>
      </div>
    </div>
  );
}

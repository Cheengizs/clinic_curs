import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authApi } from "../api/authApi";

interface LoginProps {
  onLogin: () => void;
}

export default function Login({ onLogin }: LoginProps) {
  // Состояние: true = Логин, false = Регистрация
  const [isLogin, setIsLogin] = useState(true);

  // Поля формы
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  // Статусы
  const [error, setError] = useState("");
  const [successMsg, setSuccessMsg] = useState("");
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMsg("");

    // Проверка паролей при регистрации
    if (!isLogin && password !== confirmPassword) {
      setError("Пароли не совпадают!");
      return;
    }

    setLoading(true);

    try {
      if (isLogin) {
        // ЛОГИН
        const data = await authApi.login(email, password);
        localStorage.setItem("accessToken", data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);
        onLogin();
        navigate("/profile");
        navigate("/profile");
      } else {
        // РЕГИСТРАЦИЯ
        await authApi.register(email, password);
        setSuccessMsg(
          "Вы успешно зарегистрированы! Проверьте вашу почту для подтверждения.",
        );
        // После успешной регистрации переключаем обратно на вкладку логина и очищаем пароли
        setIsLogin(true);
        setPassword("");
        setConfirmPassword("");
      }
    } catch (err: any) {
      // Пытаемся вытащить сообщение об ошибке с бэкенда
      setError(
        err.response?.data?.error ||
          err.response?.data ||
          "Произошла ошибка. Проверьте данные.",
      );
    } finally {
      setLoading(false);
    }
  };

  // Переключатель вкладок
  const toggleMode = () => {
    setIsLogin(!isLogin);
    setError("");
    setSuccessMsg("");
  };

  return (
    <div className="flex-grow flex items-center justify-center p-4 bg-slate-50">
      <div className="bg-white p-8 md:p-10 rounded-3xl shadow-xl shadow-slate-200/50 max-w-md w-full border border-slate-100 transition-all duration-300">
        {/* Заголовок */}
        <div className="text-center mb-8">
          <div className="w-12 h-12 bg-blue-600 rounded-xl flex items-center justify-center text-white font-bold text-2xl mx-auto mb-4 shadow-lg shadow-blue-600/30">
            C
          </div>
          <h1 className="text-2xl font-bold text-slate-900">
            {isLogin ? "С возвращением!" : "Создать аккаунт"}
          </h1>
          <p className="text-slate-500 text-sm mt-2">
            {isLogin
              ? "Войдите, чтобы управлять своими записями"
              : "Присоединяйтесь к нам для удобной записи"}
          </p>
        </div>

        {/* Сообщения об ошибке/успехе */}
        {error && (
          <div className="bg-red-50 text-red-600 p-4 rounded-xl text-sm mb-6 text-center border border-red-100 font-medium">
            {error}
          </div>
        )}
        {successMsg && (
          <div className="bg-green-50 text-green-700 p-4 rounded-xl text-sm mb-6 text-center border border-green-200 font-medium">
            {successMsg}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-sm font-semibold text-slate-700 mb-1.5">
              Email
            </label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-3 border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all text-slate-900 bg-slate-50 focus:bg-white"
              placeholder="mail@example.com"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-slate-700 mb-1.5">
              Пароль
            </label>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-3 border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all text-slate-900 bg-slate-50 focus:bg-white"
              placeholder="••••••••"
            />
          </div>

          {/* Поле подтверждения пароля (только при регистрации) */}
          {!isLogin && (
            <div>
              <label className="block text-sm font-semibold text-slate-700 mb-1.5">
                Подтвердите пароль
              </label>
              <input
                type="password"
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full px-4 py-3 border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all text-slate-900 bg-slate-50 focus:bg-white"
                placeholder="••••••••"
              />
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-3.5 px-4 rounded-xl transition-all shadow-md hover:shadow-lg disabled:opacity-70 disabled:cursor-not-allowed mt-2"
          >
            {loading
              ? "Загрузка..."
              : isLogin
                ? "Войти в кабинет"
                : "Зарегистрироваться"}
          </button>
        </form>

        {/* Переключатель Логин/Регистрация */}
        <div className="mt-8 text-center text-sm text-slate-500 border-t border-slate-100 pt-6">
          {isLogin ? (
            <>
              Нет аккаунта?{" "}
              <button
                onClick={toggleMode}
                type="button"
                className="text-blue-600 hover:text-blue-800 font-bold hover:underline transition-colors"
              >
                Создать сейчас
              </button>
            </>
          ) : (
            <>
              Уже есть аккаунт?{" "}
              <button
                onClick={toggleMode}
                type="button"
                className="text-blue-600 hover:text-blue-800 font-bold hover:underline transition-colors"
              >
                Войти
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

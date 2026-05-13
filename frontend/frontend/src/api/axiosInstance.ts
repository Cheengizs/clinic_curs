import axios from "axios";

const API_URL = "http://localhost:5133/api";

const axiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// 1. ПЕРЕХВАТЧИК ЗАПРОСОВ (уже был у тебя) - подставляет токен
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("accessToken");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

// 2. НОВЫЙ ПЕРЕХВАТЧИК ОТВЕТОВ - ловит 401 ошибку и обновляет токен
axiosInstance.interceptors.response.use(
  (response) => {
    // Если запрос прошел успешно, просто возвращаем ответ
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    // Если ошибка 401 (Токен протух) и мы еще не пытались его обновить (флаг _retry)
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true; // Ставим флаг, чтобы не уйти в бесконечный цикл

      try {
        const accessToken = localStorage.getItem("accessToken");
        const refreshToken = localStorage.getItem("refreshToken");

        // Если рефреша нет, значит обновлять нечем - выбрасываем пользователя
        if (!refreshToken || !accessToken) {
          throw new Error("No tokens available");
        }

        // ВАЖНО: Делаем запрос через "чистый" axios, а не через наш axiosInstance,
        // чтобы не попасть в бесконечный цикл перехватов, если рефреш тоже протух.
        const response = await axios.post(`${API_URL}/auth/refresh`, {
          accessToken: accessToken,
          refreshToken: refreshToken,
        });

        // Достаем новые токены
        const newAccessToken = response.data.accessToken;
        const newRefreshToken = response.data.refreshToken;

        // Сохраняем в память
        localStorage.setItem("accessToken", newAccessToken);
        localStorage.setItem("refreshToken", newRefreshToken);

        // Подменяем заголовок в старом запросе на новый токен
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;

        // Повторяем оригинальный запрос (например, получение профиля)
        return axiosInstance(originalRequest);
      } catch (refreshError) {
        // Если обновить не вышло (например, RefreshToken тоже протух)
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");

        // Триггерим событие, которое мы слушаем в App.tsx, чтобы разлогинить UI
        window.dispatchEvent(new Event("storage"));

        // Перекидываем на логин (опционально, можно просто оставить выброс события)
        window.location.href = "/login";

        return Promise.reject(refreshError);
      }
    }

    // Если это не 401 ошибка или мы уже пробовали обновить, прокидываем ошибку дальше
    return Promise.reject(error);
  },
);

export default axiosInstance;

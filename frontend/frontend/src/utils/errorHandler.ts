export const getErrorMessage = (err: any): string => {
  // Если сервер вообще не ответил (упал или нет интернета)
  if (!err.response) {
    return "Ошибка сети. Проверьте подключение к интернету.";
  }

  const data = err.response.data;

  // 1. Наш кастомный формат { error: "Сообщение" }
  if (data && data.error && typeof data.error === "string") {
    return data.error;
  }

  // 2. Стандартная валидация ASP.NET Core 400 Bad Request
  // Выглядит как { errors: { Email: ["Неверный формат"], Password: ["Короткий"] } }
  if (data && data.errors && typeof data.errors === "object") {
    // Берем первое поле с ошибкой и возвращаем его первое сообщение
    const firstKey = Object.keys(data.errors)[0];
    if (firstKey && Array.isArray(data.errors[firstKey])) {
      return data.errors[firstKey][0];
    }
  }

  // 3. Другие стандартные поля (например, Message)
  if (data && data.Message) {
    return data.Message;
  }

  // Если текст ошибки пришел просто строкой
  if (typeof data === "string") {
    return data;
  }

  return "Произошла неизвестная ошибка при обработке запроса.";
};

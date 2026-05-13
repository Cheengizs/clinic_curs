import axiosInstance from "./axiosInstance";

export const fileApi = {
  // 1. Загружаем сам файл в хранилище Azurite
  upload: async (file: File) => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await axiosInstance.post<{ fileId: string }>(
      "/files/upload",
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
      },
    );
    return response.data.fileId;
  },

  // 2. Обновляем аватар в профиле (PATCH /api/files/avatar)
  updateAvatar: async (fileId: string) => {
    const response = await axiosInstance.patch("/files/avatar", { fileId });
    return response.data;
  },
};

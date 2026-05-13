// src/api/verificationApi.ts
import axiosInstance from "./axiosInstance";

export interface SubmitVerificationDto {
  firstName: string;
  lastName: string;
  middleName: string;
  birthDate: string;
  passportSeriesNumber: string;
  personalNumber: string;
  officeId: string;
  scheduledAt: string;
}

// Добавляем этот интерфейс
export interface VerificationRequestDto extends SubmitVerificationDto {
  id: string;
  status: "wait" | "verified" | "declined";
  createdAt: string;
}

export const verificationApi = {
  submitRequest: async (dto: SubmitVerificationDto) => {
    const response = await axiosInstance.post("/verification/submit", dto);
    return response.data;
  },
  // ПОЛУЧИТЬ СВОЮ ЗАЯВКУ
  getMyRequest: async () => {
    const response =
      await axiosInstance.get<VerificationRequestDto>("/verification/my");
    return response.data;
  },
  // ОБНОВИТЬ СВОЮ ЗАЯВКУ
  updateRequest: async (dto: SubmitVerificationDto) => {
    const response = await axiosInstance.put("/verification/my", dto);
    return response.data;
  },
  // ДЛЯ РЕГИСТРАТОРА: СПИСОК ЖДУЩИХ
  getPendingRequests: async () => {
    const response = await axiosInstance.get<VerificationRequestDto[]>(
      "/verification/pending",
    );
    return response.data;
  },
  // ДЛЯ РЕГИСТРАТОРА: ОБРАБОТАТЬ
  processRequest: async (requestId: string, isApproved: boolean) => {
    const response = await axiosInstance.post(
      `/verification/${requestId}/process`,
      { isApproved },
    );
    return response.data;
  },
};

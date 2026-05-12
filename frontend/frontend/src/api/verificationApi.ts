import axiosInstance from "./axiosInstance";

export interface SubmitVerificationDto {
  firstName: string;
  lastName: string;
  middleName: string;
  birthDate: string; // Формат YYYY-MM-DD
  passportSeriesNumber: string;
  personalNumber: string;
  officeId: string;
  scheduledAt: string; // ISO формат даты-времени
}

export const verificationApi = {
  submitRequest: async (dto: SubmitVerificationDto) => {
    const response = await axiosInstance.post("/verification/submit", dto);
    return response.data;
  },
};

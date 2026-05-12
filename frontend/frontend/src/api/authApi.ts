import axiosInstance from "./axiosInstance";
import type { LoginResponse } from "../types/auth"; // Используем type!

export interface AccountMeDto {
  email: string;
  phone: string | null;
  emailVerified: boolean;
  phoneVerified: boolean;
  identityVerified: boolean;
  role: string;
}

export const authApi = {
  login: async (email: string, password: string) => {
    const response = await axiosInstance.post<LoginResponse>("/auth/login", {
      email,
      password,
    });
    return response.data;
  },
  // НОВЫЙ МЕТОД: Регистрация
  register: async (email: string, password: string) => {
    const response = await axiosInstance.post("/auth/register", {
      email,
      password,
    });
    return response.data;
  },
  getMe: async () => {
    const response = await axiosInstance.get<AccountMeDto>("/auth/me");
    return response.data;
  },
  sendPhoneCode: async () => {
    const response = await axiosInstance.post("/auth/phone/send-code");
    return response.data;
  },
  verifyPhoneCode: async (code: string) => {
    const response = await axiosInstance.post(
      `/auth/phone/verify?code=${code}`,
    );
    return response.data;
  },
  requestPhoneChange: async (newPhone: string) => {
    const response = await axiosInstance.post(
      `/auth/phone/change-request?newPhone=${encodeURIComponent(newPhone)}`,
    );
    return response.data;
  },
  logout: async (refreshToken: string) => {
    const response = await axiosInstance.post("/auth/logout", { refreshToken });
    return response.data;
  },
};

import axiosInstance from "./axiosInstance";

export interface RegisterRegistrarDto {
  firstName: string;
  lastName: string;
  middleName: string;
  phone: string;
  officeId: string;
}

export interface RegisterDoctorDto {
  firstName: string;
  lastName: string;
  middleName: string;
  phone: string;
  officeId: string;
  bio: string;
  specializations: {
    specializationId: string;
    isPrimary: boolean;
    careerStartDate: string | null;
  }[];
}

export const adminApi = {
  createOffice: async (data: {
    name: string;
    address: string;
    phone: string;
  }) => {
    const response = await axiosInstance.post("/admin/offices", data);
    return response.data;
  },
  createSpecialization: async (data: { name: string; description: string }) => {
    const response = await axiosInstance.post("/admin/specializations", data);
    return response.data;
  },
  registerRegistrar: async (data: RegisterRegistrarDto) => {
    const response = await axiosInstance.post("/admin/registrars", data);
    return response.data;
  },
  registerDoctor: async (data: RegisterDoctorDto) => {
    const response = await axiosInstance.post("/admin/doctors", data);
    return response.data;
  },
  createSchedule: async (data: {
    doctorId: string;
    officeId: string;
    workDate: string;
    startTime: string;
    endTime: string;
  }) => {
    const response = await axiosInstance.post("/admin/schedules", data);
    return response.data;
  },
};

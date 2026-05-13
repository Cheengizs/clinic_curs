import axiosInstance from "./axiosInstance";
import type {
  DoctorDto,
  PaginatedList,
  OfficeDto,
  AppointmentTypeDto,
} from "../types/clinic"; // Обновили импорт

export const clinicApi = {
  getDoctors: async (
    pageNumber = 1,
    pageSize = 10,
    officeId?: string,
    specializationId?: string,
  ) => {
    const response = await axiosInstance.get<PaginatedList<DoctorDto>>(
      "/clinic/doctors",
      { params: { pageNumber, pageSize, officeId, specializationId } },
    );
    return response.data;
  },
  getOffices: async () => {
    const response = await axiosInstance.get<OfficeDto[]>("/clinic/offices");
    return response.data;
  },
  // НОВЫЙ МЕТОД
  getAppointmentTypes: async () => {
    const response = await axiosInstance.get<AppointmentTypeDto[]>(
      "/clinic/appointment-types",
    );
    return response.data;
  },
};

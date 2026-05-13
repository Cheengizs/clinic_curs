import axiosInstance from "./axiosInstance";
import type {
  DoctorDto,
  PaginatedList,
  OfficeDto,
  AppointmentTypeDto,
  SpecializationDto, // Добавили импорт
} from "../types/clinic";

export const clinicApi = {
  getDoctors: async (
    pageNumber = 1,
    pageSize = 9, // Установим 9, так как у нас сетка в 3 колонки (3х3 красиво смотрится)
    officeId?: string,
    specializationId?: string,
  ) => {
    // Собираем параметры. Если ID пустой - не отправляем его на бэкенд
    const params: Record<string, any> = { pageNumber, pageSize };
    if (officeId) params.officeId = officeId;
    if (specializationId) params.specializationId = specializationId;

    const response = await axiosInstance.get<PaginatedList<DoctorDto>>(
      "/clinic/doctors",
      { params },
    );
    return response.data;
  },
  getOffices: async () => {
    const response = await axiosInstance.get<OfficeDto[]>("/clinic/offices");
    return response.data;
  },
  getAppointmentTypes: async () => {
    const response = await axiosInstance.get<AppointmentTypeDto[]>(
      "/clinic/appointment-types",
    );
    return response.data;
  },
  // НОВЫЙ МЕТОД ДЛЯ ФИЛЬТРОВ:
  getSpecializations: async () => {
    const response = await axiosInstance.get<SpecializationDto[]>(
      "/clinic/specializations",
    );
    return response.data;
  },
};

import axiosInstance from "./axiosInstance";

export interface PatientAppointmentDto {
  id: string;
  doctorName: string;
  doctorSpecialization: string;
  officeName: string;
  officeAddress: string;
  avatarUrl: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: string;
  typeName: string;
  hasReview: boolean;
}

// НОВЫЙ DTO ДЛЯ РЕГИСТРАТОРА
export interface OfficeAppointmentDto {
  id: string;
  patientName: string;
  doctorName: string;
  scheduledStart: string;
  status: string;
  typeName: string;
}

export const appointmentApi = {
  getAvailableSlots: async (doctorId: string, date: string, typeId: string) => {
    const response = await axiosInstance.get<string[]>(
      `/appointments/slots?doctorId=${doctorId}&date=${date}&typeId=${typeId}`,
    );
    return response.data;
  },
  bookAppointment: async (
    doctorId: string,
    typeId: string,
    scheduledStart: string,
  ) => {
    const response = await axiosInstance.post("/appointments/book", {
      doctorId,
      typeId,
      scheduledStart,
    });
    return response.data;
  },
  getMyAppointments: async () => {
    const response =
      await axiosInstance.get<PatientAppointmentDto[]>("/appointments/my");
    return response.data;
  },
  cancelAppointment: async (id: string) => {
    const response = await axiosInstance.patch(`/appointments/${id}/cancel`);
    return response.data;
  },
  leaveReview: async (
    appointmentId: string,
    rating: number,
    comment: string,
  ) => {
    const response = await axiosInstance.post(
      `/appointments/${appointmentId}/review`,
      { rating, comment },
    );
    return response.data;
  },

  // НОВЫЕ МЕТОДЫ ДЛЯ РЕСЕПШЕНА:
  getOfficeAppointmentsToday: async () => {
    const response = await axiosInstance.get<OfficeAppointmentDto[]>(
      "/appointments/office/today",
    );
    return response.data;
  },
  confirmAppointment: async (id: string) => {
    const response = await axiosInstance.patch(`/appointments/${id}/confirm`);
    return response.data;
  },
};

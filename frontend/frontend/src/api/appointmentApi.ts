import axiosInstance from "./axiosInstance";

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
};

export const TURNOS = [
  {id: 1, profesional: 'Dr. Martínez', especialidad: 'Diabetología', fecha: '2026-08-25', hora: '10:00', estado: 'Confirmado'},
  {id: 2, profesional: 'Lic. Gómez', especialidad: 'Nutrición', fecha: '2026-08-28', hora: '15:30', estado: 'Pendiente'},
  {id: 3, profesional: 'Dra. Fernández', especialidad: 'Cardiología', fecha: '2026-09-02', hora: '09:15', estado: 'Confirmado'}
];

const RETARDO_SIMULADO = 600;

export function obtener_turnos(){
  return new Promise(resolver => {
    window.setTimeout(() => resolver({turnos: TURNOS}), RETARDO_SIMULADO);
  });
}
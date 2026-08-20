export const HISTORIA = [
  {id: 1, fecha: '2026-08-15', tipo: 'Glucemia', valor: '110 mg/dL', notas: 'En ayunas'},
  {id: 2, fecha: '2026-08-16', tipo: 'Presión Arterial', valor: '120/80 mmHg', notas: 'Normal'},
  {id: 3, fecha: '2026-08-18', tipo: 'Peso', valor: '78 kg', notas: 'Control mensual'},
  {id: 4, fecha: '2026-08-20', tipo: 'Glucemia', valor: '125 mg/dL', notas: 'Post-prandial'}
];

const RETARDO_SIMULADO = 600;

export function obtener_historia(){
  return new Promise(resolver => {
    window.setTimeout(() => resolver({registros: HISTORIA}), RETARDO_SIMULADO);
  });
}
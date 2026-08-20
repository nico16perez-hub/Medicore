export const PLAN_NUTRICIONAL = [
  {id: 1, dia: 'Lunes', comida: 'Desayuno', detalle: 'Avena con frutas y té verde'},
  {id: 2, dia: 'Lunes', comida: 'Almuerzo', detalle: 'Pechuga de pollo a la plancha con ensalada'},
  {id: 3, dia: 'Lunes', comida: 'Cena', detalle: 'Sopa de verduras y pescado'},
  {id: 4, dia: 'Martes', comida: 'Desayuno', detalle: 'Yogur descremado con granola'}
];

const RETARDO_SIMULADO = 600;

export function obtener_nutricion(){
  return new Promise(resolver => {
    window.setTimeout(() => resolver({plan: PLAN_NUTRICIONAL}), RETARDO_SIMULADO);
  });
}
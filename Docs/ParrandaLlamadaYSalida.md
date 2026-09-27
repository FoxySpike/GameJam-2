# Parranda MVP: llamada y salida al nivel 2

La actualizacion de editor se aplica una vez al importar los scripts. Solo configura
`Assets/Scenes/Nivel 1 - La parranda MVP.unity`; los prefabs compartidos de jugador, UI y NPC
no se modifican. Si la escena tiene cambios pendientes, se conserva abierta y hay que guardar
con Ctrl+S. Si la importacion automatica no aplica la actualizacion, usar
`Tools > Parranda > Actualizar llamada, textos y salida` fuera de Play.

## Objetos y asignaciones

- `Level 1 Flow`: conserva Level1FlowController y WifeCallSequenceController. El segundo
  contiene los textos de la esposa, referencias del telefono y duraciones editables.
- `Level 1 Flow/Wife Call Canvas/Phone`: telefono en la esquina inferior derecha, con entrada
  y salida animadas, contacto Wife, estado de llamada, contador y dialogo. No tiene botones
  interactivos ni GraphicRaycaster y no captura el raton. Su Canvas escala desde 1920x1080.
- El antiguo Blackout y el panel Incoming Call quedan desactivados. La llamada permite
  caminar y mirar; no oscurece el mundo ni requiere pulsar una tecla para contestar.
- `TriggerNivel2`: SceneTrigger referencia el flujo de nivel 1. Solo permite cargar
  `Nivel-2-Asadero` cuando IsReadyForNextLevel es verdadero.
- Los otros SceneTrigger de Parranda tienen Permanently Locked activado. Ademas, el codigo
  impide que cualquier destino distinto del asadero se abra desde esta escena.
- Cada SceneTrigger de Parranda tiene un `Exit Marker/Exit Marker Cube` encima (el del
  nivel 2 conserva su padre anterior `Level 2 - Exit Marker`). Son cubos normales,
  estaticos y sin collider, situados dos metros sobre el borde superior de cada trigger.
  No tienen luces, giros ni efectos. Permanecen desactivados hasta que su propia entrada
  pueda usarse: trigger y collider habilitados, salida desbloqueada y sin transicion iniciada.
  Al entrar, el cubo se desactiva inmediatamente antes de cargar la escena.
  Se puede cambiar su malla/material; si se reemplaza el GameObject completo,
  asignar el nuevo objeto al campo Visuals del componente LevelExitBeacon en su padre.

La configuracion asigna automaticamente todas las referencias. No hace falta arrastrar assets.
La UI estatica, dialogos del NPC, objetivos, estado de borrachera y prompts de esta escena
se configuran en ingles. Los controles muestran F, que es la tecla vinculada a Interact
en NIS.inputactions. Los nombres internos de escenas y GameObjects no se traducen.

## Flujo

1. Al comenzar, GET DRUNK y todas las salidas bloqueadas; el cubo esta oculto.
2. Al alcanzar el maximo de alcohol o la intoxicacion especial existente, se inicia una sola llamada.
3. El telefono muestra INCOMING CALL, conecta automaticamente, presenta el dialogo y CALL ENDED.
4. Cuando el telefono termina de retirarse, el objetivo cambia a
   GET THE CHICKEN - GO TO THE CHICKEN SHOP, aparece el cubo y se habilita el nivel 2.
5. Entrar en el trigger del asadero carga el nivel 2. Los otros destinos siguen bloqueados.
   Si el jugador ya estaba dentro del volumen del asadero, se permite entrar al terminar la llamada.
6. Al cambiar de escena, el telefono y la senal desaparecen porque pertenecen a Parranda MVP.

## Revision manual pendiente

No se ejecutaron pruebas, builds ni Play Mode durante esta tarea.

1. Volver a Unity, esperar a que termine de importar, abrir Parranda MVP y guardar con Ctrl+S.
   Confirmar el mensaje de actualizacion en Console y los objetos indicados arriba.
2. Iniciar Play sin beber e intentar entrar en cada trigger. Ninguno debe cambiar de escena
   y no debe verse el cubo.
3. Mirar los controles, objetivos, estado de alcohol, prompts y dialogo del NPC. Deben estar
   en ingles; F permite hablar/beber segun el objeto apuntado.
4. Beber hasta completar la condicion. Debe aparecer un telefono a la derecha, sin blackout,
   mientras se puede caminar y mirar. La llamada avanza sola y solo ocurre una vez.
5. Durante la llamada, comprobar que la salida sigue bloqueada. Al terminar, comprobar el
   nuevo objetivo, solo el cubo sobre TriggerNivel2 y la desaparicion del telefono.
6. Volver a intentar los otros triggers: deben seguir bloqueados. Entrar al asadero: debe
   cargar Nivel-2-Asadero y desactivar su cubo al entrar. Confirmar que el telefono y los
   marcadores no persisten alli. Deshabilitar temporalmente el componente SceneTrigger
   o su collider durante Play tambien debe ocultar su cubo.
7. Revisar el telefono en las resoluciones/aspectos de Game que vaya a usar el juego y la altura
   del cubo sobre el trigger. Los ajustes finos de posicion quedan disponibles en sus Transforms.

Para editar o previsualizar el telefono fuera de Play, activar temporalmente el GameObject Phone;
dejarlo inactivo al guardar. Para editar un cubo, seleccionarlo en la jerarquia y activarlo
temporalmente fuera de Play; dejarlo inactivo al guardar.

Se corrigio el GUID invalido del .meta de ParrandaPresentationSetup, que hacia que Unity
ignorase el configurador y dejaba los textos anteriores. La version 2 vuelve a asignar los
dialogos, objetivos y prompts de esta escena en ingles, y muestra F como tecla de interaccion.
La version 3 agrega cubos a los demas triggers y conserva el cubo existente del nivel 2.

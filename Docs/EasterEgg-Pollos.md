# Easter egg de los pollos

Implementado para el proyecto actual. No se ejecutaron pruebas, Play Mode ni compilaciones: la validacion en Unity queda a cargo del usuario.

## Loop esperado

1. Iniciar en `Nivel 1 - La parranda MVP`.
2. Consumir una bebida con `DrinkData.IsAdulterated`: el contador existente suma uno; no hay transformacion.
3. Consumir una segunda bebida adulterada: el contador activa su bandera una sola vez y entra gradualmente el efecto de alucinacion. La apariencia de pollo queda pendiente; los NPCs siguen humanos durante la llamada.
4. La llamada de la esposa conserva su secuencia. Cuando termina y se oculta el telefono, solo si hay easter egg se bloquea temporalmente el control y aparece un fade a negro (0.8 segundos). Con la pantalla opaca se cambian los modelos y aparece durante 3.5 segundos: "Ugh... what did they give me? What did I drink? Why do I feel like this?". Se retira el texto y se revela el mundo con un fade de 1.2 segundos. Entonces se devuelve el control y se habilitan la salida normal al asadero y el atajo al nivel 3. Sin easter egg no hay blackout ni pensamiento adicional. No hay cambio automatico de escena al beber.
5. Entrar en `TriggerNivel3` para cargar `Nivel 3 - Cruzar la calle`. El jugador persistente conserva el contador y la bandera; los carros presentes y los que aparecen despues se muestran como pollos grandes.
6. El NPC conserva dialogos y movimiento. El carro conserva recorrido, velocidad y colision. El pollo que se recoge y transporta no se reemplaza.
7. La bandera dura toda la partida. Finalizar el dialogo de la escena Final descarta al jugador persistente antes de volver al inicio, de modo que la siguiente partida comienza con un jugador nuevo. Detener Play tambien descarta el estado; no se guarda en disco.

Llegar a `Wasted` por bebidas normales activa los efectos de camara, pero no transforma a nadie. La llamada sigue sus reglas existentes: alcohol maximo o dos bebidas adulteradas; `Wasted` por si solo no cambia esas reglas.

## Atajo temporal

En la escena inicial, seleccionar `TriggerNivel3`, ubicado en `(16.07, 0.8, -6.31)`.

En `SceneTrigger` esta activada **Enable Level 3 Test Shortcut**. Esta opcion permite superar el bloqueo permanente solo para el destino `Nivel 3 - Cruzar la calle`, y sigue exigiendo que termine la llamada. El cubo existente de `LevelExitBeacon` usa el mismo permiso y aparece al desbloquearse la salida.

Para terminar la etapa de pruebas, desmarcar esa casilla y guardar la escena. `Permanently Locked` sigue activado, por lo que el atajo vuelve a bloquearse sin editar codigo. La ruta normal al asadero se conserva.

## Archivos y responsabilidades

- `Assets/Scripts/Alcohol/ChickenEasterEgg.cs`: conecta el tracker del jugador persistente con los componentes visuales. Distingue entre bebida consumida y revelacion visual completada: solo permite pollos cuando la pantalla ya esta negra. Maneja el blackout Overlay, texto ingles, bloqueo temporal del control y estado persistente de la revelacion. Detecta actores de las escenas cargadas, incluidos humanos con Animator humanoide. No busca objetos cada frame ni mantiene otro contador. Si el easter egg se activa en una escena sin llamada pendiente, realiza la revelacion directamente.
- `Assets/Scripts/Level/WifeCallSequenceController.cs`: espera la revelacion despues de ocultar el telefono y antes de declarar completa la secuencia; por eso ninguna salida del nivel 1 se desbloquea antes de terminar el fade.
- `Assets/Scripts/Alcohol/ChickenAppearanceTarget.cs`: oculta los renderers originales y crea un pollo visual. Mantiene intactos colisiones, navegacion, scripts y animadores originales. Ajusta tamano y altura al modelo original; alterna idle/walk segun desplazamiento. Cancela suscripciones al desactivarse.
- `Assets/Resources/EasterEgg/ChickenVisual.prefab`: variante visual del modelo existente, con material y controlador `Pollo 1`. Sin Rigidbody, collider, interaccion de recoger ni cacareo. No modifica el prefab del pollo transportable.
- `Assets/Scripts/Alcohol/DrunkenVision.cs`: controla solamente el Weight del Volume guardado en el prefab del jugador. No crea ni destruye el Volume o su perfil, no modifica los overrides y no oscila Lens Distortion. Los pesos por estado y la transicion son editables.
- `Assets/Prefabs/Player/The Boss.prefab`: contiene `Intoxication Volume` como hijo persistente y `DrunkenVision` con su referencia asignada. El objeto ya existe antes de Play en las escenas que usan este prefab.
- `Assets/Settings/IntoxicationVolumeProfile.asset`: perfil editable con Lens Distortion, Bloom, Chromatic Aberration, Depth of Field, Motion Blur, Film Grain y Vignette. Se comparte entre las instancias del jugador.
- `Assets/Scripts/Alcohol/DrunkenLensMotion.cs`: componente del objeto `Intoxication Volume`. Anima suavemente el centro X/Y y los multiplicadores X/Y de Lens Distortion cuando hay Wasted, easter egg o previsualizacion manual. Usa un Volume secundario temporal, de mayor prioridad, que solo modifica esas coordenadas; no escribe sobre el perfil original. Respeta la intensidad, el estado activo de Lens Distortion y el Weight del Volume principal.
- `Assets/Editor/DrunkenLensMotionEditor.cs`: muestra los botones de previsualizacion y retorno al estado de alcohol directamente en el Inspector del Volume, ademas del modo de control actual.
- `Assets/Scripts/Alcohol/IntoxicationCamera.cs`: incluye la capa Intoxication en las camaras de juego y habilita postprocesado. Separa la capa UI en una camara Overlay sin postprocesado para conservar tambien el aviso World Space del parque. Los HUD Screen Space Overlay siguen dibujandose por encima.
- `PersistentPlayer.cs`: instala los dos controladores una sola vez en el jugador superviviente y limpia su referencia estatica al destruirse.
- `NPCDialogueController.cs`, `PolicePatrol.cs`, `TrafficVehicle.cs`: registran actores creados durante la partida. En trafico se crea primero el modelo original.
- `FridgeInteractable.cs`: registra tambien la camara de la nevera.
- `SceneTrigger.cs` y escena inicial: opcion reversible del atajo al nivel 3.
- `DialogueManager.cs`: descarta al jugador de la partida terminada antes de volver al inicio.
- `ProjectSettings/TagManager.asset`: capa 8, antes vacia, denominada `Intoxication`.

## Ajustes desde Inspector

En Hierarchy, desplegar el jugador y seleccionar `Intoxication Volume`. Editar el Profile asignado para ajustar, agregar o desactivar efectos. Hacer los ajustes fuera de Play para guardarlos con claridad. El script no sobrescribe parametros ni casillas de los efectos. Activar el checkbox de override junto a cada parametro que se quiera utilizar.

El Volume arranca con Weight 0. Para previsualizarlo fuera de Play, subir Weight a 1 y habilitar Post Processing en la vista Scene. Durante Play, seleccionar `Intoxication Volume` y pulsar **Previsualizar mareo (Weight = 1)** en `DrunkenLensMotion`: esto permite ver los efectos y su movimiento incluso estando sobrio. Ajustar el Weight directamente tambien hace que `DrunkenVision` ceda automaticamente el control a modo manual; ya no lo devuelve al valor anterior en el siguiente frame. Pulsar **Volver al estado de alcohol** para recuperar el comportamiento del juego. Sigue disponible la casilla `Automatic Weight` en el jugador.

Para la animacion, ajustar `Center Amplitude` (desplazamiento X/Y), `Frequency` (ciclos por segundo de cada eje), `Axis Variation` (variacion de multiplicadores) y `Fade Seconds` (entrada/salida suave). Desmarcar `Animate Lens` para detenerla gradualmente. La intensidad base se ajusta en Lens Distortion dentro del Profile.

Se agregaron valores iniciales editables: Motion Blur Camera Only con intensidad 0.3, Film Grain fino con intensidad 0.18 y Vignette con intensidad 0.28/suavidad 0.45. Motion Blur responde al movimiento de la camara; no produce desenfoque de movimiento con la camara quieta. Ningun script sobrescribe estos tres efectos. Se mezclan con el Weight principal y respetan sus casillas en el Profile.

Guardar los cambios de escena/prefab fuera de Play; los ajustes de componentes hechos durante Play son temporales. Los cambios sobre el asset compartido del Profile pueden persistir: para ajustes deliberadamente temporales, duplicar el Profile antes de editarlos. No editar `Lens Motion (runtime)`: su contenido es generado por la animacion; usar los controles del objeto principal.

Pesos iniciales: Sober 0, Tipsy 0, Drunk 0, Wasted 1 y Easter Egg 1. Son configurables en `DrunkenVision`; por ejemplo, se pueden dar pesos intermedios a Tipsy y Drunk. El easter egg tiene prioridad. `Transition Seconds` es el tiempo para recorrer de Weight 0 a 1 (1.2 segundos inicialmente).

`AlcoholSystem.cs` calcula el estado segun sus umbrales; `AdulteratedDrinkTracker.cs` activa la bandera por dos bebidas sospechosas; `DrunkenVision.GetTargetWeight()` selecciona el peso segun esos datos y `LateUpdate()` aplica la transicion. `IntoxicationCamera.cs` configura las camaras y la separacion de UI, pero no decide el estado ni activa efectos individuales. Los efectos individuales se eligen exclusivamente en el Profile.

Para personalizar un actor, agregar `ChickenAppearanceTarget` a su raiz antes de Play. Configurar `Original Renderers`, `Visual Anchor`, `Position Offset`, `Rotation Offset` y `Size Multiplier`. El prefab vacio utiliza el pollo visual predeterminado. Si se asigna otro prefab, debe ser exclusivamente visual. Para una escala completamente manual, desactivar `Fit Original Bounds`.

Los NPCs actuales y humanos humanoides de cada escena se registran automaticamente. Un nuevo NPC generico o un humano decorativo instanciado despues de cargar la escena debe llevar `ChickenAppearanceTarget` en su prefab. Los carros y NPCs con los scripts registrados ya lo agregan por codigo.

Para nuevas camaras de juego, agregar `IntoxicationCamera`. La UI dibujada por camara debe estar en la capa `UI`, incluidos sus hijos; un objeto colocado en otra capa se tratara como parte del mundo. Los Canvas Screen Space Overlay no necesitan cambios. La separacion de UI usa el camera stacking de URP.

## Pautas para tu comprobacion manual

- Una bebida sospechosa no transforma; dos si. Bebidas normales entre ambas no reinician el contador.
- La segunda bebida no transforma durante la llamada. Al terminar el telefono, aparece negro y texto en ingles; los pollos solo se ven al retirar el negro. La salida permanece cerrada hasta que termine la revelacion.
- En el nivel 3, observar tanto los carros que ya existen como los nuevos. Revisar visualmente escala, orientacion, posicion de las patas y relacion entre el pollo y la colision original.
- Verificar que los NPCs aun hablan/caminan y los vehiculos aun golpean al jugador.
- Revisar la legibilidad del telefono, dialogos, HUD, UI de la nevera y cartel del parque.
- Llegar a Wasted con bebidas normales: efectos de camara sin pollos.
- Desactivar el atajo y comprobar la ruta normal al asadero.
- Finalizar una partida y volver al inicio: contador y apariencia deben empezar de nuevo.

No se afirma que estas comprobaciones hayan pasado; son las pautas de validacion para el usuario.

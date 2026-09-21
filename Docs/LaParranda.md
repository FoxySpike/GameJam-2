# Nivel 1 — La Parranda

## Abrir y probar

Abre **Assets/Scenes/Nivel 1 - La parranda MVP.unity** y pulsa Play.
Es una copia configurada del nivel original, con su suelo, iluminación y prefab del jugador.
La escena original se conserva. El prefab compartido sí incorpora los componentes nuevos.

Controles: **WASD** caminar, **Shift** correr, **mouse** mirar, **E** interactuar.
Apunta con la cruz central; el alcance se mide desde el jugador porque la cámara existente es de tercera persona.
El diálogo del NPC libera el cursor para pulsar **ACEPTAR** o **RECHAZAR**.

Las tres botellas de la escena MVP son reutilizables para probar:
- Cerveza: +10.
- Guaro: +20.
- Trago Sospechoso: +5 y adulterado.

El NPC es una cápsula de prueba que ofrece un guaro una sola vez al aceptarlo.
Puedes reemplazar los modelos conservando los componentes y colliders.

## Archivos modificados

| Archivo | Cambio |
| --- | --- |
| Assets/Scripts/Player/PlayerMovement.cs | Mantiene CharacterController, walkSpeed, sprintSpeed y los ejes locales. Lee PlayerInputReader; emite cambio de movimiento; aplica perfiles por AlcoholState. Sin Animator ni NIS. |
| Assets/Scripts/Player/PlayerLook.cs | Cambia únicamente la fuente de input y su bloqueo, y valida cameraPivot. Conserva cálculo de pitch, yaw y transferencia del exceso de giro al personaje. |
| Assets/Prefabs/Player/The Boss.prefab | Incorpora y conecta input, alcohol, consumo, tracker, interacción y animación; asigna BossAnim; desactiva root motion; elimina PlayerInput sin callbacks que duplicaba la lectura de acciones. |
| Assets/Animaciones/Player/BossAnim.controller | Mantiene IsWalking y los clips existentes. Añade IsDrunk, estados sobrios/borrachos y transiciones sin exit time. |

Los GUID de PlayerMovement y PlayerLook se conservan. No se ha modificado NIS ni su asset de acciones.

## Scripts creados

Las rutas siguientes son relativas a **Assets/Scripts**.

| Archivo | Responsabilidad |
| --- | --- |
| Player/PlayerInputReader.cs | Única instancia de NIS; MoveInput, LookInput, Sprint, evento Interact y bloqueos por propietario. |
| Player/PlayerAnimationController.cs | Traduce movimiento y AlcoholState a IsWalking e IsDrunk. |
| Player/PlayerInteraction.cs | Raycast, oclusión, alcance, contrato IInteractable y evento del prompt. |
| Player/DrinkingSystem.cs | Consumo compartido, log, aplicación de alcohol y evento OnDrinkConsumed. |
| Alcohol/AlcoholState.cs | Enum Sober, Tipsy, Drunk y Wasted. |
| Alcohol/AlcoholSystem.cs | Valor limitado, thresholds configurables y eventos de valor, estado y máximo. |
| Alcohol/DrinkData.cs | ScriptableObject con nombre, cantidad e indicador de adulteración. |
| Alcohol/AdulteratedDrinkTracker.cs | Cuenta bebidas adulteradas y emite el evento especial una vez. |
| Interaction/IInteractable.cs | Contrato Prompt, CanInteract(GameObject) e Interact(GameObject). |
| Interaction/DrinkInteractable.cs | Solicita el consumo de DrinkData; opción de un solo uso. |
| NPC/NPCDrinkOffer.cs | Proximidad, oferta, decisión y uso del mismo DrinkingSystem. |
| NPC/NPCDialogueController.cs | Nombre y frases de atención, oferta, aceptación y rechazo. |
| UI/InteractionUI.cs | Muestra el prompt recibido por evento. |
| UI/DialogueUI.cs | Panel, textos, botones, cursor y bloqueo durante una decisión. Emite la elección, sin consumir bebidas. |
| UI/AlcoholHUD.cs | Slider y etiqueta de estado mediante eventos; prioridad visual para ???. |
| UI/ObjectiveUI.cs | Recibe y presenta el objetivo. |
| Level/Level1FlowController.cs | Escucha ambas condiciones, inicia el final una vez y publica disponibilidad para Nivel 2. |
| Level/WifeCallSequenceController.cs | Fundido, teléfono, diálogo temporal, nuevo objetivo y evento de finalización. |

Herramientas y pruebas:
- **Assets/Editor/Level1Setup.cs**: menú **La Parranda > Crear escena MVP**. Monta la escena mediante las APIs del Editor y no sobrescribe una escena MVP que ya exista.
- **Assets/Editor/Level1Automation.cs**: menú **La Parranda > Ejecutar pruebas**, que ejecuta la suite y escribe **Logs/LaParranda.Tests.xml**.
- **Assets/Tests/Editor/Level1GameplayTests.cs**: pruebas de integración que entran y salen de Play Mode.

Assets creados:
- **Assets/Scenes/Nivel 1 - La parranda MVP.unity**.
- **Assets/Data/Drinks/Cerveza.asset**, **Guaro.asset**, **Trago Sospechoso.asset**.
- Cuatro materiales en **Assets/Data/Materials** para las bebidas y el NPC de prueba.
- Recursos esenciales estándar de **TextMesh Pro** en **Assets/TextMesh Pro**, necesarios para los textos.
- Archivos .meta correspondientes.

## Componentes y referencias de Inspector

La escena MVP y el prefab ya están conectados. Estos pasos sirven para integrar los componentes en otra escena o reconstruirlos manualmente.

### Jugador

En la raíz de **The Boss**:
- CharacterController existente.
- PlayerInputReader.
- AlcoholSystem.
- PlayerMovement.
- PlayerLook.
- PlayerAnimationController.
- PlayerInteraction.
- DrinkingSystem.
- AdulteratedDrinkTracker.

Referencias:
- PlayerMovement: **Input Reader** y **Alcohol System** de la misma raíz.
- PlayerLook: **Input Reader** de la raíz y **Camera Pivot** existente. Conserva sensibilidad y límites configurados.
- PlayerAnimationController: **Movement**, **Alcohol System** y el **Animator** del modelo.
- PlayerInteraction: **Input Reader** y la cámara hija.
- DrinkingSystem: **Alcohol System**.
- AdulteratedDrinkTracker: **Drinking System**.

Las dependencias del mismo GameObject se resuelven también mediante GetComponent en Awake.
No añadas otro PlayerInput para leer el mapa Player: NIS ya pertenece a PlayerInputReader.
El EventSystem usa su propio módulo de input exclusivamente para la UI.

AlcoholSystem comienza en 0 y tiene máximo 100; thresholds iniciales 20, 40 y 70.
Los valores se ajustan desde Inspector. Los thresholds se limitan al rango y se ordenan durante la validación.
El evento de máximo se emite al cruzar hasta el máximo; el coordinador narrativo mantiene su propio guard de una sola ejecución.

**State Profiles** de PlayerMovement configura por estado:
- Speed Multiplier, multiplicador de walkSpeed/sprintSpeed.
- Lateral Amplitude, intensidad de oscilación lateral al caminar.
- Lateral Frequency, frecuencia de esa oscilación.

Sober conserva el desplazamiento estable original. Para quitar toda inestabilidad pon las amplitudes en cero.
No hay gravedad nueva, tropiezos, caídas ni efectos de cámara.

PlayerInteraction:
- **Max Interaction Distance**: 3 inicialmente, medidos desde la raíz del jugador hasta el impacto.
- **Raycast Mask**: incluye tanto interactuables como paredes/suelo que deban ocluir.
- Ignora triggers y colliders del propio jugador; las zonas de NPC deben tener también un collider sólido para apuntar.
- La UI usa el binding existente [E]. Si cambias ese binding en NIS, actualiza también los prompts.

### Canvas

Usa Canvas en Screen Space Overlay, CanvasScaler y GraphicRaycaster.
Mantén los scripts de UI en un GameObject activo; sus paneles visuales pueden ser hijos inactivos.

| Componente | Referencias |
| --- | --- |
| InteractionUI | PlayerInteraction y TMP_Text del prompt. |
| AlcoholHUD | AlcoholSystem, AdulteratedDrinkTracker, Slider y TMP_Text del estado. Slider no interactuable. |
| ObjectiveUI | TMP_Text del objetivo. |
| DialogueUI | PlayerInputReader, panel hijo, TMP_Text de nombre y diálogo, Button aceptar y rechazar. |

Los botones se conectan mediante código. No añadas un segundo listener persistente que vuelva a consumir.
Añade un **EventSystem** con **InputSystemUIInputModule**; no uses StandaloneInputModule con el input legacy.
La escena MVP ya lo incluye.

El HUD no calcula estados: muestra SOBRIO, PRENDIDO, BORRACHO y VUELTO MIERDA.
Al activarse el tracker muestra ??? durante el resto del nivel; al recargar la escena se reinicia.

### Botella o vaso

1. Crea un objeto con renderer y collider sólido.
2. Añade DrinkInteractable.
3. Arrastra un DrinkData a **Drink Data**.
4. Activa **Single Use** para consumir y ocultar el objeto una sola vez.
5. Desactívalo para una bebida reutilizable, como las botellas de prueba.
6. Incluye su layer en la máscara de PlayerInteraction.

No se necesitan tags, Rigidbody, inventario ni referencias directas al HUD.
La interfaz recibe al jugador y el objeto obtiene su DrinkingSystem.

### Crear nuevas bebidas

En Project: **Create > La Parranda > Drink Data**.
Configura **Drink Name**, **Alcohol Amount** e **Is Adulterated**.
Asigna ese asset a cualquier DrinkInteractable o NPCDrinkOffer.
No hace falta modificar código para añadir otra bebida.

### NPC que ofrece una bebida

En la raíz del NPC:
- NPCDialogueController con sus cuatro frases y nombre.
- NPCDrinkOffer con **Offered Drink**, **Dialogue** y **Dialogue UI** del Canvas.
- Un collider sólido para que el raycast pueda apuntarle.
- Un SphereCollider con **Is Trigger** y el radio de proximidad deseado.
- Rigidbody cinemático, sin gravedad, para asegurar eventos de trigger.

Los colliders del ejemplo están en la misma raíz que NPCDrinkOffer.
Si reorganizas colliders en hijos, conserva el Rigidbody y el receptor de triggers en la raíz.

**Single Offer** significa una aceptación por instancia; rechazar permite volver a hablar.
La atención se muestra al entrar. E abre la decisión y bloquea gameplay; aceptar o rechazar cierra la decisión.
Aceptar usa el mismo DrinkingSystem. Rechazar solo muestra una respuesta.
Salir de la zona, desactivar al NPC o comenzar el blackout cierra su diálogo.
NPC puramente ambientales solo necesitan su Animator y clips en loop.

### Flujo del nivel y llamada

En **Level 1 Flow**:
- Level1FlowController: AlcoholSystem, AdulteratedDrinkTracker, PlayerInputReader, WifeCallSequenceController y ObjectiveUI.
- WifeCallSequenceController: CanvasGroup del negro, panel de teléfono, TMP_Text de caller y diálogo, ObjectiveUI.

El CanvasGroup del blackout usa una imagen negra que cubre la pantalla.
El panel de teléfono debe dibujarse por encima del negro.
En el MVP, el objetivo y el texto de alcohol se dibujan también por encima para conservar visibles CONSIGUE EL POLLO y ???.
Mantén activo el GameObject del controlador; solo se oculta su panel hijo.

Configura Fade Duration, Incoming Call Duration, Line Duration, Caller Name y Dialogue Lines.
Los tiempos usan tiempo real. El flujo funciona incluso si Time.timeScale es cero.
El input permanece bloqueado cuando termina la llamada, a la espera de la transición.

**Level1FlowController.OnReadyForNextLevel** y **IsReadyForNextLevel** son el punto de conexión para Nivel 2.
No se carga una escena inexistente ni se desbloquea al jugador automáticamente.
Un componente futuro de transición puede suscribirse al evento y consultar la propiedad si se activa tarde.

## Animator

Parámetros Bool:
- **IsWalking**: desplazamiento efectivo mediante CharacterController.
- **IsDrunk**: AlcoholState distinto de Sober.

| Estado | IsWalking | IsDrunk |
| --- | --- | --- |
| Sober Idle | false | false |
| Walking | true | false |
| Drunk Idle | false | true |
| Drunk Walk | true | true |

BossAnim contiene transiciones desde Any State con ambas condiciones, sin exit time y sin transición a sí mismo.
Desactiva Apply Root Motion: CharacterController controla la traslación.
Drinking, Falling, GettingUp y HoldingChicken no se implementan.

**Limitación de assets detectada:** este repositorio solo contiene los clips Drunk Idle y Walking.
Sober Idle reutiliza temporalmente Drunk Idle; Drunk Walk reutiliza Walking.
La selección lógica de las cuatro ramas funciona, pero para obtener las cuatro animaciones visualmente distintas debes importar y asignar los clips que faltan.
Si Walking es tu clip sobrio, sustituye Drunk Walk por un clip borracho; asigna también Idle sobrio.
No se ha generado una animación ficticia.

## Prueba manual

1. Abre la escena MVP y Play: SOBRIO, barra vacía, objetivo EMBORRÁCHATE.
2. Camina, corre y mira; comprueba el límite horizontal y que el cuerpo rota al excederlo.
3. Apunta a una botella y pulsa E. Debe aparecer el log de consumo y subir la barra.
4. Con cervezas: a 20 PRENDIDO, 40 BORRACHO, 70 VUELTO MIERDA. Revisa los parámetros del Animator y perfiles del movimiento.
5. Acércate al NPC; apunta al cuerpo, pulsa E y RECHAZAR: alcohol sin cambios.
6. Vuelve a hablar y ACEPTAR: añade 20. El cursor y el input regresan al cerrar.
7. Llega a 100: se bloquean caminar, mirar e interactuar; fundido, llamada, ¿Y EL POLLO?, CONSIGUE EL POLLO.
8. Reinicia Play Mode. Bebe dos veces Trago Sospechoso: alcohol 10, HUD ???, misma secuencia final.
9. Cambia Required Adulterated Drinks para probar otro umbral. Las bebidas normales intercaladas no reinician el contador.
10. Para probar ambos eventos próximos entre sí, usa una bebida adulterada con más alcohol. La secuencia debe comenzar una sola vez.

Pruebas automatizadas: **La Parranda > Ejecutar pruebas** o Test Runner > EditMode > Level1GameplayTests.
Las pruebas entran en Play Mode, cargan la escena MVP y aceleran los tiempos solo en la instancia de prueba.
Comprueban inicialización, bloqueos simultáneos, aceptar/rechazar, thresholds, Animator, clamp, desuscripción del tracker, finales normal/especial, guard de final único, oclusión y consumo único.
El resultado se guarda en **Logs/LaParranda.Tests.xml**; Logs está excluido de Git.

Verificación realizada en Unity 6000.3.11f1: **2 pruebas pasadas, 0 fallidas**.
También se compilaron los scripts de runtime y Editor con las referencias reales del proyecto.
Capturas revisadas: **Logs/LaParranda.Start.png** y **Logs/LaParranda.End.png**.

## Pasos manuales pendientes

- Importar y asignar Idle sobrio y la variante de caminar que falta.
- Reemplazar las primitivas de prueba por tus botellas y modelos de NPC; ajustar posiciones y textos.
- Ajustar balance, perfiles, distancias y tiempos desde Inspector.
- Si deseas incluir el MVP en un build, añadir **Nivel 1 - La parranda MVP** al perfil de Build. La lista original de escenas no se reemplaza.
- Conectar OnReadyForNextLevel a la escena real del Nivel 2 cuando exista.

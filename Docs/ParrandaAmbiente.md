# Ambiente de Parranda MVP

La integracion se aplica una sola vez cuando Unity importa los scripts. Solo abre y modifica
`Assets/Scenes/Nivel 1 - La parranda MVP.unity`. No entra en Play ni ejecuta pruebas.
Si la escena ya tenia cambios sin guardar, los conserva y deja el guardado a cargo del usuario.
El menu `Tools > Parranda > Integrar ambiente MVP` permite completar la integracion si la
actualizacion automatica de assets estaba desactivada. Si el objeto `Ambiente Parranda MVP`
ya tiene su controlador de musica, la herramienta no vuelve a colocar objetos; solo actualiza
las asignaciones de los dos NPCs sentados cuando difieren de la configuracion solicitada.

## Configuracion

- `NPC_AMBIENTE_Remy_Salsa`: nuevo Remy bailando salsa.
- `NPC_AMBIENTE_Ch21_Sentado`: nuevo Ch21 en una silla libre existente, solo con Sitting,
  sin transicion a Sitting Idle; conserva la pose final. La posicion se calcula con la cadera de la pose sentada y el
  respaldo de la silla; revisar visualmente el ajuste fino de cadera, pies y respaldo.
- `NPC_BARRIO 1`: salsa. `NPC_BARRIO 3`: solo Sitting Idle en bucle. `NPC_COCINERO`: samba.
- `NPC_BARRIO 2`, sus componentes de seguimiento/oferta y el Parcero inactivo quedan intactos.
- Los modelos, las cuatro animaciones y las tres canciones se copian en
  `Assets/Ambiente/ParrandaMVP`. Los originales, prefabs y controladores compartidos se conservan.
- Las copias de los rigs son Humanoid. Los bailes y Sitting Idle repiten; Sitting termina
  y mantiene su ultima pose. Root Motion desactivado para mantener la posicion de los NPCs.
- Ambos `Speaker Pack #1` usan sus empties como emisores AudioSource 100% 3D. Cada pack
  reparte un volumen de 0.45 entre sus emisores, con atenuacion lineal entre 2 y 28 metros,
  sin Doppler. Las copias de las canciones se importan en mono y streaming.
- `Ambiente Parranda MVP > ParrandaMusicPlayer` contiene canciones y fuentes asignadas.
  Un reloj DSP sincroniza todos los emisores. Las tres canciones se barajan por rondas,
  sin repetir consecutivamente. Una segunda fuente por emisor prepara el siguiente tema;
  se crea solamente durante Play y se limpia al destruir el controlador.

## Revision manual (no ejecutada por Codex)

1. Volver a Unity, dejar que termine de importar y abrir Parranda MVP. Buscar el mensaje
   `Ambiente integrado en Parranda MVP` en Console y el objeto `Ambiente Parranda MVP`.
   Si hace falta, usar el menu de integracion indicado arriba. Guardar con Ctrl+S.
2. Entrar en Play. Confirmar que los dos modelos nuevos tienen sus materiales, Remy baila
   y Ch21 termina sentado. Revisar pies, cadera, respaldo y que no se atraviesen otros objetos.
3. Revisar los tres NPCs existentes animados y confirmar que permanecen en su lugar.
   Acercarse a NPC_BARRIO 2 y comprobar que sigue al jugador y ofrece el trago como antes.
4. Caminar hacia cada baffle, entre ellos y alejarse unos 28 metros. Debe escucharse la misma
   cancion sincronizada, cambiar la direccion del sonido al girar y bajar el volumen al alejarse.
   Si no se oye nada, revisar que Game no tenga Mute Audio activado.
5. Dejar terminar varias canciones y comprobar que los dos baffles cambian juntos.
   Para revisar la seleccion sin esperar, durante Play abrir el menu contextual del componente
   ParrandaMusicPlayer y elegir `Siguiente cancion (solo en Play)`. Este salto incluye un segundo
   de preparacion; las transiciones naturales se programan en el final exacto del clip.
6. Salir y volver a entrar en Play: no debe duplicarse la musica. Cambiar de escena y comprobar
   que la musica de Parranda se detiene.

No es necesario arrastrar ni asignar assets. El volumen y las distancias se pueden ajustar
en los AudioSource de los empties antes de Play. La colocacion final y el resultado audiovisual
quedan pendientes de la revision manual del usuario.

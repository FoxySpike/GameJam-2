# Animaciones del jugador

El prefab `Assets/Prefabs/Player/The Boss.prefab` utiliza `BossAnim.controller`.
`PlayerMovementV3` publica `IsMoving` y `IsSprinting`; `PlayerAnimationController`
los comunica al Animator mediante `IsWalking`, `IsSprinting` e `IsDrunk`.

| Estado | Entrada | Animación |
| --- | --- | --- |
| Sobrio | Sin movimiento, incluso manteniendo sprint | IdleNormal |
| Sobrio | Movimiento sin sprint | WalkingNormal |
| Sobrio | Movimiento con sprint | Sprint |
| Prendido, Tomado o VueltoMierda | Sin movimiento | Drunk Idle |
| Prendido, Tomado o VueltoMierda | Movimiento, con o sin sprint | Drunk Walk |

El sprint sólo aumenta la velocidad estando sobrio. Al cambiar a otro estado,
se cancela inmediatamente el indicador de sprint y el movimiento utiliza la
velocidad de caminar. Los perfiles conservan su interpolación de velocidad y
sus efectos de equilibrio. Bloquear la entrada, entrar a la nevera o desactivar
el componente de movimiento cancela también el sprint.

## Comprobación visual en Play Mode

1. Con alcohol inicial 0, comprobar idle, caminar, mantener sprint mientras se
   camina y soltarlo sin dejar de moverse.
2. Mantener sprint sin moverse: debe continuar IdleNormal.
3. Pasar a Prendido mientras se corre: debe cambiar a Drunk Walk y dejar de correr.
4. Repetir en Tomado y VueltoMierda: sprint no debe activar Sprint.
5. Volver a Sobrio: recuperar IdleNormal/WalkingNormal/Sprint según la entrada.
6. Bloquear controles o entrar en la nevera mientras se corre: pasar a idle.

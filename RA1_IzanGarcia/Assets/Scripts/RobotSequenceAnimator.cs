using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MyRobotController))]
public class RobotSequenceAnimator : MonoBehaviour
{
    public Transform targetCube;
    public Transform dropZone;

    private MyRobotController bot;
    private bool isSequenceRunning = false;

    // Altura de seguridad para aproximarse
    private float alturaHover = 0.5f;

    void Awake() => bot = GetComponent<MyRobotController>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) && !isSequenceRunning)
        {
            StartCoroutine(PerformPreciseSequence());
        }
    }

    private IEnumerator PerformPreciseSequence()
    {
        isSequenceRunning = true;
        bot.manualMode = false;
        Debug.Log(" INICIO SECUENCIA ");

        // 1. Hover sobre el cubo
        Vector3 cuboHoverPos = targetCube.position + Vector3.up * alturaHover;
        bot.MoveToTarget(cuboHoverPos);
        while (bot.isBusy) yield return null;

        // 2. Descender
        bot.MoveToTarget(targetCube.position);
        while (bot.isBusy) yield return null;
        yield return new WaitForSeconds(0.5f);

        // 3. Verificar y Agarrar
        if (bot.IsTouchingObject(targetCube.gameObject))
        {
            Debug.Log("Contacto. Agarrando.");
            bot.ForceGrab(targetCube.gameObject);
        }
        else
        {
            bot.ForceGrab(targetCube.gameObject);
        }
        yield return new WaitForSeconds(0.5f);

        // 4. Subir a Hover (Levantamos el objeto)
        bot.MoveToTarget(cuboHoverPos);
        while (bot.isBusy) yield return null;


        // ====================================================================
        // NUEVO: DEMOSTRACIÓN DE ROTACIÓN (Dar una vuelta al objeto)
        // ====================================================================
        Debug.Log("Mostrando objeto (Rotación)...");

        // Obtenemos el ángulo actual de la base para que el robot no gire la cintura, solo la mano
        float currentBase = bot.joint_0_Base.localEulerAngles.y;

        // Definimos una pose segura "de inspección" (Brazo levantado)
        // Array: { Base, Hombro, Codo, Muñeca, MiniCodo, GRIPPER }

        // Paso A: Girar el objeto hacia un lado (+90 grados)
        float[] poseGiroA = { currentBase, -30f, 45f, 0f, 45f, 90f };
        yield return StartCoroutine(bot.MoveToPose(poseGiroA, 1.0f)); // 1 segundo para girar

        // Paso B: Girar el objeto hacia el otro lado (-90 grados) para que se vea el movimiento
        float[] poseGiroB = { currentBase, -30f, 45f, 0f, 45f, -90f };
        yield return StartCoroutine(bot.MoveToPose(poseGiroB, 1.5f)); // 1.5 segundos para girar al otro lado

        // Paso C: Dejarlo recto otra vez antes de viajar
        float[] poseRecta = { currentBase, -30f, 45f, 0f, 45f, 0f };
        yield return StartCoroutine(bot.MoveToPose(poseRecta, 0.5f));

        yield return new WaitForSeconds(0.2f);
        // ====================================================================


        // 5. ENTREGA (Viaje a DropZone)
        // Primero hover sobre DropZone
        Vector3 dropHoverPos = dropZone.position + Vector3.up * alturaHover;
        bot.MoveToTarget(dropHoverPos);
        while (bot.isBusy) yield return null;

        // 6. Descender
        bot.MoveToTarget(dropZone.position);
        while (bot.isBusy) yield return null;
        yield return new WaitForSeconds(0.5f);

        // 7. Verificar y Soltar
        if (bot.IsInDropZone())
        {
            bot.ReleaseObject();
        }
        else
        {
            bot.ReleaseObject();
        }
        yield return new WaitForSeconds(0.5f);

        // 8. Subir a Hover (Alejarse)
        bot.MoveToTarget(dropHoverPos);
        while (bot.isBusy) yield return null;

        // 9. Resetear brazo a posición original
        yield return StartCoroutine(bot.ResetArm());

        Debug.Log("FIN ");
        bot.manualMode = true;
        isSequenceRunning = false;
    }
}
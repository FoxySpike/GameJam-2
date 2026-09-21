using UnityEngine;

public sealed class NPCDialogueController : MonoBehaviour
{
    [SerializeField] private string characterName = "Parcero";
    [SerializeField, TextArea] private string attention = "¡Venga, tómese uno!";
    [SerializeField, TextArea] private string offer = "¿Se anima a un trago?";
    [SerializeField, TextArea] private string accepted = "¡Salud!";
    [SerializeField, TextArea] private string rejected = "Bueno, será después.";

    public string CharacterName => characterName;
    public string Attention => attention;
    public string Offer => offer;
    public string Accepted => accepted;
    public string Rejected => rejected;
}

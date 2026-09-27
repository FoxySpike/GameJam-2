using UnityEngine;

public sealed class NPCDialogueController : MonoBehaviour
{
    [SerializeField] private string characterName = "Buddy";
    [SerializeField, TextArea] private string attention = "Come on, have a drink!";
    [SerializeField, TextArea] private string offer = "How about a drink?";
    [SerializeField, TextArea] private string accepted = "Cheers!";
    [SerializeField, TextArea] private string rejected = "All right, maybe later.";

    public string CharacterName => characterName;
    public string Attention => attention;
    public string Offer => offer;
    public string Accepted => accepted;
    public string Rejected => rejected;
}

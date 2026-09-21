using UnityEngine;

[CreateAssetMenu(fileName = "Drink", menuName = "La Parranda/Drink Data")]
public sealed class DrinkData : ScriptableObject
{
    [SerializeField] private string drinkName = "Cerveza";
    [SerializeField, Min(0f)] private float alcoholAmount = 10f;
    [SerializeField] private bool isAdulterated;

    public string DrinkName => drinkName;
    public float AlcoholAmount => alcoholAmount;
    public bool IsAdulterated => isAdulterated;

    private void OnValidate() => alcoholAmount = Mathf.Max(0f, alcoholAmount);
}

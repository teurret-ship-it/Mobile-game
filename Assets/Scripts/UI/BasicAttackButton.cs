using UnityEngine;
using UnityEngine.EventSystems;
using SurvivalMoba.Combat;

namespace SurvivalMoba.UI
{
    /// <summary>
    /// On-screen basic-attack button. The hero also auto-attacks, but this lets the
    /// player force an attack on demand, matching the MOBA control layout.
    /// </summary>
    public class BasicAttackButton : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private BasicAttack basicAttack;

        private void Awake()
        {
            if (basicAttack == null) basicAttack = FindObjectOfType<BasicAttack>();
        }

        public void SetTarget(BasicAttack attack) => basicAttack = attack;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (basicAttack != null) basicAttack.TryAttack();
        }
    }
}

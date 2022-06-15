using TMPro;
using UnityEngine;

namespace Cards
{
    [RequireComponent(typeof(Animator))]
    public class Player : Target
    {
        private Animator _animator;
        [SerializeField, Range(1, 50)]
        private int _health = 30;
        private int _currentHealth;
        [SerializeField]
        private TextMeshPro _healthText;
        [SerializeField, Range(1, 50)]
        private int _maxMana = 10;
        private int _currentMaxMana;
        private int _mana;
        public int GetCurrentMana()
        {
            return _mana;
        }
        [SerializeField]
        private TextMeshPro _manaText;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _currentHealth = +_health;
            _currentMaxMana = 1;
            _mana = 1;
        }

        public void MoveToNextRound()
        {
            _currentMaxMana += 1;
            if (_currentMaxMana > _maxMana) _currentMaxMana = _maxMana;
            _mana = _currentMaxMana;
        }

        public void Update()
        {
            _healthText.text = "Здоровье=" + _currentHealth;
            _manaText.text = "Мана " + _mana + " из " + _currentMaxMana;
        }

        public override void SetDamage(int damage)
        {
            _currentHealth -= damage;
            _animator.SetTrigger("Damage");
        }
        internal void AddHP(int hp)
        {
            _currentHealth += hp;
            if (_currentHealth > _health) _currentHealth = _health;
        }

        internal void RemoveManaCost(ushort cost)
        {
            _mana -= cost;
        }

        internal void Turn()
        {
            gameObject.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
        }
    }
}

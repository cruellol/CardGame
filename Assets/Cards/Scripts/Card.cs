using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cards
{
    public abstract class Target : MonoBehaviour
    {
        public abstract void SetDamage(int damage);
    }

    [RequireComponent(typeof(Animator))]
    public class Card : Target, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Animator _animator;
        [SerializeField]
        private GameObject _frontCard;
        [SerializeField]
        private MeshRenderer _picture;
        [SerializeField]
        private TextMeshPro _name;
        [SerializeField]
        private TextMeshPro _descirption;
        [SerializeField]
        private TextMeshPro _cost;
        [SerializeField]
        private TextMeshPro _attack;
        [SerializeField]
        private TextMeshPro _health;
        [SerializeField]
        private TextMeshPro _type;
        [SerializeField]
        private MeshRenderer _innerMesh;
        [SerializeField]
        private GameObject _sleepObj;

        public bool Sleeping
        {
            get
            {
                return _sleepObj.activeSelf;
            }
            set
            {
                _sleepObj.SetActive(value);
            }
        }

        internal void Restore(int hp)
        {
            Health += hp;
            if (Health > _currentData.Health)
            {
                Health = _currentData.Health;
            }
        }

        internal void ChangeAttackAndHealth(int deltaAttack, int deltaHp)
        {
            Health += deltaHp;
            if (Health <= 0)
            {
                Table.Remove(this);
            }
            Attack += deltaAttack;
        }

        internal void ChangeAttack(int delta)
        {
            Attack += delta;
        }

        public bool Acted { get; set; } = false;

        public void SetInnerMesh(bool isEnabled = true)
        {
            _innerMesh.enabled = isEnabled;
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public CardStateType State { get; set; } = CardStateType.InDeck;

        public bool FrontSide
        {
            get
            {
                return _frontCard.activeSelf;
            }
            set
            {
                _frontCard.SetActive(value);
                _picture.gameObject.SetActive(value);
            }
        }

        public PlayerHand Hand { get; set; }
        public PlayerTable Table { get; set; }
        private CardPropertiesData _currentData;
        public CardPropertiesData CardProperty
        {
            get { return _currentData; }
        }

        public bool HasTaunt
        {
            get
            {
                return CheckFeature(CardFeature.Taunt);
            }
        }
        public bool HasCharge
        {
            get
            {
                return CheckFeature(CardFeature.Charge);
            }
        }

        private bool CheckFeature(CardFeature feature)
        {
            bool result = false;
            if (_currentData.Features != null)
            {
                if (_currentData.Features.Contains(feature))
                {
                    result = true;
                }
            }
            return result;
        }

        private int _currentHealth;
        public int Health
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                _currentHealth = value;
                _health.text = _currentHealth.ToString();
            }
        }

        private int _currentAttack;
        public int Attack
        {
            get
            {
                return _currentAttack;
            }
            set
            {
                _currentAttack = value;
                if (_currentAttack < 0) _currentAttack = 0;
                _attack.text = _currentAttack.ToString();
            }
        }
        public void Configuration(CardPropertiesData data, Material material, string description)
        {
            _currentData = data;
            _picture.material = material;
            _name.text = data.Name;
            _descirption.text = description;
            _cost.text = data.Cost.ToString();
            Attack = data.Attack;
            Health = data.Health;
            _type.text = data.Type == CardUnitType.None ? "" : data.Type.ToString();
        }

        private Vector3 screenPoint;
        private Vector3 offset;
        private Vector3 _initialPosition;
        public void SetInitialPosition(Vector3 pos)
        {
            _initialPosition = pos;
        }

        private bool _dragged = false;

        public void OnBeginDrag(PointerEventData eventData)
        {

            if ((State == CardStateType.InHand && Hand.IsActive) || (State == CardStateType.OnTable && Hand.IsActive))
            {
                _dragged = true;
            }
            else
            {
                return;
            }
            GameManager.DraggedCard = this;
            _initialPosition = transform.position;
            screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, screenPoint.z));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragged) return;
            Vector3 cursorPoint = new Vector3(eventData.position.x, eventData.position.y, screenPoint.z);
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
            transform.position = cursorPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragged) return;
            if (_tablePlace != null)
            {
                _tablePlace.Item1.AddToTable(this, _tablePlace.Item2);
                Table = _tablePlace.Item1;
                Hand.RemoveCard(this);
                Hand.Player.RemoveManaCost(this.CardProperty.Cost);
                State = CardStateType.OnTable;
                if (!HasCharge) Sleeping = true;
                SetInitialPosition(_tablePlace.Item2.position + _zoomYChange);
                _tablePlace = null;
            }

            bool inAttack = false;
            if (target != null)
            {
                inAttack = true;
                Acted = true;
                _animator.SetTrigger("Attack");
            }

            if (!inAttack)
            {
                GameManager.DraggedCard = null;
                transform.position = _initialPosition;
            }
            _dragged = false;
        }

        public void AttackComplete()
        {
            GameManager.DraggedCard = null;
            _initialPosition.y = 0;
            target.SetDamage(Attack);
            StartCoroutine(MoveCardTo(_initialPosition));
            if (target is Card cardTarget)
            {
                SetDamage(cardTarget.Attack);
            };
            target = null;
        }
        private bool _inMove;
        private IEnumerator MoveCardTo(Vector3 position)
        {
            _inMove = true;
            var time = 0f;
            var startPos = this.transform.position;
            var endPos = position;
            while (time < 1f)
            {
                this.transform.position = Vector3.Lerp(startPos, endPos, time);
                time += Time.deltaTime * 2;
                yield return null;
            }
            _inMove = false;
        }

        private bool _zoomed = false;
        private Vector3 _zoomYChange = new Vector3(0f, 0.5f, 0f);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_inMove) return;
            if ((State == CardStateType.InHand && Hand.IsActive) || State == CardStateType.OnTable)
            {
                if (_zoomed) return;
                transform.localScale *= 1.2f;
                transform.localPosition += _zoomYChange;
                _zoomed = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_zoomed) return;
            transform.localScale /= 1.2f;
            transform.localPosition -= _zoomYChange;
            if (transform.localPosition.y < 0)
            {
                var costylDev = transform.localPosition;
                costylDev.y = 0;
                transform.localPosition = costylDev;
            }
            _zoomed = false;
        }

        private Tuple<PlayerTable, Transform> _tablePlace;
        internal void SetTable(PlayerTable table, Transform place)
        {
            _tablePlace = new Tuple<PlayerTable, Transform>(table, place);
        }

        internal void ResetTable()
        {
            _tablePlace = null;
        }

        private Target target = null;

        internal void SetTarget(Target newTarget)
        {
            target = newTarget;
        }

        internal void ResetTarget()
        {
            target = null;
        }

        public override void SetDamage(int damage)
        {
            if (CheckFeature(CardFeature.GainAfterDamage_A3))
            {
                Attack += 3;
            }
            Health -= damage;
            _animator.SetTrigger("Damage");
        }

        public void DamageAnimationOver()
        {
            if (Health <= 0)
            {
                Table.Remove(this);
            }
        }
    }

}
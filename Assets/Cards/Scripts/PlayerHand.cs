using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    public class PlayerHand : MonoBehaviour
    {
        private Card[] _cardsInHand;

        [SerializeField]
        private List<Transform> _postitions;
        private Player _player;
        public Player Player
        {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
            }
        }

        public Card[] CardsInHand
        {
            get
            {
                return _cardsInHand;
            }
        }

        private void Start()
        {
            _cardsInHand = new Card[_postitions.Count];
        }

        public bool SetNewCard(Card newCard)
        {
            var result = GetLastPosition();
            if (result == -1)
            {
                Destroy(newCard.gameObject);
                return false;
            }

            _cardsInHand[result] = newCard;
            newCard.Hand = this;
            StartCoroutine(MoveCardTo(newCard, _postitions[result]));
            return true;
        }

        private IEnumerator MoveCardTo(Card card, Transform position)
        {
            var time = 0f;
            var startPos = card.transform.position;
            var endPos = position.position;
            while (time < 1f)
            {
                card.transform.position = Vector3.Lerp(startPos, endPos, time);
                time += Time.deltaTime;
                yield return null;
            }
            card.FrontSide = IsActive;
            card.State = CardStateType.InHand;
        }

        private int GetLastPosition()
        {
            for (int i = 0; i < _cardsInHand.Length; i++)
            {
                if (_cardsInHand[i] == null) return i;
            }
            return -1;
        }

        public bool IsActive { get; set; } = false;
        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            SetHandVisibility(IsActive);
        }

        private void SetHandVisibility(bool isVisible = true)
        {
            if (_cardsInHand == null) return;
            foreach (var card in _cardsInHand)
            {
                if (card == null) continue;
                card.FrontSide = isVisible;
            }
        }

        internal void RemoveCard(Card card)
        {
            bool moveNext = false;
            for (int i = 0; i < _cardsInHand.Length; i++)
            {
                if (_cardsInHand[i] == null) break;
                if (moveNext)
                {
                    _cardsInHand[i - 1] = _cardsInHand[i];
                    _cardsInHand[i] = null;
                    StartCoroutine(MoveCardTo(_cardsInHand[i - 1], _postitions[i - 1]));
                }
                else if (_cardsInHand[i] == card)
                {
                    _cardsInHand[i] = null;
                    moveNext = true;
                }
            }
        }
    }
}

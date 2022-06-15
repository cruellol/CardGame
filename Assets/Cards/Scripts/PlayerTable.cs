using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    public class PlayerTable : MonoBehaviour
    {
        [SerializeField]
        private Transform[] _postitions;
        public Dictionary<Transform, Card> PointsToCards { get; } = new Dictionary<Transform, Card>();

        public void Start()
        {
            foreach (var position in _postitions)
            {
                PointsToCards.Add(position, null);
            }
        }

        internal void AddToTable(Card card, Transform key)
        {
            PointsToCards[key] = card;
            CheckFeatures(card);
        }

        private void CheckFeatures(Card card)
        {
            foreach (var pointToCard in PointsToCards)
            {
                if (pointToCard.Value != null && pointToCard.Value != card)
                {
                    CheckFeaturesInOthers(card, pointToCard.Value);
                }
            }

            if (card.CardProperty.Features != null)
            {
                foreach (var feature in card.CardProperty.Features)
                {
                    switch (feature)
                    {
                        case CardFeature.Other_A1H1:
                            {
                                foreach (var pointToCard in PointsToCards)
                                {
                                    if (pointToCard.Value != null && pointToCard.Value != card)
                                    {
                                        pointToCard.Value.ChangeAttackAndHealth(1, 1);
                                    }
                                }
                                break;
                            }
                        case CardFeature.HeroDamage_3:
                            {
                                GameManager.CurrentManager.SetEnemyPlayerDamage(3);
                                break;
                            }
                        case CardFeature.GainForOtherDriendly_A1H1:
                            {
                                int count = 0;
                                foreach (var pointToCard in PointsToCards)
                                {
                                    if (pointToCard.Value != null && pointToCard.Value != card)
                                    {
                                        count++;
                                    }
                                }
                                card.ChangeAttackAndHealth(count, count);
                                break;
                            }
                        case CardFeature.RestoreAll_2:
                            {
                                foreach (var pointToCard in PointsToCards)
                                {
                                    if (pointToCard.Value != null && pointToCard.Value != card)
                                    {
                                        pointToCard.Value.Restore(2);
                                    }
                                }
                                GameManager.CurrentManager.RestoreHpToCurrentPlayer(2);
                                break;
                            }
                        case CardFeature.Other_A1:
                            {
                                foreach (var pointToCard in PointsToCards)
                                {
                                    if (pointToCard.Value != null && pointToCard.Value != card)
                                    {
                                        pointToCard.Value.ChangeAttack(1);
                                    }
                                }
                                break;
                            }
                        case CardFeature.Restrore_2:
                            {
                                GameManager.CurrentManager.RestoreHpToCurrentPlayer(2);
                                break;
                            }
                        case CardFeature.Draw_1:
                            {
                                GameManager.CurrentManager.DrawCard();
                                break;
                            }
                        default: break;
                    }
                }
            }
        }

        private void CheckFeaturesInOthers(Card card, Card tocheck)
        {
            if (tocheck.CardProperty.Features != null)
            {
                foreach (var feature in tocheck.CardProperty.Features)
                {
                    switch (feature)
                    {
                        case CardFeature.GainForOtherDriendly_A1H1:
                            {
                                tocheck.ChangeAttackAndHealth(1, 1);
                                break;
                            }
                        case CardFeature.Other_A1:
                            {
                                card.ChangeAttack(1);
                                break;
                            }
                        default: break;
                    }
                }
            }
        }

        private void RemoveFeatures(Card card)
        {
            foreach (var pointToCard in PointsToCards)
            {
                if (pointToCard.Value != null && pointToCard.Value != card)
                {
                    RemoveInDependent(card, pointToCard.Value);
                }
            }
            if (card.CardProperty.Features != null)
            {
                foreach (var feature in card.CardProperty.Features)
                {
                    switch (feature)
                    {
                        case CardFeature.Other_A1:
                            {
                                foreach (var pointToCard in PointsToCards)
                                {
                                    if (pointToCard.Value != null && pointToCard.Value != card)
                                    {
                                        pointToCard.Value.ChangeAttack(-1);
                                    }
                                }
                                break;
                            }
                        case CardFeature.Other_A1H1:
                            {
                                foreach (var pointToCard in PointsToCards)
                                {
                                    if (pointToCard.Value != null && pointToCard.Value != card)
                                    {
                                        pointToCard.Value.ChangeAttackAndHealth(-1, -1);
                                    }
                                }
                                break;
                            }
                        default: break;
                    }
                }
            }
        }

        private void RemoveInDependent(Card card, Card tocheck)
        {
            if (tocheck.CardProperty.Features != null)
            {
                foreach (var feature in tocheck.CardProperty.Features)
                {
                    switch (feature)
                    {
                        case CardFeature.GainForOtherDriendly_A1H1:
                            {
                                tocheck.ChangeAttackAndHealth(-1, -1);
                                break;
                            }
                        default: break;
                    }
                }
            }
        }

        internal void Turn()
        {
            foreach (var pointToCard in PointsToCards)
            {
                if (pointToCard.Value != null)
                {
                    pointToCard.Value.gameObject.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
                }
            }
        }

        internal void WakeCards()
        {
            foreach (var pointToCard in PointsToCards)
            {
                if (pointToCard.Value != null)
                {
                    pointToCard.Value.Sleeping = false;
                    pointToCard.Value.Acted = false;
                }
            }
        }

        internal void Remove(Card card)
        {
            Transform removeKey = null;
            foreach (var pointToCard in PointsToCards)
            {
                if (pointToCard.Value == card)
                {
                    removeKey = pointToCard.Key;
                    break;
                }
            }
            if (removeKey != null)
            {
                RemoveFeatures(card);
                PointsToCards[removeKey] = null;
                Destroy(card.gameObject);
            }
        }

        internal KeyValuePair<Transform, Card>? CheckTaunt()
        {
            foreach (var pointToCard in PointsToCards)
            {
                if (pointToCard.Value != null && pointToCard.Value.HasTaunt)
                {
                    return pointToCard;
                }
            }
            return null;
        }
    }
}

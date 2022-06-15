using Cards.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cards
{
    public class GameManager : MonoBehaviour
    {
        private List<CardPropertiesData> _allCards;
        private Material _baseMaterial;
        private Card[] _player1Deck;
        private Card[] _player2Deck;

        [SerializeField]
        private List<CardPackConfiguration> _packs;
        [SerializeField]
        private Card _cardPrefab;

        [Space, SerializeField, Range(10, 50)]
        private int _cardDeckRange = 30;

        [SerializeField]
        private PlayerHand _playerHand1;
        [SerializeField]
        private PlayerHand _playerHand2;

        [SerializeField]
        private Transform _player1DeckPoint;
        [SerializeField]
        private Transform _player2DeckPoint;
        [SerializeField]
        private Transform _axis;

        [SerializeField]
        private Player _player1;
        [SerializeField]
        private Player _player2;

        [SerializeField]
        private PlayerTable _player1Table;
        [SerializeField]
        private PlayerTable _player2Table;

        [SerializeField]
        private GameObject _highLighCard;
        [SerializeField]
        private GameObject _attackHighLigh;
        [SerializeField]
        private Canvas _mainCanvas;
        [SerializeField]
        private Transform _placeForCard1;
        [SerializeField]
        private Transform _placeForCard2;
        [SerializeField]
        private Transform _placeForCard3;

        public static Card DraggedCard { get; set; }

        public static GameManager CurrentManager;

        private void Reset()
        {
            _allCards = new List<CardPropertiesData>();
            foreach (var pack in _packs)
            {
                _allCards = pack.UnionProperties(_allCards).ToList();
            }
        }

        private void Awake()
        {
            _allCards = new List<CardPropertiesData>();
            foreach (var pack in _packs)
            {
                _allCards = pack.UnionProperties(_allCards).ToList();
            }

            _baseMaterial = new Material(Shader.Find("TextMeshPro/Sprite"));
            _baseMaterial.renderQueue = 2995;
        }

        private bool _player1Move = true;

        private Dictionary<int, bool> CardCanChange = new Dictionary<int, bool>();
        private Dictionary<int, Transform> _indToPoint;
        private CardPropertiesData[] _firstChosenCards;

        private void Start()
        {
            CurrentManager = this;
            _indToPoint = new Dictionary<int, Transform> { { 0, _placeForCard1 }, { 1, _placeForCard2 }, { 2, _placeForCard3 } };
            _playerHand1.IsActive = true;
            _playerHand1.Player = _player1;
            _playerHand2.Player = _player2;
            _firstChosenCards = new CardPropertiesData[3];
            for (int i = 0; i < 3; i++)
            {
                CardCanChange.Add(i, true);
            }
            ChooseFirstThree();
        }

        public void ChooseFirstThree()
        {
            for (int i = 0; i < 3; i++)
            {
                Destroy(_indToPoint[i].GetChild(0).gameObject);
                CardPropertiesData random = _allCards[UnityEngine.Random.Range(0, _allCards.Count)];
                _firstChosenCards[i] = random;
                var newcard = Instantiate(_cardPrefab, _indToPoint[i]);
                var newMaterial = new Material(_baseMaterial);
                newMaterial.mainTexture = random.Texture;
                newcard.Configuration(random, newMaterial, CardUtility.GetDescriptionById(random.Id));
                newcard.FrontSide = true;
                newcard.SetInnerMesh(false);
            }
        }
        public void ChangeCard(int i)
        {
            if (CardCanChange[i])
            {
                CardCanChange[i] = false;
                Destroy(_indToPoint[i].GetChild(0).gameObject);
                CardPropertiesData random = _allCards[UnityEngine.Random.Range(0, _allCards.Count)];
                _firstChosenCards[i] = random;
                var newcard = Instantiate(_cardPrefab, _indToPoint[i]);
                var newMaterial = new Material(_baseMaterial);
                newMaterial.mainTexture = random.Texture;
                newcard.Configuration(random, newMaterial, CardUtility.GetDescriptionById(random.Id));
                newcard.FrontSide = true;
                newcard.SetInnerMesh(false);
            }
        }
        public void ApproveCards()
        {
            if (_player1Move)
            {
                _player1Deck = CreateDeckWithChosen(_player1DeckPoint);
            }
            else
            {
                _player2Deck = CreateDeckWithChosen(_player2DeckPoint);
            }
            _mainCanvas.gameObject.SetActive(false);
            DrawCard();
            DrawCard();
            DrawCard();
        }
        private Card[] CreateDeckWithChosen(Transform point)
        {
            Card[] result = new Card[_cardDeckRange];
            var vector = Vector3.zero;
            for (int i = 0; i < _cardDeckRange; i++)
            {
                var newcard = Instantiate(_cardPrefab, point);
                newcard.transform.localPosition = vector;
                result[i] = newcard;
                vector += new Vector3(0f, 0.01f, 0f);
                var diff = _cardDeckRange - 3;
                CardPropertiesData newproperties = default;
                if (i >= diff)
                {
                    newproperties = _firstChosenCards[2 - (i - diff)];
                }
                else
                {
                    newproperties = _allCards[UnityEngine.Random.Range(0, _allCards.Count)];
                }

                var newMaterial = new Material(_baseMaterial);
                newMaterial.mainTexture = newproperties.Texture;
                newcard.Configuration(newproperties, newMaterial, CardUtility.GetDescriptionById(newproperties.Id));
            }
            return result;
        }
        private Card[] CreateDeck(Transform point)
        {
            Card[] result = new Card[_cardDeckRange];
            var vector = Vector3.zero;
            for (int i = 0; i < _cardDeckRange; i++)
            {
                var newcard = Instantiate(_cardPrefab, point);
                newcard.transform.localPosition = vector;
                result[i] = newcard;
                vector += new Vector3(0f, 0.01f, 0f);

                var random = _allCards[UnityEngine.Random.Range(0, _allCards.Count)];
                var newMaterial = new Material(_baseMaterial);
                newMaterial.mainTexture = random.Texture;
                newcard.Configuration(random, newMaterial, CardUtility.GetDescriptionById(random.Id));
            }
            return result;
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    if (_player1Move)
            //    {
            //        GetCard(_player1Deck, _playerHand1);
            //    }
            //    else
            //    {
            //        GetCard(_player2Deck, _playerHand2);
            //    }
            //}
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                NextRound();
            }

            CheckDragged();
        }

        internal void DrawCard()
        {
            if (_player1Move)
            {
                GetCard(_player1Deck, _playerHand1);
            }
            else
            {
                GetCard(_player2Deck, _playerHand2);
            }
        }
        internal void RestoreHpToCurrentPlayer(int hp)
        {
            var player = _player1Move ? _player1 : _player2;
            player.AddHP(hp);
        }
        internal void SetEnemyPlayerDamage(int damage)
        {
            var player = !_player1Move ? _player1 : _player2;
            player.SetDamage(damage);
        }

        private void ChoosePlaceOnTable()
        {
            float closestDistance = float.MaxValue;
            KeyValuePair<Transform, Card>? closestCard = null;
            var table = _player1Move ? _player1Table : _player2Table;
            var player = _player1Move ? _player1 : _player2;
            if (player.GetCurrentMana() < DraggedCard.CardProperty.Cost) return;
            foreach (KeyValuePair<Transform, Card> pointCard in table.PointsToCards)
            {
                if (pointCard.Value != null) continue;
                var distance = Vector3.SqrMagnitude(DraggedCard.transform.position - pointCard.Key.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCard = pointCard;
                }
            }
            if (closestCard != null && closestDistance < 2)
            {
                var newHighlight = Instantiate(_highLighCard);
                newHighlight.transform.position = closestCard.Value.Key.position;
                StartCoroutine(DestroyDelay(newHighlight));
                DraggedCard.SetTable(table, closestCard.Value.Key);
            }
            else
            {
                DraggedCard.ResetTable();
            }
        }

        private void CheckDragged()
        {
            if (DraggedCard != null)
            {
                if (DraggedCard.State == CardStateType.InHand)
                {
                    ChoosePlaceOnTable();
                }
                if (DraggedCard.State == CardStateType.OnTable)
                {
                    if (DraggedCard.Sleeping || DraggedCard.Acted) return;
                    float closestDistance = float.MaxValue;
                    KeyValuePair<Transform, Card>? closestCard = null;
                    var table = !_player1Move ? _player1Table : _player2Table;
                    var player = !_player1Move ? _player1 : _player2;
                    KeyValuePair<Transform, Card>? possibleTaunt = table.CheckTaunt();
                    foreach (KeyValuePair<Transform, Card> pointCard in table.PointsToCards)
                    {
                        if (pointCard.Value == null) continue;
                        var distance = Vector3.SqrMagnitude(DraggedCard.transform.position - pointCard.Key.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestCard = pointCard;
                        }
                    }
                    var herodistance = Vector3.SqrMagnitude(DraggedCard.transform.position - player.gameObject.transform.position);
                    if (herodistance < closestDistance)
                    {
                        if (herodistance < 2)
                        {
                            if (possibleTaunt != null)
                            {
                                SetCardAttack(possibleTaunt);
                            }
                            else
                            {
                                SetPlayerAttack(player);
                            }
                        }
                        else
                        {
                            DraggedCard.ResetTarget();
                        }
                    }
                    else
                    {
                        if (closestCard != null && closestDistance < 2)
                        {
                            if (possibleTaunt != null)
                            {
                                SetCardAttack(possibleTaunt);
                            }
                            else
                            {
                                SetCardAttack(closestCard);
                            }
                        }
                        else
                        {
                            DraggedCard.ResetTarget();
                        }
                    }
                }
            }
        }

        private void SetPlayerAttack(Player player)
        {
            var newHighlight = Instantiate(_attackHighLigh);
            newHighlight.transform.localScale = player.gameObject.transform.localScale * 1.1f;
            newHighlight.transform.position = player.gameObject.transform.position;
            StartCoroutine(DestroyDelay(newHighlight));
            DraggedCard.SetTarget(player);
        }

        private void SetCardAttack(KeyValuePair<Transform, Card>? closestCard)
        {
            var newHighlight = Instantiate(_attackHighLigh);
            newHighlight.transform.position = closestCard.Value.Key.position;
            StartCoroutine(DestroyDelay(newHighlight));
            DraggedCard.SetTarget(closestCard.Value.Value);
        }

        public IEnumerator DestroyDelay(GameObject toDestroy)
        {
            yield return null;
            Destroy(toDestroy);
        }

        private void GetCard(Card[] from, PlayerHand to)
        {
            int i = 0;

            for (i = from.Length - 1; i >= 0; i--)
            {
                if (from[i] != null)
                {
                    break;
                }
            }

            if (i == -1) return;
            to.SetNewCard(from[i]);
            from[i] = null;
        }

        private bool _player2FirstMove = true;

        private void NextRound()
        {
            if (!_switchingTurn)
            {
                StartCoroutine(NextPlayer());
            }
        }

        private bool _switchingTurn = false;
        public IEnumerator NextPlayer()
        {
            _switchingTurn = true;
            yield return TurnCamera();
            _player1Table.Turn();
            _player2Table.Turn();
            if (_player1Move)
            {
                _playerHand2.SetActive(true);
                _player2Table.WakeCards();
                _playerHand1.SetActive(false);
                if (_player2FirstMove)
                {
                    _player2FirstMove = false;
                }
                else
                {
                    _player2.MoveToNextRound();
                }
            }
            else
            {
                _player1Table.WakeCards();
                _playerHand1.SetActive(true);
                _playerHand2.SetActive(false);
                _player1.MoveToNextRound();
            }
            _player1Move = !_player1Move;
            _player1.Turn();
            _player2.Turn();

            _switchingTurn = false;

            if (_player2Deck == null || _player2Deck.Length == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    CardCanChange[i] = true;
                }
                _mainCanvas.gameObject.SetActive(true);
                ChooseFirstThree();
            }
            else
            {
                DrawCard();
            }
        }

        private IEnumerator TurnCamera()
        {
            var time = 0f;
            var startPos = _axis.transform.rotation.eulerAngles;
            var endPos = _axis.transform.rotation.eulerAngles;
            endPos.y += 180;
            while (time < 1f)
            {
                _axis.transform.rotation = Quaternion.Euler(Vector3.Lerp(startPos, endPos, time));
                time += Time.deltaTime;
                yield return null;
            }
        }
    }
}

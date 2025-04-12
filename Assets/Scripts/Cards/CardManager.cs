using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Fusion;

namespace Projectiles
{
    public class CardManager : MonoBehaviour
    {
        [SerializeField] SceneContext Context;

        // card 1 data
        [SerializeField] TextMeshProUGUI card1Title;
        [SerializeField] TextMeshProUGUI card1Desc;
        [SerializeField] Button card1Select;

        // card 2 data
        [SerializeField] TextMeshProUGUI card2Title;
        [SerializeField] TextMeshProUGUI card2Desc;
        [SerializeField] Button card2Select;

        // card 3 data
        [SerializeField] TextMeshProUGUI card3Title;
        [SerializeField] TextMeshProUGUI card3Desc;
        [SerializeField] Button card3Select;

        //cards
        StatCard c1;
        StatCard c2;
        StatCard c3;

        StatCard selectedCard;

        // list of cards
        public List<StatCard> statCards = new List<StatCard>();
        public List<SpecialCard> specialCards = new List<SpecialCard>();

        // csv files that store cards
        public TextAsset StatCardCSV;

        public bool hasSelectedCard = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        // populate all stat cards with their information and add onclick events to cards
        public void Start()
        {
            //Context.cardManager = this;
            populateStatCards();
            addOnClickEvents();
            Debug.LogError("Card Manager Spawned");
            
        }

        // Update is called once per frame
        // check if a card has been chosen (1/2/3 has been pressed)
        // if chosen, invoke their onclick events
        void Update()
        {
            if (hasSelectedCard) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.isPressed) 
                {
                    hasSelectedCard = true;
                    card1Select.onClick.Invoke(); 
                }
                if (keyboard.digit2Key.isPressed) 
                {
                    hasSelectedCard = true;
                    card2Select.onClick.Invoke(); 
                }
                if (keyboard.digit3Key.isPressed) 
                {
                    hasSelectedCard = true;
                    card3Select.onClick.Invoke(); 
                }
            }
        }

        // on click events determine if a card is chosen, the card is selected, 
        // their game affects are implemented
        private void addOnClickEvents()
        {
            card1Select.onClick.AddListener(() =>
            {
                selectCard(c1);
                Debug.LogError("B1 Clicked");
            });

            card2Select.onClick.AddListener(() =>
            {
                selectCard(c2);
                Debug.LogError("B2 Clicked");
            });

            card3Select.onClick.AddListener(() =>
            {
                selectCard(c3);
                Debug.LogError("B3 Clicked");
            });
        }

        // modify which stat is affected by the chosen card
        private void selectCard(StatCard c)
        {
            selectedCard = c;
            switch (c.StatAffected)
            {
                case StatCard.Stat.HP:
                    Context.LocalAgent.Health.incMaxHealth(c.Value);
                    break;
                case StatCard.Stat.All:
                    break;
                case StatCard.Stat.Speed:
                    Context.LocalAgent.incMoveSpeed(c.Value);
                    break;
                case StatCard.Stat.JumpHeight:
                    Context.LocalAgent.incJumpHeight(c.Value);
                    break;
            }
            Context.Gameplay.hasSelectedCard.Set(Context.LocalAgent.Owner, true);
            Context.hideCards();
            //context.GeneralInput.RequestCursorLock();
        }

        // read all card information from csv file and loop to create the array of stat cards
        private void populateStatCards()
        {
            // read in all lines
            string[] lines = StatCardCSV.text.Split('\n');

            for (int i = 1; i < lines.Length; i++) // skip first line which is just header
            {
                string[] cardInfo = lines[i].Split(',');
                addStatCard(cardInfo, i);
            }
        }

        // add the stat card to the array 
        private void addStatCard(string[] cardInfo, int id)
        {
            Debug.Log($"{cardInfo[0]},{cardInfo[1]},{cardInfo[2]}, {cardInfo[3]}");
            int stat = int.Parse(cardInfo[2]);
            //StatCard card = new StatCard(id, cardInfo[0], cardInfo[1], (StatCard.Stat)stat, int.Parse(cardInfo[3]));
            //statCards.Add(card);
        }

        // randomly select a stat card from the array
        public StatCard chooseRandomStatCard()
        {
            int cardNum = Random.Range(0, statCards.Count);
            return statCards[cardNum];
        }

        // choose 3 random cards, then display them before the round
        // this allows players to see options before choosing their gambit
        public void showCards()
        {
            c1 = chooseRandomStatCard();
            c2 = chooseRandomStatCard();
            c3 = chooseRandomStatCard();

            // card 1
            card1Title.text = c1.Title;
            card1Desc.text = c1.Desc;

            // card 2
            card2Title.text = c2.Title;
            card2Desc.text = c2.Desc;

            // card 3
            card3Title.text = c3.Title;
            card3Desc.text = c3.Desc;
        }
    }
}

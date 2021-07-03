using System;
using System.Collections.Generic;
using System.Linq;
using CardPool.Core;
using NUnit.Framework;

namespace CardPool.Tests
{
    public class CardsPoolInitializeTests
    {
        // [Test]
        // public void TestUnexpectedCreate1()
        // {
        //     var pool = new CardsPool(new List<Card>());
        //     pool.InitializePool();
        //     Console.WriteLine(pool.GetPoolProbabilityInfo());
        //     Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.Probability));
        // }
        [Test]
        public void TestUnexpectedCreate2()
        {
            var oneStarCards = Card<int>.CreateMultiCards(Card.CardRarity.OneStar, 1);
            var pool = new CardsPool(oneStarCards);
            pool.BuildPool();
            Console.WriteLine(pool.GetPoolProbabilityInfo());
            Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.Probability));
        }
        [Test]
        public void TestUnexpectedCreate3()
        {
            var cards = new List<Card> {new Card<int>(1)};
            var pool = new CardsPool(cards);
            pool.BuildPool();
            Console.WriteLine(pool.GetPoolProbabilityInfo());
            Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.Probability));
        }
        [Test]
        public void TestRarityCreate1()
        {
            var oneStarCards = Card<int>.CreateMultiCards(Card.CardRarity.OneStar, 1, 2, 3, 4, 5);
            var twoStarCards = Card<int>.CreateMultiCards(Card.CardRarity.TwoStar, 11, 12, 13, 14, 15);
            var singleCard = new Card<int>(100) {Probability = 0.005};
            var twoStarCardsAppend = Card<TimeSpan>.CreateMultiCards(Card.CardRarity.TwoStar,
                new TimeSpan(1, 0, 0),
                new TimeSpan(1, 1, 0),
                new TimeSpan(1, 1, 10));
            var pool = new CardsPool(oneStarCards, twoStarCards, twoStarCardsAppend);
            pool.AddCards(singleCard);
            pool.RemainedCard = null;
            pool.SetPoolRarityProbability(Card.CardRarity.OneStar, 0.5)
                .SetPoolRarityProbability(Card.CardRarity.TwoStar, 0.3);
            pool.BuildPool();
            Console.WriteLine(pool.GetPoolProbabilityInfo());
            Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.Probability));
        }
    }
}
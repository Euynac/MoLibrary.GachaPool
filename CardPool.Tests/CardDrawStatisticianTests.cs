using System.Collections.Generic;
using System.Linq;
using CardPool.Conventions;
using CardPool.Implements;
using CardPool.Interfaces;
using NUnit.Framework;

namespace CardPool.Tests;

[TestFixture]
public class CardDrawStatisticianTests
{
    private ICardsPool _pool;
    private ICardDrawStatistician _statistician;
    private List<Card<int>> _testCards;

    [SetUp]
    public void Setup()
    {
        _pool = new CardsPool();
        _testCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3);
        _pool.AddCards(_testCards);
        _pool.BuildPool();
        _statistician = new CardDrawStatistician(_pool);
    }

    [Test]
    public void Constructor_ShouldInitializeWithAllPoolCards()
    {
        // Assert
        Assert.That(_statistician.CardRecordDict.Keys, Is.EquivalentTo(_pool.Cards));
        Assert.That(_statistician.CardRecordDict.Values.All(v => v.Value == 0));
    }

    [Test]
    public void RecordDrawnCard_ShouldIncrementCardCount()
    {
        // Arrange
        var card = _testCards[0];
        
        // Act
        _statistician.RecordDrawnCard(card);
        
        // Assert
        Assert.That(_statistician.CardRecordDict[card].Value, Is.EqualTo(1));
        Assert.That(_statistician.RecordedTimes, Is.EqualTo(1));
    }

    [Test]
    public void RecordDrawnCard_MultipleTimes_ShouldTrackCorrectly()
    {
        // Arrange
        var card = _testCards[0];
        const int drawCount = 5;
        
        // Act
        for (var i = 0; i < drawCount; i++)
        {
            _statistician.RecordDrawnCard(card);
        }
        
        // Assert
        Assert.That(_statistician.CardRecordDict[card].Value, Is.EqualTo(drawCount));
        Assert.That(_statistician.RecordedTimes, Is.EqualTo(drawCount));
    }

    [Test]
    public void RecordDrawnCard_WithRemainedCard_ShouldTrackCorrectly()
    {
        // Arrange
        var remainedCard = new Card<int>(CardRarity.OneStar, 999);
        _pool.RemainedCard = remainedCard;
        var statistician = new CardDrawStatistician(_pool); // Create new statistician after setting remained card
        
        // Act
        statistician.RecordDrawnCard(remainedCard);
        
        // Assert
        Assert.That(statistician.CardRecordDict.ContainsKey(remainedCard));
        Assert.That(statistician.CardRecordDict[remainedCard].Value, Is.EqualTo(1));
        Assert.That(statistician.RecordedTimes, Is.EqualTo(1));
    }

    [Test]
    public void RecordDrawnCard_WithMultipleThreads_ShouldBeThreadSafe()
    {
        // Arrange
        var card = _testCards[0];
        const int threadCount = 10;
        const int drawsPerThread = 1000;
        var threads = new System.Threading.Thread[threadCount];
        
        // Act
        for (var i = 0; i < threadCount; i++)
        {
            threads[i] = new System.Threading.Thread(() =>
            {
                for (var j = 0; j < drawsPerThread; j++)
                {
                    _statistician.RecordDrawnCard(card);
                }
            });
            threads[i].Start();
        }
        
        foreach (var thread in threads)
        {
            thread.Join();
        }
        
        // Assert
        Assert.That(_statistician.CardRecordDict[card].Value, Is.EqualTo(threadCount * drawsPerThread));
        Assert.That(_statistician.RecordedTimes, Is.EqualTo(threadCount * drawsPerThread));
    }

    [Test]
    public void GetReport_ShouldIncludeAllCards()
    {
        // Arrange
        foreach (var card in _testCards)
        {
            _statistician.RecordDrawnCard(card);
        }
        
        // Act
        var report = _statistician.GetReport();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(report.TotalDraws, Is.EqualTo(_testCards.Count));
            Assert.That(report.CardStats, Has.Count.EqualTo(_testCards.Count));
            foreach (var card in _testCards)
            {
                var cardStat = report.CardStats.FirstOrDefault(s => s.Card == card);
                Assert.That(cardStat, Is.Not.Null);
                Assert.That(cardStat!.DrawCount, Is.EqualTo(1));
                Assert.That(cardStat.ActualProbability, Is.EqualTo(1.0 / _testCards.Count));
            }
        });
    }

    [Test]
    public void GetReport_WithMultipleDraws_ShouldCalculateCorrectProbabilities()
    {
        // Arrange
        const int drawCount = 100;
        var targetCard = _testCards[0];
        
        for (var i = 0; i < drawCount; i++)
        {
            _statistician.RecordDrawnCard(targetCard);
        }
        
        // Act
        var report = _statistician.GetReport();
        
        // Assert
        var targetCardStats = report.CardStats.First(s => s.Card == targetCard);
        Assert.Multiple(() =>
        {
            Assert.That(report.TotalDraws, Is.EqualTo(drawCount));
            Assert.That(targetCardStats.DrawCount, Is.EqualTo(drawCount));
            Assert.That(targetCardStats.ActualProbability, Is.EqualTo(1.0));
            Assert.That(report.CardStats.Where(s => s.Card != targetCard).All(s => s.DrawCount == 0));
        });
    }

    [Test]
    public void GetReport_WithNoDraws_ShouldReturnZeroProbabilities()
    {
        // Act
        var report = _statistician.GetReport();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(report.TotalDraws, Is.EqualTo(0));
            Assert.That(report.CardStats, Has.Count.EqualTo(_testCards.Count));
            Assert.That(report.CardStats.All(s => s.DrawCount == 0));
            Assert.That(report.CardStats.All(s => s.ActualProbability == 0));
        });
    }

    [Test]
    public void GetReport_TableString_ShouldFormatCorrectly()
    {
        // Arrange
        const int drawCount = 100;
        var targetCard = _testCards[0];
        
        for (var i = 0; i < drawCount; i++)
        {
            _statistician.RecordDrawnCard(targetCard);
        }
        
        // Act
        var report = _statistician.GetReport();
        var tableString = report.GetTableString();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tableString, Does.Contain("sum of all probability:"));
            Assert.That(tableString, Does.Contain($"total drawn times: {drawCount}"));
            Assert.That(tableString, Does.Contain(targetCard.GetCardName()));
            Assert.That(tableString, Does.Contain("100.00%")); // Since we only drew one card
            Assert.That(tableString, Does.Contain(drawCount.ToString()));
            
            // Verify table format
            Assert.That(tableString, Does.Contain("CardName"));
            Assert.That(tableString, Does.Contain("ExpectProb"));
            Assert.That(tableString, Does.Contain("Rarity"));
            Assert.That(tableString, Does.Contain("DrawnCount"));
            Assert.That(tableString, Does.Contain("ExactProb"));
        });
    }
} 
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoLibrary.GachaPool.Conventions;

namespace MoLibrary.GachaPool.Interfaces;

/// <summary>
/// Defines the contract for a statistician that tracks and analyzes card draw statistics.
/// </summary>
public interface ICardDrawStatistician
{
    /// <summary>
    /// Gets the number of times cards have been drawn.
    /// </summary>
    int RecordedTimes { get; }

    /// <summary>
    /// Gets the dictionary containing draw records for each card.
    /// </summary>
    Dictionary<Card, StrongBox<int>> CardRecordDict { get; }

    /// <summary>
    /// Records a drawn card in the statistics.
    /// </summary>
    void RecordDrawnCard(Card card);

    /// <summary>
    /// Gets a structured report of the draw statistics.
    /// </summary>
    /// <returns>A CardDrawReport containing detailed statistics about card draws.</returns>
    CardDrawReport GetReport();
}
using System;

using GridBasedPuzzle.Components;

namespace GridBasedPuzzle.Core;

public static class GlobalEvents
{
    public static Action<BuildingComponent> BuildingPlaced;
    public static Action<BuildingComponent> BuildingDestroyed;
}
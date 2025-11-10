using GridBasedPuzzle.Scenes.Components;
using System;

namespace GridBasedPuzzle.Core;

public static class GlobalEvents
{
    public static Action<BuildingComponent> BuildingPlaced;
    public static Action<BuildingComponent> BuildingDestroyed;
}
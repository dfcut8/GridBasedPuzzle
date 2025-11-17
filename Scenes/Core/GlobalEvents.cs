using System;

using GridBasedPuzzle.Scenes.Components;

namespace GridBasedPuzzle.Core;

public static class GlobalEvents
{
    public static Action<BuildingComponent> BuildingPlaced;
    public static Action<BuildingComponent> BuildingDestroyed;
    public static Action<BuildingComponent> BuildingDisabled;
    public static Action<BuildingComponent> BuildingEnabled;
}
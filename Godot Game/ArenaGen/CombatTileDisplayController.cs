using Godot;
using System;
using System.Collections.Generic;
using BMGridWorld;

public class CombatTileDisplayController : Sprite
{
    [Export]
    public NodePath SelectableMarkerPath;
    [Export]
    public NodePath SpritesParent;
    private Sprite SelectableMarker;
    private Node spritesParent;
    private Dictionary<ArenaFeature, Sprite> features;
    public ArenaSpace associatedSpace;
    public bool IsSpaceAvailable
    {
        get { return features.Count == 0; }
    }

    public bool Selectable
    {
        get
        {
            if (SelectableMarker == null) { return false; }
            return SelectableMarker.Visible;
        }
        set
        {
            if (SelectableMarker == null) { return; }
            SelectableMarker.Visible = value;
        }
    }

    public void ResetSelectableStatus()
    {
        Selectable = false;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (SelectableMarkerPath != null) { SelectableMarker = GetNode<Sprite>(SelectableMarkerPath); }
        if (SpritesParent != null) { spritesParent = GetNode(SpritesParent); }
        Selectable = false;
        PlayerCharacterController.SelectionReset += ResetSelectableStatus;
        features = new Dictionary<ArenaFeature, Sprite>();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        PlayerCharacterController.SelectionReset -= ResetSelectableStatus;
    }

    public bool UpdateFeatures(ArenaFeature newFeature, Sprite newSprite)
    {
        if (features.ContainsKey(newFeature)) return false;

        bool available = true;
        if (features.Count > 0)
        {
            List<ArenaFeature> toFree = new List<ArenaFeature>();
            foreach (KeyValuePair<ArenaFeature, Sprite> pair in features) //Check if this layer can be added to this tile
            {
                if (pair.Key.cosmetic)
                {
                    if (newFeature.overridesCosmetic) { toFree.Add(pair.Key); }
                    continue;
                }

                if (pair.Key.overrideLayer >= newFeature.overrideLayer) { available = false; break; }
                else { toFree.Add(pair.Key); }
            }

            if (available && toFree.Count > 0)
            {
                foreach (ArenaFeature feat in toFree) //Remove overridden layers
                {
                    foreach (GridSpaceTags tag in feat.tags)
                    {
                        if (tag != GridSpaceTags.Pathable) { associatedSpace.CurrentSpaceTags.Remove(tag); }
                    }
                    features[feat].QueueFree();
                    features.Remove(feat);
                }
            }
        }

        if (available)
        {
            features.Add(newFeature, newSprite);
            spritesParent.AddChild(newSprite);
            newSprite.Position = Position;
            if (newFeature.tags != null)
            {
                foreach (GridSpaceTags tag in newFeature.tags) //Add tags to the space's tag list
                {
                    if (associatedSpace.CurrentSpaceTags.Contains(tag)) continue;
                    if (tag == GridSpaceTags.Unpathable && associatedSpace.CurrentSpaceTags.Contains(GridSpaceTags.Pathable))
                    {
                        associatedSpace.CurrentSpaceTags.Remove(GridSpaceTags.Pathable);
                    }
                    associatedSpace.CurrentSpaceTags.Add(tag);
                }
            }
            return true;
        }
        else return false;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class BHECollisionManager
{
    private Dictionary<Collider2D, HashSet<Collider2D>> collidingObjects;
    public BHECollisionManager()
    {
        collidingObjects = new();
    }

    public HashSet<Collider2D> UpdatePreviewCollisions(Collider2D collider, Action<Collider2D, Collider2D> onEnter, Action<Collider2D, Collider2D> onExit)
    {

        if (!collidingObjects.ContainsKey(collider))
        {
            collidingObjects.Add(collider, new HashSet<Collider2D>());
        }


        HashSet<Collider2D> previousContacts = collidingObjects[collider];


        Collider2D[] results = new Collider2D[32];
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        int count = collider.Overlap(
            filter,
            results
        );

        HashSet<Collider2D> currentContacts = new();

        for (int i = 0; i < count; i++)
        {
            Collider2D other = results[i];

            //If its the first time going into a phase after reloading, this gets skipped and stuff collides with itself
            if (other == null || other == collider) continue;

            currentContacts.Add(other);

            if (!previousContacts.Contains(other))
            {
                onEnter?.Invoke(collider, other);  
            }
        }

        foreach (var previous in previousContacts)
        {
            if (!currentContacts.Contains(previous))
            {
                onExit?.Invoke(collider, previous);
            }
        }

        foreach (Collider2D currentCollider in currentContacts)
        {
            //Debug.Log($"Object {collider.gameObject.name} is colliding with {currentCollider.gameObject.name}");
        }

        collidingObjects[collider] = currentContacts;

        return currentContacts;
    }
}

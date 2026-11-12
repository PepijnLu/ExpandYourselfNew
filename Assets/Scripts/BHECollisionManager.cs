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

    public HashSet<Collider2D> UpdatePreviewCollisions(Collider2D collider, Action<Collider2D, Collider2D> onEnter)
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

            if (other == null || other == collider) continue;

            currentContacts.Add(other);

            if (!previousContacts.Contains(other))
            {
                onEnter.Invoke(collider, other);  
            }
        }

        foreach (var previous in previousContacts)
        {
            if (!currentContacts.Contains(previous))
            {
                //Debug.Log("Preview collision exit: " + previous.name);
            }
        }

        foreach (Collider2D currentCollider in currentContacts)
        {
            //Debug.Log($"Object {collider.gameObject.name} is colliding with {currentCollider.gameObject.name}");
        }

        previousContacts = currentContacts;

        return currentContacts;
    }
}

using System.Collections;
using UnityEngine;

namespace Spewnity
{
    // When assigned to a set of game objects, ensures that
    // the lower objects are positioned in front of higher objects.
    // For simulating depth in a 2D game.
    //
    // This can be done either by manipulating the Z position,
    // or the SpriteRenderer sort order.
    public class VerticalDrawOrder : MonoBehaviour
    {
        public int baseOrder = 0;
        public int orderMultiplier = -1000;

        private SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            sr.sortingOrder = (int)(transform.position.y * orderMultiplier) + baseOrder;
        }
    }
}
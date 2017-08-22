using System.Collections;
using System.Collections.Generic;
using Spewnity;
using UnityEngine;

// Attributes need to be outside of the Editor folder
namespace Spewnity
{
    /// <summary>
    /// In the inspector, repositions the named properties to the top of the list.
    /// For example, [Reposition("name","age")] makes sure that the name field is 
    /// at the top, followed by age, and then the rest of the properties.
    /// </summary>
    public class RepositionAttribute : PropertyAttribute
    {
        public string[] fields;

        public RepositionAttribute(params string[] fields)
        {
            this.fields = fields;
        }
    }
}
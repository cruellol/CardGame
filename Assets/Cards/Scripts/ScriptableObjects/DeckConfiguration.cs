using OneLine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Cards.ScriptableObjects
{
    //Можно было бы сделать красиво через Dropdown Attribute, но поддержка только с 2020.3.2 Unity
    [CreateAssetMenu(fileName = "NewDeckConfiguration", menuName = "CardConfigs/Deck Configuration")]
    public class DeckConfiguration : ScriptableObject
    {
        [ReadOnly]
        public int CardCount = 0;

        [SerializeField, OneLine(Header = LineHeader.Short)]
        private List<IndexCount> _indexCounter;

        private void OnValidate()
        {
            CardCount = 0;
            if (_indexCounter != null)
            {
                foreach(var ind in _indexCounter)
                {
                    CardCount += ind.Count;
                }
            }
        }
    }

    [Serializable]
    public struct IndexCount
    {
        public int CardIndex;
        public int Count;
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}

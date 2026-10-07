using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(GateEffectData))]
    public class GateEffectDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Always draw Type
            var typeProp = property.FindPropertyRelative("type");
            EditorGUI.PropertyField(line, typeProp);
            line = Next(line);

            // Get selected type
            var effType = (GateEffectType)typeProp.enumValueIndex;

            // Draw all fields associated with this type
            if (GateEffectData.FIELDS.TryGetValue(effType, out var fieldNames))
            {
                foreach (var fieldName in fieldNames)
                {
                    SerializedProperty sp = property.FindPropertyRelative(fieldName);

                    if (sp == null) continue; // in case mapping has wrong name

                    float h = EditorGUI.GetPropertyHeight(sp, includeChildren: true);
                    EditorGUI.PropertyField(new Rect(line.x, line.y, line.width, h), sp, includeChildren: true);

                    line.y += h + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Type

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            GateEffectType effType = (GateEffectType)typeProp.enumValueIndex;

            if (GateEffectData.FIELDS.TryGetValue(effType, out var fieldNames))
            {
                foreach (var fieldName in fieldNames)
                {
                    var sp = property.FindPropertyRelative(fieldName);
                    if (sp == null) continue;
                    height += EditorGUI.GetPropertyHeight(sp, includeChildren: true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        private static Rect Next(Rect line)
        {
            return new Rect(line.x, line.y + line.height + EditorGUIUtility.standardVerticalSpacing, line.width, EditorGUIUtility.singleLineHeight);
        }
    }
}

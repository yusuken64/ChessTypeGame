using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WeightedEvaluator))]
public class WeightedEvaluatorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        var evaluatorProperty = property.FindPropertyRelative("Evaluator");
        var weightProperty = property.FindPropertyRelative("Weight");

        Rect weightRect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(weightRect, weightProperty);

        Rect evaluatorRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

        EditorGUI.LabelField(evaluatorRect, "Evaluator Type:");

        string evaluatorName = evaluatorProperty.managedReferenceValue != null
            ? ObjectNames.NicifyVariableName(evaluatorProperty.managedReferenceValue.GetType().Name)
            : "None";

        if (GUI.Button(new Rect(evaluatorRect.x + 100, evaluatorRect.y, evaluatorRect.width - 100, lineHeight), evaluatorName))
        {
            GenericMenu menu = new GenericMenu();

            var evaluatorTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(EvaluatorBase).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var type in evaluatorTypes)
            {
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                {
                    var instance = Activator.CreateInstance(type);
                    evaluatorProperty.managedReferenceValue = instance;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 4;
    }
}

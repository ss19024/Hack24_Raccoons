using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Editor
{

    /// <summary>
    /// This property drawer is the meat of the interface support implementation. When
    /// the value of field with this attribute is modified, the new value is tested
    /// against the interface expected. If the component matches, the new value is
    /// accepted. Otherwise, the old value is maintained.
    /// </summary>
    [CustomPropertyDrawer(typeof(InterfaceAttribute))]
    public class InterfaceDrawer : PropertyDrawer
    {
        private int _filteredObjectPickerID;
        private static readonly Type[] _singleMonoBehaviourType = new Type[1] { typeof(MonoBehaviour) };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _filteredObjectPickerID = GUIUtility.GetControlID(FocusType.Passive);
            if (property.serializedObject.isEditingMultipleObjects) return;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "InterfaceType Attribute can only be used with MonoBehaviour Components.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Type[] attTypes = GetInterfaceTypes(property);

            // Pick a specific component
            MonoBehaviour oldComponent = property.objectReferenceValue as MonoBehaviour;
            string oldComponentName = "";

            GameObject temporaryGameObject = null;

            string attTypesName = GetTypesName(attTypes);
            if (Event.current.type == EventType.Repaint)
            {
                if (oldComponent == null)
                {
                    temporaryGameObject = new GameObject("None (" + attTypesName + ")");
                    oldComponent = temporaryGameObject.AddComponent<InterfaceMono>();
                }
                else
                {
                    oldComponentName = oldComponent.name;
                    oldComponent.name = oldComponentName + " (" + attTypesName + ")";
                }
            }

            Component currentComponent = EditorGUI.ObjectField(position, label, oldComponent, typeof(Component), true) as Component;

            int objectPickerID = GUIUtility.GetControlID(FocusType.Passive) - 1;
            ReplaceObjectPickerForControl(attTypes, objectPickerID);
            if (Event.current.commandName == "ObjectSelectorUpdated"
                && EditorGUIUtility.GetObjectPickerControlID() == _filteredObjectPickerID)
            {
                UnityEngine.Object pickedObject = EditorGUIUtility.GetObjectPickerObject();
                if (pickedObject is GameObject)
                {
                    currentComponent = (pickedObject as GameObject).transform;
                }
                else
                {
                    currentComponent = pickedObject as Component;
                }
            }

            MonoBehaviour currentMono = currentComponent as MonoBehaviour;

            if (Event.current.type == EventType.Repaint)
            {
                if (temporaryGameObject != null)
                    GameObject.DestroyImmediate(temporaryGameObject);
                else
                    oldComponent.name = oldComponentName;
            }

            // If a component is assigned, make sure it is the interface we are looking for.
            if (currentMono != null)
            {
                // Make sure component is of the right interface
                if (!IsAssignableFromTypes(currentMono.GetType(), attTypes))
                    // Component failed. Check game object.
                    foreach (Type attType in attTypes)
                    {
                        currentMono = currentMono.gameObject.GetComponent(attType) as MonoBehaviour;
                        if (currentMono == null)
                        {
                            break;
                        }
                    }

                // Item failed test. Do not override old component
                if (currentMono == null)
                {
                    if (oldComponent != null && !IsAssignableFromTypes(oldComponent.GetType(), attTypes))
                    {
                        temporaryGameObject = new GameObject("None (" + attTypesName + ")");
                        MonoBehaviour temporaryComponent = temporaryGameObject.AddComponent<InterfaceMono>();
                        currentMono = EditorGUI.ObjectField(position, label, temporaryComponent, typeof(MonoBehaviour), true) as MonoBehaviour;
                        GameObject.DestroyImmediate(temporaryGameObject);
                    }
                }
            }
            else if (currentComponent is Transform)
            {
                // If assigned component is a Transform, this means a GameObject was dragged into the property field.
                // Find all matching components on the transform's GameObject and open the picker window.

                List<MonoBehaviour> monos = new List<MonoBehaviour>();
                monos.AddRange(currentComponent.gameObject.GetComponents<MonoBehaviour>().
                    Where((mono) => IsAssignableFromTypes(mono.GetType(), attTypes)));

                if (monos.Count > 1)
                {
                    EditorApplication.delayCall += () => InterfacePicker.Show(property, monos);
                }
                else
                {
                    currentMono = monos.Count == 1 ? monos[0] : null;
                }
            }

            if (currentComponent == null || currentMono != null)
            {
                property.objectReferenceValue = currentMono;
            }

            EditorGUI.EndProperty();
        }

        private bool IsAssignableFromTypes(Type source, Type[] targets)
        {
            foreach (Type t in targets)
            {
                if (!t.IsAssignableFrom(source))
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetTypesName(Type[] attTypes)
        {
            if (attTypes.Length == 1)
            {
                return GetTypeName(attTypes[0]);
            }

            string typesString = "";
            for (int i = 0; i < attTypes.Length; i++)
            {
                if (i > 0)
                {
                    typesString += ", ";
                }

                typesString += GetTypeName(attTypes[i]);
            }

            return typesString;
        }

        private static string GetTypeName(Type attType)
        {
            if (!attType.IsGenericType)
            {
                return attType.Name;
            }

            var genericTypeNames = attType.GenericTypeArguments.Select(GetTypeName);
            return $"{attType.Name}<{string.Join(", ", genericTypeNames)}>";
        }

        private Type[] GetInterfaceTypes(SerializedProperty property)
        {
            InterfaceAttribute att = (InterfaceAttribute)attribute;
            Type[] t = att.Types;
            if (!String.IsNullOrEmpty(att.TypeFromFieldName))
            {
                var thisType = property.serializedObject.targetObject.GetType();
                while (thisType != null)
                {
                    var referredFieldInfo = thisType.GetField(att.TypeFromFieldName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (referredFieldInfo != null)
                    {
                        t = new Type[1] { referredFieldInfo.FieldType };
                        break;
                    }

                    thisType = thisType.BaseType;
                }
            }

            return t ?? _singleMonoBehaviourType;
        }

        void ReplaceObjectPickerForControl(Type[] attTypes, int replacePickerID)
        {
            var currentObjectPickerID = EditorGUIUtility.GetObjectPickerControlID();
            if (currentObjectPickerID != replacePickerID)
            {
                return;
            }

            var derivedTypes = TypeCache.GetTypesDerivedFrom(attTypes[0]);
            HashSet<Type> validTypes = new HashSet<Type>(derivedTypes);
            for (int i = 1; i < attTypes.Length; i++)
            {
                var derivedTypesIntersect = TypeCache.GetTypesDerivedFrom(attTypes[i]);
                validTypes.IntersectWith(derivedTypesIntersect);
            }

            //start filter with a long empty area to allow for easy clicking and typing
            var filterBuilder = new System.Text.StringBuilder("                       ");
            foreach (Type type in validTypes)
            {
                if (type.IsGenericType)
                {
                    continue;
                }
                filterBuilder.Append("t:" + type.FullName + " ");
            }
            string filter = filterBuilder.ToString();
            EditorGUIUtility.ShowObjectPicker<Component>(null, true, filter, _filteredObjectPickerID);
        }
    }


    public sealed class InterfaceMono : MonoBehaviour { }

}

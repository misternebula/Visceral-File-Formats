using UnityEngine;
using UnityEditor;
using System.Reflection;

// https://www.reddit.com/r/Unity3D/comments/1s6czv/inspectorbutton_add_a_custom_button_to_your/

[System.AttributeUsage(System.AttributeTargets.Field)]
public class InspectorButtonAttribute : PropertyAttribute
{
	public readonly string MethodName;

	public float ButtonWidth { get; set; } = 80;

	public InspectorButtonAttribute(string methodName) => MethodName = methodName;
}

[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
public class InspectorButtonPropertyDrawer : PropertyDrawer
{
	private MethodInfo _eventMethodInfo = null;

	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		var inspectorButtonAttribute = (InspectorButtonAttribute)attribute;
		var buttonRect = new Rect(position.x + (position.width - inspectorButtonAttribute.ButtonWidth) * 0.5f, position.y, inspectorButtonAttribute.ButtonWidth, position.height);
		if (GUI.Button(buttonRect, label.text))
		{
			var eventOwnerType = prop.serializedObject.targetObject.GetType();
			var eventName = inspectorButtonAttribute.MethodName;

			if (_eventMethodInfo == null)
			{
				_eventMethodInfo = eventOwnerType.GetMethod(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			}

			if (_eventMethodInfo != null)
			{
				_eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
			}
			else
			{
				Debug.LogWarning($"InspectorButton: Unable to find method {eventName} in {eventOwnerType}");
			}
		}
	}
}
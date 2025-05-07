using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A node for triggering script methods.
    /// </summary>
    [Serializable]
    public class TriggerScriptNode : Node, DialogueFlowNode, FunctionalNode
    {
        /// <summary>
        /// The name of the class to trigger a method on.
        /// </summary>
        [SerializeField]
        private string triggeredClassName;

        /// <summary>
        /// The name of the method to trigger.
        /// </summary>
        [SerializeField]
        private string triggeredMethodName;

        /// <summary>
        /// A string representation of the method signature for the triggered method.
        /// </summary>
        [SerializeField]
        private string methodSignature;

        /// <summary>
        /// The type names for each parameter required for the triggered method.
        /// </summary>
        [SerializeField]
        private string[] parameterTypes;

        /// <summary>
        /// The values to pass into the triggered method.
        /// </summary>
        [SerializeField]
        private string[] parameterValues;

        /// <summary>
        /// The mode or type of method to trigger.
        /// </summary>
        [SerializeField]
        private TriggerFilterType triggerType = TriggerFilterType.SELF;

        /// <summary>
        /// The name of the GameObject or the tag of the GameObject to trigger the method on.
        /// </summary>
        [SerializeField]
        private string objectTagOrName;

        /// <summary>
        /// Creates a new TriggerScriptNode.
        /// </summary>
        public TriggerScriptNode()
        {
            this.name = "TRIGGER";
            this.nodeType = NodeType.TRIGGER;
        }

        /// <summary>
        /// Gets or sets the class name to trigger a method on.
        /// </summary>
        public string TriggeredClassName
        {
            get { return this.triggeredClassName; }
            set { this.triggeredClassName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the method to trigger.
        /// </summary>
        public string TriggeredMethodName
        {
            get { return this.triggeredMethodName; }
            set { this.triggeredMethodName = value; }
        }

        /// <summary>
        /// Gets or sets the string representation of the method signature for the method to be triggered.
        /// </summary>
        public string MethodSignature
        {
            get { return this.methodSignature; }
            set { this.methodSignature = value; }
        }

        /// <summary>
        /// Gets or sets the type names for the parameters passed into the method to be triggered.
        /// </summary>
        public string[] ParameterTypes
        {
            get { return this.parameterTypes; }
            set { this.parameterTypes = value; }
        }

        /// <summary>
        /// Gets or sets the parameter values to pass into the method to be triggered.
        /// </summary>
        public string[] ParameterValues
        {
            get { return this.parameterValues; }
            set { this.parameterValues = value; }
        }

        /// <summary>
        /// Gets or sets the mode or type of method to trigger (from a string equivalent to a TriggerFilterType toString() value).
        /// </summary>
        public string TriggerTypeString
        {
            get { return this.triggerType.ToString(); }
            set { this.triggerType = Enum.Parse<TriggerFilterType>(value); }
        }

        /// <summary>
        /// Gets or sets the mode or type of method to trigger.
        /// </summary>
        public TriggerFilterType TriggerType
        {
            get { return this.triggerType; }
            set { this.triggerType = value; }
        }

        /// <summary>
        /// Gets or sets the name or tag of the GameObject to use when triggering the method.
        /// </summary>
        public string ObjectTagOrName
        {
            get { return this.objectTagOrName; }
            set { this.objectTagOrName = value;}
        }

        /// <summary>
        /// Triggers the script as defined by the node.
        /// </summary>
        /// <param name="nodeHandler">The node handler currently processing dialogue.</param>
        /// <param name="nodeOutputValues">A mapping of node IDs and connection IDs to the corresponding output values attributed to those IDs.</param>
        /// <param name="convoOwner">The GameObject which is currently running the dialogue.</param>
        /// <returns>Returns the result of the method.</returns>
        public object TriggerScript(NodeHandler nodeHandler, Dictionary<int, object> nodeOutputValues, GameObject convoOwner = null)
        {
            Type checkedClassType = Type.GetType(this.TriggeredClassName);

            if (checkedClassType == null)
            {
                bool foundClass = false;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.ToString().Equals(this.triggeredClassName))
                        {
                            checkedClassType = type;
                            foundClass = true;
                            break;
                        }
                    }

                    if (foundClass) { break; }
                }
            }

            if(checkedClassType == null)
            {
                Debug.LogError("Unable to find class '" + this.triggeredClassName + "' in loaded assemblies. Cannot call from trigger script node.");
                return null;
            }

            if (this.triggerType == TriggerFilterType.STATIC)
            {
                //Execute static method
                Type[] paramTypes = GetParameterTypes();
                MethodInfo method = checkedClassType.GetMethod(this.TriggeredMethodName, paramTypes);
                object[] parameters = DetermineParameterValues(nodeHandler, nodeOutputValues, paramTypes);

                object returnValue = method.Invoke(null, BindingFlags.Static | BindingFlags.Public, null, parameters, null);
                NodeConnection valueOutput = Outputs[0];
                nodeOutputValues.TryAdd(valueOutput.ID, returnValue);
                return returnValue;
            }
            else
            {
                UnityEngine.Object[] unityObjects = new UnityEngine.Object[1];

                if (this.triggerType == TriggerFilterType.SELF)
                {
                    unityObjects[0] = convoOwner.gameObject.GetComponentInChildren(checkedClassType);
                }
                else if (this.triggerType == TriggerFilterType.FIRST)
                {
#if UNITY_2022_3_OR_NEWER
                    unityObjects[0] = GameObject.FindFirstObjectByType(checkedClassType, FindObjectsInactive.Exclude);
#else
                    unityObjects[0] = GameObject.FindObjectOfType(checkedClassType);
#endif
                }
                else if (this.triggerType == TriggerFilterType.NAME)
                {
                    unityObjects[0] = GameObject.Find(objectTagOrName).GetComponentInChildren(checkedClassType);
                }
                else if (this.triggerType == TriggerFilterType.TAG)
                {
                    unityObjects[0] = GameObject.FindGameObjectWithTag(objectTagOrName).GetComponentInChildren(checkedClassType);
                }
                else if (this.triggerType == TriggerFilterType.ALL)
                {
#if UNITY_2022_3_OR_NEWER
                    unityObjects = GameObject.FindObjectsByType(checkedClassType, FindObjectsSortMode.None);
#else
                    unityObjects = GameObject.FindObjectsOfType(checkedClassType);
#endif
                }

                if (unityObjects[0] != null)
                {
                    Type[] paramTypes = GetParameterTypes();
                    MethodInfo method = checkedClassType.GetMethod(this.TriggeredMethodName, paramTypes);
                    object[] parameters = DetermineParameterValues(nodeHandler, nodeOutputValues, paramTypes);
                    NodeConnection valueOutput = Outputs[0];

                    object returnValue = null;
                    foreach (UnityEngine.Object unityObj in unityObjects)
                    {
                        returnValue = method.Invoke(unityObj, parameters);
                        nodeOutputValues.TryAdd(valueOutput.ID, returnValue);
                    }

                    return returnValue;
                }

                return null;
            }
        }

        /// <summary>
        /// Determines the values for each parameter to pass to the method to be called.
        /// </summary>
        /// <param name="nodeHandler">The node handler currently processing dialogue.</param>
        /// <param name="nodeOutputValues">A mapping of node IDs and connection IDs to the corresponding output values attributed to those IDs.</param>
        /// <param name="paramTypes">The GameObject which is currently running the dialogue.</param>
        /// <returns>Returns an array of values to use for each parameter.</returns>
        private object[] DetermineParameterValues(NodeHandler nodeHandler, Dictionary<int, object> nodeOutputValues, Type[] paramTypes)
        {
            object[] parameters = null;
            if (this.ParameterValues != null)
            {
                if (paramTypes.Length > 0)
                {
                    parameters = new object[paramTypes.Length];

                    for (int i = 0; i < paramTypes.Length; i++)
                    {
                        object paramValue = null;

                        if (Inputs[i+1].AttachedIDs.Count > 0)
                        {
                            int incomingId = Inputs[i + 1].AttachedIDs[0];
                            paramValue = ConvertToType(nodeOutputValues[incomingId], paramTypes[i]);
                        }
                        else
                        {
                            string paramValueString = nodeHandler.ReplaceVariablesInString(this.ParameterValues[i]);
                            paramValue = ConvertToType(paramValueString, paramTypes[i]);
                        }

                        parameters[i] = paramValue;
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Returns the actual Type for the simplified type string provided.
        /// </summary>
        /// <param name="simpleTypeName">A simplified type name.</param>
        /// <returns>The Type attributed to the simple type name specified.</returns>
        public static Type GetTypeForString(string simpleTypeName)
        {
            if (simpleTypeName.Equals("string"))
            {
                return typeof(string);
            }
            else if (simpleTypeName.Equals("byte"))
            {
                return typeof(byte);
            }
            else if (simpleTypeName.Equals("short"))
            {
                return typeof(short);
            }
            else if (simpleTypeName.Equals("int"))
            {
                return typeof(int);
            }
            else if (simpleTypeName.Equals("long"))
            {
                return typeof(long);
            }
            else if (simpleTypeName.Equals("float"))
            {
                return typeof(float);
            }
            else if (simpleTypeName.Equals("double"))
            {
                return typeof(double);
            }
            else if (simpleTypeName.Equals("decimal"))
            {
                return typeof(decimal);
            }
            else if (simpleTypeName.Equals("bool"))
            {
                return typeof(bool);
            }
            else if (simpleTypeName.Equals("object"))
            {
                return typeof(object);
            }

            return typeof(object);
        }

        /// <summary>
        /// Attempts to convert the object value provided to the specified Type.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <param name="type">The Type to convert the provided object to.</param>
        /// <returns>The converted value.</returns>
        public static object ConvertToType(object obj, Type type)
        {
            if (obj.GetType() == type)
            {
                return obj;
            }

            if (type == typeof(float))
            {
                return Convert.ToSingle(obj);
            }
            else if (type == typeof(double))
            {
                return Convert.ToDouble(obj);
            }
            else if (type == typeof(decimal))
            {
                return Convert.ToDecimal(obj);
            }
            else if (type == typeof(byte))
            {
                return Convert.ToByte(obj);
            }
            else if (type == typeof(short))
            {
                return Convert.ToInt16(obj);
            }
            else if (type == typeof(int))
            {
                return Convert.ToInt32(obj);
            }
            else if (type == typeof(long))
            {
                return Convert.ToInt64(obj);
            }
            else if (type == typeof(bool))
            {
                return Convert.ToBoolean(obj);
            }
            else if (type == typeof(string))
            {
                string value = "";
                if (obj != null)
                {
                    value = obj.ToString();
                }
                return value;
            }

            return null;
        }

        /// <summary>
        /// Returns an array of Types to use for the parameters of the method to be called.
        /// </summary>
        /// <returns>An array of the parameter Types to pass into the called method.</returns>
        private Type[] GetParameterTypes()
        {
            if (parameterTypes.Length > 0)
            {
                Type[] types = new Type[parameterTypes.Length];
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    types[i] = GetTypeForString(parameterTypes[i]);
                }

                return types;
            }

            return new Type[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return FindFlowInputs()[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            return FindFlowOutputs()[0];
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            TriggerScript(nodeHandler, nodeValues, convoOwner);
            return true;
        }

        /// <inheritdoc/>
        public bool HasDependencies()
        {
            return FindDependencyOutputIDs().Count > 0;
        }

        /// <inheritdoc/>
        public List<int> GetDependencyOutputIDs()
        {
            return FindDependencyOutputIDs();
        }
    }

    /// <summary>
    /// An enumeration defining types of method calls to use on trigger script nodes.
    /// </summary>
    public enum TriggerFilterType { SELF, ALL, FIRST, TAG, NAME, STATIC }
}
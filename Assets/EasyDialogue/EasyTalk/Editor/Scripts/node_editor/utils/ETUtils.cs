using EasyTalk.Editor.Nodes;
using EasyTalk.Nodes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Utils
{
    public class ETUtils
    {
        public static string GetSimplifiedStringForPrimitiveType(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(long))
            {
                return "long";
            }
            else if (type == typeof(short))
            {
                return "short";
            }
            else if (type == typeof(byte))
            {
                return "byte";
            }
            else if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(double))
            {
                return "double";
            }
            else if (type == typeof(decimal))
            {
                return "decimal";
            }
            else if (type == typeof(bool))
            {
                return "bool";
            }
            else if (type == typeof(object))
            {
                return "object";
            }

            return "object";
        }

        public static InputOutputType GetInputOutputTypeForType(Type type)
        {
            if (type == typeof(string))
            {
                return InputOutputType.STRING;
            }
            else if (type == typeof(int))
            {
                return InputOutputType.INT;
            }
            else if (type == typeof(long))
            {
                return InputOutputType.INT;
            }
            else if (type == typeof(short))
            {
                return InputOutputType.INT;
            }
            else if (type == typeof(byte))
            {
                return InputOutputType.INT;
            }
            else if (type == typeof(float))
            {
                return InputOutputType.FLOAT;
            }
            else if (type == typeof(double))
            {
                return InputOutputType.FLOAT;
            }
            else if (type == typeof(decimal))
            {
                return InputOutputType.FLOAT;
            }
            else if (type == typeof(bool))
            {
                return InputOutputType.BOOL;
            }
            else if (type == typeof(object))
            {
                return InputOutputType.VALUE;
            }

            return InputOutputType.VALUE;
        }

        public static InputOutputType GetInputOutputTypeForString(string type)
        {
            if (type.Equals("string"))
            {
                return InputOutputType.STRING;
            }
            else if (type.Equals("int"))
            {
                return InputOutputType.NUMBER;
            }
            else if (type.Equals("float"))
            {
                return InputOutputType.NUMBER;
            }
            else if (type.Equals("bool"))
            {
                return InputOutputType.BOOL;
            }
            else if (type.Equals("byte"))
            {
                return InputOutputType.NUMBER;
            }
            else if (type.Equals("short"))
            {
                return InputOutputType.NUMBER;
            }
            else if (type.Equals("long"))
            {
                return InputOutputType.NUMBER;
            }
            else if (type.Equals("double"))
            {
                return InputOutputType.NUMBER;
            }
            else if (type.Equals("decimal"))
            {
                return InputOutputType.NUMBER;
            }

            return InputOutputType.VALUE;
        }

        public static string GetClassListForNodeInputOutputType(InputOutputType inputType, bool connected)
        {
            switch (inputType)
            {
                case InputOutputType.ANY:
                    return connected ? "in-out-any-connected" : "in-out-any";
                case InputOutputType.DIALGOUE_FLOW:
                    return connected ? "in-out-dialogue-flow-connected" : "in-out-dialogue-flow";
                case InputOutputType.BOOL:
                    return connected ? "in-out-bool-connected" : "in-out-bool";
                case InputOutputType.FLOAT:
                    return connected ? "in-out-float-connected" : "in-out-float";
                case InputOutputType.INT:
                    return connected ? "in-out-int-connected" : "in-out-int";
                case InputOutputType.STRING:
                    return connected ? "in-out-string-connected" : "in-out-string";
                case InputOutputType.SPECIAL:
                    return connected ? "in-out-special-connected" : "in-out-special";
                case InputOutputType.OPTION_FILTER:
                    return connected ? "in-out-option-filter-connected" : "in-out-option-filter";
                case InputOutputType.VALUE:
                    return connected ? "in-out-value-connected" : "in-out-value";
                case InputOutputType.NUMBER:
                    return connected ? "in-out-number-connected" : "in-out-number";
                case InputOutputType.DIALOGUE_TRUE_FLOW:
                    return connected ? "in-out-dialogue-true-flow-connected" : "in-out-dialogue-true-flow";
                case InputOutputType.DIALOGUE_FALSE_FLOW:
                    return connected ? "in-out-dialogue-false-flow-connected" : "in-out-dialogue-false-flow";
            }

            return connected ? "node-connected" : "node-disconnected";
        }

        public static ETNode FindNodeParent(VisualElement element)
        {
            int i = 0;
            VisualElement parent = element.parent;
            do
            {
                if (parent is ETNode)
                {
                    return parent as ETNode;
                }

                i++;
            } while ((parent = parent.parent) != null && i < 10);

            return null;
        }

        public static bool IsConnectionAllowed(InputOutputType inputType, InputOutputType outputType)
        {
            bool allowConnection = false;

            //Make sure that the input and the output can be connected
            if (inputType == InputOutputType.ANY || outputType == InputOutputType.ANY)
            {
                allowConnection = true;
            }
            else if (inputType == outputType)
            {
                allowConnection = true;
            }
            else if (inputType == InputOutputType.DIALGOUE_FLOW &&
                (outputType == InputOutputType.DIALOGUE_TRUE_FLOW || outputType == InputOutputType.DIALOGUE_FALSE_FLOW))
            {
                allowConnection = true;
            }
            else if (inputType == InputOutputType.VALUE &&
                (outputType == InputOutputType.INT || outputType == InputOutputType.STRING ||
                outputType == InputOutputType.FLOAT || outputType == InputOutputType.BOOL ||
                outputType == InputOutputType.NUMBER))
            {
                allowConnection = true;
            }
            else if (inputType == InputOutputType.NUMBER &&
                (outputType == InputOutputType.INT || outputType == InputOutputType.FLOAT))
            {
                allowConnection = true;
            }
            else if ((inputType == InputOutputType.INT || inputType == InputOutputType.FLOAT) && outputType == InputOutputType.NUMBER)
            {
                allowConnection = true;
            }

            return allowConnection;
        }
    }
}
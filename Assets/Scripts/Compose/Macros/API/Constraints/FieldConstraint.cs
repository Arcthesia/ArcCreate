using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("Constraint for checking text input.")]
    [EmmyGroup("Macros")]
    public class FieldConstraint
    {
        private float? lowerBound = null;
        private float? upperBound = null;
        private bool includeLowerBound = true;
        private bool includeUpperBound = true;
        private InputType inputType = InputType.Any;
        private DynValue customCheck = null;
        private string customMessage = "";
        private FieldConstraint unionWith = null;

        public enum InputType
        {
            Any,
            Integer,
            Float,
        }

        [MoonSharpHidden]
        public InputType Type
        {
            get => inputType;
        }

        [EmmyDoc("Create a new constraint.")]
        public static FieldConstraint Create()
        {
            return new FieldConstraint();
        }

        [EmmyDoc("Filter for all texts.")]
        public FieldConstraint Any()
        {
            inputType = InputType.Any;
            return this;
        }

        [EmmyDoc("Filter for integers.")]
        public FieldConstraint Integer()
        {
            inputType = InputType.Integer;
            return this;
        }

        [EmmyDoc("Filter for float numbers (i.e decimal numbers).")]
        public FieldConstraint Float()
        {
            inputType = InputType.Float;
            return this;
        }

        [EmmyDoc("Filter for number greater or equal than the provided value.")]
        public FieldConstraint GEqual(float value)
        {
            includeLowerBound = true;
            lowerBound = value;
            return this;
        }

        [EmmyDoc("Filter for number less or equal than the provided value.")]
        public FieldConstraint LEqual(float value)
        {
            includeUpperBound = true;
            upperBound = value;
            return this;
        }

        [EmmyDoc("Filter for number greater than the provided value.")]
        public FieldConstraint Greater(float value)
        {
            includeLowerBound = false;
            lowerBound = value;
            return this;
        }

        [EmmyDoc("Filter for number less than the provided value.")]
        public FieldConstraint Less(float value)
        {
            includeUpperBound = false;
            upperBound = value;
            return this;
        }

        [EmmyDoc("Set a custom filter.")]
        public FieldConstraint Custom(DynValue function, string message = "Invalid")
        {
            customCheck = function;
            customMessage = message;
            return this;
        }

        [EmmyDoc("Also include texts that passes the provided constraint.")]
        public FieldConstraint Union(FieldConstraint constraint)
        {
            unionWith = constraint;
            return this;
        }

        [EmmyDoc("Get the constraint description.")]
        public string GetConstraintDescription()
        {
            string result = "";
            switch (inputType)
            {
                case InputType.Integer:
                    result = "Must input an integer";
                    break;
                case InputType.Float:
                    result = "Must input an number";
                    break;
                case InputType.Any:
                    result = "Must input any string";
                    break;
            }

            if (lowerBound != null)
            {
                if (includeLowerBound)
                {
                    result += $" greater than or equal to {lowerBound}";
                }
                else
                {
                    result += $" greater than {lowerBound}";
                }
            }

            if (upperBound != null)
            {
                if (lowerBound != null)
                {
                    result += " and";
                }

                if (includeUpperBound)
                {
                    result += $" less than or equal to {upperBound}";
                }
                else
                {
                    result += $" less than {upperBound}";
                }
            }

            return result;
        }

        [MoonSharpHidden]
        public (bool valid, string invalidReason) CheckValue(DynValue value)
        {
            if (value.IsNil())
            {
                return (false, "Input something");
            }

            if (customCheck != null)
            {
                return (customCheck.Function.Call(value).Boolean, customMessage);
            }

            string message = GetConstraintDescription();
            if (inputType == InputType.Any)
            {
                return (true, "");
            }

            if (unionWith != null)
            {
                bool unionResult = unionWith.CheckValue(value).valid;
                if (unionResult)
                {
                    return (true, "");
                }
            }

            // Convert to float
            if (value.CastToNumber() == null)
            {
                return (false, message);
            }

            float number = (float)(value.CastToNumber() ?? 0);

            if (inputType == InputType.Integer)
            {
                if ((int)number != number)
                {
                    return (false, message);
                }
            }

            if (number < lowerBound && includeLowerBound)
            {
                return (false, message);
            }

            if (number <= lowerBound && !includeLowerBound)
            {
                return (false, message);
            }

            if (number > upperBound && includeUpperBound)
            {
                return (false, message);
            }

            if (number >= upperBound && !includeUpperBound)
            {
                return (false, message);
            }

            return (true, "");
        }
    }
}
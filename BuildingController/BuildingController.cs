namespace SmartBuilding
{
    /// <summary>
    /// Controller for the UCLan Smart Building.
    /// </summary>
    public class BuildingController : IBuildingController
    {
        /// <summary>
        /// Hold valid building states.
        /// </summary>
        private class State
        {
            public const string closed = "closed";
            public const string open = "open";
            public const string outOfHours = "out of hours";
            public const string fireAlarm = "fire alarm";
            public const string fireDrill = "fire drill";

            /// <summary>
            /// Hold all valid states as an array.
            /// </summary>
            public static string[] validStates = new string[] { closed, open, outOfHours, fireAlarm, fireDrill };
        }

        private string buildingID;
        private string currentState;

        public BuildingController(string id)
        {
            buildingID = id;
            currentState = State.outOfHours;
            SetBuildingID(id);
        }

        public string GetBuildingID()
        {
            return buildingID;
        }

        public string GetCurrentState()
        {
            return currentState;
        }

        public string GetStatusReport()
        {
            throw new NotImplementedException();
        }

        public void SetBuildingID(string id)
        {
            buildingID = id.ToLower();
        }

        public bool SetCurrentState(string state)
        {
            bool isValidState = Array.Exists(State.validStates, (s) => { return s == state; });
            if (isValidState)
            {
                currentState = state;
            }

            return isValidState;
        }
    }
}
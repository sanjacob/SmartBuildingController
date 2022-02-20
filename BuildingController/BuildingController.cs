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
            public const string initialState = outOfHours;

            /// <summary>
            /// Hold all valid states as an array.
            /// </summary>
            private static string[] validStates = new string[] { closed, open, outOfHours, fireAlarm, fireDrill };

            /// <summary>
            /// Hold all normal states as an array.
            /// </summary>
            private static string[] normalStates = new string[] { closed, open, outOfHours };

            public static Dictionary<string, string> forbiddenTransition = new(){
                {closed, open},
                {open, closed}
            };

            public static bool isValid(string state)
            {
                return validStates.Contains(state);
            }

            public static bool isNormal(string state)
            {
                return normalStates.Contains(state);
            }

            public static bool isEmergency(string state)
            {
                return state == fireAlarm || state == fireDrill;
            }
        }

        private class ErrorMessages
        {
            public const string invalidStartState = "Argument Exception: BuildingController can only be initialised "
                + "to the following states 'open', 'closed', 'out of hours'";
        }

        private string buildingID;
        private string currentState;
        private string pastState = State.outOfHours;

        private ILightManager? lightManager;
        private IFireAlarmManager? fireAlarmManager;
        private IDoorManager? doorManager;
        private IWebService? webService;
        private IEmailService? emailService;

        public BuildingController(string id)
        {
            buildingID = id.ToLower();
            currentState = InitialState();
        }

        public BuildingController(string id, string startState)
        {
            buildingID = id.ToLower();
            currentState = InitialState(startState);
        }

        public BuildingController(string id, ILightManager iLightManager, IFireAlarmManager iFireAlarmManager,
            IDoorManager iDoorManager, IWebService iWebService, IEmailService iEmailService)
        {
            buildingID = id.ToLower();
            currentState = InitialState();

            lightManager = iLightManager;
            fireAlarmManager = iFireAlarmManager;
            doorManager = iDoorManager;
            webService = iWebService;
            emailService = iEmailService;
        }

        private string InitialState(string state = State.initialState)
        {
            state = state.ToLower();

            if (!State.isNormal(state))
            {
                throw new ArgumentException(ErrorMessages.invalidStartState);
            }

            return state;            
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
            string report = "";

            if (lightManager != null && doorManager != null && fireAlarmManager != null)
            {
                report = string.Format("{0}{1}{2}", lightManager.GetStatus(), doorManager.GetStatus(), fireAlarmManager.GetStatus());
            }

            return report;
        }

        public void SetBuildingID(string id)
        {
            buildingID = id.ToLower();
        }

        public bool SetCurrentState(string state)
        {
            state = state.ToLower();
            bool validTransition = false;

            // Only consider valid states
            if (State.isValid(state))
            {
                bool sameState = (state == currentState);
                validTransition = true;
   
                if (State.isNormal(currentState))
                {
                    // Disallow transition from open to close or viceversa
                    if (State.forbiddenTransition.ContainsKey(currentState))
                    {
                        validTransition = State.forbiddenTransition[currentState] != state;
                    }
                }
                else if (State.isEmergency(currentState))
                {
                    // New state matches state at the time of entering emergency state.
                    validTransition = (state == pastState);
                }

                if (validTransition && !sameState)
                {
                    if (state == State.open)
                    {
                        if (doorManager != null)
                        {
                            bool result = doorManager.OpenAllDoors();
                            if (!result)
                            {
                                return false;
                            }
                        }
                    }

                    // Remember and update currentState
                    pastState = currentState;
                    currentState = state;
                }
            }

            return validTransition;
        }
    }
}